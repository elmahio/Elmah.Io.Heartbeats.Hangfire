namespace Elmah.Io.Heartbeats.Hangfire.AspNetCore70.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}