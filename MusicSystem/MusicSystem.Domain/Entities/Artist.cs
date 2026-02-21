using System;
using System.Collections.Generic;

namespace MusicSystem.Domain.Entities;

public partial class Artist
{
    public Guid ArtistId { get; set; }

    public string ArtistName { get; set; } = null!;

    public string? AvatarUrl { get; set; }

    public string? Biography { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<SongArtist> SongArtists { get; set; } = new List<SongArtist>();

    public virtual User? UpdatedByNavigation { get; set; }
}
