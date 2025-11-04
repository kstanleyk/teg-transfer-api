using Microsoft.AspNetCore.Authorization;
using TegWallet.Application.Authorization;

namespace TegWallet.WalletApi.Attributes
{
    public class MustHavePermissionAttribute : AuthorizeAttribute
    {
        public MustHavePermissionAttribute(string feature, string action)
            => Policy = AppPermission.NameFor(feature, action);        
    }
}
