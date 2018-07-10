using System;

namespace kin_stellar_dashboard.Models
{
    public class OperationRecordModel
    {
        public string CursorId { get; }
        public DateTimeOffset Time { get; }
        public string TableName { get; }
    }
}