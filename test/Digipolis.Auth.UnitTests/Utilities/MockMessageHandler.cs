using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Digipolis.Auth.UnitTests
{
    public class MockMessageHandler<T> : HttpMessageHandler
    {
        private readonly T _responseContent;
        private readonly HttpStatusCode _responseCode;

        public MockMessageHandler(HttpStatusCode responseCode, T responseContent)
        {
            _responseCode = responseCode;
            _responseContent = responseContent;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage response = null;

            switch (_responseCode)
            {
                case HttpStatusCode.OK:
                    response = new HttpResponseMessage(HttpStatusCode.OK);

                    if (typeof(T) == typeof(string))
                    {
                        response.Content = new StringContent(_responseContent as String);
                    }
                    else
                    {
                        response.Content = new StringContent(JsonConvert.SerializeObject(_responseContent));
                    }

                    break;
                default:
                    response = new HttpResponseMessage(_responseCode);
                    break;
            }

            return Task.FromResult(response);
        }
    }
}
