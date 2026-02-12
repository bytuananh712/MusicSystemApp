using System;
using System.Collections.Generic;

namespace MusicSystem.Domain.Entities;

public partial class Playlist
{
    public Guid PlaylistId { get; set; }

    public Guid UserId { get; set; }

    public string Title { get; set; } = null!;

    public bool IsPublic { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<PlaylistSong> PlaylistSongs { get; set; } = new List<PlaylistSong>();

    public virtual User User { get; set; } = null!;
}
