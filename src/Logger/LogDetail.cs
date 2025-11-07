using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Core.Logger;

public class LogDetail
{
    public LogDetail()
    {
        Timestamp = DateTime.UtcNow;
        AdditionalInfo = new Dictionary<string, object>();
    }

    public DateTime Timestamp { get; set; }
    public string Message { get; set; }

    // Context
    public string Product { get; set; }
    public string Layer { get; set; }
    public string Location { get; set; }
    public string Hostname { get; set; }

    // User info
    public string UserId { get; set; }
    public string UserName { get; set; }
    public string CustomerId { get; set; }
    public string CustomerName { get; set; }
    public string Client { get; set; }
    public string BranchCode { get; set; }

    // Metrics and errors
    public long? ElapsedMilliseconds { get; set; }
    public Exception Exception { get; set; }
    public CustomException CustomException { get; set; }
    public string CorrelationId { get; set; }
    public Dictionary<string, object> AdditionalInfo { get; set; }
}