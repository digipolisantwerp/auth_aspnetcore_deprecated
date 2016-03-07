using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Auth.PDP;

namespace Toolbox.Auth.UnitTests
{
    public class MockMessageHandler : HttpMessageHandler
    {
        private readonly PdpResponse _pdpResponseContent;
        private readonly HttpStatusCode _responseCode;

        public MockMessageHandler(HttpStatusCode responseCode, PdpResponse responseContent)
        {
            _responseCode = responseCode;
            _pdpResponseContent = responseContent;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage response = null;

            switch (_responseCode)
            {
                case HttpStatusCode.OK:
                    response = new HttpResponseMessage(HttpStatusCode.OK);
                    response.Content = new ObjectContent<PdpResponse>(_pdpResponseContent, new JsonMediaTypeFormatter());
                    break;
                case HttpStatusCode.NotFound:
                    response = new HttpResponseMessage(HttpStatusCode.NotFound);
                    break;
                default:
                    break;
            }

            return Task.FromResult(response);
        }
    }
}
