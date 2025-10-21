using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace ConfRadar.Repositories.Data;

public partial class ConfRadarDbContext : DbContext
{
    public ConfRadarDbContext()
    {
    }

    public ConfRadarDbContext(DbContextOptions<ConfRadarDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Conference> Conferences { get; set; }

    public virtual DbSet<ConferenceCategory> ConferenceCategories { get; set; }

    public virtual DbSet<ConferenceMedium> ConferenceMedia { get; set; }

    public virtual DbSet<ConferencePolicy> ConferencePolicies { get; set; }

    public virtual DbSet<ConferencePrice> ConferencePrices { get; set; }

    public virtual DbSet<ConferenceSession> ConferenceSessions { get; set; }

    public virtual DbSet<Destination> Destinations { get; set; }

    public virtual DbSet<FavouriteConference> FavouriteConferences { get; set; }

    public virtual DbSet<GlobalStatus> GlobalStatuses { get; set; }

    public virtual DbSet<MediaType> MediaTypes { get; set; }

    public virtual DbSet<PaymentMethod> PaymentMethods { get; set; }

    public virtual DbSet<PricePhase> PricePhases { get; set; }

    public virtual DbSet<RefundRequest> RefundRequests { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Room> Rooms { get; set; }

    public virtual DbSet<Speaker> Speakers { get; set; }

    public virtual DbSet<Sponsor> Sponsors { get; set; }

    public virtual DbSet<Status> Statuses { get; set; }

    public virtual DbSet<TechnicalConferenceDetail> TechnicalConferenceDetails { get; set; }

    public virtual DbSet<Ticket> Tickets { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    public virtual DbSet<TransactionStatus> TransactionStatuses { get; set; }

    public virtual DbSet<TransactionType> TransactionTypes { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserRefreshToken> UserRefreshTokens { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    public static string GetConnectionString(string connectionStringName)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .Build();

        string connectionString = config.GetConnectionString(connectionStringName);
        return connectionString;
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql(GetConnectionString("DefaultConnection")).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Conference>(entity =>
        {
            entity.HasKey(e => e.ConferenceId).HasName("Conference_pkey");

            entity.ToTable("Conference");

            entity.Property(e => e.ConferenceId).HasMaxLength(50);
            entity.Property(e => e.ConferenceCategoryId).HasMaxLength(50);
            entity.Property(e => e.ConferenceName).HasMaxLength(255);
            entity.Property(e => e.ConferenceRankingId).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.GlobalStatusId).HasMaxLength(50);
            entity.Property(e => e.LocationId).HasMaxLength(50);
            entity.Property(e => e.UserId).HasMaxLength(50);

            entity.HasOne(d => d.ConferenceCategory).WithMany(p => p.Conferences)
                .HasForeignKey(d => d.ConferenceCategoryId)
                .HasConstraintName("Conference_ConferenceCategoryId_fkey");
        });

        modelBuilder.Entity<ConferenceCategory>(entity =>
        {
            entity.HasKey(e => e.ConferenceCategoryId).HasName("ConferenceCategory_pkey");

            entity.ToTable("ConferenceCategory");

            entity.Property(e => e.ConferenceCategoryId).HasMaxLength(50);
            entity.Property(e => e.ConferenceCategoryName).HasMaxLength(50);
        });

        modelBuilder.Entity<ConferenceMedium>(entity =>
        {
            entity.HasKey(e => e.ConferenceMediaId).HasName("ConferenceMedia_pkey");

            entity.Property(e => e.ConferenceMediaId).HasMaxLength(50);
            entity.Property(e => e.ConferenceId).HasMaxLength(50);
            entity.Property(e => e.MediaTypeId).HasMaxLength(50);

            entity.HasOne(d => d.Conference).WithMany(p => p.ConferenceMedia)
                .HasForeignKey(d => d.ConferenceId)
                .HasConstraintName("ConferenceMedia_ConferenceId_fkey");

            entity.HasOne(d => d.MediaType).WithMany(p => p.ConferenceMedia)
                .HasForeignKey(d => d.MediaTypeId)
                .HasConstraintName("ConferenceMedia_MediaTypeId_fkey");
        });

        modelBuilder.Entity<ConferencePolicy>(entity =>
        {
            entity.HasKey(e => e.PolicyId).HasName("ConferencePolicy_pkey");

            entity.ToTable("ConferencePolicy");

            entity.Property(e => e.PolicyId).HasMaxLength(50);
            entity.Property(e => e.ConferenceId).HasMaxLength(50);
            entity.Property(e => e.PolicyName).HasMaxLength(255);

            entity.HasOne(d => d.Conference).WithMany(p => p.ConferencePolicies)
                .HasForeignKey(d => d.ConferenceId)
                .HasConstraintName("ConferencePolicy_ConferenceId_fkey");
        });

        modelBuilder.Entity<ConferencePrice>(entity =>
        {
            entity.HasKey(e => e.ConferencePriceId).HasName("ConferencePrice_pkey");

            entity.ToTable("ConferencePrice");

            entity.Property(e => e.ConferencePriceId).HasMaxLength(50);
            entity.Property(e => e.ActualPrice).HasPrecision(10, 2);
            entity.Property(e => e.ConferenceId).HasMaxLength(50);
            entity.Property(e => e.PricePhaseId).HasMaxLength(50);
            entity.Property(e => e.TicketName).HasMaxLength(255);
            entity.Property(e => e.TicketPrice).HasPrecision(10, 2);

            entity.HasOne(d => d.Conference).WithMany(p => p.ConferencePrices)
                .HasForeignKey(d => d.ConferenceId)
                .HasConstraintName("ConferencePrice_ConferenceId_fkey");

            entity.HasOne(d => d.PricePhase).WithMany(p => p.ConferencePrices)
                .HasForeignKey(d => d.PricePhaseId)
                .HasConstraintName("ConferencePrice_PricePhaseId_fkey");
        });

        modelBuilder.Entity<ConferenceSession>(entity =>
        {
            entity.HasKey(e => e.ConferenceSessionId).HasName("ConferenceSession_pkey");

            entity.ToTable("ConferenceSession");

            entity.Property(e => e.ConferenceSessionId).HasMaxLength(50);
            entity.Property(e => e.ConferenceId).HasMaxLength(50);
            entity.Property(e => e.Date).HasColumnType("timestamp without time zone");
            entity.Property(e => e.EndTime).HasColumnType("timestamp without time zone");
            entity.Property(e => e.RoomId).HasMaxLength(50);
            entity.Property(e => e.StartTime).HasColumnType("timestamp without time zone");
            entity.Property(e => e.StatusId).HasMaxLength(50);
            entity.Property(e => e.Title).HasMaxLength(50);

            entity.HasOne(d => d.Conference).WithMany(p => p.ConferenceSessions)
                .HasForeignKey(d => d.ConferenceId)
                .HasConstraintName("ConferenceSession_ConferenceId_fkey");

            entity.HasOne(d => d.Room).WithMany(p => p.ConferenceSessions)
                .HasForeignKey(d => d.RoomId)
                .HasConstraintName("ConferenceSession_RoomId_fkey");
        });

        modelBuilder.Entity<Destination>(entity =>
        {
            entity.HasKey(e => e.DestinationId).HasName("Destination_pkey");

            entity.ToTable("Destination");

            entity.Property(e => e.DestinationId).HasMaxLength(50);
            entity.Property(e => e.City).HasMaxLength(50);
            entity.Property(e => e.District).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.Street).HasMaxLength(50);
        });

        modelBuilder.Entity<FavouriteConference>(entity =>
        {
            entity.HasKey(e => e.FavouriteConferenceId).HasName("FavouriteConference_pkey");

            entity.ToTable("FavouriteConference");

            entity.Property(e => e.FavouriteConferenceId).HasMaxLength(50);
            entity.Property(e => e.ConferenceId).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.UserId).HasMaxLength(50);

            entity.HasOne(d => d.Conference).WithMany(p => p.FavouriteConferences)
                .HasForeignKey(d => d.ConferenceId)
                .HasConstraintName("FavouriteConference_ConferenceId_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.FavouriteConferences)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FavouriteConference_UserId_fkey");
        });

        modelBuilder.Entity<GlobalStatus>(entity =>
        {
            entity.HasKey(e => e.GlobalStatusId).HasName("GlobalStatus_pkey");

            entity.ToTable("GlobalStatus");

            entity.Property(e => e.GlobalStatusId).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<MediaType>(entity =>
        {
            entity.HasKey(e => e.MediaTypeId).HasName("MediaType_pkey");

            entity.ToTable("MediaType");

            entity.Property(e => e.MediaTypeId).HasMaxLength(50);
            entity.Property(e => e.MediaTypeName).HasMaxLength(255);
        });

        modelBuilder.Entity<PaymentMethod>(entity =>
        {
            entity.HasKey(e => e.PaymentMethodId).HasName("PaymentMethod_pkey");

            entity.ToTable("PaymentMethod");

            entity.Property(e => e.PaymentMethodId).HasMaxLength(50);
            entity.Property(e => e.MethodName).HasMaxLength(50);
        });

        modelBuilder.Entity<PricePhase>(entity =>
        {
            entity.HasKey(e => e.PricePhaseId).HasName("PricePhase_pkey");

            entity.ToTable("PricePhase");

            entity.Property(e => e.PricePhaseId).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<RefundRequest>(entity =>
        {
            entity.HasKey(e => e.RefundRequestId).HasName("RefundRequest_pkey");

            entity.ToTable("RefundRequest");

            entity.HasIndex(e => e.TicketId, "RefundRequest_TicketId_key").IsUnique();

            entity.Property(e => e.RefundRequestId).HasMaxLength(50);
            entity.Property(e => e.GlobalStatusId).HasMaxLength(50);
            entity.Property(e => e.TicketId).HasMaxLength(50);
            entity.Property(e => e.TransactionId).HasMaxLength(50);

            entity.HasOne(d => d.GlobalStatus).WithMany(p => p.RefundRequests)
                .HasForeignKey(d => d.GlobalStatusId)
                .HasConstraintName("RefundRequest_GlobalStatusId_fkey");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("Role_pkey");

            entity.ToTable("Role");

            entity.HasIndex(e => e.RoleName, "Role_RoleName_key").IsUnique();

            entity.Property(e => e.RoleId).HasMaxLength(50);
            entity.Property(e => e.RoleName).HasMaxLength(100);
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(e => e.RoomId).HasName("Room_pkey");

            entity.ToTable("Room");

            entity.Property(e => e.RoomId).HasMaxLength(50);
            entity.Property(e => e.DestinationId).HasMaxLength(50);
            entity.Property(e => e.DisplayName).HasMaxLength(50);
            entity.Property(e => e.Number).HasMaxLength(255);

            entity.HasOne(d => d.Destination).WithMany(p => p.Rooms)
                .HasForeignKey(d => d.DestinationId)
                .HasConstraintName("Room_DestinationId_fkey");
        });

        modelBuilder.Entity<Speaker>(entity =>
        {
            entity.HasKey(e => e.ConferenceSessionId).HasName("Speaker_pkey");

            entity.ToTable("Speaker");

            entity.Property(e => e.ConferenceSessionId).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(255);

            entity.HasOne(d => d.ConferenceSession).WithOne(p => p.Speaker)
                .HasForeignKey<Speaker>(d => d.ConferenceSessionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Speaker_ConferenceSessionId_fkey");
        });

        modelBuilder.Entity<Sponsor>(entity =>
        {
            entity.HasKey(e => e.SponsorId).HasName("Sponsor_pkey");

            entity.ToTable("Sponsor");

            entity.Property(e => e.SponsorId).HasMaxLength(50);
            entity.Property(e => e.ConferenceId).HasMaxLength(50);
            entity.Property(e => e.ImageUrl).HasColumnName("ImageURL");
            entity.Property(e => e.Name).HasMaxLength(50);

            entity.HasOne(d => d.Conference).WithMany(p => p.Sponsors)
                .HasForeignKey(d => d.ConferenceId)
                .HasConstraintName("Sponsor_ConferenceId_fkey");
        });

        modelBuilder.Entity<Status>(entity =>
        {
            entity.HasKey(e => e.StatusId).HasName("Status_pkey");

            entity.ToTable("Status");

            entity.Property(e => e.StatusId).HasMaxLength(50);
            entity.Property(e => e.StatusName).HasMaxLength(255);
        });

        modelBuilder.Entity<TechnicalConferenceDetail>(entity =>
        {
            entity.HasKey(e => e.ConferenceId).HasName("TechnicalConferenceDetail_pkey");

            entity.ToTable("TechnicalConferenceDetail");

            entity.Property(e => e.ConferenceId).HasMaxLength(50);

            entity.HasOne(d => d.Conference).WithOne(p => p.TechnicalConferenceDetail)
                .HasForeignKey<TechnicalConferenceDetail>(d => d.ConferenceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("TechnicalConferenceDetail_ConferenceId_fkey");
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(e => e.TicketId).HasName("Ticket_pkey");

            entity.ToTable("Ticket");

            entity.Property(e => e.TicketId).HasMaxLength(50);
            entity.Property(e => e.ActualPrice).HasPrecision(10, 2);
            entity.Property(e => e.ConferencePriceId).HasMaxLength(50);
            entity.Property(e => e.RegisteredDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.TransactionId).HasMaxLength(50);
            entity.Property(e => e.UserId).HasMaxLength(50);

            entity.HasOne(d => d.ConferencePrice).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.ConferencePriceId)
                .HasConstraintName("Ticket_ConferencePriceId_fkey");

            entity.HasOne(d => d.Transaction).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.TransactionId)
                .HasConstraintName("Ticket_TransactionId_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("Ticket_UserId_fkey");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("Transaction_pkey");

            entity.ToTable("Transaction");

            entity.Property(e => e.TransactionId).HasMaxLength(50);
            entity.Property(e => e.Amount).HasPrecision(10, 2);
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.Currency).HasMaxLength(50);
            entity.Property(e => e.PaymentMethodId).HasMaxLength(50);
            entity.Property(e => e.TransactionStatusId).HasMaxLength(50);
            entity.Property(e => e.TransactionTypeId).HasMaxLength(50);
            entity.Property(e => e.UserId).HasMaxLength(50);

            entity.HasOne(d => d.PaymentMethod).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.PaymentMethodId)
                .HasConstraintName("Transaction_PaymentMethodId_fkey");

            entity.HasOne(d => d.TransactionStatus).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.TransactionStatusId)
                .HasConstraintName("Transaction_TransactionStatusId_fkey");

            entity.HasOne(d => d.TransactionType).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.TransactionTypeId)
                .HasConstraintName("Transaction_TransactionTypeId_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("Transaction_UserId_fkey");
        });

        modelBuilder.Entity<TransactionStatus>(entity =>
        {
            entity.HasKey(e => e.TransactionStatusId).HasName("TransactionStatus_pkey");

            entity.ToTable("TransactionStatus");

            entity.Property(e => e.TransactionStatusId).HasMaxLength(50);
            entity.Property(e => e.StatusName).HasMaxLength(255);
        });

        modelBuilder.Entity<TransactionType>(entity =>
        {
            entity.HasKey(e => e.TransactionTypeId).HasName("TransactionType_pkey");

            entity.ToTable("TransactionType");

            entity.Property(e => e.TransactionTypeId).HasMaxLength(50);
            entity.Property(e => e.TypeName).HasMaxLength(255);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("User_pkey");

            entity.ToTable("User");

            entity.HasIndex(e => e.Email, "User_Email_key").IsUnique();

            entity.Property(e => e.UserId).HasMaxLength(50);
            entity.Property(e => e.AvatarUrl).HasMaxLength(255);
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FullName).HasMaxLength(255);
            entity.Property(e => e.Gender).HasMaxLength(20);
            entity.Property(e => e.LastLogin).HasColumnType("timestamp without time zone");
            entity.Property(e => e.LoginProvider).HasMaxLength(50);
            entity.Property(e => e.PasswordHash).HasMaxLength(500);
            entity.Property(e => e.PasswordResetToken).HasMaxLength(255);
            entity.Property(e => e.PasswordResetTokenExpiry).HasColumnType("timestamp without time zone");
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.VerificationToken).HasMaxLength(255);
            entity.Property(e => e.VerificationTokenExpiry).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<UserRefreshToken>(entity =>
        {
            entity.HasKey(e => e.TokenId).HasName("UserRefreshToken_pkey");

            entity.ToTable("UserRefreshToken");

            entity.HasIndex(e => e.Token, "UserRefreshToken_Token_key").IsUnique();

            entity.Property(e => e.TokenId).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.Expiry).HasColumnType("timestamp without time zone");
            entity.Property(e => e.Token).HasMaxLength(500);
            entity.Property(e => e.UserId).HasMaxLength(50);

            entity.HasOne(d => d.User).WithMany(p => p.UserRefreshTokens)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("UserRefreshToken_UserId_fkey");
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.RoleId }).HasName("UserRole_pkey");

            entity.ToTable("UserRole");

            entity.Property(e => e.UserId).HasMaxLength(50);
            entity.Property(e => e.RoleId).HasMaxLength(50);
            entity.Property(e => e.AssignedAt).HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.Role).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("UserRole_RoleId_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("UserRole_UserId_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
