using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace PhissingScaming.Models
{
    // ViewModel used by the Detector views
    public class DetectorViewModel
    {
        // Text analyzer input
        public string MessageText { get; set; }

        // Results
        public int RiskScore { get; set; }
        public string RiskLevel { get; set; }
        public List<string> DetectedFlags { get; set; } = new List<string>();
        public List<string> RecommendedActions { get; set; } = new List<string>();
        public string ResultMessage { get; set; }

        // Image checker results
        public string ImageValidationMessage { get; set; }
        public List<string> ImageChecklist { get; set; } = new List<string>();
    }
}
