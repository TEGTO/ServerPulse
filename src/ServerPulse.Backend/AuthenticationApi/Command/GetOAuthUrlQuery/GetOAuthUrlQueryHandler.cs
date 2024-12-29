using AuthenticationApi.Dtos.OAuth;
using AuthenticationApi.Services;
using MediatR;

namespace AuthenticationApi.Command.GetOAuthUrlQuery
{
    public class GetOAuthUrlQueryHandler : IRequestHandler<GetOAuthUrlQuery, GetOAuthUrlResponse>
    {
        private readonly Dictionary<OAuthLoginProvider, IOAuthService> oAuthServices;

        public GetOAuthUrlQueryHandler(Dictionary<OAuthLoginProvider, IOAuthService> oAuthServices)
        {
            this.oAuthServices = oAuthServices;
        }

        public Task<GetOAuthUrlResponse> Handle(GetOAuthUrlQuery request, CancellationToken cancellationToken)
        {
            var oathProvider = request.QueryParams.OAuthLoginProvider;

            var url = oAuthServices[oathProvider].GenerateOAuthRequestUrl(
                new OAuthRequestUrlParams(request.QueryParams.RedirectUrl!, request.QueryParams.CodeVerifier!));

            return Task.FromResult(new GetOAuthUrlResponse { Url = url });
        }
    }
}
