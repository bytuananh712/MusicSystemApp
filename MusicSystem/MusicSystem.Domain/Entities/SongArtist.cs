using System;
using System.Collections.Generic;

namespace MusicSystem.Domain.Entities;

public partial class SongArtist
{
    public Guid SongArtistId { get; set; }

    public Guid SongId { get; set; }

    public Guid ArtistId { get; set; }

    public bool IsPrimary { get; set; }

    public virtual Artist Artist { get; set; } = null!;

    public virtual Song Song { get; set; } = null!;
}
