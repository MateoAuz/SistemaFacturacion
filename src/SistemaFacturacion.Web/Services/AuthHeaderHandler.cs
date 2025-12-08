using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace SistemaFacturacion.Web.Services;

public class AuthHeaderHandler : DelegatingHandler
{
    private readonly TokenHolder _tokenHolder;

    public AuthHeaderHandler(TokenHolder tokenHolder)
    {
        _tokenHolder = tokenHolder;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(_tokenHolder.Token))
        {
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", _tokenHolder.Token);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
