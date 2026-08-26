using System;
using EtrmService.Domain.Enums;

namespace EtrmService.Domain.Entities;

public class CustomScenario : PrecipitationScenario
{
    public string? UploadUrl { get; private set; }
    public string? BlendConfig { get; private set; }

    protected CustomScenario() : base() { }

    public CustomScenario(string name, DateTime referenceDate, int horizonDays, string? uploadUrl = null, string? blendConfig = null) 
        : base(name, ScenarioSource.Custom, referenceDate, horizonDays)
    {
        UploadUrl = uploadUrl;
        BlendConfig = blendConfig;
    }

    public void UpdateUploadUrl(string uploadUrl)
    {
        UploadUrl = uploadUrl;
    }

    public void UpdateBlendConfig(string blendConfig)
    {
        BlendConfig = blendConfig;
    }
}
