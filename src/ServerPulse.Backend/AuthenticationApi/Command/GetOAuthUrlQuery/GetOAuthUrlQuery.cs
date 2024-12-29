using AuthenticationApi.Dtos.OAuth;
using MediatR;

namespace AuthenticationApi.Command.GetOAuthUrlQuery
{
    public record GetOAuthUrlQuery(GetOAuthUrlQueryParams QueryParams) : IRequest<GetOAuthUrlResponse>;
}
