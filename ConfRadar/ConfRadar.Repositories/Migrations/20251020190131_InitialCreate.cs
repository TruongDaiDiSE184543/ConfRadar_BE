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
                name: "Destination",
                columns: table => new
                {
                    DestinationId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    City = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    District = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Street = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Destination_pkey", x => x.DestinationId);
                });

            migrationBuilder.CreateTable(
                name: "GlobalStatus",
                columns: table => new
                {
                    GlobalStatusId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("GlobalStatus_pkey", x => x.GlobalStatusId);
                });

            migrationBuilder.CreateTable(
                name: "MediaType",
                columns: table => new
                {
                    MediaTypeId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MediaTypeName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("MediaType_pkey", x => x.MediaTypeId);
                });

            migrationBuilder.CreateTable(
                name: "PaymentMethod",
                columns: table => new
                {
                    PaymentMethodId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MethodName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    MethodDescription = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PaymentMethod_pkey", x => x.PaymentMethodId);
                });

            migrationBuilder.CreateTable(
                name: "PricePhase",
                columns: table => new
                {
                    PricePhaseId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    EarlierBirdEndInterval = table.Column<DateOnly>(type: "date", nullable: true),
                    PercentForEarly = table.Column<int>(type: "integer", nullable: true),
                    StandardEndInterval = table.Column<DateOnly>(type: "date", nullable: true),
                    LateEndInterval = table.Column<DateOnly>(type: "date", nullable: true),
                    PercentForEnd = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PricePhase_pkey", x => x.PricePhaseId);
                });

            migrationBuilder.CreateTable(
                name: "Role",
                columns: table => new
                {
                    RoleId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RoleName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Role_pkey", x => x.RoleId);
                });

            migrationBuilder.CreateTable(
                name: "Status",
                columns: table => new
                {
                    StatusId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    StatusName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Status_pkey", x => x.StatusId);
                });

            migrationBuilder.CreateTable(
                name: "TransactionStatus",
                columns: table => new
                {
                    TransactionStatusId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    StatusName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("TransactionStatus_pkey", x => x.TransactionStatusId);
                });

            migrationBuilder.CreateTable(
                name: "TransactionType",
                columns: table => new
                {
                    TransactionTypeId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TypeName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    TypeDescription = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("TransactionType_pkey", x => x.TransactionTypeId);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FullName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    BirthDay = table.Column<DateOnly>(type: "date", nullable: true),
                    PhoneNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Gender = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    LastLogin = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    AvatarUrl = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    BioDescription = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: true),
                    IsEmailConfirmed = table.Column<bool>(type: "boolean", nullable: true),
                    LoginProvider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    VerificationToken = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    VerificationTokenExpiry = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    PasswordResetToken = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    PasswordResetTokenExpiry = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("User_pkey", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "Conference",
                columns: table => new
                {
                    ConferenceId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ConferenceName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Capacity = table.Column<int>(type: "integer", nullable: true),
                    Address = table.Column<string>(type: "text", nullable: true),
                    BannerImageUrl = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsInternalHosted = table.Column<bool>(type: "boolean", nullable: true),
                    IsResearchConference = table.Column<bool>(type: "boolean", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: true),
                    ConferenceRankingId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LocationId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ConferenceCategoryId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    GlobalStatusId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Conference_pkey", x => x.ConferenceId);
                    table.ForeignKey(
                        name: "Conference_ConferenceCategoryId_fkey",
                        column: x => x.ConferenceCategoryId,
                        principalTable: "ConferenceCategory",
                        principalColumn: "ConferenceCategoryId");
                });

            migrationBuilder.CreateTable(
                name: "Room",
                columns: table => new
                {
                    RoomId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Number = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    DisplayName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DestinationId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Room_pkey", x => x.RoomId);
                    table.ForeignKey(
                        name: "Room_DestinationId_fkey",
                        column: x => x.DestinationId,
                        principalTable: "Destination",
                        principalColumn: "DestinationId");
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
                name: "ConferenceMedia",
                columns: table => new
                {
                    ConferenceMediaId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ConferenceMediaUrl = table.Column<string>(type: "text", nullable: true),
                    ConferenceId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    MediaTypeId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("ConferenceMedia_pkey", x => x.ConferenceMediaId);
                    table.ForeignKey(
                        name: "ConferenceMedia_ConferenceId_fkey",
                        column: x => x.ConferenceId,
                        principalTable: "Conference",
                        principalColumn: "ConferenceId");
                    table.ForeignKey(
                        name: "ConferenceMedia_MediaTypeId_fkey",
                        column: x => x.MediaTypeId,
                        principalTable: "MediaType",
                        principalColumn: "MediaTypeId");
                });

            migrationBuilder.CreateTable(
                name: "ConferencePolicy",
                columns: table => new
                {
                    PolicyId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PolicyName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ConferenceId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("ConferencePolicy_pkey", x => x.PolicyId);
                    table.ForeignKey(
                        name: "ConferencePolicy_ConferenceId_fkey",
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
                    TicketName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    TicketDescription = table.Column<string>(type: "text", nullable: true),
                    ActualPrice = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    PricePhaseId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ConferenceId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("ConferencePrice_pkey", x => x.ConferencePriceId);
                    table.ForeignKey(
                        name: "ConferencePrice_ConferenceId_fkey",
                        column: x => x.ConferenceId,
                        principalTable: "Conference",
                        principalColumn: "ConferenceId");
                    table.ForeignKey(
                        name: "ConferencePrice_PricePhaseId_fkey",
                        column: x => x.PricePhaseId,
                        principalTable: "PricePhase",
                        principalColumn: "PricePhaseId");
                });

            migrationBuilder.CreateTable(
                name: "FavouriteConference",
                columns: table => new
                {
                    FavouriteConferenceId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ConferenceId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("FavouriteConference_pkey", x => x.FavouriteConferenceId);
                    table.ForeignKey(
                        name: "FavouriteConference_ConferenceId_fkey",
                        column: x => x.ConferenceId,
                        principalTable: "Conference",
                        principalColumn: "ConferenceId");
                    table.ForeignKey(
                        name: "FavouriteConference_UserId_fkey",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "Sponsor",
                columns: table => new
                {
                    SponsorId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ImageURL = table.Column<string>(type: "text", nullable: true),
                    ConferenceId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Sponsor_pkey", x => x.SponsorId);
                    table.ForeignKey(
                        name: "Sponsor_ConferenceId_fkey",
                        column: x => x.ConferenceId,
                        principalTable: "Conference",
                        principalColumn: "ConferenceId");
                });

            migrationBuilder.CreateTable(
                name: "TechnicalConferenceDetail",
                columns: table => new
                {
                    ConferenceId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TargetAudience = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("TechnicalConferenceDetail_pkey", x => x.ConferenceId);
                    table.ForeignKey(
                        name: "TechnicalConferenceDetail_ConferenceId_fkey",
                        column: x => x.ConferenceId,
                        principalTable: "Conference",
                        principalColumn: "ConferenceId");
                });

            migrationBuilder.CreateTable(
                name: "ConferenceSession",
                columns: table => new
                {
                    ConferenceSessionId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ConferenceId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    StatusId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RoomId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("ConferenceSession_pkey", x => x.ConferenceSessionId);
                    table.ForeignKey(
                        name: "ConferenceSession_ConferenceId_fkey",
                        column: x => x.ConferenceId,
                        principalTable: "Conference",
                        principalColumn: "ConferenceId");
                    table.ForeignKey(
                        name: "ConferenceSession_RoomId_fkey",
                        column: x => x.RoomId,
                        principalTable: "Room",
                        principalColumn: "RoomId");
                });

            migrationBuilder.CreateTable(
                name: "Ticket",
                columns: table => new
                {
                    TicketId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ConferencePriceId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RegisteredDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsRefunded = table.Column<bool>(type: "boolean", nullable: true),
                    ActualPrice = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Ticket_pkey", x => x.TicketId);
                    table.ForeignKey(
                        name: "Ticket_ConferencePriceId_fkey",
                        column: x => x.ConferencePriceId,
                        principalTable: "ConferencePrice",
                        principalColumn: "ConferencePriceId");
                    table.ForeignKey(
                        name: "Ticket_UserId_fkey",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "Speaker",
                columns: table => new
                {
                    ConferenceSessionId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Speaker_pkey", x => x.ConferenceSessionId);
                    table.ForeignKey(
                        name: "Speaker_ConferenceSessionId_fkey",
                        column: x => x.ConferenceSessionId,
                        principalTable: "ConferenceSession",
                        principalColumn: "ConferenceSessionId");
                });

            migrationBuilder.CreateTable(
                name: "Transaction",
                columns: table => new
                {
                    TransactionId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TicketId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Currency = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    TransactionCode = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    TransactionStatusId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TransactionTypeId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PaymentMethodId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Transaction_pkey", x => x.TransactionId);
                    table.ForeignKey(
                        name: "Transaction_PaymentMethodId_fkey",
                        column: x => x.PaymentMethodId,
                        principalTable: "PaymentMethod",
                        principalColumn: "PaymentMethodId");
                    table.ForeignKey(
                        name: "Transaction_TicketId_fkey",
                        column: x => x.TicketId,
                        principalTable: "Ticket",
                        principalColumn: "TicketId");
                    table.ForeignKey(
                        name: "Transaction_TransactionStatusId_fkey",
                        column: x => x.TransactionStatusId,
                        principalTable: "TransactionStatus",
                        principalColumn: "TransactionStatusId");
                    table.ForeignKey(
                        name: "Transaction_TransactionTypeId_fkey",
                        column: x => x.TransactionTypeId,
                        principalTable: "TransactionType",
                        principalColumn: "TransactionTypeId");
                    table.ForeignKey(
                        name: "Transaction_UserId_fkey",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "RefundRequest",
                columns: table => new
                {
                    RefundRequestId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TransactionId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TicketId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    GlobalStatusId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("RefundRequest_pkey", x => x.RefundRequestId);
                    table.ForeignKey(
                        name: "RefundRequest_GlobalStatusId_fkey",
                        column: x => x.GlobalStatusId,
                        principalTable: "GlobalStatus",
                        principalColumn: "GlobalStatusId");
                    table.ForeignKey(
                        name: "RefundRequest_TicketId_fkey",
                        column: x => x.TicketId,
                        principalTable: "Ticket",
                        principalColumn: "TicketId");
                    table.ForeignKey(
                        name: "RefundRequest_TransactionId_fkey",
                        column: x => x.TransactionId,
                        principalTable: "Transaction",
                        principalColumn: "TransactionId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Conference_ConferenceCategoryId",
                table: "Conference",
                column: "ConferenceCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ConferenceMedia_ConferenceId",
                table: "ConferenceMedia",
                column: "ConferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_ConferenceMedia_MediaTypeId",
                table: "ConferenceMedia",
                column: "MediaTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ConferencePolicy_ConferenceId",
                table: "ConferencePolicy",
                column: "ConferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_ConferencePrice_ConferenceId",
                table: "ConferencePrice",
                column: "ConferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_ConferencePrice_PricePhaseId",
                table: "ConferencePrice",
                column: "PricePhaseId");

            migrationBuilder.CreateIndex(
                name: "IX_ConferenceSession_ConferenceId",
                table: "ConferenceSession",
                column: "ConferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_ConferenceSession_RoomId",
                table: "ConferenceSession",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_FavouriteConference_ConferenceId",
                table: "FavouriteConference",
                column: "ConferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_FavouriteConference_UserId",
                table: "FavouriteConference",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RefundRequest_GlobalStatusId",
                table: "RefundRequest",
                column: "GlobalStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_RefundRequest_TransactionId",
                table: "RefundRequest",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "RefundRequest_TicketId_key",
                table: "RefundRequest",
                column: "TicketId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "Role_RoleName_key",
                table: "Role",
                column: "RoleName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Room_DestinationId",
                table: "Room",
                column: "DestinationId");

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
                name: "IX_Transaction_TransactionStatusId",
                table: "Transaction",
                column: "TransactionStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_TransactionTypeId",
                table: "Transaction",
                column: "TransactionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_UserId",
                table: "Transaction",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "User_Email_key",
                table: "User",
                column: "Email",
                unique: true);

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
                name: "ConferenceMedia");

            migrationBuilder.DropTable(
                name: "ConferencePolicy");

            migrationBuilder.DropTable(
                name: "FavouriteConference");

            migrationBuilder.DropTable(
                name: "RefundRequest");

            migrationBuilder.DropTable(
                name: "Speaker");

            migrationBuilder.DropTable(
                name: "Sponsor");

            migrationBuilder.DropTable(
                name: "Status");

            migrationBuilder.DropTable(
                name: "TechnicalConferenceDetail");

            migrationBuilder.DropTable(
                name: "UserRefreshToken");

            migrationBuilder.DropTable(
                name: "UserRole");

            migrationBuilder.DropTable(
                name: "MediaType");

            migrationBuilder.DropTable(
                name: "GlobalStatus");

            migrationBuilder.DropTable(
                name: "Transaction");

            migrationBuilder.DropTable(
                name: "ConferenceSession");

            migrationBuilder.DropTable(
                name: "Role");

            migrationBuilder.DropTable(
                name: "PaymentMethod");

            migrationBuilder.DropTable(
                name: "Ticket");

            migrationBuilder.DropTable(
                name: "TransactionStatus");

            migrationBuilder.DropTable(
                name: "TransactionType");

            migrationBuilder.DropTable(
                name: "Room");

            migrationBuilder.DropTable(
                name: "ConferencePrice");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "Destination");

            migrationBuilder.DropTable(
                name: "Conference");

            migrationBuilder.DropTable(
                name: "PricePhase");

            migrationBuilder.DropTable(
                name: "ConferenceCategory");
        }
    }
}
