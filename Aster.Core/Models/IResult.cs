using System;
using System.Collections.Generic;
using System.Linq;

namespace Aster.Core.Models;

public interface IResult
{
    bool IsOk { get; }
    IReadOnlyList<string> Errors { get; }
}

public sealed class SimpleResult : IResult
{
    private static readonly IReadOnlyList<string> EmptyErrors = Array.Empty<string>();

    private readonly bool _isOk;
    private readonly IReadOnlyList<string> _errors;

    private SimpleResult(bool isOk, IReadOnlyList<string> errors)
    {
        _isOk = isOk;
        _errors = errors;
    }

    public bool IsOk => _isOk;
    public IReadOnlyList<string> Errors => _errors;

    public static SimpleResult Ok() => new(true, EmptyErrors);

    public static SimpleResult Error() => new(false, EmptyErrors);

    public static SimpleResult Error(string error)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(error);
        return new(false, new[] { error });
    }

    public static SimpleResult Error(IEnumerable<string> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);

        string[] materializedErrors = errors
            .Where(error => !string.IsNullOrWhiteSpace(error))
            .ToArray();

        return new(false, materializedErrors);
    }
}

public sealed class ValueResult<T> : IResult
{
    private readonly IResult _result;
    private readonly T? _value;

    private ValueResult(IResult result, T? value)
    {
        _result = result;
        _value = value;
    }

    public bool IsOk => _result.IsOk;
    public IReadOnlyList<string> Errors => _result.Errors;

    public T Value => IsOk
        ? _value!
        : throw new InvalidOperationException("Результат не OK.");

    public static ValueResult<T> Ok(T value) =>
        new(SimpleResult.Ok(), value);

    public static ValueResult<T> Error() =>
        new(SimpleResult.Error(), default);

    public static ValueResult<T> Error(string error) =>
        new(SimpleResult.Error(error), default);

    public static ValueResult<T> Error(IResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return result.IsOk
            ? throw new ArgumentException(
                "Невозможно создать ошибочный результат из успешного результата.",
                nameof(result))
            : new ValueResult<T>(result, default);
    }
}
