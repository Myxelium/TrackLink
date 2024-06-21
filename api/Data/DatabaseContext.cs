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

        public virtual DbSet<Member> Members { get; set; }

        public virtual DbSet<MemberRole> MemberRoles { get; set; }

        public virtual DbSet<Role> Roles { get; set; }

        public virtual DbSet<Song> Songs { get; set; }

        public virtual DbSet<SongIdentifier> SongIdentifiers { get; set; }

        public virtual DbSet<Vote> Votes { get; set; }

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
            });

            modelBuilder.Entity<BandMember>(entity =>
            {
                entity
                    .HasNoKey()
                    .ToTable("BandMember");

                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.HasOne(d => d.Member).WithMany()
                    .HasForeignKey(d => d.MemberId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_BandMember_Band");

                entity.HasOne(d => d.MemberNavigation).WithMany()
                    .HasForeignKey(d => d.MemberId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_BandMember_Member");
            });

            modelBuilder.Entity<Member>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK_Members");

                entity.ToTable("Member");

                entity.Property(e => e.Fullname)
                    .HasMaxLength(50)
                    .IsUnicode(false);
                
                entity.Property(e => e.Image)
                    .HasMaxLength(100)
                    .IsUnicode(false);
                
                entity.Property(e => e.Username)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<MemberRole>(entity =>
            {
                entity.ToTable("MemberRole");

                entity.HasOne(d => d.Member).WithMany(p => p.MemberRoles)
                    .HasForeignKey(d => d.MemberId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_MemberRole_Member");

                entity.HasOne(d => d.Role).WithMany(p => p.InverseRole)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_MemberRole_MemberRole");
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("Role");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");
                
                entity.Property(e => e.RoleDescription).HasMaxLength(150);
                
                entity.Property(e => e.RoleName).HasMaxLength(50);

                entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Roles)
                    .HasForeignKey(d => d.CreatedBy)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Role_Member");

                entity.HasOne(d => d.CreatedForNavigation).WithMany(p => p.Roles)
                    .HasForeignKey(d => d.CreatedFor)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Role_Band");
            });

            modelBuilder.Entity<Song>(entity =>
            {
                entity.ToTable("Song");

                entity.Property(e => e.Description).HasMaxLength(500);
                
                entity.Property(e => e.Name).HasMaxLength(50);
                
                entity.Property(e => e.Url)
                    .HasMaxLength(100)
                    .HasColumnName("URL");

                entity.HasOne(d => d.UploadedByNavigation).WithMany(p => p.Songs)
                    .HasForeignKey(d => d.UploadedBy)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Song_Member");
            });

            modelBuilder.Entity<SongIdentifier>(entity => { entity.ToTable("SongIdentifier"); });

            modelBuilder.Entity<Vote>(entity =>
            {
                entity.HasKey(e => new { e.Id, e.MemberId, e.SongId });

                entity.ToTable("Vote");

                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                
                entity.Property(e => e.Comment).HasMaxLength(50);

                entity.HasOne(d => d.Member).WithMany(p => p.Votes)
                    .HasForeignKey(d => d.MemberId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Vote_Member");

                entity.HasOne(d => d.Song).WithMany(p => p.Votes)
                    .HasForeignKey(d => d.SongId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Vote_Song");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}