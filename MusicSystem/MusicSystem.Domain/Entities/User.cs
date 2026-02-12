using System;
using System.Collections.Generic;

namespace MusicSystem.Domain.Entities;

public partial class User
{
    public Guid UserId { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? FullName { get; set; }

    public string? Avatar { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public virtual ICollection<Artist> Artists { get; set; } = new List<Artist>();

    public virtual ICollection<ListeningHistory> ListeningHistories { get; set; } = new List<ListeningHistory>();

    public virtual ICollection<Playlist> Playlists { get; set; } = new List<Playlist>();

    public virtual ICollection<Song> SongApprovedByNavigations { get; set; } = new List<Song>();

    public virtual ICollection<Song> SongCreatedByNavigations { get; set; } = new List<Song>();

    public virtual ICollection<SongLike> SongLikes { get; set; } = new List<SongLike>();

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
