namespace SupplyChain.Web.ViewModels;

/// <summary>
/// Data shown on the error page.
/// </summary>
/// <remarks>
/// The correlation identifier replaces the legacy error experience, which was a <c>MsgBox</c>
/// containing the raw ADO error description — meaningful to a developer standing at the machine,
/// and to nobody afterwards. The identifier ties what the user saw to the structured Serilog entry.
/// </remarks>
public class ErrorViewModel
{
    /// <summary>Correlation identifier for the failed request.</summary>
    public string? RequestId { get; set; }

    /// <summary>Whether there is a request identifier worth showing.</summary>
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

    /// <summary>A message safe to show the user. Never contains exception detail.</summary>
    public string Message { get; set; } = "Something went wrong while processing your request.";
}
