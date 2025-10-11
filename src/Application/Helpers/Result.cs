namespace TegWallet.Application.Helpers;

public class Result<T>
{
    public bool Success { get; }
    public T Data { get; }
    public string? Error { get; }
    public Dictionary<string, string[]> ValidationErrors { get; }

    private Result(bool success, T data, string? error, Dictionary<string, string[]>? validationErrors = null)
    {
        Success = success;
        Data = data;
        Error = error;
        ValidationErrors = validationErrors ?? new Dictionary<string, string[]>();
    }

    public static Result<T> Succeeded(T value) => new Result<T>(true, value, null);
    public static Result<T> Failed(string? error) => new Result<T>(false, default!, error);

    public static Result<T?> ValidationFailure(Dictionary<string, string[]>? validationErrors)
    {
        return new Result<T?>(false, default, "Validation failed", validationErrors);
    }
}

public class Result
{
    public bool Success { get; }
    public string? Error { get; }
    public Dictionary<string, string[]> ValidationErrors { get; }

    private Result(bool success, string? error, Dictionary<string, string[]>? validationErrors = null)
    {
        Success = success;
        Error = error;
        ValidationErrors = validationErrors ?? new Dictionary<string, string[]>();
    }

    public static Result Succeeded() => new Result(true, null);
    public static Result Failed(string? error) => new Result(false, error);
    public static Result ValidationFailed(Dictionary<string, string[]>? validationErrors)
        => new Result(false, "Validation failed", validationErrors);
}