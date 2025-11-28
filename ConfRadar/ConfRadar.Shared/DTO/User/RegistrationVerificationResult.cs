using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfRadar.Shared.DTO.User
{
    public class RegistrationVerificationResult
    {
        public int? Result { get; set; }           
        public string? ErrorCode { get; set; }    
        public bool Success => Result.HasValue && Result > 0;
    }
    public static class RegistrationVerificationMessage
    {
        public const string TokenNotFound = "TOKEN_NOT_FOUND";
        public const string EmailAlreadyConfirmed = "EMAIL_ALREADY_CONFIRMED";
        public const string TokenExpired = "TOKEN_EXPIRED";
        public const string UnknownError = "UNKNOWN_ERROR";
        public const string Success = "SUCCESS";

    }
}
