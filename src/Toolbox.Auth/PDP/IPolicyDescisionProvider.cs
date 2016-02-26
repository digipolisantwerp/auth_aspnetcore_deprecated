using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Toolbox.Auth.PDP
{
    public interface IPolicyDescisionProvider : IDisposable
    {
        /// <summary>
        /// Request the PDP if a user has access to a single resource in the context of an application.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="application"></param>
        /// <param name="resource">Returns true if the user has access to the resource.</param>
        /// <returns></returns>
        Task<bool> HasAccessAsync(string user, string application, string resource);

        /// <summary>
        /// Request the PDP if a user has access to any of multiple resources in the context of an application.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="application"></param>
        /// <param name="resource">Returns true if the user has access to at least one resource.</param>
        /// <returns></returns>
        Task<bool> HasAccessAsync(string user, string application, IEnumerable<string> resources);
    }
}