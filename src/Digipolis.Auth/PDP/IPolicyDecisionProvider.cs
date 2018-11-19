using System.Threading.Tasks;

namespace Digipolis.Auth.PDP
{
    public interface IPolicyDecisionProvider
    {
        Task<PdpResponse> GetPermissionsAsync(string user, string application);
    }
}