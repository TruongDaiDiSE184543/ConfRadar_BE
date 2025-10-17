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

    public virtual DbSet<Role> Roles { get; set; }

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
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Roleid).HasName("Role_pkey");

            entity.ToTable("Role");

            entity.HasIndex(e => e.Rolename, "Role_rolename_key").IsUnique();

            entity.Property(e => e.Roleid)
                .HasMaxLength(50)
                .HasColumnName("roleid");
            entity.Property(e => e.Rolename)
                .HasMaxLength(100)
                .HasColumnName("rolename");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Userid).HasName("User_pkey");

            entity.ToTable("User");

            entity.HasIndex(e => e.Email, "User_email_key").IsUnique();

            entity.Property(e => e.Userid)
                .HasMaxLength(50)
                .HasColumnName("userid");
            entity.Property(e => e.Avatarurl)
                .HasMaxLength(255)
                .HasColumnName("avatarurl");
            entity.Property(e => e.Biodescription).HasColumnName("biodescription");
            entity.Property(e => e.Birthday).HasColumnName("birthday");
            entity.Property(e => e.Createdat)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.Fullname)
                .HasMaxLength(255)
                .HasColumnName("fullname");
            entity.Property(e => e.Gender)
                .HasMaxLength(20)
                .HasColumnName("gender");
            entity.Property(e => e.Isactive).HasColumnName("isactive");
            entity.Property(e => e.Isemailconfirmed).HasColumnName("isemailconfirmed");
            entity.Property(e => e.Lastlogin)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("lastlogin");
            entity.Property(e => e.Loginprovider)
                .HasMaxLength(50)
                .HasColumnName("loginprovider");
            entity.Property(e => e.Passwordhash)
                .HasMaxLength(500)
                .HasColumnName("passwordhash");
            entity.Property(e => e.Passwordresettoken)
                .HasMaxLength(255)
                .HasColumnName("passwordresettoken");
            entity.Property(e => e.Passwordresettokenexpiry)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("passwordresettokenexpiry");
            entity.Property(e => e.Phonenumber)
                .HasMaxLength(20)
                .HasColumnName("phonenumber");
            entity.Property(e => e.Verificationtoken)
                .HasMaxLength(255)
                .HasColumnName("verificationtoken");
            entity.Property(e => e.Verificationtokenexpiry)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("verificationtokenexpiry");
        });

        modelBuilder.Entity<UserRefreshToken>(entity =>
        {
            entity.HasKey(e => e.Tokenid).HasName("UserRefreshToken_pkey");

            entity.ToTable("UserRefreshToken");

            entity.HasIndex(e => e.Token, "UserRefreshToken_token_key").IsUnique();

            entity.Property(e => e.Tokenid)
                .HasMaxLength(50)
                .HasColumnName("tokenid");
            entity.Property(e => e.Createdat)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Expiry)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("expiry");
            entity.Property(e => e.Isrevoked).HasColumnName("isrevoked");
            entity.Property(e => e.Token)
                .HasMaxLength(500)
                .HasColumnName("token");
            entity.Property(e => e.Userid)
                .HasMaxLength(50)
                .HasColumnName("userid");

            entity.HasOne(d => d.User).WithMany(p => p.UserRefreshTokens)
                .HasForeignKey(d => d.Userid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("UserRefreshToken_userid_fkey");
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => new { e.Userid, e.Roleid }).HasName("UserRole_pkey");

            entity.ToTable("UserRole");

            entity.Property(e => e.Userid)
                .HasMaxLength(50)
                .HasColumnName("userid");
            entity.Property(e => e.Roleid)
                .HasMaxLength(50)
                .HasColumnName("roleid");
            entity.Property(e => e.Assignedat)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("assignedat");

            entity.HasOne(d => d.Role).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.Roleid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("UserRole_roleid_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.Userid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("UserRole_userid_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
