using System;
using System.Threading.Tasks;

namespace Toolbox.Auth.PDP
{
    public interface IPolicyDescisionProvider : IDisposable
    {
        Task<PepResponse> GetPermissions(string user, string application);
    }
}