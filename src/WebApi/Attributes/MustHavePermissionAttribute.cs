using Microsoft.AspNetCore.Authorization;
using Transfer.Application.Authorization;

namespace Transfer.WebApi.Attributes
{
    public class MustHavePermissionAttribute : AuthorizeAttribute
    {
        public MustHavePermissionAttribute(string feature, string action)
            => Policy = AppPermission.NameFor(feature, action);        
    }
}
