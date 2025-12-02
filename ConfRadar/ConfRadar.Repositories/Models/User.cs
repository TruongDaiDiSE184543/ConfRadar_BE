using System;
using System.Collections.Generic;

namespace ConfRadar.Repositories.Models;

public partial class User
{
    public string UserId { get; set; } = null!;

    public string? Email { get; set; }

    public string? PasswordHash { get; set; }

    public string? FullName { get; set; }

    public DateOnly? BirthDay { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Gender { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsEmailConfirmed { get; set; }

    public string? LoginProvider { get; set; }

    public string? VerificationToken { get; set; }

    public DateTime? VerificationTokenExpiry { get; set; }

    public string? PasswordResetToken { get; set; }

    public DateTime? PasswordResetTokenExpiry { get; set; }

    public DateTime? LastLogin { get; set; }

    public string? AvatarUrl { get; set; }

    public string? BioDescription { get; set; }

    public string? FirebaseWebFcmToken { get; set; }

    public string? FirebaseMobileFcmToken { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? CurrentSuspendReason { get; set; }

    public DateTime? CurrentSuspendedAt { get; set; }

    public virtual ICollection<AcademicProfile> AcademicProfiles { get; set; } = new List<AcademicProfile>();

    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    public virtual ICollection<CollaboratorContract> CollaboratorContracts { get; set; } = new List<CollaboratorContract>();

    public virtual ICollection<ConferenceFeedback> ConferenceFeedbacks { get; set; } = new List<ConferenceFeedback>();

    public virtual ICollection<Conference> Conferences { get; set; } = new List<Conference>();

    public virtual ICollection<FavouriteConference> FavouriteConferences { get; set; } = new List<FavouriteConference>();

    public virtual ICollection<FullPaperReview> FullPaperReviews { get; set; } = new List<FullPaperReview>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual Organization? Organization { get; set; }

    public virtual ICollection<PaperAuthor> PaperAuthors { get; set; } = new List<PaperAuthor>();

    public virtual ICollection<PaperReviewer> PaperReviewers { get; set; } = new List<PaperReviewer>();

    public virtual ICollection<PaperWaitList> PaperWaitLists { get; set; } = new List<PaperWaitList>();

    public virtual ICollection<PresenterChangeRequest> PresenterChangeRequestNewPresenters { get; set; } = new List<PresenterChangeRequest>();

    public virtual ICollection<PresenterChangeRequest> PresenterChangeRequestRequestedBies { get; set; } = new List<PresenterChangeRequest>();

    public virtual ICollection<ReportFeedback> ReportFeedbacks { get; set; } = new List<ReportFeedback>();

    public virtual ICollection<Report> Reports { get; set; } = new List<Report>();

    public virtual ICollection<ReviewerContract> ReviewerContracts { get; set; } = new List<ReviewerContract>();

    public virtual ICollection<RevisionPaperReview> RevisionPaperReviews { get; set; } = new List<RevisionPaperReview>();

    public virtual ICollection<RevisionSubmissionFeedback> RevisionSubmissionFeedbacks { get; set; } = new List<RevisionSubmissionFeedback>();

    public virtual ICollection<SessionChangeRequest> SessionChangeRequests { get; set; } = new List<SessionChangeRequest>();

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public virtual ICollection<UserCheckIn> UserCheckIns { get; set; } = new List<UserCheckIn>();

    public virtual ICollection<UserRefreshToken> UserRefreshTokens { get; set; } = new List<UserRefreshToken>();

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    public virtual ICollection<UserSuspendHistory> UserSuspendHistories { get; set; } = new List<UserSuspendHistory>();

    public virtual Wallet? Wallet { get; set; }
}
