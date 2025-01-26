// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.EntityFrameworkCore;
using GitClub.Database.Models;

namespace GitClub.Database
{
    /// <summary>
    /// A <see cref="DbContext"/> to query the database.
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="options">Options to configure the base <see cref="DbContext"/></param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Gets or sets the Users.
        /// </summary>
        public DbSet<User> Users { get; set; } = null!;

        /// <summary>
        /// Gets or sets the Issues.
        /// </summary>
        public DbSet<Issue> Issues { get; set; } = null!;

        /// <summary>
        /// Gets or sets the Organizations.
        /// </summary>
        public DbSet<Organization> Organizations { get; set; } = null!;
        /// <summary>
        /// Gets or sets the Repositories.
        /// </summary>
        public DbSet<Repository> Repositories { get; set; } = null!;

        /// <summary>
        /// Gets or sets the Teams.
        /// </summary>
        public DbSet<Team> Teams { get; set; } = null!;

        /// <summary>
        /// Gets or sets the Outbox Events.
        /// </summary>
        public DbSet<OutboxEvent> OutboxEvents { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Sequences
            modelBuilder.HasSequence<int>("outbox_event_seq", schema: "gitclub")
                .StartsAt(38187)
                .IncrementsBy(1);

            modelBuilder.HasSequence<int>("organization_seq", schema: "gitclub")
                .StartsAt(38187)
                .IncrementsBy(1);

            modelBuilder.HasSequence<int>("team_seq", schema: "gitclub")
                .StartsAt(38187)
                .IncrementsBy(1);

            modelBuilder.HasSequence<int>("user_seq", schema: "gitclub")
                .StartsAt(38187)
                .IncrementsBy(1);

            modelBuilder.HasSequence<int>("issue_seq", schema: "gitclub")
                .StartsAt(38187)
                .IncrementsBy(1);

            modelBuilder.HasSequence<int>("repository_seq", schema: "gitclub")
                .StartsAt(38187)
                .IncrementsBy(1);

            // Tables
            modelBuilder.Entity<OutboxEvent>(entity =>
            {
                entity.ToTable("outbox_event", "gitclub");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnType("INT")
                    .HasColumnName("outbox_event_id")
                    .UseHiLo("user_seq", "gitclub")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.CorrelationId1)
                    .HasColumnType("varchar(2000)")
                    .HasColumnName("correlation_id_1")
                    .HasMaxLength(2000)
                    .IsRequired(false);

                entity.Property(e => e.CorrelationId2)
                    .HasColumnType("varchar(2000)")
                    .HasColumnName("correlation_id_2")
                    .HasMaxLength(2000)
                    .IsRequired(false);

                entity.Property(e => e.CorrelationId3)
                    .HasColumnType("varchar(2000)")
                    .HasColumnName("correlation_id_3")
                    .HasMaxLength(2000)
                    .IsRequired(false);

                entity.Property(e => e.CorrelationId4)
                    .HasColumnType("varchar(2000)")
                    .HasColumnName("correlation_id_4")
                    .HasMaxLength(2000)
                    .IsRequired(false);

                entity.Property(e => e.EventTime)
                    .HasColumnType("timestamptz")
                    .HasColumnName("event_time")
                    .IsRequired(true);

                entity.Property(e => e.EventType)
                    .HasColumnType("varchar(2000)")
                    .HasColumnName("event_type")
                    .HasMaxLength(2000)
                    .IsRequired(true);

                entity.Property(e => e.EventSource)
                    .HasColumnType("varchar(2000)")
                    .HasColumnName("event_source")
                    .HasMaxLength(2000)
                    .IsRequired(true);

                entity.Property(e => e.Payload)
                    .HasColumnType("jsonb")
                    .HasColumnName("payload")
                    .HasMaxLength(2000)
                    .IsRequired(true);

                entity.Property(e => e.RowVersion)
                    .HasColumnType("xid")
                    .HasColumnName("xmin")
                    .IsRowVersion()
                    .IsConcurrencyToken()
                    .IsRequired(false)
                    .ValueGeneratedOnAddOrUpdate();

                entity.Property(e => e.LastEditedBy)
                    .HasColumnType("integer")
                    .HasColumnName("last_edited_by")
                    .IsRequired(true);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("user", "gitclub");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnType("INT")
                    .HasColumnName("user_id")
                    .UseHiLo("user_seq", "gitclub")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Email)
                    .HasColumnType("varchar(2000)")
                    .HasColumnName("email")
                    .HasMaxLength(2000)
                    .IsRequired(true);

                entity.Property(e => e.PreferredName)
                    .HasColumnType("varchar(2000)")
                    .HasColumnName("preferred_name")
                    .HasMaxLength(2000)
                    .IsRequired(true);

                entity.Property(e => e.RowVersion)
                    .HasColumnType("xid")
                    .HasColumnName("xmin")
                    .IsRowVersion()
                    .IsConcurrencyToken()
                    .IsRequired(false)
                    .ValueGeneratedOnAddOrUpdate();

                entity.Property(e => e.LastEditedBy)
                    .HasColumnType("integer")
                    .HasColumnName("last_edited_by")
                    .IsRequired(true);
            });

            modelBuilder.Entity<Issue>(entity =>
            {
                entity.ToTable("issue", "gitclub");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnType("integer")
                    .HasColumnName("issue_id")
                    .UseHiLo("issue_seq", "gitclub")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Title)
                    .HasColumnType("varchar(2000)")
                    .HasColumnName("title")
                    .HasMaxLength(2000)
                    .IsRequired(true);

                entity.Property(e => e.Content)
                    .HasColumnType("text")
                    .HasColumnName("content")
                    .HasMaxLength(2000)
                    .IsRequired(true);

                entity.Property(e => e.Closed)
                    .HasColumnType("boolean")
                    .HasColumnName("closed")
                    .IsRequired(true);

                entity.Property(e => e.RepositoryId)
                    .HasColumnType("integer")
                    .HasColumnName("repository_id")
                    .IsRequired(true);

                entity.Property(e => e.CreatedBy)
                    .HasColumnType("integer")
                    .HasColumnName("created_by")
                    .IsRequired(true);

                entity.Property(e => e.RowVersion)
                    .HasColumnType("xid")
                    .HasColumnName("xmin")
                    .IsRowVersion()
                    .IsConcurrencyToken()
                    .IsRequired(false)
                    .ValueGeneratedOnAddOrUpdate();

                entity.Property(e => e.LastEditedBy)
                    .HasColumnType("integer")
                    .HasColumnName("last_edited_by")
                    .IsRequired(true);
            });

            modelBuilder.Entity<Organization>(entity =>
            {
                entity.ToTable("organization", "gitclub");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnType("integer")
                    .HasColumnName("organization_id")
                    .UseHiLo("organization_seq", "gitclub")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Name)
                    .HasColumnType("varchar(255)")
                    .HasColumnName("name")
                    .IsRequired(true);

                entity.Property(e => e.BillingAddress)
                    .HasColumnType("text")
                    .HasColumnName("billing_address")
                    .IsRequired(false);

                entity.Property(e => e.RowVersion)
                    .HasColumnType("xid")
                    .HasColumnName("xmin")
                    .IsRowVersion()
                    .IsConcurrencyToken()
                    .IsRequired(false)
                    .ValueGeneratedOnAddOrUpdate();

                entity.Property(e => e.LastEditedBy)
                    .HasColumnType("integer")
                    .HasColumnName("last_edited_by")
                    .IsRequired(true);
            });

            modelBuilder.Entity<Repository>(entity =>
            {
                entity.ToTable("repository", "gitclub");

                entity.HasKey(e => e.Id);

                entity.Property(x => x.Id)
                    .HasColumnType("integer")
                    .HasColumnName("repository_id")
                    .UseHiLo("repository_seq", "gitclub")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Name)
                    .HasColumnType("varchar(255)")
                    .HasColumnName("name")
                    .IsRequired(true);

                entity.Property(e => e.OrganizationId)
                    .HasColumnType("integer")
                    .HasColumnName("organization_id")
                    .IsRequired(true);

                entity.Property(e => e.RowVersion)
                    .HasColumnType("xid")
                    .HasColumnName("xmin")
                    .IsRowVersion()
                    .IsConcurrencyToken()
                    .IsRequired(false)
                    .ValueGeneratedOnAddOrUpdate();

                entity.Property(e => e.LastEditedBy)
                    .HasColumnType("integer")
                    .HasColumnName("last_edited_by")
                    .IsRequired(true);
            });

            modelBuilder.Entity<Team>(entity =>
            {
                entity.ToTable("team", "gitclub");

                entity.HasKey(e => e.Id);

                entity.Property(x => x.Id)
                    .HasColumnType("integer")
                    .HasColumnName("team_id")
                    .UseHiLo("team_seq", "gitclub")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Name)
                    .HasColumnType("varchar(255)")
                    .HasColumnName("name")
                    .IsRequired(true);

                entity.Property(e => e.OrganizationId)
                    .HasColumnType("integer")
                    .HasColumnName("organization_id")
                    .IsRequired(true);

                entity.Property(e => e.RowVersion)
                    .HasColumnType("xid")
                    .HasColumnName("xmin")
                    .IsRowVersion()
                    .IsConcurrencyToken()
                    .IsRequired(false)
                    .ValueGeneratedOnAddOrUpdate();

                entity.Property(e => e.LastEditedBy)
                    .HasColumnType("integer")
                    .HasColumnName("last_edited_by")
                    .IsRequired(true);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
