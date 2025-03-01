namespace EmailControl
{
    internal class AwsEmailSettings
    {
        public const string SETTINGS_SECTION = "Email";

        public string SenderAddress { get; set; } = string.Empty;
        public string AccessKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
    }
}
