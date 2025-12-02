namespace ConfRadar.Shared.DTO.Payment
{
    public static class PaymentMessageResult
    {
        // VNPay result codes
        public const string VnPaySuccess = "VNPAY_SUCCESS";
        public const string VnPayFail = "VNPAY_FAIL";
        public const string VnPayPending = "VNPAY_PENDING";

        // MoMo result codes
        public const string MoMoSuccess = "MOMO_SUCCESS";
        public const string MoMoFail = "MOMO_FAIL";
        public const string MoMoPending = "MOMO_PENDING";

        // PayOS result codes
        public const string PayOsSuccess = "PAYOS_SUCCESS";
        public const string PayOsFail = "PAYOS_FAIL";
        public const string PayOsPending = "PAYOS_PENDING";
    }
}
