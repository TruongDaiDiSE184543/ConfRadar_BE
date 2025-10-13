namespace ConfRadar.Api.Responses
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public Dictionary<string, string[]> Errors { get; set; } = new Dictionary<string, string[]>();
        public static ApiResponse<T> SuccessResponse(T data, string? message = null)
        {
            return new ApiResponse<T> { Success = true, Data = data, Message = message };
        }
        public static ApiResponse<T> FailResponse(string? message = null)
        {
            return new ApiResponse<T> { Success = false, Message = message };

        }
        public static ApiResponse<T> ValidationErrorResponse(Dictionary<string, string[]> errors, string? message = "Validation failed")
        {
            return new ApiResponse<T> { Success = false, Errors = errors, Message = message };
        }
    };

}
