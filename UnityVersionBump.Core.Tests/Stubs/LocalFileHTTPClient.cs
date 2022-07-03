using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

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
            var executingAssembly = Assembly.GetExecutingAssembly();
            Assert.NotNull(request.RequestUri);
            var path = new Regex("[^\\/.a-zA-Z0-9_]").Replace($"{executingAssembly.GetName().Name}.{_resourceStreamPathToResponse}.{request.Method}{request.RequestUri.PathAndQuery.Replace("/", ".")}.out", "_");
            IEnumerable<string> list = path.Split(".");
            list = list.Select(entry =>
            {
                if (long.TryParse(entry, out _))
                {
                    return "_" + entry;
                }

                return entry;
            });

            var stream = executingAssembly.GetManifestResourceStream(string.Join(".", list));
            if (stream == null)
            {
                throw new NotImplementedException($"No response for the following request exists: {request.Method} {request.RequestUri.PathAndQuery}");
            }

            using var streamReader = new StreamReader(stream);
            var content = await streamReader.ReadToEndAsync();
            var splitContent = content.Split("\n");
            var statusCode = (HttpStatusCode)int.Parse(splitContent.First());
            return new(statusCode)
            {
                Content = new StringContent(string.Join(Environment.NewLine, splitContent.Skip(1)), Encoding.UTF8, "application/json")
            };

        }
    }
}
