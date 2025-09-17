﻿using Agrovet.Application.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace Agrovet.WebApi.Attributes
{
    public class MustHavePermissionAttribute : AuthorizeAttribute
    {
        public MustHavePermissionAttribute(string feature, string action)
            => Policy = AppPermission.NameFor(feature, action);        
    }
}
