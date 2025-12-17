using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

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

    public virtual DbSet<Abstract> Abstracts { get; set; }

    public virtual DbSet<AcademicProfile> AcademicProfiles { get; set; }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<AuditLogCategory> AuditLogCategories { get; set; }

    public virtual DbSet<CameraReady> CameraReadies { get; set; }

    public virtual DbSet<CheckinStatus> CheckinStatuses { get; set; }

    public virtual DbSet<City> Cities { get; set; }

    public virtual DbSet<CollaboratorContract> CollaboratorContracts { get; set; }

    public virtual DbSet<Conference> Conferences { get; set; }

    public virtual DbSet<ConferenceCategory> ConferenceCategories { get; set; }

    public virtual DbSet<ConferenceFeedback> ConferenceFeedbacks { get; set; }

    public virtual DbSet<ConferenceMedium> ConferenceMedia { get; set; }

    public virtual DbSet<ConferencePrice> ConferencePrices { get; set; }

    public virtual DbSet<ConferenceSession> ConferenceSessions { get; set; }

    public virtual DbSet<ConferenceSessionMedium> ConferenceSessionMedia { get; set; }

    public virtual DbSet<ConferenceStatus> ConferenceStatuses { get; set; }

    public virtual DbSet<ConferenceTimeline> ConferenceTimelines { get; set; }

    public virtual DbSet<Destination> Destinations { get; set; }

    public virtual DbSet<FavouriteConference> FavouriteConferences { get; set; }

    public virtual DbSet<FullPaper> FullPapers { get; set; }

    public virtual DbSet<FullPaperReview> FullPaperReviews { get; set; }

    public virtual DbSet<GeneralFaq> GeneralFaqs { get; set; }

    public virtual DbSet<GlobalStatus> GlobalStatuses { get; set; }

    public virtual DbSet<MaterialDownload> MaterialDownloads { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<OrcidDataCache> OrcidDataCaches { get; set; }

    public virtual DbSet<Organization> Organizations { get; set; }

    public virtual DbSet<Paper> Papers { get; set; }

    public virtual DbSet<PaperAuthor> PaperAuthors { get; set; }

    public virtual DbSet<PaperPhase> PaperPhases { get; set; }

    public virtual DbSet<PaperReviewer> PaperReviewers { get; set; }

    public virtual DbSet<PaperWaitList> PaperWaitLists { get; set; }

    public virtual DbSet<PaymentMethod> PaymentMethods { get; set; }

    public virtual DbSet<Policy> Policies { get; set; }

    public virtual DbSet<PresentAuthor> PresentAuthors { get; set; }

    public virtual DbSet<PresenterChangeRequest> PresenterChangeRequests { get; set; }

    public virtual DbSet<PricePhase> PricePhases { get; set; }

    public virtual DbSet<RankingCategory> RankingCategories { get; set; }

    public virtual DbSet<RankingFileUrl> RankingFileUrls { get; set; }

    public virtual DbSet<RankingReferenceUrl> RankingReferenceUrls { get; set; }

    public virtual DbSet<RefundPolicy> RefundPolicies { get; set; }

    public virtual DbSet<RefundRequest> RefundRequests { get; set; }

    public virtual DbSet<Report> Reports { get; set; }

    public virtual DbSet<ReportFeedback> ReportFeedbacks { get; set; }

    public virtual DbSet<ResearchConferenceDetail> ResearchConferenceDetails { get; set; }

    public virtual DbSet<ResearchConferencePhase> ResearchConferencePhases { get; set; }

    public virtual DbSet<ReviewStatus> ReviewStatuses { get; set; }

    public virtual DbSet<ReviewerContract> ReviewerContracts { get; set; }

    public virtual DbSet<RevisionPaper> RevisionPapers { get; set; }

    public virtual DbSet<RevisionPaperSubmission> RevisionPaperSubmissions { get; set; }

    public virtual DbSet<RevisionRoundDeadline> RevisionRoundDeadlines { get; set; }

    public virtual DbSet<RevisionSubmissionFeedback> RevisionSubmissionFeedbacks { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Room> Rooms { get; set; }

    public virtual DbSet<SessionChangeRequest> SessionChangeRequests { get; set; }

    public virtual DbSet<Speaker> Speakers { get; set; }

    public virtual DbSet<Sponsor> Sponsors { get; set; }

    public virtual DbSet<TechnicalConferenceDetail> TechnicalConferenceDetails { get; set; }

    public virtual DbSet<Ticket> Tickets { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserCheckIn> UserCheckIns { get; set; }

    public virtual DbSet<UserRefreshToken> UserRefreshTokens { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    public virtual DbSet<UserSuspendHistory> UserSuspendHistories { get; set; }

    public virtual DbSet<WaitListStatus> WaitListStatuses { get; set; }

    public virtual DbSet<Wallet> Wallets { get; set; }

    public virtual DbSet<WalletTransaction> WalletTransactions { get; set; }

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
        => optionsBuilder.UseNpgsql(GetConnectionString("DefaultConnection"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Abstract>(entity =>
        {
            entity.HasKey(e => e.AbstractId).HasName("Abstract_pkey");

            entity.ToTable("Abstract");

            entity.Property(e => e.AbstractId).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.GlobalStatusId).HasMaxLength(50);
            entity.Property(e => e.ReviewAt).HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.GlobalStatus).WithMany(p => p.Abstracts)
                .HasForeignKey(d => d.GlobalStatusId)
                .HasConstraintName("FK_Abstract_GlobalStatusId");
        });

        modelBuilder.Entity<AcademicProfile>(entity =>
        {
            entity.ToTable("AcademicProfile");

            entity.Property(e => e.AcademicProfileId).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.ExpiresAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.OrcidId).HasMaxLength(50);
            entity.Property(e => e.Scope).HasMaxLength(250);
            entity.Property(e => e.UserId).HasMaxLength(50);

            entity.HasOne(d => d.User).WithMany(p => p.AcademicProfiles)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("Fk_AcademicProfile_User");
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.AuditLogId).HasName("AuditLog_pkey");

            entity.ToTable("AuditLog");

            entity.Property(e => e.AuditLogId).HasMaxLength(50);
            entity.Property(e => e.CategoryId).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.UserId).HasMaxLength(50);

            entity.HasOne(d => d.Category).WithMany(p => p.AuditLogs)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK_AuditLog_CategoryId");

            entity.HasOne(d => d.User).WithMany(p => p.AuditLogs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_AuditLog_UserId");
        });

        modelBuilder.Entity<AuditLogCategory>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("AuditLogCategory_pkey");

            entity.ToTable("AuditLogCategory");

            entity.Property(e => e.CategoryId).HasMaxLength(50);
        });

        modelBuilder.Entity<CameraReady>(entity =>
        {
            entity.HasKey(e => e.CameraReadyId).HasName("CameraReady_pkey");

            entity.ToTable("CameraReady");

            entity.Property(e => e.CameraReadyId).HasMaxLength(50);
            entity.Property(e => e.CameraReadyUrl).HasColumnName("CameraReadyURL");
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.ReviewAt).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<CheckinStatus>(entity =>
        {
            entity.HasKey(e => e.CheckinStatusId).HasName("CheckinStatus_pkey");

            entity.ToTable("CheckinStatus");

            entity.Property(e => e.CheckinStatusId).HasMaxLength(50);
            entity.Property(e => e.CheckinStatusName).HasMaxLength(255);
        });

        modelBuilder.Entity<City>(entity =>
        {
            entity.HasKey(e => e.CityId).HasName("City_pkey");

            entity.ToTable("City");

            entity.Property(e => e.CityId).HasMaxLength(50);
        });

        modelBuilder.Entity<CollaboratorContract>(entity =>
        {
            entity.HasKey(e => e.CollaboratorContractId).HasName("CollaboratorContract_pkey");

            entity.ToTable("CollaboratorContract");

            entity.HasIndex(e => e.ConferenceId, "CollaboratorContract_ConferenceId_key").IsUnique();

            entity.Property(e => e.CollaboratorContractId).HasMaxLength(50);
            entity.Property(e => e.ConferenceId).HasMaxLength(50);
            entity.Property(e => e.UserId).HasMaxLength(50);

            entity.HasOne(d => d.Conference).WithOne(p => p.CollaboratorContract)
                .HasForeignKey<CollaboratorContract>(d => d.ConferenceId)
                .HasConstraintName("FK_CollaboratorContract_ConferenceId");

            entity.HasOne(d => d.User).WithMany(p => p.CollaboratorContracts)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_CollaboratorContract_UserId");
        });

        modelBuilder.Entity<Conference>(entity =>
        {
            entity.HasKey(e => e.ConferenceId).HasName("Conference_pkey");

            entity.ToTable("Conference");

            entity.Property(e => e.ConferenceId).HasMaxLength(50);
            entity.Property(e => e.CityId).HasMaxLength(50);
            entity.Property(e => e.ConferenceCategoryId).HasMaxLength(50);
            entity.Property(e => e.ConferenceName).HasMaxLength(100);
            entity.Property(e => e.ConferenceStatusId).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.CreatedBy).HasMaxLength(50);

            entity.HasOne(d => d.City).WithMany(p => p.Conferences)
                .HasForeignKey(d => d.CityId)
                .HasConstraintName("FK_Conference_CityId");

            entity.HasOne(d => d.ConferenceCategory).WithMany(p => p.Conferences)
                .HasForeignKey(d => d.ConferenceCategoryId)
                .HasConstraintName("FK_Conference_ConferenceCategoryId");

            entity.HasOne(d => d.ConferenceStatus).WithMany(p => p.Conferences)
                .HasForeignKey(d => d.ConferenceStatusId)
                .HasConstraintName("FK_Conference_ConferenceStatusId");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Conferences)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_Conference_CreatedBy");
        });

        modelBuilder.Entity<ConferenceCategory>(entity =>
        {
            entity.HasKey(e => e.ConferenceCategoryId).HasName("ConferenceCategory_pkey");

            entity.ToTable("ConferenceCategory");

            entity.Property(e => e.ConferenceCategoryId).HasMaxLength(50);
            entity.Property(e => e.ConferenceCategoryName).HasMaxLength(50);
        });

        modelBuilder.Entity<ConferenceFeedback>(entity =>
        {
            entity.HasKey(e => e.ConferenceFeedbackId).HasName("ConferenceFeedback_pkey");

            entity.ToTable("ConferenceFeedback");

            entity.Property(e => e.ConferenceFeedbackId).HasMaxLength(50);
            entity.Property(e => e.ConferenceSessionId).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.UserId).HasMaxLength(50);

            entity.HasOne(d => d.ConferenceSession).WithMany(p => p.ConferenceFeedbacks)
                .HasForeignKey(d => d.ConferenceSessionId)
                .HasConstraintName("FK_ConferenceFeedback_ConferenceSessionId");

            entity.HasOne(d => d.User).WithMany(p => p.ConferenceFeedbacks)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_ConferenceFeedback_UserId");
        });

        modelBuilder.Entity<ConferenceMedium>(entity =>
        {
            entity.HasKey(e => e.ConferenceMediaId).HasName("ConferenceMedia_pkey");

            entity.Property(e => e.ConferenceMediaId).HasMaxLength(50);
            entity.Property(e => e.ConferenceId).HasMaxLength(50);

            entity.HasOne(d => d.Conference).WithMany(p => p.ConferenceMedia)
                .HasForeignKey(d => d.ConferenceId)
                .HasConstraintName("FK_ConferenceMedia_ConferenceId");
        });

        modelBuilder.Entity<ConferencePrice>(entity =>
        {
            entity.HasKey(e => e.ConferencePriceId).HasName("ConferencePrice_pkey");

            entity.ToTable("ConferencePrice");

            entity.Property(e => e.ConferencePriceId).HasMaxLength(50);
            entity.Property(e => e.ConferenceId).HasMaxLength(50);
            entity.Property(e => e.TicketPrice).HasPrecision(10, 2);

            entity.HasOne(d => d.Conference).WithMany(p => p.ConferencePrices)
                .HasForeignKey(d => d.ConferenceId)
                .HasConstraintName("FK_ConferencePrice_ConferenceId");
        });

        modelBuilder.Entity<ConferenceSession>(entity =>
        {
            entity.HasKey(e => e.ConferenceSessionId).HasName("ConferenceSession_pkey");

            entity.ToTable("ConferenceSession");

            entity.Property(e => e.ConferenceSessionId).HasMaxLength(50);
            entity.Property(e => e.ConferenceId).HasMaxLength(50);
            entity.Property(e => e.EndTime).HasColumnType("timestamp without time zone");
            entity.Property(e => e.RoomId).HasMaxLength(50);
            entity.Property(e => e.StartTime).HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.Conference).WithMany(p => p.ConferenceSessions)
                .HasForeignKey(d => d.ConferenceId)
                .HasConstraintName("FK_ConferenceSession_ConferenceId");

            entity.HasOne(d => d.Room).WithMany(p => p.ConferenceSessions)
                .HasForeignKey(d => d.RoomId)
                .HasConstraintName("FK_ConferenceSession_RoomId");
        });

        modelBuilder.Entity<ConferenceSessionMedium>(entity =>
        {
            entity.HasKey(e => e.ConferenceSessionMediaId).HasName("ConferenceSessionMedia_pkey");

            entity.Property(e => e.ConferenceSessionMediaId).HasMaxLength(50);
            entity.Property(e => e.ConferenceSessionId).HasMaxLength(50);

            entity.HasOne(d => d.ConferenceSession).WithMany(p => p.ConferenceSessionMedia)
                .HasForeignKey(d => d.ConferenceSessionId)
                .HasConstraintName("FK_ConferenceSessionMedia_ConferenceSessionId");
        });

        modelBuilder.Entity<ConferenceStatus>(entity =>
        {
            entity.HasKey(e => e.ConferenceStatusId).HasName("ConferenceStatus_pkey");

            entity.ToTable("ConferenceStatus");

            entity.Property(e => e.ConferenceStatusId).HasMaxLength(50);
            entity.Property(e => e.ConferenceStatusName).HasMaxLength(255);
        });

        modelBuilder.Entity<ConferenceTimeline>(entity =>
        {
            entity.HasKey(e => e.ConferenceTimelineId).HasName("ConferenceTimeline_pkey");

            entity.ToTable("ConferenceTimeline");

            entity.Property(e => e.ConferenceTimelineId).HasMaxLength(50);
            entity.Property(e => e.AfterwardStatusId).HasMaxLength(50);
            entity.Property(e => e.ConferenceId).HasMaxLength(50);
            entity.Property(e => e.PreviousStatusId).HasMaxLength(50);

            entity.HasOne(d => d.AfterwardStatus).WithMany(p => p.ConferenceTimelineAfterwardStatuses)
                .HasForeignKey(d => d.AfterwardStatusId)
                .HasConstraintName("FK_ConferenceTimeline_AfterwardStatusId");

            entity.HasOne(d => d.Conference).WithMany(p => p.ConferenceTimelines)
                .HasForeignKey(d => d.ConferenceId)
                .HasConstraintName("FK_ConferenceTimeline_ConferenceId");

            entity.HasOne(d => d.PreviousStatus).WithMany(p => p.ConferenceTimelinePreviousStatuses)
                .HasForeignKey(d => d.PreviousStatusId)
                .HasConstraintName("FK_ConferenceTimeline_PreviousStatusId");
        });

        modelBuilder.Entity<Destination>(entity =>
        {
            entity.HasKey(e => e.DestinationId).HasName("Destination_pkey");

            entity.ToTable("Destination");

            entity.Property(e => e.DestinationId).HasMaxLength(50);
            entity.Property(e => e.CityId).HasMaxLength(50);
            entity.Property(e => e.District).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Street).HasMaxLength(255);

            entity.HasOne(d => d.City).WithMany(p => p.Destinations)
                .HasForeignKey(d => d.CityId)
                .HasConstraintName("FK_Destination_CityId");
        });

        modelBuilder.Entity<FavouriteConference>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.ConferenceId }).HasName("FavouriteConference_pkey");

            entity.ToTable("FavouriteConference");

            entity.Property(e => e.UserId).HasMaxLength(50);
            entity.Property(e => e.ConferenceId).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.Conference).WithMany(p => p.FavouriteConferences)
                .HasForeignKey(d => d.ConferenceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FavouriteConference_ConferenceId");

            entity.HasOne(d => d.User).WithMany(p => p.FavouriteConferences)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FavouriteConference_UserId");
        });

        modelBuilder.Entity<FullPaper>(entity =>
        {
            entity.HasKey(e => e.FullPaperId).HasName("FullPaper_pkey");

            entity.ToTable("FullPaper");

            entity.Property(e => e.FullPaperId).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.FullPaperUrl).HasColumnName("FullPaperURL");
            entity.Property(e => e.ReviewAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.ReviewStatusId).HasMaxLength(50);

            entity.HasOne(d => d.ReviewStatus).WithMany(p => p.FullPapers)
                .HasForeignKey(d => d.ReviewStatusId)
                .HasConstraintName("FK_FullPaper_ReviewStatusId");
        });

        modelBuilder.Entity<FullPaperReview>(entity =>
        {
            entity.HasKey(e => e.FullPaperReviewId).HasName("FullPaperReview_pkey");

            entity.ToTable("FullPaperReview");

            entity.Property(e => e.FullPaperReviewId).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.FullPaperId).HasMaxLength(50);
            entity.Property(e => e.ReviewStatusId).HasMaxLength(50);
            entity.Property(e => e.ReviewerId).HasMaxLength(50);

            entity.HasOne(d => d.FullPaper).WithMany(p => p.FullPaperReviews)
                .HasForeignKey(d => d.FullPaperId)
                .HasConstraintName("FK_FullPaperReview_FullPaperId");

            entity.HasOne(d => d.ReviewStatus).WithMany(p => p.FullPaperReviews)
                .HasForeignKey(d => d.ReviewStatusId)
                .HasConstraintName("FK_FullPaperReview_ReviewStatusId");

            entity.HasOne(d => d.Reviewer).WithMany(p => p.FullPaperReviews)
                .HasForeignKey(d => d.ReviewerId)
                .HasConstraintName("FK_FullPaperReview_ReviewerId");
        });

        modelBuilder.Entity<GeneralFaq>(entity =>
        {
            entity.HasKey(e => e.GeneralFaqid).HasName("GeneralFAQ_pkey");

            entity.ToTable("GeneralFAQ");

            entity.Property(e => e.GeneralFaqid)
                .HasMaxLength(50)
                .HasColumnName("GeneralFAQId");
            entity.Property(e => e.Name).HasMaxLength(255);
        });

        modelBuilder.Entity<GlobalStatus>(entity =>
        {
            entity.HasKey(e => e.GlobalStatusId).HasName("GlobalStatus_pkey");

            entity.ToTable("GlobalStatus");

            entity.Property(e => e.GlobalStatusId).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(255);
        });

        modelBuilder.Entity<MaterialDownload>(entity =>
        {
            entity.HasKey(e => e.MaterialDownloadId).HasName("MaterialDownload_pkey");

            entity.ToTable("MaterialDownload");

            entity.Property(e => e.MaterialDownloadId).HasMaxLength(50);
            entity.Property(e => e.ConferenceId).HasMaxLength(50);
            entity.Property(e => e.FileName).HasMaxLength(255);

            entity.HasOne(d => d.Conference).WithMany(p => p.MaterialDownloads)
                .HasForeignKey(d => d.ConferenceId)
                .HasConstraintName("FK_MaterialDownload_ConferenceId");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("Notification_pkey");

            entity.ToTable("Notification");

            entity.Property(e => e.NotificationId).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.Title).HasMaxLength(255);
            entity.Property(e => e.Type).HasMaxLength(255);
            entity.Property(e => e.UserId).HasMaxLength(50);

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Notification_UserId");
        });

        modelBuilder.Entity<OrcidDataCache>(entity =>
        {
            entity.ToTable("OrcidDataCache");

            entity.Property(e => e.OrcidDataCacheId).HasMaxLength(50);
            entity.Property(e => e.AcademicProfileId).HasMaxLength(50);
            entity.Property(e => e.DataType).HasMaxLength(250);
            entity.Property(e => e.JsonContent).HasColumnType("jsonb");
            entity.Property(e => e.LastSyncedAt).HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.AcademicProfile).WithMany(p => p.OrcidDataCaches)
                .HasForeignKey(d => d.AcademicProfileId)
                .HasConstraintName("FK_OrcidDataCache_AcademicProfile");
        });

        modelBuilder.Entity<Organization>(entity =>
        {
            entity.HasKey(e => e.OrganizationId).HasName("Organization_pkey");

            entity.ToTable("Organization");

            entity.HasIndex(e => e.UserId, "Organization_UserId_key").IsUnique();

            entity.Property(e => e.OrganizationId).HasMaxLength(50);
            entity.Property(e => e.UserId).HasMaxLength(50);

            entity.HasOne(d => d.User).WithOne(p => p.Organization)
                .HasForeignKey<Organization>(d => d.UserId)
                .HasConstraintName("FK_Organization_UserId");
        });

        modelBuilder.Entity<Paper>(entity =>
        {
            entity.HasKey(e => e.PaperId).HasName("Paper_pkey");

            entity.ToTable("Paper");

            entity.HasIndex(e => e.TicketId, "Paper_TicketId_key").IsUnique();

            entity.Property(e => e.PaperId).HasMaxLength(50);
            entity.Property(e => e.AbstractId).HasMaxLength(50);
            entity.Property(e => e.CameraReadyId).HasMaxLength(50);
            entity.Property(e => e.ConferenceId).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.FullPaperId).HasMaxLength(50);
            entity.Property(e => e.PaperPhaseId).HasMaxLength(50);
            entity.Property(e => e.ResearchConferencePhaseId).HasMaxLength(50);
            entity.Property(e => e.RevisionPaperId).HasMaxLength(50);
            entity.Property(e => e.TicketId).HasMaxLength(50);

            entity.HasOne(d => d.Abstract).WithMany(p => p.Papers)
                .HasForeignKey(d => d.AbstractId)
                .HasConstraintName("FK_Paper_AbstractId");

            entity.HasOne(d => d.CameraReady).WithMany(p => p.Papers)
                .HasForeignKey(d => d.CameraReadyId)
                .HasConstraintName("FK_Paper_CameraReadyId");

            entity.HasOne(d => d.Conference).WithMany(p => p.Papers)
                .HasForeignKey(d => d.ConferenceId)
                .HasConstraintName("FK_Paper_ConferenceId");

            entity.HasOne(d => d.FullPaper).WithMany(p => p.Papers)
                .HasForeignKey(d => d.FullPaperId)
                .HasConstraintName("FK_Paper_FullPaperId");

            entity.HasOne(d => d.PaperPhase).WithMany(p => p.Papers)
                .HasForeignKey(d => d.PaperPhaseId)
                .HasConstraintName("FK_Paper_PaperPhaseId");

            entity.HasOne(d => d.ResearchConferencePhase).WithMany(p => p.Papers)
                .HasForeignKey(d => d.ResearchConferencePhaseId)
                .HasConstraintName("FK_Paper_ResearchConferencePhaseId");

            entity.HasOne(d => d.RevisionPaper).WithMany(p => p.Papers)
                .HasForeignKey(d => d.RevisionPaperId)
                .HasConstraintName("FK_Paper_RevisionPaperId");

            entity.HasOne(d => d.Ticket).WithOne(p => p.Paper)
                .HasForeignKey<Paper>(d => d.TicketId)
                .HasConstraintName("FK_Paper_TicketId");
        });

        modelBuilder.Entity<PaperAuthor>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.PaperId });

            entity.Property(e => e.UserId).HasMaxLength(50);
            entity.Property(e => e.PaperId).HasMaxLength(50);

            entity.HasOne(d => d.Paper).WithMany(p => p.PaperAuthors)
                .HasForeignKey(d => d.PaperId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PaperAuthors_PaperId");

            entity.HasOne(d => d.User).WithMany(p => p.PaperAuthors)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PaperAuthors_UserId");
        });

        modelBuilder.Entity<PaperPhase>(entity =>
        {
            entity.HasKey(e => e.PaperPhaseId).HasName("PaperPhase_pkey");

            entity.ToTable("PaperPhase");

            entity.Property(e => e.PaperPhaseId).HasMaxLength(50);
            entity.Property(e => e.PhaseName).HasMaxLength(50);
        });

        modelBuilder.Entity<PaperReviewer>(entity =>
        {
            entity.HasKey(e => new { e.PaperId, e.UserId }).HasName("PaperReviewers_pkey");

            entity.Property(e => e.PaperId).HasMaxLength(50);
            entity.Property(e => e.UserId).HasMaxLength(50);

            entity.HasOne(d => d.Paper).WithMany(p => p.PaperReviewers)
                .HasForeignKey(d => d.PaperId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PaperReviewers_PaperId");

            entity.HasOne(d => d.User).WithMany(p => p.PaperReviewers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PaperReviewers_UserId");
        });

        modelBuilder.Entity<PaperWaitList>(entity =>
        {
            entity.HasKey(e => e.PaperWaitListId).HasName("PaperWaitList_pkey");

            entity.ToTable("PaperWaitList");

            entity.Property(e => e.PaperWaitListId).HasMaxLength(50);
            entity.Property(e => e.ConferenceId).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.NotifiedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.UserId).HasMaxLength(50);
            entity.Property(e => e.WaitListStatusId).HasMaxLength(50);

            entity.HasOne(d => d.Conference).WithMany(p => p.PaperWaitLists)
                .HasForeignKey(d => d.ConferenceId)
                .HasConstraintName("FK_PaperWaitList_ConferenceId");

            entity.HasOne(d => d.User).WithMany(p => p.PaperWaitLists)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_PaperWaitList_UserId");

            entity.HasOne(d => d.WaitListStatus).WithMany(p => p.PaperWaitLists)
                .HasForeignKey(d => d.WaitListStatusId)
                .HasConstraintName("FK_PaperWaitList_WaitListStatusId");
        });

        modelBuilder.Entity<PaymentMethod>(entity =>
        {
            entity.HasKey(e => e.PaymentMethodId).HasName("PaymentMethod_pkey");

            entity.ToTable("PaymentMethod");

            entity.Property(e => e.PaymentMethodId).HasMaxLength(50);
            entity.Property(e => e.MethodName).HasMaxLength(255);
        });

        modelBuilder.Entity<Policy>(entity =>
        {
            entity.HasKey(e => e.PolicyId).HasName("Policy_pkey");

            entity.ToTable("Policy");

            entity.Property(e => e.PolicyId).HasMaxLength(50);
            entity.Property(e => e.ConferenceId).HasMaxLength(50);
            entity.Property(e => e.PolicyName).HasMaxLength(255);

            entity.HasOne(d => d.Conference).WithMany(p => p.Policies)
                .HasForeignKey(d => d.ConferenceId)
                .HasConstraintName("FK_Policy_ConferenceId");
        });

        modelBuilder.Entity<PresentAuthor>(entity =>
        {
            entity.HasKey(e => new { e.ConferenceSessionId, e.PaperId }).HasName("PresentAuthors_pkey");

            entity.Property(e => e.ConferenceSessionId).HasMaxLength(50);
            entity.Property(e => e.PaperId).HasMaxLength(50);
            entity.Property(e => e.AssignedAt).HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.ConferenceSession).WithMany(p => p.PresentAuthors)
                .HasForeignKey(d => d.ConferenceSessionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PresentAuthors_ConferenceSessionId");

            entity.HasOne(d => d.Paper).WithMany(p => p.PresentAuthors)
                .HasForeignKey(d => d.PaperId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PresentAuthors_PaperId");
        });

        modelBuilder.Entity<PresenterChangeRequest>(entity =>
        {
            entity.HasKey(e => e.PresenterChangeRequestId).HasName("PresenterChangeRequest_pkey");

            entity.ToTable("PresenterChangeRequest");

            entity.Property(e => e.PresenterChangeRequestId).HasMaxLength(50);
            entity.Property(e => e.GlobalStatusId).HasMaxLength(50);
            entity.Property(e => e.NewPresenterId).HasMaxLength(50);
            entity.Property(e => e.PaperId).HasMaxLength(50);
            entity.Property(e => e.RequestAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.RequestedById).HasMaxLength(50);
            entity.Property(e => e.ReviewedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.TicketId).HasMaxLength(50);

            entity.HasOne(d => d.GlobalStatus).WithMany(p => p.PresenterChangeRequests)
                .HasForeignKey(d => d.GlobalStatusId)
                .HasConstraintName("FK_PresenterChangeRequest_GlobalStatusId");

            entity.HasOne(d => d.NewPresenter).WithMany(p => p.PresenterChangeRequestNewPresenters)
                .HasForeignKey(d => d.NewPresenterId)
                .HasConstraintName("FK_PresenterChangeRequest_NewPresenterId");

            entity.HasOne(d => d.Paper).WithMany(p => p.PresenterChangeRequests)
                .HasForeignKey(d => d.PaperId)
                .HasConstraintName("FK_PresenterChangeRequest_PaperId");

            entity.HasOne(d => d.RequestedBy).WithMany(p => p.PresenterChangeRequestRequestedBies)
                .HasForeignKey(d => d.RequestedById)
                .HasConstraintName("FK_PresenterChangeRequest_RequestedById");

            entity.HasOne(d => d.Ticket).WithMany(p => p.PresenterChangeRequests)
                .HasForeignKey(d => d.TicketId)
                .HasConstraintName("FK_PresenterChangeRequest_TicketId");
        });

        modelBuilder.Entity<PricePhase>(entity =>
        {
            entity.HasKey(e => e.PricePhaseId).HasName("PricePhase_pkey");

            entity.ToTable("PricePhase");

            entity.Property(e => e.PricePhaseId).HasMaxLength(50);
            entity.Property(e => e.ApplyPercent).HasPrecision(10, 2);
            entity.Property(e => e.ConferencePriceId).HasMaxLength(50);
            entity.Property(e => e.PhaseName).HasMaxLength(255);
            entity.Property(e => e.ResearchConferencePhaseId).HasMaxLength(50);

            entity.HasOne(d => d.ConferencePrice).WithMany(p => p.PricePhases)
                .HasForeignKey(d => d.ConferencePriceId)
                .HasConstraintName("FK_PricePhase_ConferencePriceId");

            entity.HasOne(d => d.ResearchConferencePhase).WithMany(p => p.PricePhases)
                .HasForeignKey(d => d.ResearchConferencePhaseId)
                .HasConstraintName("FK_PricePhase_ResearchConferencePhaseId");
        });

        modelBuilder.Entity<RankingCategory>(entity =>
        {
            entity.HasKey(e => e.RankingCategoryId).HasName("RankingCategories_pkey");

            entity.Property(e => e.RankingCategoryId).HasMaxLength(50);
            entity.Property(e => e.RankName).HasMaxLength(255);
        });

        modelBuilder.Entity<RankingFileUrl>(entity =>
        {
            entity.HasKey(e => e.RankingFileUrlId).HasName("RankingFileUrl_pkey");

            entity.ToTable("RankingFileUrl");

            entity.Property(e => e.RankingFileUrlId).HasMaxLength(50);
            entity.Property(e => e.ConferenceId).HasMaxLength(50);

            entity.HasOne(d => d.Conference).WithMany(p => p.RankingFileUrls)
                .HasForeignKey(d => d.ConferenceId)
                .HasConstraintName("FK_RankingFileUrl_ConferenceId");
        });

        modelBuilder.Entity<RankingReferenceUrl>(entity =>
        {
            entity.HasKey(e => e.ReferenceUrlId).HasName("RankingReferenceUrl_pkey");

            entity.ToTable("RankingReferenceUrl");

            entity.Property(e => e.ReferenceUrlId).HasMaxLength(50);
            entity.Property(e => e.ConferenceId).HasMaxLength(50);

            entity.HasOne(d => d.Conference).WithMany(p => p.RankingReferenceUrls)
                .HasForeignKey(d => d.ConferenceId)
                .HasConstraintName("FK_RankingReferenceUrl_ConferenceId");
        });

        modelBuilder.Entity<RefundPolicy>(entity =>
        {
            entity.HasKey(e => e.RefundPolicyId).HasName("RefundPolicy_pkey");

            entity.ToTable("RefundPolicy");

            entity.Property(e => e.RefundPolicyId).HasMaxLength(50);
            entity.Property(e => e.ConferenceId).HasMaxLength(50);
            entity.Property(e => e.PricePhaseId).HasMaxLength(50);

            entity.HasOne(d => d.Conference).WithMany(p => p.RefundPolicies)
                .HasForeignKey(d => d.ConferenceId)
                .HasConstraintName("FK_RefundPolicy_ConferenceId");

            entity.HasOne(d => d.PricePhase).WithMany(p => p.RefundPolicies)
                .HasForeignKey(d => d.PricePhaseId)
                .HasConstraintName("FK_RefundPolicy_PricePhaseId");
        });

        modelBuilder.Entity<RefundRequest>(entity =>
        {
            entity.HasKey(e => e.RefundRequestId).HasName("RefundRequest_pkey");

            entity.ToTable("RefundRequest");

            entity.Property(e => e.RefundRequestId).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.GlobalStatusId).HasMaxLength(50);
            entity.Property(e => e.TicketId).HasMaxLength(50);
            entity.Property(e => e.TransactionId).HasMaxLength(50);

            entity.HasOne(d => d.GlobalStatus).WithMany(p => p.RefundRequests)
                .HasForeignKey(d => d.GlobalStatusId)
                .HasConstraintName("FK_RefundRequest_GlobalStatusId");

            entity.HasOne(d => d.Ticket).WithMany(p => p.RefundRequests)
                .HasForeignKey(d => d.TicketId)
                .HasConstraintName("FK_RefundRequest_TicketId");

            entity.HasOne(d => d.Transaction).WithMany(p => p.RefundRequests)
                .HasForeignKey(d => d.TransactionId)
                .HasConstraintName("FK_RefundRequest_TransactionId");
        });

        modelBuilder.Entity<Report>(entity =>
        {
            entity.HasKey(e => e.ReportId).HasName("Report_pkey");

            entity.ToTable("Report");

            entity.Property(e => e.ReportId).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.ReportSubject).HasMaxLength(255);
            entity.Property(e => e.UserId).HasMaxLength(50);

            entity.HasOne(d => d.User).WithMany(p => p.Reports)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Report_UserId");
        });

        modelBuilder.Entity<ReportFeedback>(entity =>
        {
            entity.HasKey(e => e.ReportId).HasName("ReportFeedback_pkey");

            entity.ToTable("ReportFeedback");

            entity.Property(e => e.ReportId).HasMaxLength(50);
            entity.Property(e => e.AdminId).HasMaxLength(50);
            entity.Property(e => e.ReportSubject).HasMaxLength(255);

            entity.HasOne(d => d.Admin).WithMany(p => p.ReportFeedbacks)
                .HasForeignKey(d => d.AdminId)
                .HasConstraintName("FK_ReportFeedback_User_UserId");

            entity.HasOne(d => d.Report).WithOne(p => p.ReportFeedback)
                .HasForeignKey<ReportFeedback>(d => d.ReportId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ReportFeedback_ReportId");
        });

        modelBuilder.Entity<ResearchConferenceDetail>(entity =>
        {
            entity.HasKey(e => e.ConferenceId).HasName("ResearchConferenceDetail_pkey");

            entity.ToTable("ResearchConferenceDetail");

            entity.Property(e => e.ConferenceId).HasMaxLength(50);
            entity.Property(e => e.RankValue).HasMaxLength(255);
            entity.Property(e => e.RankingCategoryId).HasMaxLength(50);
            entity.Property(e => e.SubmitPaperFee).HasPrecision(10, 2);

            entity.HasOne(d => d.Conference).WithOne(p => p.ResearchConferenceDetail)
                .HasForeignKey<ResearchConferenceDetail>(d => d.ConferenceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ResearchConferenceDetail_ConferenceId");

            entity.HasOne(d => d.RankingCategory).WithMany(p => p.ResearchConferenceDetails)
                .HasForeignKey(d => d.RankingCategoryId)
                .HasConstraintName("FK_ResearchConferenceDetail_RankingCategoryId");
        });

        modelBuilder.Entity<ResearchConferencePhase>(entity =>
        {
            entity.HasKey(e => e.ResearchConferencePhaseId).HasName("ResearchConferencePhase_pkey");

            entity.ToTable("ResearchConferencePhase");

            entity.Property(e => e.ResearchConferencePhaseId).HasMaxLength(50);
            entity.Property(e => e.ConferenceId).HasMaxLength(50);

            entity.HasOne(d => d.Conference).WithMany(p => p.ResearchConferencePhases)
                .HasForeignKey(d => d.ConferenceId)
                .HasConstraintName("FK_ResearchConferencePhase_ConferenceId");
        });

        modelBuilder.Entity<ReviewStatus>(entity =>
        {
            entity.HasKey(e => e.ReviewStatusId).HasName("ReviewStatus_pkey");

            entity.ToTable("ReviewStatus");

            entity.Property(e => e.ReviewStatusId).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<ReviewerContract>(entity =>
        {
            entity.HasKey(e => e.ReviewerContractId).HasName("ReviewerContract_pkey");

            entity.ToTable("ReviewerContract");

            entity.Property(e => e.ReviewerContractId).HasMaxLength(50);
            entity.Property(e => e.ConferenceId).HasMaxLength(50);
            entity.Property(e => e.UserId).HasMaxLength(50);
            entity.Property(e => e.Wage).HasPrecision(10, 2);

            entity.HasOne(d => d.Conference).WithMany(p => p.ReviewerContracts)
                .HasForeignKey(d => d.ConferenceId)
                .HasConstraintName("FK_ReviewerContract_ConferenceId");

            entity.HasOne(d => d.User).WithMany(p => p.ReviewerContracts)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_ReviewerContract_UserId");
        });

        modelBuilder.Entity<RevisionPaper>(entity =>
        {
            entity.HasKey(e => e.RevisionPaperId).HasName("RevisionPaper_pkey");

            entity.ToTable("RevisionPaper");

            entity.Property(e => e.RevisionPaperId).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.GlobalStatusId).HasMaxLength(50);
            entity.Property(e => e.ReviewAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.RevisionRoundDeadlineId).HasMaxLength(50);

            entity.HasOne(d => d.GlobalStatus).WithMany(p => p.RevisionPapers)
                .HasForeignKey(d => d.GlobalStatusId)
                .HasConstraintName("FK_RevisionPaper_GlobalStatusId");

            entity.HasOne(d => d.RevisionRoundDeadline).WithMany(p => p.RevisionPapers)
                .HasForeignKey(d => d.RevisionRoundDeadlineId)
                .HasConstraintName("FK_RevisionPaper_RevisionRoundDeadlineId");
        });

        modelBuilder.Entity<RevisionPaperSubmission>(entity =>
        {
            entity.HasKey(e => e.RevisionPaperSubmissionId).HasName("RevisionPaperSubmission_pkey");

            entity.ToTable("RevisionPaperSubmission");

            entity.Property(e => e.RevisionPaperSubmissionId).HasMaxLength(50);
            entity.Property(e => e.RevisionDeadlineRoundId).HasMaxLength(50);
            entity.Property(e => e.RevisionPaperId).HasMaxLength(50);
            entity.Property(e => e.RevisionPaperUrl).HasColumnName("RevisionPaperURL");

            entity.HasOne(d => d.RevisionDeadlineRound).WithMany(p => p.RevisionPaperSubmissions)
                .HasForeignKey(d => d.RevisionDeadlineRoundId)
                .HasConstraintName("FK_RevisionPaperSubmission_RevisionDeadlineRoundId");

            entity.HasOne(d => d.RevisionPaper).WithMany(p => p.RevisionPaperSubmissions)
                .HasForeignKey(d => d.RevisionPaperId)
                .HasConstraintName("FK_RevisionPaperSubmission_RevisionPaperId");
        });

        modelBuilder.Entity<RevisionRoundDeadline>(entity =>
        {
            entity.HasKey(e => e.RevisionRoundDeadlineId).HasName("RevisionRoundDeadline_pkey");

            entity.ToTable("RevisionRoundDeadline");

            entity.Property(e => e.RevisionRoundDeadlineId).HasMaxLength(50);
            entity.Property(e => e.ResearchConferencePhaseId).HasMaxLength(50);

            entity.HasOne(d => d.ResearchConferencePhase).WithMany(p => p.RevisionRoundDeadlines)
                .HasForeignKey(d => d.ResearchConferencePhaseId)
                .HasConstraintName("FK_RevisionRoundDeadline_ResearchConferencePhaseId");
        });

        modelBuilder.Entity<RevisionSubmissionFeedback>(entity =>
        {
            entity.HasKey(e => e.RevisionSubmissionFeedbackId).HasName("RevisionSubmissionFeedback_pkey");

            entity.ToTable("RevisionSubmissionFeedback");

            entity.Property(e => e.RevisionSubmissionFeedbackId).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.RevisionPaperSubmissionId).HasMaxLength(50);
            entity.Property(e => e.UserId).HasMaxLength(50);

            entity.HasOne(d => d.RevisionPaperSubmission).WithMany(p => p.RevisionSubmissionFeedbacks)
                .HasForeignKey(d => d.RevisionPaperSubmissionId)
                .HasConstraintName("FK_RevisionSubmissionFeedback_RevisionPaperSubmissionId");

            entity.HasOne(d => d.User).WithMany(p => p.RevisionSubmissionFeedbacks)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_RevisionSubmissionFeedback_UserId");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("Role_pkey");

            entity.ToTable("Role");

            entity.Property(e => e.RoleId).HasMaxLength(50);
            entity.Property(e => e.RoleName).HasMaxLength(255);
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(e => e.RoomId).HasName("Room_pkey");

            entity.ToTable("Room");

            entity.Property(e => e.RoomId).HasMaxLength(50);
            entity.Property(e => e.DestinationId).HasMaxLength(50);
            entity.Property(e => e.DisplayName).HasMaxLength(255);
            entity.Property(e => e.Number).HasMaxLength(255);

            entity.HasOne(d => d.Destination).WithMany(p => p.Rooms)
                .HasForeignKey(d => d.DestinationId)
                .HasConstraintName("FK_Room_DestinationId");
        });

        modelBuilder.Entity<SessionChangeRequest>(entity =>
        {
            entity.HasKey(e => e.SessionChangeRequestId).HasName("SessionChangeRequest_pkey");

            entity.ToTable("SessionChangeRequest");

            entity.Property(e => e.SessionChangeRequestId).HasMaxLength(50);
            entity.Property(e => e.CustomerId).HasMaxLength(50);
            entity.Property(e => e.GlobalStatusId).HasMaxLength(50);
            entity.Property(e => e.NewConferenceSessionId).HasMaxLength(50);
            entity.Property(e => e.PaperId).HasMaxLength(50);
            entity.Property(e => e.Reason).HasMaxLength(255);
            entity.Property(e => e.RequestAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.ReviewedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.TicketId).HasMaxLength(50);

            entity.HasOne(d => d.Customer).WithMany(p => p.SessionChangeRequests)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK_SessionChangeRequest_CustomerId");

            entity.HasOne(d => d.GlobalStatus).WithMany(p => p.SessionChangeRequests)
                .HasForeignKey(d => d.GlobalStatusId)
                .HasConstraintName("FK_SessionChangeRequest_GlobalStatusId");

            entity.HasOne(d => d.NewConferenceSession).WithMany(p => p.SessionChangeRequests)
                .HasForeignKey(d => d.NewConferenceSessionId)
                .HasConstraintName("FK_SessionChangeRequest_NewConferenceSessionId");

            entity.HasOne(d => d.Paper).WithMany(p => p.SessionChangeRequests)
                .HasForeignKey(d => d.PaperId)
                .HasConstraintName("FK_SessionChangeRequest_PaperId");

            entity.HasOne(d => d.Ticket).WithMany(p => p.SessionChangeRequests)
                .HasForeignKey(d => d.TicketId)
                .HasConstraintName("FK_SessionChangeRequest_TicketId");
        });

        modelBuilder.Entity<Speaker>(entity =>
        {
            entity.HasKey(e => e.SpeakerId).HasName("Speaker_pkey");

            entity.ToTable("Speaker");

            entity.Property(e => e.SpeakerId).HasMaxLength(50);
            entity.Property(e => e.ConferenceSessionId).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(255);

            entity.HasOne(d => d.ConferenceSession).WithMany(p => p.Speakers)
                .HasForeignKey(d => d.ConferenceSessionId)
                .HasConstraintName("FK_Speaker_ConferenceSessionId");
        });

        modelBuilder.Entity<Sponsor>(entity =>
        {
            entity.HasKey(e => e.SponsorId).HasName("Sponsor_pkey");

            entity.ToTable("Sponsor");

            entity.Property(e => e.SponsorId).HasMaxLength(50);
            entity.Property(e => e.ConferenceId).HasMaxLength(50);
            entity.Property(e => e.ImageUrl).HasColumnName("ImageURL");
            entity.Property(e => e.Name).HasMaxLength(255);

            entity.HasOne(d => d.Conference).WithMany(p => p.Sponsors)
                .HasForeignKey(d => d.ConferenceId)
                .HasConstraintName("FK_Sponsor_ConferenceId");
        });

        modelBuilder.Entity<TechnicalConferenceDetail>(entity =>
        {
            entity.HasKey(e => e.ConferenceId).HasName("TechnicalConferenceDetail_pkey");

            entity.ToTable("TechnicalConferenceDetail");

            entity.Property(e => e.ConferenceId).HasMaxLength(50);
            entity.Property(e => e.TargetAudience).HasMaxLength(255);

            entity.HasOne(d => d.Conference).WithOne(p => p.TechnicalConferenceDetail)
                .HasForeignKey<TechnicalConferenceDetail>(d => d.ConferenceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TechnicalConferenceDetail_ConferenceId");
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(e => e.TicketId).HasName("Ticket_pkey");

            entity.ToTable("Ticket");

            entity.Property(e => e.TicketId).HasMaxLength(50);
            entity.Property(e => e.ActualPrice).HasPrecision(10, 2);
            entity.Property(e => e.PricePhaseId).HasMaxLength(50);
            entity.Property(e => e.UserId).HasMaxLength(50);

            entity.HasOne(d => d.PricePhase).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.PricePhaseId)
                .HasConstraintName("FK_Ticket_PricePhaseId");

            entity.HasOne(d => d.User).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Ticket_UserId");
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
            entity.Property(e => e.TicketId).HasMaxLength(50);
            entity.Property(e => e.TransactionCode).HasMaxLength(255);
            entity.Property(e => e.UserId).HasMaxLength(50);

            entity.HasOne(d => d.PaymentMethod).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.PaymentMethodId)
                .HasConstraintName("FK_Transaction_PaymentMethodId");

            entity.HasOne(d => d.Ticket).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.TicketId)
                .HasConstraintName("FK_Transaction_TicketId");

            entity.HasOne(d => d.User).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Transaction_UserId");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("User_pkey");

            entity.ToTable("User");

            entity.Property(e => e.UserId).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.CurrentSuspendedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FullName).HasMaxLength(50);
            entity.Property(e => e.Gender).HasMaxLength(50);
            entity.Property(e => e.LastLogin).HasColumnType("timestamp without time zone");
            entity.Property(e => e.LoginProvider).HasMaxLength(50);
            entity.Property(e => e.PasswordResetToken).HasMaxLength(255);
            entity.Property(e => e.PasswordResetTokenExpiry).HasColumnType("timestamp without time zone");
            entity.Property(e => e.PhoneNumber).HasMaxLength(50);
            entity.Property(e => e.VerificationToken).HasMaxLength(255);
            entity.Property(e => e.VerificationTokenExpiry).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<UserCheckIn>(entity =>
        {
            entity.HasKey(e => e.UserCheckinId).HasName("UserCheckIn_pkey");

            entity.ToTable("UserCheckIn");

            entity.Property(e => e.UserCheckinId).HasMaxLength(50);
            entity.Property(e => e.CheckInTime).HasColumnType("timestamp without time zone");
            entity.Property(e => e.CheckinStatusId).HasMaxLength(50);
            entity.Property(e => e.ConferenceSessionId).HasMaxLength(50);
            entity.Property(e => e.TicketId).HasMaxLength(50);
            entity.Property(e => e.UserId).HasMaxLength(50);

            entity.HasOne(d => d.CheckinStatus).WithMany(p => p.UserCheckIns)
                .HasForeignKey(d => d.CheckinStatusId)
                .HasConstraintName("FK_UserCheckIn_CheckinStatusId");

            entity.HasOne(d => d.ConferenceSession).WithMany(p => p.UserCheckIns)
                .HasForeignKey(d => d.ConferenceSessionId)
                .HasConstraintName("FK_UserCheckIn_ConferenceSessionId");

            entity.HasOne(d => d.Ticket).WithMany(p => p.UserCheckIns)
                .HasForeignKey(d => d.TicketId)
                .HasConstraintName("FK_UserCheckIn_TicketId");

            entity.HasOne(d => d.User).WithMany(p => p.UserCheckIns)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_UserCheckIn_UserId");
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
                .HasConstraintName("FK_UserRole_RoleId");

            entity.HasOne(d => d.User).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserRole_UserId");
        });

        modelBuilder.Entity<UserSuspendHistory>(entity =>
        {
            entity.HasKey(e => e.SuspendId).HasName("UserSuspendHistory_pkey");

            entity.ToTable("UserSuspendHistory");

            entity.Property(e => e.SuspendId).HasMaxLength(50);
            entity.Property(e => e.ResumedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.SuspendType).HasMaxLength(255);
            entity.Property(e => e.SuspendedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.UserId).HasMaxLength(50);

            entity.HasOne(d => d.User).WithMany(p => p.UserSuspendHistories)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<WaitListStatus>(entity =>
        {
            entity.HasKey(e => e.WaitListStatusId).HasName("WaitListStatus_pkey");

            entity.ToTable("WaitListStatus");

            entity.Property(e => e.WaitListStatusId).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(255);
        });

        modelBuilder.Entity<Wallet>(entity =>
        {
            entity.HasKey(e => e.WalletId).HasName("Wallet_pkey");

            entity.ToTable("Wallet");

            entity.HasIndex(e => e.UserId, "Wallet_UserId_key").IsUnique();

            entity.Property(e => e.WalletId).HasMaxLength(50);
            entity.Property(e => e.Balance).HasPrecision(18, 2);
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.UserId).HasMaxLength(50);

            entity.HasOne(d => d.User).WithOne(p => p.Wallet)
                .HasForeignKey<Wallet>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<WalletTransaction>(entity =>
        {
            entity.HasKey(e => e.WalletTransactionId).HasName("WalletTransaction_pkey");

            entity.ToTable("WalletTransaction");

            entity.Property(e => e.WalletTransactionId).HasMaxLength(50);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.TransactionType).HasMaxLength(50);
            entity.Property(e => e.WalletId).HasMaxLength(50);

            entity.HasOne(d => d.Wallet).WithMany(p => p.WalletTransactions).HasForeignKey(d => d.WalletId);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
