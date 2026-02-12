using System;
using System.Collections.Generic;

namespace MusicSystem.Domain.Entities;

public partial class PlaylistSong
{
    public Guid PlaylistSongId { get; set; }

    public Guid PlaylistId { get; set; }

    public Guid SongId { get; set; }

    public DateTime AddedAt { get; set; }

    public virtual Playlist Playlist { get; set; } = null!;

    public virtual Song Song { get; set; } = null!;
}
