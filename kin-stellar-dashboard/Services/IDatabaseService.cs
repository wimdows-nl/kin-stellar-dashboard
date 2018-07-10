using System.Threading.Tasks;

namespace kin_stellar_dashboard.Services
{
    public interface IDatabaseService
    {
        Task<string> Query(string queryString);
    }
}