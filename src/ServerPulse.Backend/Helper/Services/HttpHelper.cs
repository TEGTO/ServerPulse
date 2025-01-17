﻿using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace Helper.Services
{
    public class HttpHelper : IHttpHelper
    {
        private readonly IHttpClientFactory httpClientFactory;

        public HttpHelper(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
        }

        public async Task<T?> SendGetRequestAsync<T>(
            string endpoint,
            Dictionary<string, string>? queryParams = null,
            Dictionary<string, string>? headers = null,
            string? accessToken = null,
            CancellationToken cancellationToken = default)
        {
            return await SendHttpRequestAsync<T>(
                HttpMethod.Get,
                endpoint: endpoint,
                accessToken: accessToken,
                queryParams: queryParams,
                headers: headers,
                cancellationToken: cancellationToken);
        }

        public async Task<T?> SendPostRequestAsync<T>(
            string endpoint,
            Dictionary<string, string> bodyParams,
            string? accessToken = null,
            CancellationToken cancellationToken = default)
        {
            var httpContent = new FormUrlEncodedContent(bodyParams);
            return await SendHttpRequestAsync<T>(
                HttpMethod.Post,
                endpoint: endpoint,
                httpContent: httpContent,
                accessToken: accessToken,
                cancellationToken: cancellationToken);
        }

        public async Task<T?> SendPostRequestAsync<T>(
            string endpoint,
            string json,
            string? accessToken = null,
            CancellationToken cancellationToken = default)
        {
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
            return await SendHttpRequestAsync<T>(
                HttpMethod.Post,
                endpoint: endpoint,
                httpContent: httpContent,
                accessToken: accessToken,
                cancellationToken: cancellationToken);
        }

        public async Task SendPostRequestAsync(
            string endpoint,
            string json,
            string? accessToken = null,
            CancellationToken cancellationToken = default)
        {
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
            await SendHttpRequestAsync<dynamic>(
                HttpMethod.Post,
                endpoint: endpoint,
                httpContent: httpContent,
                accessToken: accessToken,
                cancellationToken: cancellationToken);
        }

        public async Task SendPutRequestAsync(
            string endpoint,
            string jsonBody,
            Dictionary<string, string>? queryParams = null,
            string? accessToken = null,
            CancellationToken cancellationToken = default)
        {
            var httpContent = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            await SendHttpRequestAsync<dynamic>(
                HttpMethod.Put,
                endpoint: endpoint,
                accessToken: accessToken,
                queryParams: queryParams,
                httpContent: httpContent,
                cancellationToken: cancellationToken);
        }

        public async Task SendDeleteRequestAsync(
            string endpoint,
            string? accessToken = null,
            CancellationToken cancellationToken = default)
        {
            await SendHttpRequestAsync<dynamic>(HttpMethod.Get, endpoint: endpoint, accessToken: accessToken, cancellationToken: cancellationToken);
        }

        private async Task<T?> SendHttpRequestAsync<T>(
            HttpMethod httpMethod,
            string endpoint,
            string? accessToken = null,
            Dictionary<string, string>? queryParams = null,
            Dictionary<string, string>? headers = null,
            HttpContent? httpContent = null,
            CancellationToken cancellationToken = default)
        {
            var url = queryParams != null
                ? QueryHelpers.AddQueryString(endpoint, queryParams!)
                : endpoint;

            var request = new HttpRequestMessage(httpMethod, url);

            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            if (accessToken != null)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }

            if (httpContent != null)
            {
                request.Content = httpContent;
            }

            using var httpClient = httpClientFactory.CreateClient(HelperConfigurationKeys.HTTP_CLIENT_HELPER);
            using var response = await httpClient.SendAsync(request: request, cancellationToken: cancellationToken);

            var resultJson = await response.Content.ReadAsStringAsync(cancellationToken: cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(resultJson);
            }

            var result = JsonConvert.DeserializeObject<T>(resultJson);
            return result;
        }
    }
}
