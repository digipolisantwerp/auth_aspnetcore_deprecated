using System;
using System.Threading.Tasks;

namespace Toolbox.Auth.PDP
{
    public interface IPolicyDescisionProvider
    {
        Task<PdpResponse> GetPermissionsAsync(string user, string application);
    }
}