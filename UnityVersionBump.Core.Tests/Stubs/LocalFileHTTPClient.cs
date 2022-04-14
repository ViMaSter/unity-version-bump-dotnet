using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UnityVersionBump.Core.Tests.Stubs
{
    public class LocalFileMessageHandler : HttpMessageHandler
    {
        private readonly string _resourceStreamPathToResponse;

        public LocalFileMessageHandler(string resourceStreamPathToResponse)
        {
            _resourceStreamPathToResponse = resourceStreamPathToResponse;
        }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await new StreamReader(GetType().Assembly.GetManifestResourceStream(_resourceStreamPathToResponse)!).ReadToEndAsync().ConfigureAwait(false);
            return new(HttpStatusCode.OK)
            {
                Content = new StringContent(response, Encoding.UTF8, "application/json"),
                StatusCode = HttpStatusCode.OK
            };
        }
    }
}
