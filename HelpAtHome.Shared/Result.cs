namespace HelpAtHome.Shared
{
    public class Result
    {
        public bool IsSuccess { get; protected set; }
        public string? ErrorMessage { get; protected set; }
        public static Result Ok() => new() { IsSuccess = true };
        public static Result Fail(string msg) => new() { IsSuccess = false, ErrorMessage = msg };
    }

    public class Result<T> : Result
    {
        public T? Data { get; private set; }
        public static Result<T> Ok(T data) => new() { IsSuccess = true, Data = data };
        public new static Result<T> Fail(string msg) => new() { IsSuccess = false, ErrorMessage = msg };
    }
}
