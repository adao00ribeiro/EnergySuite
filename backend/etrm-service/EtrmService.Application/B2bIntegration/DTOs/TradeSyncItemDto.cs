using System;

namespace EtrmService.Application.B2bIntegration.DTOs;

/// <summary>
/// Contrato do payload de trades pendentes exposto pela CCEE (GET {base}/trades/sync?status=pending).
/// Contrato presumido (proxy de integração externa), mapeado campo a campo:
///   {
///     "externalId":         string  (id real do trade na fonte externa — NUNCA Guid.NewGuid),
///     "ticketId":           string? (referência de chamado/trade na fonte externa, se houver),
///     "volumeMwMed":        number  (volume médio em MWmed),
///     "price":              number  (preço em R$/MWh),
///     "startDate":          "yyyy-MM-ddTHH:mm:ssZ",
///     "endDate":            "yyyy-MM-ddTHH:mm:ssZ",
///     "counterpartyCode":   string  (CceeCode/CceeAcronym da contraparte cadastrada em Companies),
///     "type":               "PURCHASE" | "SALE"
///   }
/// </summary>
public class TradeSyncItemDto
{
    public string ExternalId { get; set; } = string.Empty;
    public string? TicketId { get; set; }
    public decimal VolumeMwMed { get; set; }
    public decimal Price { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string CounterpartyCode { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}