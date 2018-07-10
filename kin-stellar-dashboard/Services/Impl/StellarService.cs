using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kin_stellar_dashboard.Models;
using log4net;
using stellar_dotnet_sdk;
using stellar_dotnet_sdk.requests;
using stellar_dotnet_sdk.responses.operations;

namespace kin_stellar_dashboard.Services.Impl
{
    public class StellarService : IStellarService
    {
        private readonly IDatabaseService _databaseService;
        private readonly ILog _logger;
        private EventSource _eventSource;
        private OperationsRequestBuilder _operationsRequestBuilder;
        private readonly Server _server;

        public StellarService(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
            _logger = LogManager.GetLogger(typeof(StellarService));

            _server = new Server("http://horizon-kin-ecosystem.kininfrastructure.com");
            Network.UsePublicNetwork();
        }

        public async Task StartAsync()
        {
            string cursor = await GetCurrentCursorFromDatabase("operations");
            _logger.Debug($"{nameof(cursor)}: {cursor}");

            await DeleteLastCursorId(cursor);

            _operationsRequestBuilder = _server.Operations.Cursor(cursor).Limit(10);

            _eventSource = _operationsRequestBuilder.Stream((sender, response) =>
            {
                var operationsRequestBuilder = (OperationsRequestBuilder) sender;
                OperationRecordModel operationRecord = HandleOperationResponse(response);

                if (!string.IsNullOrEmpty(response.PagingToken))
                {
                    _eventSource.Headers.Remove("Last-Event-Id");
                    _eventSource.Headers.Add("Last-Event-Id", response.PagingToken);
                }
            });

            _eventSource.Error += (sender, args) =>
            {
                _logger.Error(args.Exception.Message, args.Exception);
            };

            _eventSource.Connect();
        }

        private OperationRecordModel HandleOperationResponse(OperationResponse response)
        {
            OperationRecordModel operationRecord = null;

            if (response is PaymentOperationResponse paymentOperationResponse)
            {
                if (paymentOperationResponse.AssetCode == "KIN" &&
                    double.Parse(paymentOperationResponse.Amount) < 10000)
                {
                    _logger.Info(
                        $"{response.Type}-KIN\t{paymentOperationResponse.CreatedAt}, {paymentOperationResponse.Amount}, {paymentOperationResponse.Id}");
                }
            }
            else if (response is CreateAccountOperationResponse createAccountOperationResponse)
            {
                _logger.Info(
                    $"{response.Type}\t{createAccountOperationResponse.CreatedAt}, {createAccountOperationResponse.StartingBalance}, {createAccountOperationResponse.Id}");
            }

            return operationRecord;
        }

        private async Task DeleteLastCursorId(string cursorId, params string[] operationTypes)
        {
            if (operationTypes.Length == 0)
            {
                operationTypes = new[] {"payment", "create_account"};
            }

            List<Task> tasks = operationTypes
                .Select(operationType =>
                    _databaseService.Query($"'DELETE FROM {operationType} WHERE cursor_id = {cursorId}'"))
                .Cast<Task>()
                .ToList();

            await Task.WhenAll(tasks);
        }

        private async Task<string> GetCurrentCursorFromDatabase(string cursorType)
        {
            return await _databaseService.Query($"'SELECT cursor_id FROM pagination WHERE cursor_type = {cursorType}'");
        }
    }
}