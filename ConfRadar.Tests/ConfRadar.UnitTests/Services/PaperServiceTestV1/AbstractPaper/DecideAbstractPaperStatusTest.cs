using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Repositories.Repositories;
using ConfRadar.Services.Common;
using ConfRadar.Services.DTOs.Abstract;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Services;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using static ConfRadar.Services.Common.AppSettingConfig;

namespace ConfRadar.UnitTests.Services.PaperServiceTestV1.AbstractPaper
{
    public class DecideAbstractPaperStatusTest
    {
        private readonly Mock<IUnitOfWork> _uowMock;
        private readonly Mock<ITimeProviderService> _timeMock;
        private readonly PaperService _service;

        public DecideAbstractPaperStatusTest()
        {
            _uowMock = new Mock<IUnitOfWork>();
            _timeMock = new Mock<ITimeProviderService>();

            _service = new PaperService(
                _uowMock.Object,
                Mock.Of<IMomoService>(),
                Mock.Of<ITokenService>(),
                Options.Create(new ObjectStorageSettings()),
                Mock.Of<IObjectStorageFileService>(),
                Mock.Of<ITicketService>(),
                _timeMock.Object,
                Mock.Of<INotificationService>(),
                Mock.Of<IConferenceStepService>(),
                Mock.Of<IEmailService>()
            );
        }
        [Fact]
        public async Task DecideAbstractPaperStatus_Pending_ThrowsBadRequest()
        {
            var request = new UpdateAbstractPaperStatusRequest
            {
                PaperId = "P1",
                AbstractId = "A1",
                GlobalStatus = GlobalStatusEnum.Pending
            };

            var act = async () => await _service.DecideAbstractPaperStatus(request);

            var ex = await Assert.ThrowsAsync<BadRequestException>(act);
            ex.Message.Should().Contain("Không thể chuyển pending");
        }

        [Fact]
        public async Task DecideAbstractPaperStatus_PaperNotFound_ThrowsNotFound()
        {
            // Setup repo tối thiểu để KHÔNG bị NullReference
            _uowMock.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(It.IsAny<string>()))
      .ReturnsAsync(new GlobalStatus());

            _uowMock.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByName(It.IsAny<string>()))
                .ReturnsAsync(new PaperPhase());

            _uowMock.Setup(u => u.ConferenceStatusRepository
                    .GetConferenceStatusByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new ConferenceStatus());
            _uowMock.Setup(u => u.PaperRepository.GetPaperByIdAsync(It.IsAny<string>()))
                    .ReturnsAsync((Paper)null!);

            var request = new UpdateAbstractPaperStatusRequest
            {
                PaperId = "P1",
                AbstractId = "A1",
                GlobalStatus = GlobalStatusEnum.Accepted
            };

            var act = async () => await _service.DecideAbstractPaperStatus(request);

            var ex = await Assert.ThrowsAsync<NotFoundException>(act);
            ex.Message.Should().Contain("Không tìm thấy bài báo");
        }
        [Fact]
        public async Task DecideAbstractPaperStatus_ConferenceNotReady_ThrowsBadRequest()
        {
            // Arrange
            _uowMock.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(It.IsAny<string>()))
                .ReturnsAsync(new GlobalStatus());

            _uowMock.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByName(It.IsAny<string>()))
                .ReturnsAsync(new PaperPhase { PaperPhaseId = "ABSTRACT" });

            // READY status để so sánh
            _uowMock.Setup(u => u.ConferenceStatusRepository
                    .GetConferenceStatusByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "READY" });

            // Paper có conference nhưng status KHÔNG READY
            _uowMock.Setup(u => u.PaperRepository.GetPaperByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new Paper
                {
                    PaperPhaseId = "ABSTRACT",
                    Conference = new Conference
                    {
                        ConferenceStatus = new ConferenceStatus
                        {
                            ConferenceStatusId = "NOT_READY"
                        }
                    },
                    ResearchConferencePhase = new ResearchConferencePhase(), // tránh NRE
                    PaperAuthors = new List<PaperAuthor>
                    {
                new PaperAuthor
                {
                    IsRootAuthor = true,
                    User = new User()
                }
                    }
                });

            var request = new UpdateAbstractPaperStatusRequest
            {
                PaperId = "P1",
                AbstractId = "A1",
                GlobalStatus = GlobalStatusEnum.Accepted
            };

            // Act
            var act = async () => await _service.DecideAbstractPaperStatus(request);

            // Assert
            var ex = await Assert.ThrowsAsync<BadRequestException>(act);
            ex.Message.Should().Contain("Hội nghị chưa ready");
        }
        [Fact]
        public async Task DecideAbstractPaperStatus_OutOfDecideTime_ThrowsBadRequest()
        {
            // Arrange
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            _timeMock.Setup(t => t.GetVietnamDate())
                .ReturnsAsync(today);

            _uowMock.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(It.IsAny<string>()))
                .ReturnsAsync(new GlobalStatus());

            _uowMock.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByName(It.IsAny<string>()))
                .ReturnsAsync(new PaperPhase { PaperPhaseId = "ABSTRACT" });

            _uowMock.Setup(u => u.ConferenceStatusRepository
                    .GetConferenceStatusByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "READY" });

            _uowMock.Setup(u => u.PaperRepository.GetPaperByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new Paper
                {
                    PaperPhaseId = "ABSTRACT",
                    Conference = new Conference
                    {
                        ConferenceStatus = new ConferenceStatus { ConferenceStatusId = "READY" }
                    },
                    ResearchConferencePhase = new ResearchConferencePhase
                    {
                        AbstractDecideStatusStart = today.AddDays(1), //
                        AbstractDecideStatusEnd = today.AddDays(2)
                    },
                    PaperAuthors = new List<PaperAuthor>
                    {
                new PaperAuthor
                {
                    IsRootAuthor = true,
                    User = new User()
                }
                    }
                });

            var request = new UpdateAbstractPaperStatusRequest
            {
                PaperId = "P1",
                AbstractId = "A1",
                GlobalStatus = GlobalStatusEnum.Accepted
            };

            // Act
            var act = async () => await _service.DecideAbstractPaperStatus(request);

            // Assert
            var ex = await Assert.ThrowsAsync<BadRequestException>(act);
            ex.Message.Should().Contain("Ngày quyết định abstract");
        }

        [Fact]
        public async Task DecideAbstractPaperStatus_AbstractNotFound_ThrowsNotFound()
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            _timeMock.Setup(t => t.GetVietnamDate())
                .ReturnsAsync(today);

            _timeMock.Setup(t => t.GetVietnamTime())
                .ReturnsAsync(DateTime.Now);

            _uowMock.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(It.IsAny<string>()))
                .ReturnsAsync(new GlobalStatus { GlobalStatusId = "PENDING" });

            _uowMock.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByName(It.IsAny<string>()))
                .ReturnsAsync(new PaperPhase { PaperPhaseId = "ABSTRACT" });

            _uowMock.Setup(u => u.ConferenceStatusRepository
                    .GetConferenceStatusByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new ConferenceStatus { ConferenceStatusId = "READY" });

            _uowMock.Setup(u => u.PaperRepository.GetPaperByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new Paper
                {
                    PaperId = "P1",
                    PaperPhaseId = "ABSTRACT",
                    Conference = new Conference
                    {
                        ConferenceStatus = new ConferenceStatus { ConferenceStatusId = "READY" }
                    },
                    ResearchConferencePhase = new ResearchConferencePhase
                    {
                        AbstractDecideStatusStart = today.AddDays(-1),
                        AbstractDecideStatusEnd = today.AddDays(1)
                    },
                    PaperAuthors = new List<PaperAuthor>
                    {
                new PaperAuthor
                {
                    IsRootAuthor = true,
                    User = new User()
                }
                    }
                });

            // ⬇️ ĐIỂM TEST DUY NHẤT
            _uowMock.Setup(u => u.AbstractRepository.GetAbstractByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((Abstract)null!);

            var request = new UpdateAbstractPaperStatusRequest
            {
                PaperId = "P1",
                AbstractId = "A1",
                GlobalStatus = GlobalStatusEnum.Accepted
            };

            var act = async () => await _service.DecideAbstractPaperStatus(request);

            var ex = await Assert.ThrowsAsync<NotFoundException>(act);
            ex.Message.Should().Contain("Không tìm thấy abstract");
        }
        [Fact]
        public async Task DecideAbstractPaperStatus_AbstractNotPending_ThrowsBadRequest()
        {
            // Arrange
            var timeNow = DateTime.Now;
            var dateNow = DateOnly.FromDateTime(timeNow);
            _timeMock.Setup(t => t.GetVietnamTime()).ReturnsAsync(timeNow);
            _timeMock.Setup(t => t.GetVietnamDate()).ReturnsAsync(dateNow);

            var pendingStatus = new GlobalStatus { GlobalStatusId = "PENDING" };
            var acceptedStatus = new GlobalStatus { GlobalStatusId = "ACCEPTED" };
            var rejectedStatus = new GlobalStatus { GlobalStatusId = "REJECTED" };

            var abstractPhase = new PaperPhase { PaperPhaseId = "ABSTRACT" };
            var fullPaperPhase = new PaperPhase { PaperPhaseId = "FULL" };
            var readyConfStatus = new ConferenceStatus { ConferenceStatusId = "READY" };

            var paper = new Paper
            {
                PaperId = "P1",
                PaperPhaseId = abstractPhase.PaperPhaseId,
                ResearchConferencePhase = new ResearchConferencePhase
                {
                    AbstractDecideStatusStart = dateNow.AddDays(-1),
                    AbstractDecideStatusEnd = dateNow.AddDays(1)
                },
                Conference = new Conference
                {
                    ConferenceStatus = readyConfStatus
                },
                PaperAuthors = new List<PaperAuthor>
        {
            new PaperAuthor
            {
                IsRootAuthor = true,
                User = new User { UserId = "U1", FullName = "Root", Email = "root@test.com" }
            }
        }
            };

            var abstractPaper = new Abstract
            {
                AbstractId = "A1",
                GlobalStatusId = acceptedStatus.GlobalStatusId // 
            };

            _uowMock.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Pending.GetDescription()))
                    .ReturnsAsync(pendingStatus);
            _uowMock.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Accepted.GetDescription()))
                    .ReturnsAsync(acceptedStatus);
            _uowMock.Setup(u => u.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Rejected.GetDescription()))
                    .ReturnsAsync(rejectedStatus);

            _uowMock.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByName(PaperPhaseEnum.Abstract.GetDescription()))
                    .ReturnsAsync(abstractPhase);
            _uowMock.Setup(u => u.PaperPhaseRepository.GetPaperPhaseByName(PaperPhaseEnum.FullPaper.GetDescription()))
                    .ReturnsAsync(fullPaperPhase);

            _uowMock.Setup(u => u.ConferenceStatusRepository
                    .GetConferenceStatusByNameAsync(ConferenceStatusEnum.Ready.GetDescription()))
                    .ReturnsAsync(readyConfStatus);

            _uowMock.Setup(u => u.PaperRepository.GetPaperByIdAsync(It.IsAny<string>()))
                    .ReturnsAsync(paper);

            _uowMock.Setup(u => u.AbstractRepository.GetAbstractByIdAsync(It.IsAny<string>()))
                    .ReturnsAsync(abstractPaper);

            var request = new UpdateAbstractPaperStatusRequest
            {
                PaperId = "P1",
                AbstractId = "A1",
                GlobalStatus = GlobalStatusEnum.Accepted
            };

            // Act
            var act = async () => await _service.DecideAbstractPaperStatus(request);

            // Assert
            var ex = await Assert.ThrowsAsync<BadRequestException>(act);
            ex.Message.Should().Contain("abstract không trong quá trình pending");
        }


    }

}



