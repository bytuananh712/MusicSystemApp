using System;
using System.Collections.Generic;

namespace MusicSystem.Domain.Entities;

public partial class ListeningHistory
{
    public Guid HistoryId { get; set; }

    public Guid UserId { get; set; }

    public Guid SongId { get; set; }

    public DateTime PlayedAt { get; set; }

    public virtual Song Song { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
