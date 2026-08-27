using System;
using System.Collections.Generic;
using EtrmService.Domain.Enums;

namespace EtrmService.Domain.Entities.Prospect;

public class Study
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string Model { get; private set; } // e.g., NEWAVE, DECOMP
    public DateTime StartDate { get; private set; }
    public int HorizonMonths { get; private set; }
    public StudyState State { get; private set; }
    public Guid CreatedBy { get; private set; }
    public Guid TenantId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public ICollection<StudyTag> Tags { get; private set; }
    public ICollection<StudyFile> Files { get; private set; }

    protected Study()
    {
        Tags = new List<StudyTag>();
        Files = new List<StudyFile>();
    }

    public Study(string name, string description, string model, DateTime startDate, int horizonMonths, Guid createdBy, Guid tenantId) : this()
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        Model = model;
        StartDate = startDate;
        HorizonMonths = horizonMonths;
        State = StudyState.Created;
        CreatedBy = createdBy;
        TenantId = tenantId;
        CreatedAt = DateTime.UtcNow;
    }

    public void ChangeState(StudyState newState)
    {
        State = newState;
        UpdatedAt = DateTime.UtcNow;
    }
}
