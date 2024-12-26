using Authentication.OAuth.Google;
using AuthenticationApi.Dtos.OAuth;
using MediatR;

namespace AuthenticationApi.Command.GetOAuthUrlQuery
{
    public class GetOAuthUrlQueryHandler : IRequestHandler<GetOAuthUrlQuery, GetOAuthUrlResponse>
    {
        private readonly IGoogleOAuthHttpClient googleHttpClient;

        public GetOAuthUrlQueryHandler(IGoogleOAuthHttpClient httpClient)
        {
            this.googleHttpClient = httpClient;
        }

        public Task<GetOAuthUrlResponse> Handle(GetOAuthUrlQuery request, CancellationToken cancellationToken)
        {
            var oauthParams = request.QueryParams;

            string oauthUrlRquest;

            switch (oauthParams.OAuthLoginProvider)
            {
                case OAuthLoginProvider.Google:
                    oauthUrlRquest = googleHttpClient.GenerateOAuthRequestUrl(oauthParams.Scope, oauthParams.RedirectUrl, oauthParams.CodeVerifier);
                    break;
                default:
                    throw new InvalidOperationException($"Could not process '{oathProvider}' oauth login provider");
            }

            return Task.FromResult(new GetOAuthUrlResponse { Url = oauthUrlRquest });
        }
    }
}
