using System;
using System.Collections.Generic;
using EtrmService.Domain.Enums;

namespace EtrmService.Domain.Entities.Prospect;

public class Deck
{
    public Guid Id { get; private set; }
    public Guid StudyId { get; private set; }
    public string Model { get; private set; } // e.g., DECOMP, NEWAVE
    public DateTime Period { get; private set; } // The base month/week for this deck
    public int SequenceOrder { get; private set; } // To define execution sequence
    public DeckState State { get; private set; }
    public DateTime CreatedAt { get; private set; }
    
    public Study Study { get; private set; }
    public ICollection<DeckVersion> Versions { get; private set; }

    protected Deck() 
    {
        Versions = new List<DeckVersion>();
    }

    public Deck(Guid studyId, string model, DateTime period, int sequenceOrder) : this()
    {
        Id = Guid.NewGuid();
        StudyId = studyId;
        Model = model;
        Period = period;
        SequenceOrder = sequenceOrder;
        State = DeckState.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public void ChangeState(DeckState state)
    {
        State = state;
    }
}
