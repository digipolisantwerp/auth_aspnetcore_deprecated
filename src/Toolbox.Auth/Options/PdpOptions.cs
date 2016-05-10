using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Toolbox.Auth.Options
{
    public class PdpOptions
    {
        /// <summary>
        /// The url to the PDP endpoint.
        /// </summary>
        public string PdpUrl { get; set; }

        /// <summary>
        /// The duration in minutes the responses from the PDP are cached.
        /// Set to zero to disable caching. Default = 60.
        /// </summary>
        public int PdpCacheDuration { get; set; } = 60;
    }
}
