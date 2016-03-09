using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Auth.PDP;

namespace Toolbox.Auth.UnitTests
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
                        response.Content = new ObjectContent<T>(_responseContent, new JsonMediaTypeFormatter());
                    }

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
