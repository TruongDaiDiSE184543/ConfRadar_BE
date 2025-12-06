using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Common;
using ConfRadar.Services.Services;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;

namespace ConfRadar.UnitTests.Services.ConferenceManangment.ExportStatistics
{
    public class StatisticsServiceTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IExcelExportService> _mockExcelService;
        private readonly Mock<IObjectStorageFileService> _mockObjectStorage;
        private readonly StatisticsService _service;

        public StatisticsServiceTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockExcelService = new Mock<IExcelExportService>();
            _mockObjectStorage = new Mock<IObjectStorageFileService>();

            var options = Options.Create(new AppSettingConfig.ObjectStorageSettings());

            _service = new StatisticsService(
                _mockUnitOfWork.Object,
                _mockExcelService.Object,
                _mockObjectStorage.Object,
                options
            );
        }

        [Fact]
        public async Task GetSoldTicketStatisticsAsync_InternalHosted_ShouldCalculateCorrectly()
        {
            // ARRANGE
            var confId = "conf-1";
            var phaseId = "phase-1";

            // 1. Conference (Internal Hosted -> Không chia hoa hồng)
            var conference = new Conference
            {
                ConferenceId = confId,
                IsInternalHosted = true, // Quan trọng
                ConferenceName = "Internal Conf"
            };

            // 2. Ticket Data: 3 vé (2 bán, 1 hoàn)
            var tickets = new List<Ticket>
            {
                new Ticket { TicketId = "t1", PricePhaseId = phaseId, ActualPrice = 100, IsRefunded = false },
                new Ticket { TicketId = "t2", PricePhaseId = phaseId, ActualPrice = 100, IsRefunded = false },
                // Vé hoàn: Mua 100, Hoàn 80 -> Giữ lại 20
                new Ticket
                {
                    TicketId = "t3",
                    PricePhaseId = phaseId,
                    ActualPrice = 100,
                    IsRefunded = true,
                    Transactions = new List<Transaction>
                    {
                        new Transaction { IsRefunded = true, Amount = 80 }
                    }
                }
            };

            // 3. Price & Phase Structure
            var prices = new List<ConferencePrice>
            {
                new ConferencePrice
                {
                    ConferencePriceId = "price-1",
                    PricePhases = new List<PricePhase>
                    {
                        new PricePhase { PricePhaseId = phaseId, PhaseName = "Early Bird" }
                    }
                }
            };

            // Mock Data
            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync(confId)).ReturnsAsync(conference);
            _mockUnitOfWork.Setup(u => u.TicketRepository.GetPaidTicketIncludeRefunded(confId)).ReturnsAsync(tickets);
            _mockUnitOfWork.Setup(u => u.ConferencePriceRepository.GetPricesWithDetailsByConferenceIdAsync(confId)).ReturnsAsync(prices);

            // ACT
            var result = await _service.GetSoldTicketStatisticsAsync(confId);

            // ASSERT
            result.TotalTicketsSold.Should().Be(3); // Tổng số vé phát sinh
            result.TotalRevenue.Should().Be(220); // 100 + 100 + (100 - 80) = 220

            // Check chi tiết từng phase
            var phaseStat = result.TicketPhaseStatistics.First();
            phaseStat.TotalSold.Should().Be(3);
            phaseStat.TotalRefunded.Should().Be(1);
            phaseStat.TotalNotRefuned.Should().Be(2); // 2 vé active
            phaseStat.TotalAmountRefunded.Should().Be(-80); // Số tiền trả khách (âm)
        }

        [Fact]
        public async Task GetSoldTicketStatisticsAsync_ExternalHosted_ShouldCalculateCommission()
        {
            // ARRANGE
            var confId = "conf-external";
            var phaseId = "phase-1";

            // 1. Conference External (Có hợp đồng)
            var conference = new Conference { ConferenceId = confId, IsInternalHosted = false };

            // 2. Contract (Hoa hồng 10%)
            var contract = new CollaboratorContract
            {
                IsTicketSelling = true,
                Commission = 10
            };

            // 3. Ticket: 1 vé giá 1000
            var tickets = new List<Ticket>
            {
                new Ticket { TicketId = "t1", PricePhaseId = phaseId, ActualPrice = 1000, IsRefunded = false }
            };

            var prices = new List<ConferencePrice>
            {
                new ConferencePrice
                {
                    PricePhases = new List<PricePhase> { new PricePhase { PricePhaseId = phaseId } }
                }
            };

            // Mock Data
            _mockUnitOfWork.Setup(u => u.ConferenceRepository.GetConferenceByIdAsync(confId)).ReturnsAsync(conference);
            _mockUnitOfWork.Setup(u => u.CollaboratorContractRepository.GetCollaboratorContractByConferenceId(confId)).ReturnsAsync(contract);
            _mockUnitOfWork.Setup(u => u.TicketRepository.GetPaidTicketIncludeRefunded(confId)).ReturnsAsync(tickets);
            _mockUnitOfWork.Setup(u => u.ConferencePriceRepository.GetPricesWithDetailsByConferenceIdAsync(confId)).ReturnsAsync(prices);

            // ACT
            var result = await _service.GetSoldTicketStatisticsAsync(confId);

            // ASSERT
            var stat = result.TicketPhaseStatistics.First();

            stat.TotalAmount.Should().Be(1000); // Doanh thu
            stat.CommissionPercentage.Should().Be(10); // % Hoa hồng

            // ConfRadar nhận 10% của 1000 = 100
            stat.AmountToConfRadar.Should().Be(100);

            // Collaborator nhận còn lại = 900
            stat.AmountToCollaborator.Should().Be(900);
        }
    }
}
