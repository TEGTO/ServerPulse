using Refit;

namespace Authentication.OAuth.GitHub
{
    public interface IGitHubApi
    {
        [Get("/user")]
        public Task<GitHubUserResult?> GetUserInfoAsync([Header("Authorization")] string accessToken, CancellationToken cancellationToken);
    }
}
