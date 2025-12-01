namespace ConfRadar.Services.Common
{
    public static class ConfRadarApiEndPoint
    {
        public static string ConfirmRegistrationEmail_BE => "api/auth/confirm-registration-email";




        public static string EmailResetPassword_FE => "auth/set-password/reset";
        public static string EmailCreatePassword_FE => "auth/set-password/create";

        //public static string ForgetPassword_FE => "auth/reset-password";
        public static string EmailConfirmSuccess_FE => "email-confirm/success";
        public static string EmailConfirmFail_FE => "email-confirm/fail";
        public static string PaymentSuccess_FE => "payment/success";
        public static string PaymentFail_FE => "payment/fail";

    }
}
