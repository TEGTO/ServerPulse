using AuthenticationApi.Dtos;
using AuthenticationApi.Infrastructure;
using AuthenticationApi.Infrastructure.Models;
using AuthenticationApi.Services;
using AutoMapper;
using EmailControl;
using ExceptionHandling;
using Hangfire;
using MediatR;
using Microsoft.FeatureManagement;
using IBackgroundJobClient = BackgroundTask.IBackgroundJobClient;

namespace AuthenticationApi.Command.RegisterUser
{
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Unit>
    {
        private readonly IAuthService authService;
        private readonly IFeatureManager featureManager;
        private readonly IEmailSender emailSender;
        private readonly IBackgroundJobClient backgroundJobClient;
        private readonly IMapper mapper;

        public RegisterUserCommandHandler(
          IAuthService authService,
          IFeatureManager featureManager,
          IEmailSender emailSender,
          IBackgroundJobClient backgroundJobClient,
          IMapper mapper)
        {
            this.authService = authService;
            this.featureManager = featureManager;
            this.emailSender = emailSender;
            this.backgroundJobClient = backgroundJobClient;
            this.mapper = mapper;
        }

        public async Task<Unit> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            var user = mapper.Map<User>(request);

            var registerModel = new RegisterUserModel { User = user, Password = request.Password };

            var errors = (await authService.RegisterUserAsync(registerModel, cancellationToken)).Errors;
            if (Utilities.HasErrors(errors, out var errorResponse))
            {
                throw new AuthorizationException(errorResponse);
            }

            backgroundJobClient.Enqueue(() => SendEmailConfirmationMessage(request));

            return Unit.Value;
        }

        [AutomaticRetry(Attempts = 3)]
        public async Task SendEmailConfirmationMessage(UserRegistrationRequest request)
        {
            if (await featureManager.IsEnabledAsync(ConfigurationKeys.REQUIRE_EMAIL_CONFIRMATION))
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