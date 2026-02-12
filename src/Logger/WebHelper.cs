using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Core.Logger;

public static class WebHelper
{
    public static void LogWebUsage(string product, string layer, string activityName,
        HttpContext context, Dictionary<string, object> additionalInfo = null)
    {
        var details = GetWebLogDetail(product, layer, activityName, context, additionalInfo);            
        Logger.WriteUsage(details);            
    }

    public static void LogWebDiagnostic(string product, string layer, string message,
        HttpContext context, Dictionary<string, object> diagnosticInfo = null)
    {
        var details = GetWebLogDetail(product, layer, message, context, diagnosticInfo);
        Logger.WriteDiagnostic(details);            
    }

    public static void LogWebError(string product, string layer, Exception ex, 
        HttpContext context)
    {
        var details = GetWebLogDetail(product, layer, null, context);
        details.Exception = ex;

        Logger.WriteError(details);
    }

    public static LogDetail GetWebLogDetail(string product, string layer, 
        string activityName, HttpContext context, 
        Dictionary<string, object> additionalInfo = null)
    {
        var detail = new LogDetail
        {
            Product = product,
            Layer = layer,
            Message = activityName,
            Hostname = Environment.MachineName,
            CorrelationId = Activity.Current?.Id ?? context.TraceIdentifier,
            AdditionalInfo = additionalInfo ?? new Dictionary<string, object>()
        };

        GetUserData(detail, context);
        GetRequestData(detail, context);
        // Session data??
        // Cookie data??

        return detail;
    }

    private static void GetRequestData(LogDetail detail, HttpContext context)
    {
        var request = context.Request;
        if (request == null) return;
        detail.Location = request.Path;
                                      
        detail.AdditionalInfo.Add("UserAgent", request.Headers["User-Agent"]);
        // non en-US preferences here??
        detail.AdditionalInfo.Add("Languages", request.Headers["Accept-Language"]);  

        var qdict = Microsoft.AspNetCore.WebUtilities
            .QueryHelpers.ParseQuery(request.QueryString.ToString());
        foreach (var key in qdict.Keys)
        {
            detail.AdditionalInfo.Add($"QueryString-{key}", qdict[key]);
        }
    }        

    private static void GetUserData(LogDetail detail, HttpContext context)
    {
        var userId = "";
        var userName = "";
        var clientId = "";
        var branchCode = "";

        var user = context.User;  // ClaimsPrincipal.Current is not sufficient
        if (user != null)
        {
            var i = 1; // i included in dictionary key to ensure uniqueness
            foreach (var claim in user.Claims)
            {
                switch (claim.Type)
                {
                    case ClaimTypes.NameIdentifier:
                        userId = claim.Value;
                        break;
                    case "user_email":
                        userName = claim.Value;
                        break;
                    case "client_id":
                        clientId = claim.Value;
                        break;
                    case "branch_code":
                        branchCode = claim.Value;
                        break;
                    // example dictionary key: UserClaim-4-role 
                    default:
                        detail.AdditionalInfo.Add($"UserClaim-{i++}-{claim.Type}", claim.Value);
                        break;
                }
            }
        }
        detail.UserId = userId;
        detail.UserName = userName;
        detail.Client = clientId;
        detail.BranchCode = branchCode;
    }
}