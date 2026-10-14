namespace PhissingScaming.Models
{
    using System;

    // Simple Error view model used by the Error view.
    public class ErrorViewModel
    {
        public string RequestId { get; set; } = string.Empty;

        // True when RequestId is available
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
