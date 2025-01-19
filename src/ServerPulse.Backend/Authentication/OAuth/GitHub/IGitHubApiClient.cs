
namespace Authentication.OAuth.GitHub
{
    public interface IGitHubApiClient
    {
        public Task<GitHubUserResult?> GetUserInfoAsync(string accessToken, CancellationToken cancellationToken);
    }
}