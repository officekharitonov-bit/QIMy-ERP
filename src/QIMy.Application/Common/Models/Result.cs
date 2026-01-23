namespace QIMy.Application.Common.Models;

/// <summary>
/// Представляет результат операции без возвращаемого значения
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public string[] Errors { get; }
    public string? Error => Errors.Length > 0 ? string.Join(", ", Errors) : null;

    protected Result(bool isSuccess, string[] errors)
    {
        IsSuccess = isSuccess;
        Errors = errors ?? Array.Empty<string>();
    }

    public static Result Success() => new(true, Array.Empty<string>());
    public static Result Failure(params string[] errors) => new(false, errors);
}

/// <summary>
/// Представляет результат операции с возвращаемым значением типа T
/// </summary>
public class Result<T> : Result
{
    public T? Value { get; }

    private Result(bool isSuccess, T? value, string[] errors) : base(isSuccess, errors)
    {
        Value = value;
    }

    public static Result<T> Success(T value) => new(true, value, Array.Empty<string>());
    public new static Result<T> Failure(params string[] errors) => new(false, default, errors);
}
