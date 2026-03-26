using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using MusicSystem.Domain.Entities;

namespace MusicSystem.Infrastructure.Data;

public partial class MusicStreamingDbContext : DbContext
{
    public MusicStreamingDbContext()
    {
    }

    public MusicStreamingDbContext(DbContextOptions<MusicStreamingDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Artist> Artists { get; set; }

    public virtual DbSet<ListeningHistory> ListeningHistories { get; set; }

    public virtual DbSet<Playlist> Playlists { get; set; }

    public virtual DbSet<PlaylistSong> PlaylistSongs { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Song> Songs { get; set; }

    public virtual DbSet<SongArtist> SongArtists { get; set; }

    public virtual DbSet<SongLike> SongLikes { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=TUANANH\\SQLEXPRESS;Database=MusicStreamingDB;User Id=sa;Password=123;TrustServerCertificate=true;Encrypt=false;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Artist>(entity =>
        {
            entity.HasKey(e => e.ArtistId).HasName("PK__Artists__25706B5027BE1501");

            entity.Property(e => e.ArtistId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ArtistName).HasMaxLength(200);
            entity.Property(e => e.AvatarUrl).HasMaxLength(500);
            entity.Property(e => e.Biography).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Active");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.Artists)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("FK__Artists__Updated__5EBF139D");
        });

        modelBuilder.Entity<ListeningHistory>(entity =>
        {
            entity.HasKey(e => e.HistoryId).HasName("PK__Listenin__4D7B4ABDA2525956");

            entity.ToTable("ListeningHistory", tb => tb.HasTrigger("trg_UpdateSongPlays"));

            entity.Property(e => e.HistoryId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.PlayedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Song).WithMany(p => p.ListeningHistories)
                .HasForeignKey(d => d.SongId)
                .HasConstraintName("FK__Listening__SongI__04E4BC85");

            entity.HasOne(d => d.User).WithMany(p => p.ListeningHistories)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Listening__UserI__03F0984C");
        });

        modelBuilder.Entity<Playlist>(entity =>
        {
            entity.HasKey(e => e.PlaylistId).HasName("PK__Playlist__B30167A066AC78F1");

            entity.Property(e => e.PlaylistId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsPublic).HasDefaultValue(true);
            entity.Property(e => e.Title).HasMaxLength(255);

            entity.HasOne(d => d.User).WithMany(p => p.Playlists)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Playlists__UserI__73BA3083");
        });

        modelBuilder.Entity<PlaylistSong>(entity =>
        {
            entity.HasKey(e => e.PlaylistSongId).HasName("PK__Playlist__D58F7B2E0678262A");

            entity.Property(e => e.PlaylistSongId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.AddedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Playlist).WithMany(p => p.PlaylistSongs)
                .HasForeignKey(d => d.PlaylistId)
                .HasConstraintName("FK__PlaylistS__Playl__787EE5A0");

            entity.HasOne(d => d.Song).WithMany(p => p.PlaylistSongs)
                .HasForeignKey(d => d.SongId)
                .HasConstraintName("FK__PlaylistS__SongI__797309D9");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Roles__8AFACE1AFBDF8140");

            entity.HasIndex(e => e.RoleName, "UQ__Roles__8A2B61600C5226FA").IsUnique();

            entity.Property(e => e.RoleId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.RoleName).HasMaxLength(50);
        });

        modelBuilder.Entity<Song>(entity =>
        {
            entity.HasKey(e => e.SongId).HasName("PK__Songs__12E3D697B5BD6D89");

            entity.Property(e => e.SongId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CoverImageUrl).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.FileUrl).HasMaxLength(1000);
            entity.Property(e => e.Format)
                .HasMaxLength(20)
                .HasDefaultValue("MP3");
            entity.Property(e => e.Genre).HasMaxLength(100);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Pending");
            entity.Property(e => e.Title).HasMaxLength(255);

            entity.Property(e => e.RejectReason).HasMaxLength(500);

            entity.HasOne(d => d.ApprovedByNavigation).WithMany(p => p.SongApprovedByNavigations)
                .HasForeignKey(d => d.ApprovedBy)
                .HasConstraintName("FK__Songs__ApprovedB__68487DD7");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.SongCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK__Songs__CreatedBy__6754599E");
        });

        modelBuilder.Entity<SongArtist>(entity =>
        {
            entity.HasKey(e => e.SongArtistId).HasName("PK__SongArti__131E887FF20FBFD4");

            entity.Property(e => e.SongArtistId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.IsPrimary).HasDefaultValue(true);

            entity.HasOne(d => d.Artist).WithMany(p => p.SongArtists)
                .HasForeignKey(d => d.ArtistId)
                .HasConstraintName("FK__SongArtis__Artis__6E01572D");

            entity.HasOne(d => d.Song).WithMany(p => p.SongArtists)
                .HasForeignKey(d => d.SongId)
                .HasConstraintName("FK__SongArtis__SongI__6D0D32F4");
        });

        modelBuilder.Entity<SongLike>(entity =>
        {
            entity.HasKey(e => e.SongLikeId).HasName("PK__SongLike__FA5A018894B5743E");

            entity.ToTable(tb =>
                {
                    tb.HasTrigger("trg_UpdateSongLikes_Delete");
                    tb.HasTrigger("trg_UpdateSongLikes_Insert");
                });

            entity.Property(e => e.SongLikeId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.LikedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Song).WithMany(p => p.SongLikes)
                .HasForeignKey(d => d.SongId)
                .HasConstraintName("FK__SongLikes__SongI__7F2BE32F");

            entity.HasOne(d => d.User).WithMany(p => p.SongLikes)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__SongLikes__UserI__7E37BEF6");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4C57E0E94B");

            entity.HasIndex(e => e.Email, "IX_Users_Email");

            entity.HasIndex(e => e.Username, "IX_Users_Username");

            entity.HasIndex(e => e.Username, "UQ__Users__536C85E4A164A7BA").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Users__A9D10534D12B8061").IsUnique();

            entity.Property(e => e.UserId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Avatar).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Active");
            entity.Property(e => e.Username).HasMaxLength(50);
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => e.UserRoleId).HasName("PK__UserRole__3D978A35391441BE");

            entity.HasIndex(e => new { e.UserId, e.RoleId }, "UQ_UserRoles").IsUnique();

            entity.Property(e => e.UserRoleId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.AssignedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Role).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK__UserRoles__RoleI__59063A47");

            entity.HasOne(d => d.User).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__UserRoles__UserI__5812160E");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
