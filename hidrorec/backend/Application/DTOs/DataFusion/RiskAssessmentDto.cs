namespace HidroRec.Backend.Application.DTOs.DataFusion;

public sealed class RiskAssessmentDto
{
    public int Score { get; set; }

    public string Level { get; set; } = string.Empty;

    public decimal Reliability { get; set; }

    public decimal RainContribution { get; set; }

    public decimal TideContribution { get; set; }

    public decimal HistoryContribution { get; set; }

    public string DecisionPriority { get; set; } = string.Empty;

    public string RecommendedAction { get; set; } = string.Empty;

    public string CriticalWindow { get; set; } = string.Empty;

    public IReadOnlyCollection<string> Drivers { get; set; } = Array.Empty<string>();

    public string Summary { get; set; } = string.Empty;
}
