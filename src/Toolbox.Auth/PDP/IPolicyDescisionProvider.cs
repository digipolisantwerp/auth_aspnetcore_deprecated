using System;
using System.Threading.Tasks;

namespace Toolbox.Auth.PDP
{
    public interface IPolicyDescisionProvider
    {
        Task<PdpResponse> GetPermissions(string user, string application);
    }
}