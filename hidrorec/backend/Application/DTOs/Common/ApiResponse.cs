namespace HidroRec.Backend.Application.DTOs.Common;

public sealed class ApiResponse<T>
{
    public bool Success { get; init; } = true;

    public string Message { get; init; } = string.Empty;

    public T? Data { get; init; }

    public static ApiResponse<T> Ok(T? data, string message = "") => new()
    {
        Success = true,
        Message = message,
        Data = data
    };
}
