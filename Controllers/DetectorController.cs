using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using PhissingScaming.Models;
using System.Text.RegularExpressions;
using System.Linq;

namespace PhissingScaming.Controllers
{
    public class DetectorController : Controller
    {
        // GET: /Detector/Index
        [HttpGet]
        public IActionResult Index()
        {
            var model = new DetectorViewModel();
            return View(model);
        }

        // POST: /Detector/AnalyzeText
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AnalyzeText(DetectorViewModel model)
        {
            if (model == null) model = new DetectorViewModel();

            if (string.IsNullOrWhiteSpace(model.MessageText))
            {
                ModelState.AddModelError("MessageText", "Please paste a message to analyze.");
                return View("Index", model);
            }

            // Lowercase for simpler checks
            string text = model.MessageText.Trim();
            string lower = text.ToLowerInvariant();

            int score = 0;
            var flags = new List<string>();

            // Keywords with weights
            var keywordRules = new Dictionary<string, int>
            {
                { "urgent", 2 },
                { "immediately", 2 },
                { "verify account", 1 },
                { "account suspended", 2 },
                { "bank", 1 },
                { "otp", 3 },
                { "one time password", 3 },
                { "password", 3 },
                { "click here", 2 },
                { "congratulations", 1 },
                { "prize", 1 },
                { "lottery", 1 },
                { "refund", 1 },
                { "kyc", 1 },
                { "blocked", 2 },
                { "limited time", 2 },
                { "claim now", 2 },
                { "verify", 1 }
            };

            foreach (var kv in keywordRules)
            {
                if (lower.Contains(kv.Key))
                {
                    score += kv.Value;
                    flags.Add($"Keyword detected: \"{kv.Key}\" (+{kv.Value})");
                }
            }

            // Suspicious link checks
            if (lower.Contains("http://") || lower.Contains("https://"))
            {
                score += 2;
                flags.Add("Suspicious link detected (+2)");
            }

            // Too many capital letters (heuristic)
            int uppercaseCount = text.Count(c => char.IsUpper(c));
            if (uppercaseCount > 10)
            {
                score += 1;
                flags.Add("Many capital letters (possible urgency/attention tactic) (+1)");
            }

            // Multiple exclamation marks
            int exclaim = text.Count(c => c == '!');
            if (exclaim >= 2)
            {
                score += 1;
                flags.Add("Multiple exclamation marks (+1)");
            }

            // Requests for money/payment
            var moneyWords = new[] { "pay", "payment", "transfer", "send money", "bank transfer" };
            if (moneyWords.Any(w => lower.Contains(w)))
            {
                score += 2;
                flags.Add("Request for money/payment detected (+2)");
            }

            // OTP or password request (higher severity)
            if (lower.Contains("otp") || lower.Contains("one time password") || lower.Contains("password") || lower.Contains("pin"))
            {
                score += 3;
                flags.Add("Request for OTP/password/PIN detected (+3)");
            }

            // Build risk level
            string riskLevel = "Low Risk";
            if (score >= 6) riskLevel = "HIGH RISK";
            else if (score >= 3) riskLevel = "MEDIUM RISK";
            else riskLevel = "Low Risk";

            model.RiskScore = score;
            model.RiskLevel = riskLevel;
            model.DetectedFlags = flags;
            model.ResultMessage = "This is an awareness-based simulation and not a real scam detection system.";

            // Recommended actions (short)
            var recommendations = new List<string>
            {
                "Do not click links in suspicious messages.",
                "Do not share OTP, PIN or passwords.",
                "Verify via official app/website or call official numbers.",
                "Report the message to the service provider or cybercrime authority."
            };

            model.RecommendedActions = recommendations;

            return View("Index", model);
        }

        // POST: /Detector/AnalyzeImage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AnalyzeImage(IFormFile screenshot)
        {
            var model = new DetectorViewModel();

            if (screenshot == null)
            {
                ModelState.AddModelError("ImageError", "Please select an image to upload.");
                return View("Index", model);
            }

            // Validate extension
            var permitted = new[] { ".jpg", ".jpeg", ".png" };
            var ext = Path.GetExtension(screenshot.FileName).ToLowerInvariant();

            if (string.IsNullOrEmpty(ext) || !permitted.Contains(ext))
            {
                ModelState.AddModelError("ImageError", "Invalid file type. Only .jpg, .jpeg, .png allowed.");
                return View("Index", model);
            }

            // Validate size (<= 2MB)
            const long maxBytes = 2 * 1024 * 1024;
            if (screenshot.Length > maxBytes)
            {
                ModelState.AddModelError("ImageError", "File too large. Maximum allowed size is approximately 2 MB.");
                return View("Index", model);
            }

            // Do not save file - this project does not perform OCR or analysis.
            model.ImageValidationMessage = "Image uploaded successfully. This college-project version does not perform OCR or actual image analysis. Please review the screenshot manually using the red-flag checklist.";

            // Provide checklist items
            model.ImageChecklist = new List<string>
            {
                "Unknown sender",
                "Urgent language",
                "Suspicious URL or mismatched domain",
                "OTP/password request",
                "Payment request or bank transfer instruction",
                "Fake bank/KYC message formatting errors"
            };

            return View("Index", model);
        }
    }
}
