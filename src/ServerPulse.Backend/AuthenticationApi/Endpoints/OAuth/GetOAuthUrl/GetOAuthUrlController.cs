using AuthenticationApi.Application;
using AuthenticationApi.Application.Services;
using AuthenticationApi.Core.Dtos.Endpoints.OAuth.GetOAuthUrl;
using AuthenticationApi.Core.Enums;
using ExceptionHandling;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace AuthenticationApi.Endpoints.OAuth.GetOAuthUrl
{
    [FeatureGate(Features.OAUTH)]
    [Route("oauth")]
    [ApiController]
    public class GetOAuthUrlController : ControllerBase
    {
        private readonly Dictionary<OAuthLoginProvider, IOAuthService> oAuthServices;

        public GetOAuthUrlController(Dictionary<OAuthLoginProvider, IOAuthService> oAuthServices)
        {
            this.oAuthServices = oAuthServices;
        }

        [HttpGet]
        [SwaggerOperation(
            Summary = "Get OAuth Confirmation Url.",
            Description = "Gets OAuth Confirmation Url for the requested oauth provider."
        )]
        [ProducesResponseType(typeof(GetOAuthUrlResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseError), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<GetOAuthUrlResponse>> GetOAuthUrlAsync([FromQuery] GetOAuthUrlParams queryParams, CancellationToken cancellationToken)
        {
            var oathProvider = queryParams.OAuthLoginProvider;

            var url = await oAuthServices[oathProvider]
                .GenerateOAuthRequestUrlAsync(queryParams.RedirectUrl, cancellationToken);

            return Ok(new GetOAuthUrlResponse { Url = url });
        }
    }
}
