namespace TegWallet.Application.Helpers;

public enum RepositoryActionStatus
{
    Okay,
    Created,
    Updated,
    NotFound,
    ConcurrencyConflict,
    AlreadyExists,
    Deadlock,
    Deleted,
    NothingModified,
    Error,
    Invalid,        // FK constraint violation
    Canceled
}

public class RepositoryActionResult
{

}

public class RepositoryActionResult<T>(T? entity, RepositoryActionStatus status) : RepositoryActionResult
{
    public T? Entity { get; } = entity;
    public RepositoryActionStatus Status { get; } = status;
    public Exception? Exception { get; }

    public string? Message { get; } = string.Empty;

    public RepositoryActionResult(T? entity, RepositoryActionStatus status, Exception? exception) :
        this(entity, status)
    {
        Exception = exception;
    }

    public RepositoryActionResult(T? entity, RepositoryActionStatus status, string? message = "Operation failed!") :
        this(entity, status)
    {
        Message = message;
    }
}