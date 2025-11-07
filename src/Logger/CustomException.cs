using System.Text.Json;

namespace Core.Logger;

public class CustomException
{
    public string? ExceptionName { get; set; }
    public string? ModuleName { get; set; }
    public string? DeclaringTypeName { get; set; }
    public string? TargetSiteName { get; set; }
    public string? Message { get; set; }
    public string? StackTrace { get; set; }
    public Dictionary<string, string?> Data { get; set; } = new();
    public CustomException? InnerException { get; set; }

    public static CustomException? FromException(Exception? ex)
    {
        if (ex == null) return null;

        return new CustomException
        {
            ExceptionName = ex.GetType().Name,
            Message = ex.Message,
            StackTrace = ex.StackTrace,
            Data = ex.Data.Keys.Cast<object>().ToDictionary(k => k.ToString(), k => ex.Data[k]?.ToString())!,
            InnerException = FromException(ex.InnerException)
        };
    }

    public override string ToString() =>
        JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
}