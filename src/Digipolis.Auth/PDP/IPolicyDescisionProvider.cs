using System.Threading.Tasks;

namespace Digipolis.Auth.PDP
{
    public interface IPolicyDescisionProvider
    {
        Task<PdpResponse> GetPermissionsAsync(string user, string application);
    }
}