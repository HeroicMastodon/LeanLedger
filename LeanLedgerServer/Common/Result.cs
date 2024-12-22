namespace LeanLedgerServer.Common;

using System.Diagnostics;

public abstract record Result<T> {
    public T Unwrap => this switch {
        Ok<T>(var val) => val,
        _ => throw new InvalidOperationException("Attempted to unwrap a failed result")
    };

    public T? UnwrapOr(T? defaultValue) => this switch {
        Ok<T>(var val) => val,
        _ => defaultValue
    };

    public Result<TNew> Then<TNew>(Func<T, Result<TNew>> action) => this switch {
        Ok<T>(var val) => action(val),
        Err<T>(var error) => new Err<TNew>(error),
        _ => throw new UnreachableException()
    };

    public Result<TNew> Then<TNew>(Func<T, TNew> action) => this switch {
        Ok<T>(var val) => action(val).AsResult(),
        Err<T>(var error) => new Err<TNew>(error),
        _ => throw new UnreachableException()
    };

    public async Task<Result<TNew>> ThenAsync<TNew>(Func<T, Task<Result<TNew>>> action) => this switch {
        Ok<T>(var val) => await action(val),
        Err<T>(var errs) => new Err<TNew>(errs),
        _ => throw new UnreachableException()
    };

    public async Task<Result<TNew>> ThenAsync<TNew>(Func<T, Task<TNew>> action) => this switch {
        Ok<T>(var val) => (await action(val)).AsResult(),
        Err<T>(var errs) => new Err<TNew>(errs),
        _ => throw new UnreachableException()
    };

    public TNew When<TNew>(Func<T, TNew> ok, Func<Err, TNew> error) => this switch {
        Ok<T>(var val) => ok(val),
        Err<T>(var err) => error(err),
        _ => throw new UnreachableException()
    };

    public Task<TNew> WhenAsync<TNew>(Func<T, Task<TNew>> ok, Func<Err, Task<TNew>> error) => this switch {
        Ok<T>(var val) => ok(val),
        Err<T>(var err) => error(err),
        _ => throw new UnreachableException()
    };

    public bool IsOk() => this switch {
        Ok<T> => true,
        Err<T> => false,
        _ => throw new UnreachableException()
    };

    public bool IsOk(out T? value) {
        value = UnwrapOr(default);
        return IsOk();
    }

    public bool IsOk(out T? value, out Result<T> result) {
        value = UnwrapOr(default);
        result = this;
        return IsOk();
    }

    public Err? GetError() => this switch {
        Ok<T> => null,
        Err<T>(var err) => err,
        _ => throw new UnreachableException()
    };

    public static implicit operator Result<T>(T value) => new Ok<T>(value);
    public static implicit operator Result<T>(Err error) => new Err<T>(error);
}

public static class ResultExtensions {
    public static Result<T> AsResult<T>(this T value) => new Ok<T>(value);

    public static async Task<Result<TNew>> Then<T, TNew>(this Task<Result<T>> task, Func<T, Result<TNew>> action) {
        var result = await task;
        return result.Then(action);
    }

    public static async Task<Result<TNew>> Then<T, TNew>(this Task<Result<T>> task, Func<T, TNew> action) {
        var result = await task;
        return result.Then(action);
    }

    public static async Task<Result<TNew>> ThenAsync<T, TNew>(this Task<Result<T>> task, Func<T, Task<Result<TNew>>> action) {
        var result = await task;
        return await result.ThenAsync(action);
    }

    public static async Task<Result<TNew>> ThenAsync<T, TNew>(this Task<Result<T>> task, Func<T, Task<TNew>> action) {
        var result = await task;
        return await result.ThenAsync(action);
    }

    public static async Task<TNew> When<T, TNew>(this Task<Result<T>> task, Func<T, TNew> ok, Func<Err, TNew> error)
        => (await task).When(ok, error);

    public static async Task<TNew> WhenAsync<T, TNew>(this Task<Result<T>> task, Func<T, Task<TNew>> ok, Func<Err, Task<TNew>> error)
        => await (await task).WhenAsync(ok, error);
}

public record Ok<T>(T Value): Result<T>;

public record Err<T>(Err Error): Result<T>;
