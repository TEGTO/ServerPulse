﻿using AuthenticationApi.Application;
using AuthenticationApi.Application.Services;
using AuthenticationApi.Core.Dtos.Endpoints.OAuth.GetOAuthUrl;
using AuthenticationApi.Core.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;

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
        public ActionResult<GetOAuthUrlResponse> GetOAuthUrl([FromQuery] GetOAuthUrlParams queryParams)
        {
            var oathProvider = queryParams.OAuthLoginProvider;

            var url = oAuthServices[oathProvider].GenerateOAuthRequestUrl(
                new OAuthRequestUrlParams(queryParams.RedirectUrl!, queryParams.CodeVerifier!));

            return Ok(new GetOAuthUrlResponse { Url = url });
        }
    }
}
