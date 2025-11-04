namespace Domain.Helper;

public class ApiResponse<T>
{
    public ApiResponse(bool success, string status, string message, T? data)
    {
        Success = success;
        Message = message;
        Data = data;
    }
    public ApiResponse()
    {
        
    }
    public bool Success { get; set; }
    public string Message { get; set; }
    public T? Data { get; set; }

    public static ApiResponse<T> SuccessResponse(T? value, string message = "Success")
    {
        return new ApiResponse<T>(success: true, "Success", message, value);
    }
    public static ApiResponse<T> FailureResponse(string message, string status = "Error", T data = default)
    {
        return new ApiResponse<T>(false, status, message, data);
    }

}