namespace Authentication.OAuth.GitHub
{
    public class GitHubUserResult
    {
        public long Id { get; set; }
        public string Login { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string Email { get; set; } = default!;
    }
}
