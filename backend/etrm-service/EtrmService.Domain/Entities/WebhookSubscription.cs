using System;

namespace EtrmService.Domain.Entities;

public class WebhookSubscription
{
    public Guid Id { get; private set; }
    public Guid CompanyId { get; private set; }
    public string Url { get; private set; } = string.Empty;
    public string SecretKey { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    protected WebhookSubscription() { }

    public WebhookSubscription(Guid companyId, string url, string secretKey)
    {
        Id = Guid.NewGuid();
        CompanyId = companyId;
        Url = url;
        SecretKey = secretKey;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateUrl(string url, string secretKey)
    {
        Url = url;
        SecretKey = secretKey;
        UpdatedAt = DateTime.UtcNow;
    }
}
