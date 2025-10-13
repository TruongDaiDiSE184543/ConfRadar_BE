using System;
using System.Collections.Generic;
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

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<User> Users { get; set; }

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
            entity.HasKey(e => e.Roleid).HasName("role_pkey");

            entity.ToTable("role");

            entity.HasIndex(e => e.Rolename, "role_rolename_key").IsUnique();

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
            entity.Property(e => e.Passwordhash)
                .HasMaxLength(500)
                .HasColumnName("passwordhash");
            entity.Property(e => e.Phonenumber)
                .HasMaxLength(20)
                .HasColumnName("phonenumber");
            entity.Property(e => e.Verificationtoken)
                .HasMaxLength(255)
                .HasColumnName("verificationtoken");
            entity.Property(e => e.Verificationtokenexpiry)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("verificationtokenexpiry");

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "Userrole",
                    r => r.HasOne<Role>().WithMany()
                        .HasForeignKey("Roleid")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("userrole_roleid_fkey"),
                    l => l.HasOne<User>().WithMany()
                        .HasForeignKey("Userid")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("userrole_userid_fkey"),
                    j =>
                    {
                        j.HasKey("Userid", "Roleid").HasName("userrole_pkey");
                        j.ToTable("userrole");
                        j.IndexerProperty<string>("Userid")
                            .HasMaxLength(50)
                            .HasColumnName("userid");
                        j.IndexerProperty<string>("Roleid")
                            .HasMaxLength(50)
                            .HasColumnName("roleid");
                    });
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
