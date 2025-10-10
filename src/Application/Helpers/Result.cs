namespace Transfer.Application.Helpers;

public class Result<T>
{
    public bool IsSuccess { get; }
    public T Data { get; }
    public string? Error { get; }
    public Dictionary<string, string[]> ValidationErrors { get; }

    private Result(bool isSuccess, T data, string? error, Dictionary<string, string[]>? validationErrors = null)
    {
        IsSuccess = isSuccess;
        Data = data;
        Error = error;
        ValidationErrors = validationErrors ?? new Dictionary<string, string[]>();
    }

    public static Result<T> Success(T value) => new Result<T>(true, value, null);
    public static Result<T> Failure(string? error) => new Result<T>(false, default!, error);

    public static Result<T?> ValidationFailure(Dictionary<string, string[]>? validationErrors)
    {
        return new Result<T?>(false, default, "Validation failed", validationErrors);
    }
}

public class Result
{
    public bool IsSuccess { get; }
    public string? Error { get; }
    public Dictionary<string, string[]> ValidationErrors { get; }

    private Result(bool isSuccess, string? error, Dictionary<string, string[]>? validationErrors = null)
    {
        IsSuccess = isSuccess;
        Error = error;
        ValidationErrors = validationErrors ?? new Dictionary<string, string[]>();
    }

    public static Result Success() => new Result(true, null);
    public static Result Failure(string? error) => new Result(false, error);
    public static Result ValidationFailure(Dictionary<string, string[]>? validationErrors)
        => new Result(false, "Validation failed", validationErrors);
}