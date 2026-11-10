using System;
using System.Collections.Generic;
using System.Linq;

namespace Aster.Core.Models;

public interface IResult
{
    public bool IsOk { get; }
    
    public IEnumerable<string> Errors { get; }
}

public class SimpleResult : IResult
{
    private readonly IEnumerable<string> _errors;
    
    private static SimpleResult OkResult => new(true, null);
    private static SimpleResult ErrorEmptyResult => new(false, null);
    private static IEnumerable<string> EmptyErrorsEnumerable => Enumerable.Empty<string>();

    public bool IsOk => _errors is null;
    public IEnumerable<string> Errors => _errors ?? EmptyErrorsEnumerable;
    
    private SimpleResult(bool isOk, string? error)
    {
        if (!isOk)
            _errors = error is null ? EmptyErrorsEnumerable : GetEnumerableErrors(error);
    }

    private static IEnumerable<string> GetEnumerableErrors(string error)
    {
        yield return error;
    }
    
    public static SimpleResult Ok() => OkResult;
    public static SimpleResult Error(string error) => new(true, error);
    public static SimpleResult Error() => ErrorEmptyResult;
    
}

public class ValueResult<T> : IResult
{
    private static ValueResult<T> ErrorEmptyResult = new(SimpleResult.Error(), default);
    private IResult _internalResult;
    public bool IsOk => _internalResult.IsOk;
    
    public IEnumerable<string> Errors => _internalResult.Errors;

    public T Value => IsOk ? _value! : throw new InvalidOperationException("Результат не OK.");
    private readonly T? _value = default;

    private ValueResult(IResult result, T? value)
    {
        _internalResult = result;
        _value = value;
    }

    public static ValueResult<T> Ok(T value) => new(SimpleResult.Ok(), value);
    public static ValueResult<T> Error() => ErrorEmptyResult;

    public static ValueResult<T> Error(IResult result)
    {
        return !result.IsOk
            ? new ValueResult<T>(result, default)
            : throw new InvalidOperationException("Невозможно использовать успешный результат.");
    }

    public static ValueResult<T> Error(string error) => new(SimpleResult.Error(error), default);
}