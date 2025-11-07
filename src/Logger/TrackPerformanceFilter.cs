using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Core.Logger;

public class TrackPerformanceFilter : IActionFilter
{
    private PerfTracker _tracker;
    private readonly string _product;
    private readonly string _layer;

    public TrackPerformanceFilter(string product, string layer)
    {
        _product = product;
        _layer = layer;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        var request = context.HttpContext.Request;
        var activity = $"{request.Path}-{request.Method}";

        var dict = new Dictionary<string, object>();
        if (context.RouteData.Values?.Keys != null)
            foreach (var key in context.RouteData.Values?.Keys)
                dict.Add($"RouteData-{key}", (string)context.RouteData.Values[key]);

        var details = WebHelper.GetWebLogDetail(_product, _layer, activity,
            context.HttpContext, dict);

        _tracker = new PerfTracker(details);
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        _tracker?.Stop();
    }
}