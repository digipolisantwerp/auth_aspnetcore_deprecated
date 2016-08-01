using System.Collections.Generic;

namespace Digipolis.Auth.PDP
{
    public class PdpResponse
    {
        public string applicationId { get; set; }
        public string userId { get; set; }
        public IEnumerable<string> permissions { get; set; }
    }
}
