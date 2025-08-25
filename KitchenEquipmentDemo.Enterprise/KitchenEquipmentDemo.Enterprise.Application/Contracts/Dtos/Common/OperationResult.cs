// Contracts/Dtos/Common/OperationResult.cs
using System.Collections.Generic;

public class OperationResult
{
    public bool Succeeded { get; protected set; }   // <- was private set
    public string Message { get; protected set; }   // <- was private set
    public List<string> Errors { get; } = new List<string>();

    public static OperationResult Success(string message = null)
        => new OperationResult { Succeeded = true, Message = message };

    public static OperationResult Fail(params string[] errors)
    {
        var r = new OperationResult { Succeeded = false };
        if (errors != null) r.Errors.AddRange(errors);
        return r;
    }
}

public class OperationResult<T> : OperationResult
{
    public T Data { get; private set; }

    public static OperationResult<T> Success(T data, string message = null)
        => new OperationResult<T> { Data = data, Succeeded = true, Message = message };

    public new static OperationResult<T> Fail(params string[] errors)
    {
        var r = new OperationResult<T> { Succeeded = false };
        if (errors != null) r.Errors.AddRange(errors);
        return r;
    }
}
