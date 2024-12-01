﻿using AuthenticationApi.Dtos;
using MediatR;

namespace AuthenticationApi.Command.RefreshToken
{
    public record RefreshTokenCommand(AuthToken Request) : IRequest<AuthToken>;
}
