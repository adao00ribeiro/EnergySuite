using System.Diagnostics.Metrics;

namespace EtrmService.Domain.Metrics;

public static class EtrmMetrics
{
    public const string MeterName = "EtrmService.Metrics";

    private static readonly Meter Meter = new(MeterName, "1.0.0");

    public static readonly Counter<long> TradesProcessedCounter =
        Meter.CreateCounter<long>("energy_trades_processed_total", "Count", "Total energy trades and operations processed.");

    public static readonly Counter<long> CceeXmlGeneratedCounter =
        Meter.CreateCounter<long>("ccee_xml_generated_total", "Count", "Total CCEE XML documents generated.");

    public static readonly Counter<long> OpportunitySimulationsCounter =
        Meter.CreateCounter<long>("opportunity_simulations_total", "Count", "Total opportunity simulations executed.");
}
