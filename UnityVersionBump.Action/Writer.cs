using System.Text.RegularExpressions;

class Writer : DelegatingHandler
{
    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return SendAsync(request, cancellationToken).Result;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var res = base.SendAsync(request, cancellationToken);
        File.WriteAllText(
            $"{request.Method}_{new Regex("\\W").Replace(request.RequestUri.ToString(), "_")}", 
            res.Result.Content.ReadAsStringAsync(cancellationToken).Result
        );
        return res;
    }
}