using AuthenticationApi.Core.Dtos.Endpoints.Auth.Register;
using EmailControl;

namespace AuthenticationApi.Application.Services
{
    public class EmailJobService : IEmailJobService
    {
        private readonly IAuthService authService;
        private readonly IEmailSender emailSender;

        public EmailJobService(IAuthService authService, IEmailSender emailSender)
        {
            this.authService = authService;
            this.emailSender = emailSender;
        }

        public async Task SendEmailConfirmationMessageAsync(RegisterRequest request)
        {
            var token = await authService.GetEmailConfirmationTokenAsync(request.Email);

            var confirmationUrl = $"{request.RedirectConfirmUrl}?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(request.Email)}";

            var emailBody = GenerateConfirmationEmailBody(confirmationUrl);

            await emailSender.SendEmailAsync(
                toEmail: request.Email,
                subject: "[Server Pulse] Confirm Your Email Address",
                body: emailBody,
                cancellationToken: CancellationToken.None
            );
        }

        private static string GenerateConfirmationEmailBody(string confirmationUrl)
        {
            return $@"
            <html>
                <head>
                    <style>
                        body {{
                            font-family: Arial, sans-serif;
                            line-height: 1.6;
                            color: #333;
                        }}
                        a {{
                            color: #1a73e8;
                            text-decoration: none;
                        }}
                        .button {{
                            display: inline-block;
                            padding: 10px 20px;
                            font-size: 16px;
                            color: #fff !important;
                            background-color: #1a73e8;
                            border-radius: 5px;
                            text-align: center;
                            text-decoration: none;
                        }}
                        .button:hover {{
                            background-color: #155cb0;
                        }}
                    </style>
                </head>
                <body>
                    <h2>Welcome!</h2>
                    <p>Thank you for registering. Please confirm your email address by clicking the button below:</p>
                    <a href='{confirmationUrl}' class='button'>Confirm Email</a>
                    <p>If the button above doesn't work, copy and paste the following link into your browser:</p>
                    <p><a href='{confirmationUrl}'>{confirmationUrl}</a></p>
                    <p>Thank you for joining us!</p>
                </body>
            </html>";
        }
    }
}
