using Refit;

namespace Authentication.OAuth.GitHub
{
    public interface IGitHubApi
    {
        [Get(ExternalEndpoints.GITHUB_API_USER)]
        public Task<GitHubUserResult?> GetUserInfoAsync([Header("Authorization")] string accessToken, CancellationToken cancellationToken);
    }
}
