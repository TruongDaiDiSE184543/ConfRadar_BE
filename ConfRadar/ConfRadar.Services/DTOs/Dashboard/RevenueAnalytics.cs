namespace ConfRadar.Services.DTOs.Dashboard
{
    public class RevenueAnalyticsResponse
    {
        public decimal TotalRevenue { get; set; }
        public int TotalTicketsSold { get; set; }
        public List<MonthlyRevenueStats> MonthlyStats { get; set; } = new List<MonthlyRevenueStats>();
    }

    public class MonthlyRevenueStats
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public string MonthLabel { get; set; } // "11/2023"
        public decimal MonthlyTotal { get; set; }
        public int MonthlyTickets { get; set; }
        public List<ConferenceRevenueStats> Conferences { get; set; } = new List<ConferenceRevenueStats>();
    }

    public class ConferenceRevenueStats
    {
        public string ConferenceId { get; set; }
        public string ConferenceName { get; set; }
        public int TicketsSold { get; set; }
        public decimal Revenue { get; set; }
    }
}
