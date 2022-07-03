using System.Text.RegularExpressions;

public class Writer : DelegatingHandler
{
    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return SendAsync(request, cancellationToken).Result;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var res = base.SendAsync(request, cancellationToken);
        var path = $"{request.Method}/{new Regex("[^\\/.a-zA-Z0-9_]").Replace(request.RequestUri.PathAndQuery[1..], "_")}.out";
        path = string.Join("/", path.Split("/").Select(entry =>
        {
            if (long.TryParse(entry, out _))
            {
                return "_" + entry;
            }

            return entry;
        }));
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(
            path,
            $"{(int)res.Result.StatusCode}\n{res.Result.Content.ReadAsStringAsync(cancellationToken).Result}"
        );
        return res;
    }
}