using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConfRadar.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AcademicProfile",
                columns: table => new
                {
                    AcademicProfileId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("AcademicProfile_pkey", x => x.AcademicProfileId);
                });

            migrationBuilder.CreateTable(
                name: "CheckinStatus",
                columns: table => new
                {
                    CheckinStatusId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CheckinStatusName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("CheckinStatus_pkey", x => x.CheckinStatusId);
                });

            migrationBuilder.CreateTable(
                name: "City",
                columns: table => new
                {
                    CityId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CityName = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("City_pkey", x => x.CityId);
                });

            migrationBuilder.CreateTable(
                name: "ConferenceCategory",
                columns: table => new
                {
                    ConferenceCategoryId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ConferenceCategoryName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("ConferenceCategory_pkey", x => x.ConferenceCategoryId);
                });

            migrationBuilder.CreateTable(
                name: "ConferenceStatus",
                columns: table => new
                {
                    ConferenceStatusId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ConferenceStatusName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("ConferenceStatus_pkey", x => x.ConferenceStatusId);
                });

            migrationBuilder.CreateTable(
                name: "GeneralFAQ",
                columns: table => new
                {
                    GeneralFAQId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("GeneralFAQ_pkey", x => x.GeneralFAQId);
                });

            migrationBuilder.CreateTable(
                name: "GlobalStatus",
                columns: table => new
                {
                    GlobalStatusId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("GlobalStatus_pkey", x => x.GlobalStatusId);
                });

            migrationBuilder.CreateTable(
                name: "PaperPhase",
                columns: table => new
                {
                    PaperPhaseId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PhaseName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PaperPhase_pkey", x => x.PaperPhaseId);
                });

            migrationBuilder.CreateTable(
                name: "PaymentMethod",
                columns: table => new
                {
                    PaymentMethodId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MethodName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    MethodDescription = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PaymentMethod_pkey", x => x.PaymentMethodId);
                });

            migrationBuilder.CreateTable(
                name: "RankingCategories",
                columns: table => new
                {
                    RankingCategoryId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RankName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    RankDescription = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("RankingCategories_pkey", x => x.RankingCategoryId);
                });

            migrationBuilder.CreateTable(
                name: "ReviewStatus",
                columns: table => new
                {
                    ReviewStatusId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("ReviewStatus_pkey", x => x.ReviewStatusId);
                });

            migrationBuilder.CreateTable(
                name: "Role",
                columns: table => new
                {
                    RoleId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RoleName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Role_pkey", x => x.RoleId);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    FullName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    BirthDay = table.Column<DateOnly>(type: "date", nullable: true),
                    PhoneNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Gender = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: true),
                    IsEmailConfirmed = table.Column<bool>(type: "boolean", nullable: true),
                    LoginProvider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    VerificationToken = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    VerificationTokenExpiry = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    PasswordResetToken = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    PasswordResetTokenExpiry = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastLogin = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    AvatarUrl = table.Column<string>(type: "text", nullable: true),
                    BioDescription = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("User_pkey", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "WaitListStatus",
                columns: table => new
                {
                    WaitListStatusId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("WaitListStatus_pkey", x => x.WaitListStatusId);
                });

            migrationBuilder.CreateTable(
                name: "Destination",
                columns: table => new
                {
                    DestinationId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CityId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    District = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Street = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Destination_pkey", x => x.DestinationId);
                    table.ForeignKey(
                        name: "FK_Destination_CityId",
                        column: x => x.CityId,
                        principalTable: "City",
                        principalColumn: "CityId");
                });

            migrationBuilder.CreateTable(
                name: "Abstract",
                columns: table => new
                {
                    AbstractId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    GlobalStatusId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AbstractUrl = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Abstract_pkey", x => x.AbstractId);
                    table.ForeignKey(
                        name: "FK_Abstract_GlobalStatusId",
                        column: x => x.GlobalStatusId,
                        principalTable: "GlobalStatus",
                        principalColumn: "GlobalStatusId");
                });

            migrationBuilder.CreateTable(
                name: "CameraReady",
                columns: table => new
                {
                    CameraReadyId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    GlobalStatusId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CameraReadyURL = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("CameraReady_pkey", x => x.CameraReadyId);
                    table.ForeignKey(
                        name: "FK_CameraReady_GlobalStatusId",
                        column: x => x.GlobalStatusId,
                        principalTable: "GlobalStatus",
                        principalColumn: "GlobalStatusId");
                });

            migrationBuilder.CreateTable(
                name: "Refundrequest",
                columns: table => new
                {
                    RefundRequestId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TransactionId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TicketId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    GlobalStatusId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Refundrequest_pkey", x => x.RefundRequestId);
                    table.ForeignKey(
                        name: "FK_Refundrequest_GlobalStatusId",
                        column: x => x.GlobalStatusId,
                        principalTable: "GlobalStatus",
                        principalColumn: "GlobalStatusId");
                });

            migrationBuilder.CreateTable(
                name: "RevisionPaper",
                columns: table => new
                {
                    RevisionPaperId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RevisionRound = table.Column<int>(type: "integer", nullable: true),
                    GlobalStatusId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("RevisionPaper_pkey", x => x.RevisionPaperId);
                    table.ForeignKey(
                        name: "FK_RevisionPaper_GlobalStatusId",
                        column: x => x.GlobalStatusId,
                        principalTable: "GlobalStatus",
                        principalColumn: "GlobalStatusId");
                });

            migrationBuilder.CreateTable(
                name: "FullPaper",
                columns: table => new
                {
                    FullPaperId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ReviewStatusId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    FullPaperURL = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("FullPaper_pkey", x => x.FullPaperId);
                    table.ForeignKey(
                        name: "FK_FullPaper_ReviewStatusId",
                        column: x => x.ReviewStatusId,
                        principalTable: "ReviewStatus",
                        principalColumn: "ReviewStatusId");
                });

            migrationBuilder.CreateTable(
                name: "AuditLog",
                columns: table => new
                {
                    AuditLogId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    EntityName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ActionDescription = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("AuditLog_pkey", x => x.AuditLogId);
                    table.ForeignKey(
                        name: "FK_AuditLog_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "Conference",
                columns: table => new
                {
                    ConferenceId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ConferenceName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    TotalSlot = table.Column<int>(type: "integer", nullable: true),
                    AvailableSlot = table.Column<int>(type: "integer", nullable: true),
                    Address = table.Column<string>(type: "text", nullable: true),
                    BannerImageUrl = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateOnly>(type: "date", nullable: true),
                    TicketSaleStart = table.Column<DateOnly>(type: "date", nullable: true),
                    TicketSaleEnd = table.Column<DateOnly>(type: "date", nullable: true),
                    IsInternalHosted = table.Column<bool>(type: "boolean", nullable: true),
                    IsResearchConference = table.Column<bool>(type: "boolean", nullable: true),
                    CityId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ConferenceCategoryId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ConferenceStatusId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Conference_pkey", x => x.ConferenceId);
                    table.ForeignKey(
                        name: "FK_Conference_CityId",
                        column: x => x.CityId,
                        principalTable: "City",
                        principalColumn: "CityId");
                    table.ForeignKey(
                        name: "FK_Conference_ConferenceCategoryId",
                        column: x => x.ConferenceCategoryId,
                        principalTable: "ConferenceCategory",
                        principalColumn: "ConferenceCategoryId");
                    table.ForeignKey(
                        name: "FK_Conference_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "User",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "Notification",
                columns: table => new
                {
                    NotificationId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Message = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ReadStatus = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Notification_pkey", x => x.NotificationId);
                    table.ForeignKey(
                        name: "FK_Notification_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "PresenterChangeRequest",
                columns: table => new
                {
                    PresenterChangeRequestId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TicketId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RequestedById = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    NewPresenterId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    GlobalStatusId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    RequestAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PresenterChangeRequest_pkey", x => x.PresenterChangeRequestId);
                    table.ForeignKey(
                        name: "FK_PresenterChangeRequest_GlobalStatusId",
                        column: x => x.GlobalStatusId,
                        principalTable: "GlobalStatus",
                        principalColumn: "GlobalStatusId");
                    table.ForeignKey(
                        name: "FK_PresenterChangeRequest_NewPresenterId",
                        column: x => x.NewPresenterId,
                        principalTable: "User",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_PresenterChangeRequest_RequestedById",
                        column: x => x.RequestedById,
                        principalTable: "User",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "Report",
                columns: table => new
                {
                    ReportId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ReportSubject = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    HasResolve = table.Column<bool>(type: "boolean", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    UserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Report_pkey", x => x.ReportId);
                    table.ForeignKey(
                        name: "FK_Report_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "ReportFeedback",
                columns: table => new
                {
                    ReportId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ReportSubject = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    AdminId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("ReportFeedback_pkey", x => x.ReportId);
                    table.ForeignKey(
                        name: "FK_ReportFeedback_User_UserId",
                        column: x => x.AdminId,
                        principalTable: "User",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "UserRefreshToken",
                columns: table => new
                {
                    TokenId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Token = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Expiry = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsRevoked = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("UserRefreshToken_pkey", x => x.TokenId);
                    table.ForeignKey(
                        name: "UserRefreshToken_UserId_fkey",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "UserRole",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RoleId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("UserRole_pkey", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "UserRole_RoleId_fkey",
                        column: x => x.RoleId,
                        principalTable: "Role",
                        principalColumn: "RoleId");
                    table.ForeignKey(
                        name: "UserRole_UserId_fkey",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "Room",
                columns: table => new
                {
                    RoomId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Number = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    DisplayName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    DestinationId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Room_pkey", x => x.RoomId);
                    table.ForeignKey(
                        name: "FK_Room_DestinationId",
                        column: x => x.DestinationId,
                        principalTable: "Destination",
                        principalColumn: "DestinationId");
                });

            migrationBuilder.CreateTable(
                name: "RevisionPaperReview",
                columns: table => new
                {
                    RevisionPaperReviewId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    GlobalStatusId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    FeedbackToAuthor = table.Column<string>(type: "text", nullable: true),
                    FeedbackMaterialUrl = table.Column<string>(type: "text", nullable: true),
                    ReviewerId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RevisionPaperId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("RevisionPaperReview_pkey", x => x.RevisionPaperReviewId);
                    table.ForeignKey(
                        name: "FK_RevisionPaperReview_GlobalStatusId",
                        column: x => x.GlobalStatusId,
                        principalTable: "GlobalStatus",
                        principalColumn: "GlobalStatusId");
                    table.ForeignKey(
                        name: "FK_RevisionPaperReview_ReviewerId",
                        column: x => x.ReviewerId,
                        principalTable: "User",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_RevisionPaperReview_RevisionPaperId",
                        column: x => x.RevisionPaperId,
                        principalTable: "RevisionPaper",
                        principalColumn: "RevisionPaperId");
                });

            migrationBuilder.CreateTable(
                name: "FullPaperReview",
                columns: table => new
                {
                    FullPaperReviewId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ReviewStatusId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    FeedbackToAuthor = table.Column<string>(type: "text", nullable: true),
                    FeedbackMaterialUrl = table.Column<string>(type: "text", nullable: true),
                    FullPaperId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ReviewerId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("FullPaperReview_pkey", x => x.FullPaperReviewId);
                    table.ForeignKey(
                        name: "FK_FullPaperReview_FullPaperId",
                        column: x => x.FullPaperId,
                        principalTable: "FullPaper",
                        principalColumn: "FullPaperId");
                    table.ForeignKey(
                        name: "FK_FullPaperReview_ReviewStatusId",
                        column: x => x.ReviewStatusId,
                        principalTable: "ReviewStatus",
                        principalColumn: "ReviewStatusId");
                    table.ForeignKey(
                        name: "FK_FullPaperReview_ReviewerId",
                        column: x => x.ReviewerId,
                        principalTable: "User",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "ConferenceMedia",
                columns: table => new
                {
                    ConferenceMediaId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ConferenceMediaUrl = table.Column<string>(type: "text", nullable: true),
                    ConferenceId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("ConferenceMedia_pkey", x => x.ConferenceMediaId);
                    table.ForeignKey(
                        name: "FK_ConferenceMedia_ConferenceId",
                        column: x => x.ConferenceId,
                        principalTable: "Conference",
                        principalColumn: "ConferenceId");
                });

            migrationBuilder.CreateTable(
                name: "ConferencePrice",
                columns: table => new
                {
                    ConferencePriceId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TicketPrice = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    TicketName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TicketDescription = table.Column<string>(type: "text", nullable: true),
                    IsAuthor = table.Column<bool>(type: "boolean", nullable: true),
                    TotalSlot = table.Column<int>(type: "integer", nullable: true),
                    AvailableSlot = table.Column<int>(type: "integer", nullable: true),
                    ConferenceId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("ConferencePrice_pkey", x => x.ConferencePriceId);
                    table.ForeignKey(
                        name: "FK_ConferencePrice_ConferenceId",
                        column: x => x.ConferenceId,
                        principalTable: "Conference",
                        principalColumn: "ConferenceId");
                });

            migrationBuilder.CreateTable(
                name: "ConferenceTimeline",
                columns: table => new
                {
                    ConferenceTimelineId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ConferenceId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ChangeDate = table.Column<DateOnly>(type: "date", nullable: true),
                    PreviousStatusId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AfterwardStatusId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Reason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("ConferenceTimeline_pkey", x => x.ConferenceTimelineId);
                    table.ForeignKey(
                        name: "FK_ConferenceTimeline_AfterwardStatusId",
                        column: x => x.AfterwardStatusId,
                        principalTable: "ConferenceStatus",
                        principalColumn: "ConferenceStatusId");
                    table.ForeignKey(
                        name: "FK_ConferenceTimeline_ConferenceId",
                        column: x => x.ConferenceId,
                        principalTable: "Conference",
                        principalColumn: "ConferenceId");
                    table.ForeignKey(
                        name: "FK_ConferenceTimeline_PreviousStatusId",
                        column: x => x.PreviousStatusId,
                        principalTable: "ConferenceStatus",
                        principalColumn: "ConferenceStatusId");
                });

            migrationBuilder.CreateTable(
                name: "FavouriteConference",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ConferenceId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.ForeignKey(
                        name: "FK_FavouriteConference_ConferenceId",
                        column: x => x.ConferenceId,
                        principalTable: "Conference",
                        principalColumn: "ConferenceId");
                    table.ForeignKey(
                        name: "FK_FavouriteConference_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "MaterialDownload",
                columns: table => new
                {
                    MaterialDownloadId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ConferenceId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    FileDescription = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("MaterialDownload_pkey", x => x.MaterialDownloadId);
                    table.ForeignKey(
                        name: "FK_MaterialDownload_ConferenceId",
                        column: x => x.ConferenceId,
                        principalTable: "Conference",
                        principalColumn: "ConferenceId");
                });

            migrationBuilder.CreateTable(
                name: "Paper",
                columns: table => new
                {
                    PaperId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PresenterId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    FullPaperId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RevisionPaperId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CameraReadyId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AbstractId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ConferenceId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PaperPhaseId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Paper_pkey", x => x.PaperId);
                    table.ForeignKey(
                        name: "FK_Paper_CameraReadyId",
                        column: x => x.CameraReadyId,
                        principalTable: "CameraReady",
                        principalColumn: "CameraReadyId");
                    table.ForeignKey(
                        name: "FK_Paper_ConferenceId",
                        column: x => x.ConferenceId,
                        principalTable: "Conference",
                        principalColumn: "ConferenceId");
                    table.ForeignKey(
                        name: "FK_Paper_PaperPhaseId",
                        column: x => x.PaperPhaseId,
                        principalTable: "PaperPhase",
                        principalColumn: "PaperPhaseId");
                    table.ForeignKey(
                        name: "FK_Paper_Presenter",
                        column: x => x.PresenterId,
                        principalTable: "User",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "PaperWaitList",
                columns: table => new
                {
                    PaperWaitListId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    NotifiedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    WaitListStatusId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ConferenceId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PaperWaitList_pkey", x => x.PaperWaitListId);
                    table.ForeignKey(
                        name: "FK_PaperWaitList_ConferenceId",
                        column: x => x.ConferenceId,
                        principalTable: "Conference",
                        principalColumn: "ConferenceId");
                    table.ForeignKey(
                        name: "FK_PaperWaitList_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_PaperWaitList_WaitListStatusId",
                        column: x => x.WaitListStatusId,
                        principalTable: "WaitListStatus",
                        principalColumn: "WaitListStatusId");
                });

            migrationBuilder.CreateTable(
                name: "Policy",
                columns: table => new
                {
                    PolicyId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PolicyName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ConferenceId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Policy_pkey", x => x.PolicyId);
                    table.ForeignKey(
                        name: "FK_Policy_ConferenceId",
                        column: x => x.ConferenceId,
                        principalTable: "Conference",
                        principalColumn: "ConferenceId");
                });

            migrationBuilder.CreateTable(
                name: "RankingFileUrl",
                columns: table => new
                {
                    RankingFileUrlId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FileUrl = table.Column<string>(type: "text", nullable: true),
                    ConferenceId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("RankingFileUrl_pkey", x => x.RankingFileUrlId);
                    table.ForeignKey(
                        name: "FK_RankingFileUrl_ConferenceId",
                        column: x => x.ConferenceId,
                        principalTable: "Conference",
                        principalColumn: "ConferenceId");
                });

            migrationBuilder.CreateTable(
                name: "RankingReferenceUrl",
                columns: table => new
                {
                    ReferenceUrlId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ConferenceId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ReferenceUrl = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("RankingReferenceUrl_pkey", x => x.ReferenceUrlId);
                    table.ForeignKey(
                        name: "FK_RankingReferenceUrl_ConferenceId",
                        column: x => x.ConferenceId,
                        principalTable: "Conference",
                        principalColumn: "ConferenceId");
                });

            migrationBuilder.CreateTable(
                name: "RefundPolicy",
                columns: table => new
                {
                    RefundPolicyId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ConferenceId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PercentRefund = table.Column<int>(type: "integer", nullable: true),
                    RefundDeadline = table.Column<DateOnly>(type: "date", nullable: true),
                    RefundOrder = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("RefundPolicy_pkey", x => x.RefundPolicyId);
                    table.ForeignKey(
                        name: "FK_RefundPolicy_ConferenceId",
                        column: x => x.ConferenceId,
                        principalTable: "Conference",
                        principalColumn: "ConferenceId");
                });

            migrationBuilder.CreateTable(
                name: "ResearchConferenceDetail",
                columns: table => new
                {
                    ConferenceId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    PaperFormat = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    NumberPaperAccept = table.Column<int>(type: "integer", nullable: true),
                    RevisionAttemptAllowed = table.Column<int>(type: "integer", nullable: true),
                    RankingDescription = table.Column<string>(type: "text", nullable: true),
                    AllowListener = table.Column<bool>(type: "boolean", nullable: true),
                    RankValue = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    RankYear = table.Column<int>(type: "integer", nullable: true),
                    ReviewFee = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    RankingCategoryId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("ResearchConferenceDetail_pkey", x => x.ConferenceId);
                    table.ForeignKey(
                        name: "FK_ResearchConferenceDetail_ConferenceId",
                        column: x => x.ConferenceId,
                        principalTable: "Conference",
                        principalColumn: "ConferenceId");
                    table.ForeignKey(
                        name: "FK_ResearchConferenceDetail_RankingCategoryId",
                        column: x => x.RankingCategoryId,
                        principalTable: "RankingCategories",
                        principalColumn: "RankingCategoryId");
                });

            migrationBuilder.CreateTable(
                name: "ResearchConferencePhase",
                columns: table => new
                {
                    ResearchConferencePhaseId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ConferenceId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RegistrationStartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    RegistrationEndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    FullPaperStartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    FullPaperEndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ReviewStartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ReviewEndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ReviseStartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ReviseEndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CameraReadyStartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CameraReadyEndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    IsWaitlist = table.Column<bool>(type: "boolean", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("ResearchConferencePhase_pkey", x => x.ResearchConferencePhaseId);
                    table.ForeignKey(
                        name: "FK_ResearchConferencePhase_ConferenceId",
                        column: x => x.ConferenceId,
                        principalTable: "Conference",
                        principalColumn: "ConferenceId");
                });

            migrationBuilder.CreateTable(
                name: "ReviewerContract",
                columns: table => new
                {
                    ReviewerContractId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: true),
                    SignDay = table.Column<DateOnly>(type: "date", nullable: true),
                    ExpireDay = table.Column<DateOnly>(type: "date", nullable: true),
                    Wage = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    ContractUrl = table.Column<string>(type: "text", nullable: true),
                    ConferenceId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("ReviewerContract_pkey", x => x.ReviewerContractId);
                    table.ForeignKey(
                        name: "FK_ReviewerContract_ConferenceId",
                        column: x => x.ConferenceId,
                        principalTable: "Conference",
                        principalColumn: "ConferenceId");
                    table.ForeignKey(
                        name: "FK_ReviewerContract_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "Sponsor",
                columns: table => new
                {
                    SponsorId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ImageURL = table.Column<string>(type: "text", nullable: true),
                    ConferenceId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Sponsor_pkey", x => x.SponsorId);
                    table.ForeignKey(
                        name: "FK_Sponsor_ConferenceId",
                        column: x => x.ConferenceId,
                        principalTable: "Conference",
                        principalColumn: "ConferenceId");
                });

            migrationBuilder.CreateTable(
                name: "TechnicalConferenceDetail",
                columns: table => new
                {
                    ConferenceId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TargetAudience = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("TechnicalConferenceDetail_pkey", x => x.ConferenceId);
                    table.ForeignKey(
                        name: "FK_TechnicalConferenceDetail_ConferenceId",
                        column: x => x.ConferenceId,
                        principalTable: "Conference",
                        principalColumn: "ConferenceId");
                });

            migrationBuilder.CreateTable(
                name: "ConferenceSession",
                columns: table => new
                {
                    ConferenceSessionId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    StartTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    EndTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    SessionDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ConferenceId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RoomId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("ConferenceSession_pkey", x => x.ConferenceSessionId);
                    table.ForeignKey(
                        name: "FK_ConferenceSession_ConferenceId",
                        column: x => x.ConferenceId,
                        principalTable: "Conference",
                        principalColumn: "ConferenceId");
                    table.ForeignKey(
                        name: "FK_ConferenceSession_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Room",
                        principalColumn: "RoomId");
                });

            migrationBuilder.CreateTable(
                name: "PricePhase",
                columns: table => new
                {
                    PricePhaseId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PhaseName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ApplyPercent = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    TotalSlot = table.Column<int>(type: "integer", nullable: true),
                    AvailableSlot = table.Column<int>(type: "integer", nullable: true),
                    ConferencePriceId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PricePhase_pkey", x => x.PricePhaseId);
                    table.ForeignKey(
                        name: "FK_PricePhase_ConferencePriceId",
                        column: x => x.ConferencePriceId,
                        principalTable: "ConferencePrice",
                        principalColumn: "ConferencePriceId");
                });

            migrationBuilder.CreateTable(
                name: "Ticket",
                columns: table => new
                {
                    TicketId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RegisteredDate = table.Column<DateOnly>(type: "date", nullable: true),
                    IsRefunded = table.Column<bool>(type: "boolean", nullable: true),
                    ActualPrice = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    UserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ConferencePriceId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Ticket_pkey", x => x.TicketId);
                    table.ForeignKey(
                        name: "FK_Ticket_ConferencePriceId",
                        column: x => x.ConferencePriceId,
                        principalTable: "ConferencePrice",
                        principalColumn: "ConferencePriceId");
                    table.ForeignKey(
                        name: "FK_Ticket_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "PaperAuthors",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PaperId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsPresenter = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaperAuthors", x => new { x.UserId, x.PaperId });
                    table.ForeignKey(
                        name: "FK_PaperAuthors_PaperId",
                        column: x => x.PaperId,
                        principalTable: "Paper",
                        principalColumn: "PaperId");
                    table.ForeignKey(
                        name: "FK_PaperAuthors_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "PaperReviewers",
                columns: table => new
                {
                    PaperId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsHeadReviewer = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.ForeignKey(
                        name: "FK_PaperReviewers_PaperId",
                        column: x => x.PaperId,
                        principalTable: "Paper",
                        principalColumn: "PaperId");
                    table.ForeignKey(
                        name: "FK_PaperReviewers_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "RevisionRoundDeadline",
                columns: table => new
                {
                    RevisionRoundDeadlineId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    RoundNumber = table.Column<int>(type: "integer", nullable: true),
                    ResearchConferencePhaseId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("RevisionRoundDeadline_pkey", x => x.RevisionRoundDeadlineId);
                    table.ForeignKey(
                        name: "FK_RevisionRoundDeadline_ResearchConferencePhaseId",
                        column: x => x.ResearchConferencePhaseId,
                        principalTable: "ResearchConferencePhase",
                        principalColumn: "ResearchConferencePhaseId");
                });

            migrationBuilder.CreateTable(
                name: "ConferenceFeedback",
                columns: table => new
                {
                    ConferenceFeedbackId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ConferenceSessionId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Rating = table.Column<int>(type: "integer", nullable: true),
                    Message = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("ConferenceFeedback_pkey", x => x.ConferenceFeedbackId);
                    table.ForeignKey(
                        name: "FK_ConferenceFeedback_ConferenceSessionId",
                        column: x => x.ConferenceSessionId,
                        principalTable: "ConferenceSession",
                        principalColumn: "ConferenceSessionId");
                    table.ForeignKey(
                        name: "FK_ConferenceFeedback_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "ConferenceSessionMedia",
                columns: table => new
                {
                    ConferenceSessionMediaId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MediaUrl = table.Column<string>(type: "text", nullable: true),
                    ConferenceSessionId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("ConferenceSessionMedia_pkey", x => x.ConferenceSessionMediaId);
                    table.ForeignKey(
                        name: "FK_ConferenceSessionMedia_ConferenceSessionId",
                        column: x => x.ConferenceSessionId,
                        principalTable: "ConferenceSession",
                        principalColumn: "ConferenceSessionId");
                });

            migrationBuilder.CreateTable(
                name: "PresentAuthors",
                columns: table => new
                {
                    ConferenceSessionId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.ForeignKey(
                        name: "FK_PresentAuthors_ConferenceSessionId",
                        column: x => x.ConferenceSessionId,
                        principalTable: "ConferenceSession",
                        principalColumn: "ConferenceSessionId");
                    table.ForeignKey(
                        name: "FK_PresentAuthors_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "SessionChangeRequest",
                columns: table => new
                {
                    SessionChangeRequestId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TicketId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CustomerId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    NewConferenceSessionId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    GlobalStatusId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Reason = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    RequestAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("SessionChangeRequest_pkey", x => x.SessionChangeRequestId);
                    table.ForeignKey(
                        name: "FK_SessionChangeRequest_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "User",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_SessionChangeRequest_GlobalStatusId",
                        column: x => x.GlobalStatusId,
                        principalTable: "GlobalStatus",
                        principalColumn: "GlobalStatusId");
                    table.ForeignKey(
                        name: "FK_SessionChangeRequest_NewConferenceSessionId",
                        column: x => x.NewConferenceSessionId,
                        principalTable: "ConferenceSession",
                        principalColumn: "ConferenceSessionId");
                });

            migrationBuilder.CreateTable(
                name: "Speaker",
                columns: table => new
                {
                    SpeakerId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Image = table.Column<string>(type: "text", nullable: true),
                    ConferenceSessionId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Speaker_pkey", x => x.SpeakerId);
                    table.ForeignKey(
                        name: "FK_Speaker_ConferenceSessionId",
                        column: x => x.ConferenceSessionId,
                        principalTable: "ConferenceSession",
                        principalColumn: "ConferenceSessionId");
                });

            migrationBuilder.CreateTable(
                name: "Transaction",
                columns: table => new
                {
                    TransactionId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Currency = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    TransactionCode = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    IsRefunded = table.Column<bool>(type: "boolean", nullable: true),
                    PaymentMethodId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TicketId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Transaction_pkey", x => x.TransactionId);
                    table.ForeignKey(
                        name: "FK_Transaction_PaymentMethodId",
                        column: x => x.PaymentMethodId,
                        principalTable: "PaymentMethod",
                        principalColumn: "PaymentMethodId");
                    table.ForeignKey(
                        name: "FK_Transaction_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Ticket",
                        principalColumn: "TicketId");
                    table.ForeignKey(
                        name: "FK_Transaction_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "UserCheckIn",
                columns: table => new
                {
                    UserCheckinId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsPresenter = table.Column<bool>(type: "boolean", nullable: true),
                    CheckinStatusId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CheckInTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    UserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TicketId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ConferenceSessionId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("UserCheckIn_pkey", x => x.UserCheckinId);
                    table.ForeignKey(
                        name: "FK_UserCheckIn_CheckinStatusId",
                        column: x => x.CheckinStatusId,
                        principalTable: "CheckinStatus",
                        principalColumn: "CheckinStatusId");
                    table.ForeignKey(
                        name: "FK_UserCheckIn_ConferenceSessionId",
                        column: x => x.ConferenceSessionId,
                        principalTable: "ConferenceSession",
                        principalColumn: "ConferenceSessionId");
                    table.ForeignKey(
                        name: "FK_UserCheckIn_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Ticket",
                        principalColumn: "TicketId");
                    table.ForeignKey(
                        name: "FK_UserCheckIn_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "RevisionPaperSubmission",
                columns: table => new
                {
                    RevisionPaperSubmissionId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RevisionPaperURL = table.Column<string>(type: "text", nullable: true),
                    RevisionPaperId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RevisionDeadlineRoundId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("RevisionPaperSubmission_pkey", x => x.RevisionPaperSubmissionId);
                    table.ForeignKey(
                        name: "FK_RevisionPaperSubmission_RevisionDeadlineRoundId",
                        column: x => x.RevisionDeadlineRoundId,
                        principalTable: "RevisionRoundDeadline",
                        principalColumn: "RevisionRoundDeadlineId");
                    table.ForeignKey(
                        name: "FK_RevisionPaperSubmission_RevisionPaperId",
                        column: x => x.RevisionPaperId,
                        principalTable: "RevisionPaper",
                        principalColumn: "RevisionPaperId");
                });

            migrationBuilder.CreateTable(
                name: "RevisionSubmissionFeedback",
                columns: table => new
                {
                    RevisionSubmissionFeedbackId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Feedback = table.Column<string>(type: "text", nullable: true),
                    Response = table.Column<string>(type: "text", nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    RevisionPaperSubmissionId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("RevisionSubmissionFeedback_pkey", x => x.RevisionSubmissionFeedbackId);
                    table.ForeignKey(
                        name: "FK_RevisionSubmissionFeedback_RevisionPaperSubmissionId",
                        column: x => x.RevisionPaperSubmissionId,
                        principalTable: "RevisionPaperSubmission",
                        principalColumn: "RevisionPaperSubmissionId");
                    table.ForeignKey(
                        name: "FK_RevisionSubmissionFeedback_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Abstract_GlobalStatusId",
                table: "Abstract",
                column: "GlobalStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_UserId",
                table: "AuditLog",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CameraReady_GlobalStatusId",
                table: "CameraReady",
                column: "GlobalStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Conference_CityId",
                table: "Conference",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Conference_ConferenceCategoryId",
                table: "Conference",
                column: "ConferenceCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Conference_CreatedBy",
                table: "Conference",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ConferenceFeedback_ConferenceSessionId",
                table: "ConferenceFeedback",
                column: "ConferenceSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_ConferenceFeedback_UserId",
                table: "ConferenceFeedback",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ConferenceMedia_ConferenceId",
                table: "ConferenceMedia",
                column: "ConferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_ConferencePrice_ConferenceId",
                table: "ConferencePrice",
                column: "ConferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_ConferenceSession_ConferenceId",
                table: "ConferenceSession",
                column: "ConferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_ConferenceSession_RoomId",
                table: "ConferenceSession",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_ConferenceSessionMedia_ConferenceSessionId",
                table: "ConferenceSessionMedia",
                column: "ConferenceSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_ConferenceTimeline_AfterwardStatusId",
                table: "ConferenceTimeline",
                column: "AfterwardStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_ConferenceTimeline_ConferenceId",
                table: "ConferenceTimeline",
                column: "ConferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_ConferenceTimeline_PreviousStatusId",
                table: "ConferenceTimeline",
                column: "PreviousStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Destination_CityId",
                table: "Destination",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_FavouriteConference_ConferenceId",
                table: "FavouriteConference",
                column: "ConferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_FavouriteConference_UserId",
                table: "FavouriteConference",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_FullPaper_ReviewStatusId",
                table: "FullPaper",
                column: "ReviewStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_FullPaperReview_FullPaperId",
                table: "FullPaperReview",
                column: "FullPaperId");

            migrationBuilder.CreateIndex(
                name: "IX_FullPaperReview_ReviewerId",
                table: "FullPaperReview",
                column: "ReviewerId");

            migrationBuilder.CreateIndex(
                name: "IX_FullPaperReview_ReviewStatusId",
                table: "FullPaperReview",
                column: "ReviewStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialDownload_ConferenceId",
                table: "MaterialDownload",
                column: "ConferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_Notification_UserId",
                table: "Notification",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Paper_CameraReadyId",
                table: "Paper",
                column: "CameraReadyId");

            migrationBuilder.CreateIndex(
                name: "IX_Paper_ConferenceId",
                table: "Paper",
                column: "ConferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_Paper_PaperPhaseId",
                table: "Paper",
                column: "PaperPhaseId");

            migrationBuilder.CreateIndex(
                name: "IX_Paper_PresenterId",
                table: "Paper",
                column: "PresenterId");

            migrationBuilder.CreateIndex(
                name: "IX_PaperAuthors_PaperId",
                table: "PaperAuthors",
                column: "PaperId");

            migrationBuilder.CreateIndex(
                name: "IX_PaperReviewers_PaperId",
                table: "PaperReviewers",
                column: "PaperId");

            migrationBuilder.CreateIndex(
                name: "IX_PaperReviewers_UserId",
                table: "PaperReviewers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PaperWaitList_ConferenceId",
                table: "PaperWaitList",
                column: "ConferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_PaperWaitList_UserId",
                table: "PaperWaitList",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PaperWaitList_WaitListStatusId",
                table: "PaperWaitList",
                column: "WaitListStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Policy_ConferenceId",
                table: "Policy",
                column: "ConferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_PresentAuthors_ConferenceSessionId",
                table: "PresentAuthors",
                column: "ConferenceSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_PresentAuthors_UserId",
                table: "PresentAuthors",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PresenterChangeRequest_GlobalStatusId",
                table: "PresenterChangeRequest",
                column: "GlobalStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_PresenterChangeRequest_NewPresenterId",
                table: "PresenterChangeRequest",
                column: "NewPresenterId");

            migrationBuilder.CreateIndex(
                name: "IX_PresenterChangeRequest_RequestedById",
                table: "PresenterChangeRequest",
                column: "RequestedById");

            migrationBuilder.CreateIndex(
                name: "IX_PricePhase_ConferencePriceId",
                table: "PricePhase",
                column: "ConferencePriceId");

            migrationBuilder.CreateIndex(
                name: "IX_RankingFileUrl_ConferenceId",
                table: "RankingFileUrl",
                column: "ConferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_RankingReferenceUrl_ConferenceId",
                table: "RankingReferenceUrl",
                column: "ConferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_RefundPolicy_ConferenceId",
                table: "RefundPolicy",
                column: "ConferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_Refundrequest_GlobalStatusId",
                table: "Refundrequest",
                column: "GlobalStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Report_UserId",
                table: "Report",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportFeedback_AdminId",
                table: "ReportFeedback",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_ResearchConferenceDetail_RankingCategoryId",
                table: "ResearchConferenceDetail",
                column: "RankingCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ResearchConferencePhase_ConferenceId",
                table: "ResearchConferencePhase",
                column: "ConferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewerContract_ConferenceId",
                table: "ReviewerContract",
                column: "ConferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewerContract_UserId",
                table: "ReviewerContract",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RevisionPaper_GlobalStatusId",
                table: "RevisionPaper",
                column: "GlobalStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_RevisionPaperReview_GlobalStatusId",
                table: "RevisionPaperReview",
                column: "GlobalStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_RevisionPaperReview_ReviewerId",
                table: "RevisionPaperReview",
                column: "ReviewerId");

            migrationBuilder.CreateIndex(
                name: "IX_RevisionPaperReview_RevisionPaperId",
                table: "RevisionPaperReview",
                column: "RevisionPaperId");

            migrationBuilder.CreateIndex(
                name: "IX_RevisionPaperSubmission_RevisionDeadlineRoundId",
                table: "RevisionPaperSubmission",
                column: "RevisionDeadlineRoundId");

            migrationBuilder.CreateIndex(
                name: "IX_RevisionPaperSubmission_RevisionPaperId",
                table: "RevisionPaperSubmission",
                column: "RevisionPaperId");

            migrationBuilder.CreateIndex(
                name: "IX_RevisionRoundDeadline_ResearchConferencePhaseId",
                table: "RevisionRoundDeadline",
                column: "ResearchConferencePhaseId");

            migrationBuilder.CreateIndex(
                name: "IX_RevisionSubmissionFeedback_RevisionPaperSubmissionId",
                table: "RevisionSubmissionFeedback",
                column: "RevisionPaperSubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_RevisionSubmissionFeedback_UserId",
                table: "RevisionSubmissionFeedback",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Room_DestinationId",
                table: "Room",
                column: "DestinationId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionChangeRequest_CustomerId",
                table: "SessionChangeRequest",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionChangeRequest_GlobalStatusId",
                table: "SessionChangeRequest",
                column: "GlobalStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionChangeRequest_NewConferenceSessionId",
                table: "SessionChangeRequest",
                column: "NewConferenceSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Speaker_ConferenceSessionId",
                table: "Speaker",
                column: "ConferenceSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Sponsor_ConferenceId",
                table: "Sponsor",
                column: "ConferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_ConferencePriceId",
                table: "Ticket",
                column: "ConferencePriceId");

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_UserId",
                table: "Ticket",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_PaymentMethodId",
                table: "Transaction",
                column: "PaymentMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_TicketId",
                table: "Transaction",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_UserId",
                table: "Transaction",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCheckIn_CheckinStatusId",
                table: "UserCheckIn",
                column: "CheckinStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCheckIn_ConferenceSessionId",
                table: "UserCheckIn",
                column: "ConferenceSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCheckIn_TicketId",
                table: "UserCheckIn",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCheckIn_UserId",
                table: "UserCheckIn",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRefreshToken_UserId",
                table: "UserRefreshToken",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "UserRefreshToken_Token_key",
                table: "UserRefreshToken",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_RoleId",
                table: "UserRole",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Abstract");

            migrationBuilder.DropTable(
                name: "AcademicProfile");

            migrationBuilder.DropTable(
                name: "AuditLog");

            migrationBuilder.DropTable(
                name: "ConferenceFeedback");

            migrationBuilder.DropTable(
                name: "ConferenceMedia");

            migrationBuilder.DropTable(
                name: "ConferenceSessionMedia");

            migrationBuilder.DropTable(
                name: "ConferenceTimeline");

            migrationBuilder.DropTable(
                name: "FavouriteConference");

            migrationBuilder.DropTable(
                name: "FullPaperReview");

            migrationBuilder.DropTable(
                name: "GeneralFAQ");

            migrationBuilder.DropTable(
                name: "MaterialDownload");

            migrationBuilder.DropTable(
                name: "Notification");

            migrationBuilder.DropTable(
                name: "PaperAuthors");

            migrationBuilder.DropTable(
                name: "PaperReviewers");

            migrationBuilder.DropTable(
                name: "PaperWaitList");

            migrationBuilder.DropTable(
                name: "Policy");

            migrationBuilder.DropTable(
                name: "PresentAuthors");

            migrationBuilder.DropTable(
                name: "PresenterChangeRequest");

            migrationBuilder.DropTable(
                name: "PricePhase");

            migrationBuilder.DropTable(
                name: "RankingFileUrl");

            migrationBuilder.DropTable(
                name: "RankingReferenceUrl");

            migrationBuilder.DropTable(
                name: "RefundPolicy");

            migrationBuilder.DropTable(
                name: "Refundrequest");

            migrationBuilder.DropTable(
                name: "Report");

            migrationBuilder.DropTable(
                name: "ReportFeedback");

            migrationBuilder.DropTable(
                name: "ResearchConferenceDetail");

            migrationBuilder.DropTable(
                name: "ReviewerContract");

            migrationBuilder.DropTable(
                name: "RevisionPaperReview");

            migrationBuilder.DropTable(
                name: "RevisionSubmissionFeedback");

            migrationBuilder.DropTable(
                name: "SessionChangeRequest");

            migrationBuilder.DropTable(
                name: "Speaker");

            migrationBuilder.DropTable(
                name: "Sponsor");

            migrationBuilder.DropTable(
                name: "TechnicalConferenceDetail");

            migrationBuilder.DropTable(
                name: "Transaction");

            migrationBuilder.DropTable(
                name: "UserCheckIn");

            migrationBuilder.DropTable(
                name: "UserRefreshToken");

            migrationBuilder.DropTable(
                name: "UserRole");

            migrationBuilder.DropTable(
                name: "ConferenceStatus");

            migrationBuilder.DropTable(
                name: "FullPaper");

            migrationBuilder.DropTable(
                name: "Paper");

            migrationBuilder.DropTable(
                name: "WaitListStatus");

            migrationBuilder.DropTable(
                name: "RankingCategories");

            migrationBuilder.DropTable(
                name: "RevisionPaperSubmission");

            migrationBuilder.DropTable(
                name: "PaymentMethod");

            migrationBuilder.DropTable(
                name: "CheckinStatus");

            migrationBuilder.DropTable(
                name: "ConferenceSession");

            migrationBuilder.DropTable(
                name: "Ticket");

            migrationBuilder.DropTable(
                name: "Role");

            migrationBuilder.DropTable(
                name: "ReviewStatus");

            migrationBuilder.DropTable(
                name: "CameraReady");

            migrationBuilder.DropTable(
                name: "PaperPhase");

            migrationBuilder.DropTable(
                name: "RevisionRoundDeadline");

            migrationBuilder.DropTable(
                name: "RevisionPaper");

            migrationBuilder.DropTable(
                name: "Room");

            migrationBuilder.DropTable(
                name: "ConferencePrice");

            migrationBuilder.DropTable(
                name: "ResearchConferencePhase");

            migrationBuilder.DropTable(
                name: "GlobalStatus");

            migrationBuilder.DropTable(
                name: "Destination");

            migrationBuilder.DropTable(
                name: "Conference");

            migrationBuilder.DropTable(
                name: "City");

            migrationBuilder.DropTable(
                name: "ConferenceCategory");

            migrationBuilder.DropTable(
                name: "User");
        }
    }
}
