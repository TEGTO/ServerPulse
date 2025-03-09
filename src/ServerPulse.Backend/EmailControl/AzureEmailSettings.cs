namespace EmailControl
{
    public class AzureEmailSettings
    {
        public const string SETTINGS_SECTION = "Email";

        public string ConnectionString { get; set; } = string.Empty;
        public string SenderAddress { get; set; } = string.Empty;
    }
}
