using System;
using System.Collections.Generic;

namespace MusicSystem.Domain.Entities;

public partial class Song
{
    public Guid SongId { get; set; }

    public string Title { get; set; } = null!;

    public int Duration { get; set; }

    public string? Lyrics { get; set; }

    public string FileUrl { get; set; } = null!;

    public long? FileSize { get; set; }

    public string? Format { get; set; }

    public string? Genre { get; set; }

    public int? ReleaseYear { get; set; }

    public string? CoverImageUrl { get; set; }

    public long TotalPlays { get; set; }

    public int TotalLikes { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public virtual User? ApprovedByNavigation { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual ICollection<ListeningHistory> ListeningHistories { get; set; } = new List<ListeningHistory>();

    public virtual ICollection<PlaylistSong> PlaylistSongs { get; set; } = new List<PlaylistSong>();

    public virtual ICollection<SongArtist> SongArtists { get; set; } = new List<SongArtist>();

    public virtual ICollection<SongLike> SongLikes { get; set; } = new List<SongLike>();
}
