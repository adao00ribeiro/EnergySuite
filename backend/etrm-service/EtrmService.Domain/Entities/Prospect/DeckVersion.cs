using System;

namespace EtrmService.Domain.Entities.Prospect;

public class DeckVersion
{
    public Guid Id { get; private set; }
    public Guid DeckId { get; private set; }
    public int VersionNumber { get; private set; }
    public string StoragePath { get; private set; } = string.Empty;
    public string ChangeReason { get; private set; } = string.Empty; // e.g., "Initial", "Auto-adjusted after infeasibility"
    public DateTime CreatedAt { get; private set; }

    public Deck Deck { get; private set; } = null!;

    protected DeckVersion() { }

    public DeckVersion(Guid deckId, int versionNumber, string storagePath, string changeReason)
    {
        Id = Guid.NewGuid();
        DeckId = deckId;
        VersionNumber = versionNumber;
        StoragePath = storagePath;
        ChangeReason = changeReason;
        CreatedAt = DateTime.UtcNow;
    }
}
