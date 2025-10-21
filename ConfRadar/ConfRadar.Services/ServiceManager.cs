using ConfRadar.Services.Services;

namespace ConfRadar.Services
{
    public interface IServiceManager
    {
        public IAuthService AuthService { get; }
        public IMomoService MomoService { get; }
        public ITicketService TicketService { get; }    
        public IPaymentService PaymentService { get; }

    }

    public class ServiceManager : IServiceManager
    {
        private readonly IAuthService _authService;
        private readonly IMomoService _momoService; 
        private readonly ITicketService _ticketService;
        private readonly IPaymentService _paymentService;

        public ServiceManager(IAuthService authService, IMomoService momoService,ITicketService ticketService,IPaymentService paymentService    )
        {

            _authService = authService;
            _momoService = momoService;
            _ticketService = ticketService;
            _paymentService = paymentService;
        }

        public IAuthService AuthService => _authService;

        public IMomoService MomoService => _momoService;

        public ITicketService TicketService => _ticketService;

        public IPaymentService PaymentService => _paymentService;
    }

}
