using Microsoft.AspNetCore.Identity;

namespace AuthenticationApi
{
    public static class Utilities
    {
        public static bool HasErrors(IEnumerable<IdentityError> identityErrors, out string[] errorResponse)
        {
            if (identityErrors.Count() > 0)
            {
                var errors = identityErrors.Select(e => e.Description).ToArray();
                errorResponse = errors;
                return true;
            }

            errorResponse = [];
            return false;
        }
    }
}
