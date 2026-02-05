namespace Transaction.Aggregator.Domain.Models;

public record class Result<T>
    where T : class
{

    public T? Value { get; init; }
    public ErrorDetail? Error { get; init; }
    public bool IsSuccess => Error == null;

    public static Result<T> Success(T value) => new (){ Value = value };
    public static Result<T> Failure(ErrorDetail error) => new() { Error = error };
}


public record ErrorDetail
{
    public string Source { get; init; }= null!;
    public string Message { get; init; }= null!;
}