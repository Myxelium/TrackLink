using api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace api.Data
{
    public partial class DatabaseContext : DbContext
    {
        public DatabaseContext()
        {
        }

        public DatabaseContext(DbContextOptions<DatabaseContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Band> Bands { get; set; }

        public virtual DbSet<BandMember> BandMembers { get; set; }

        public virtual DbSet<BandInvite> BandInvites { get; set; }

        public virtual DbSet<GoogleAccount> GoogleAccounts { get; set; }

        public virtual DbSet<Member> Members { get; set; }

        public virtual DbSet<MemberRole> MemberRoles { get; set; }

        public virtual DbSet<Role> Roles { get; set; }

        public virtual DbSet<Song> Songs { get; set; }

        public virtual DbSet<SongIdentifier> SongIdentifiers { get; set; }

        public virtual DbSet<Vote> Votes { get; set; }

        public virtual DbSet<Album> Albums { get; set; }

        public virtual DbSet<AlbumTrack> AlbumTracks { get; set; }

        public virtual DbSet<AlbumProposal> AlbumProposals { get; set; }

        public virtual DbSet<AlbumProposalDecision> AlbumProposalDecisions { get; set; }

        public virtual DbSet<AlbumProposalReview> AlbumProposalReviews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Band>(entity =>
            {
                entity.ToTable("Band");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.Genre)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Image)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DriveFolderId)
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.DriveFolderName)
                    .HasMaxLength(200);

                entity.HasOne(d => d.OwnerMember)
                    .WithMany(p => p.OwnedBands)
                    .HasForeignKey(d => d.OwnerMemberId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Band_OwnerMember");
            });

            modelBuilder.Entity<BandMember>(entity =>
            {
                entity.ToTable("BandMember");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.HasOne(d => d.Band)
                    .WithMany(p => p.BandMembers)
                    .HasForeignKey(d => d.BandId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_BandMember_Band");

                entity.HasOne(d => d.Member)
                    .WithMany(p => p.BandMembers)
                    .HasForeignKey(d => d.MemberId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_BandMember_Member");

                entity.Property(e => e.RoleName)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasDefaultValue("member");
            });

            modelBuilder.Entity<BandInvite>(entity =>
            {
                entity.ToTable("BandInvite");

                entity.HasIndex(e => e.Code).IsUnique();

                entity.Property(e => e.Email)
                    .HasMaxLength(320)
                    .IsUnicode(false);

                entity.Property(e => e.Code)
                    .HasMaxLength(32)
                    .IsUnicode(false);

                entity.Property(e => e.RoleName)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");
                entity.Property(e => e.ExpiresAt).HasColumnType("datetime");
                entity.Property(e => e.AcceptedAt).HasColumnType("datetime");

                entity.HasOne(d => d.Band)
                    .WithMany(p => p.BandInvites)
                    .HasForeignKey(d => d.BandId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_BandInvite_Band");

                entity.HasOne(d => d.CreatedByNavigation)
                    .WithMany(p => p.CreatedInvites)
                    .HasForeignKey(d => d.CreatedBy)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_BandInvite_Member");
            });

            modelBuilder.Entity<GoogleAccount>(entity =>
            {
                entity.ToTable("GoogleAccount");
                entity.HasIndex(e => e.MemberId).IsUnique();
                entity.Property(e => e.Email).HasMaxLength(320);
                entity.Property(e => e.AccessToken).HasMaxLength(4000);
                entity.Property(e => e.RefreshToken).HasMaxLength(4000);

                entity.HasOne(d => d.Member)
                    .WithOne(p => p.GoogleAccount)
                    .HasForeignKey<GoogleAccount>(d => d.MemberId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_GoogleAccount_Member");
            });

            modelBuilder.Entity<Member>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK_Members");

                entity.ToTable("Member");

                entity.Property(e => e.Fullname)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Image)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.Email)
                    .HasMaxLength(320)
                    .IsUnicode(false);

                entity.Property(e => e.GoogleSubject)
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.HasIndex(e => e.Email);

                entity.HasIndex(e => e.GoogleSubject);

                entity.Property(e => e.Username)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<MemberRole>(entity =>
            {
                entity.ToTable("MemberRole");

                entity.HasOne(d => d.Member)
                    .WithMany(p => p.MemberRoles)
                    .HasForeignKey(d => d.MemberId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_MemberRole_Member");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.MemberRoles)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_MemberRole_Role");
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("Role");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.RoleDescription).HasMaxLength(150);

                entity.Property(e => e.RoleName).HasMaxLength(50);

                entity.HasOne(d => d.CreatedByNavigation)
                    .WithMany(p => p.Roles)
                    .HasForeignKey(d => d.CreatedBy)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Role_Member");

                entity.HasOne(d => d.CreatedForNavigation)
                    .WithMany(p => p.Roles)
                    .HasForeignKey(d => d.CreatedFor)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Role_Band");
            });

            modelBuilder.Entity<Song>(entity =>
            {
                entity.ToTable("Song");

                entity.Property(e => e.Description).HasMaxLength(500);

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.Property(e => e.ContentMd5)
                    .HasMaxLength(32)
                    .IsUnicode(false);

                entity.Property(e => e.SourceModifiedAt).HasColumnType("datetime");

                entity.Property(e => e.Url)
                    .HasMaxLength(2000)
                    .HasColumnName("URL");

                entity.HasOne(d => d.UploadedByNavigation)
                    .WithMany(p => p.Songs)
                    .HasForeignKey(d => d.UploadedBy)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Song_Member");
            });

            modelBuilder.Entity<SongIdentifier>(entity =>
            {
                entity.ToTable("SongIdentifier");

                entity.HasOne(d => d.Band)
                    .WithMany(p => p.SongIdentifiers)
                    .HasForeignKey(d => d.BandId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SongIdentifier_Band");

                entity.HasOne(d => d.Song)
                    .WithMany(p => p.SongIdentifiers)
                    .HasForeignKey(d => d.SongId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SongIdentifier_Song");
            });

            modelBuilder.Entity<Vote>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.ToTable("Vote");

                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.Kind)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.Choice)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Subject).HasMaxLength(128);

                entity.Property(e => e.Comment).HasMaxLength(50);

                entity.HasIndex(e => new { e.AlbumId, e.MemberId, e.SongId })
                    .IsUnique()
                    .HasDatabaseName("IX_Vote_Inclusion")
                    .HasFilter("[Kind] = 'inclusion' AND [AlbumId] IS NOT NULL AND [SongId] IS NOT NULL");

                entity.HasIndex(e => new { e.AlbumId, e.MemberId })
                    .IsUnique()
                    .HasDatabaseName("IX_Vote_AlbumName")
                    .HasFilter("[Kind] = 'album_name'");

                entity.HasIndex(e => new { e.AlbumId, e.MemberId, e.SongId })
                    .IsUnique()
                    .HasDatabaseName("IX_Vote_SongName")
                    .HasFilter("[Kind] = 'song_name' AND [SongId] IS NOT NULL");

                entity.HasIndex(e => new { e.AlbumId, e.MemberId, e.SongId })
                    .IsUnique()
                    .HasDatabaseName("IX_Vote_Order")
                    .HasFilter("[Kind] = 'order' AND [AlbumId] IS NOT NULL AND [SongId] IS NOT NULL");

                entity.HasIndex(e => new { e.AlbumId, e.MemberId })
                    .IsUnique()
                    .HasDatabaseName("IX_Vote_Art")
                    .HasFilter("[Kind] = 'art' AND [AlbumId] IS NOT NULL");

                entity.HasOne(d => d.Member)
                    .WithMany(p => p.Votes)
                    .HasForeignKey(d => d.MemberId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Vote_Member");

                entity.HasOne(d => d.Song)
                    .WithMany(p => p.Votes)
                    .HasForeignKey(d => d.SongId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Vote_Song");

                entity.HasOne(d => d.Album)
                    .WithMany(p => p.Votes)
                    .HasForeignKey(d => d.AlbumId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Vote_Album");
            });

            modelBuilder.Entity<Album>(entity =>
            {
                entity.ToTable("Album");

                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ApprovalRule)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasDefaultValue("all");

                entity.Property(e => e.ArtDriveFileId)
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.OrderLocked).HasDefaultValue(false);

                entity.Property(e => e.ArtLocked).HasDefaultValue(false);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.HasOne(d => d.Band)
                    .WithMany(p => p.Albums)
                    .HasForeignKey(d => d.BandId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Album_Band");

                entity.HasOne(d => d.CreatedByNavigation)
                    .WithMany(p => p.CreatedAlbums)
                    .HasForeignKey(d => d.CreatedBy)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Album_Member");
            });

            modelBuilder.Entity<AlbumTrack>(entity =>
            {
                entity.ToTable("AlbumTrack");

                entity.HasIndex(e => new { e.AlbumId, e.SongId }).IsUnique();

                entity.Property(e => e.AddedAt).HasColumnType("datetime");

                entity.HasOne(d => d.Album)
                    .WithMany(p => p.Tracks)
                    .HasForeignKey(d => d.AlbumId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_AlbumTrack_Album");

                entity.HasOne(d => d.Song)
                    .WithMany(p => p.AlbumTracks)
                    .HasForeignKey(d => d.SongId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_AlbumTrack_Song");

                entity.HasOne(d => d.Proposal)
                    .WithMany()
                    .HasForeignKey(d => d.ProposalId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_AlbumTrack_Proposal");
            });

            modelBuilder.Entity<AlbumProposal>(entity =>
            {
                entity.ToTable("AlbumProposal");

                entity.Property(e => e.Status)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");
                entity.Property(e => e.ResolvedAt).HasColumnType("datetime");

                entity.HasOne(d => d.Album)
                    .WithMany(p => p.Proposals)
                    .HasForeignKey(d => d.AlbumId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_AlbumProposal_Album");

                entity.HasOne(d => d.Song)
                    .WithMany(p => p.AlbumProposals)
                    .HasForeignKey(d => d.SongId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_AlbumProposal_Song");

                entity.HasOne(d => d.ProposedByNavigation)
                    .WithMany(p => p.AlbumProposals)
                    .HasForeignKey(d => d.ProposedBy)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_AlbumProposal_Member");
            });

            modelBuilder.Entity<AlbumProposalDecision>(entity =>
            {
                entity.ToTable("AlbumProposalDecision");

                entity.HasIndex(e => new { e.ProposalId, e.MemberId }).IsUnique();

                entity.Property(e => e.Decision)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

                entity.HasOne(d => d.Proposal)
                    .WithMany(p => p.Decisions)
                    .HasForeignKey(d => d.ProposalId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_AlbumProposalDecision_Proposal");

                entity.HasOne(d => d.Member)
                    .WithMany(p => p.AlbumProposalDecisions)
                    .HasForeignKey(d => d.MemberId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_AlbumProposalDecision_Member");
            });

            modelBuilder.Entity<AlbumProposalReview>(entity =>
            {
                entity.ToTable("AlbumProposalReview");

                entity.HasIndex(e => e.ProposalId);

                entity.Property(e => e.Body)
                    .HasMaxLength(2000);

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.HasOne(d => d.Proposal)
                    .WithMany(p => p.Reviews)
                    .HasForeignKey(d => d.ProposalId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_AlbumProposalReview_Proposal");

                entity.HasOne(d => d.Member)
                    .WithMany(p => p.AlbumProposalReviews)
                    .HasForeignKey(d => d.MemberId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_AlbumProposalReview_Member");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
