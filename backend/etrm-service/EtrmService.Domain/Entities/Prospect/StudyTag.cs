using System;

namespace EtrmService.Domain.Entities.Prospect;

public class StudyTag
{
    public Guid Id { get; private set; }
    public Guid StudyId { get; private set; }
    public string Name { get; private set; }
    public string ColorHex { get; private set; }

    public Study Study { get; private set; }

    protected StudyTag() { }

    public StudyTag(Guid studyId, string name, string colorHex)
    {
        Id = Guid.NewGuid();
        StudyId = studyId;
        Name = name;
        ColorHex = colorHex;
    }
}
