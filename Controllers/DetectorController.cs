using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PhissingScaming.Models;

namespace PhissingScaming.Controllers
{
    public class DetectorController : Controller
    {
        // GET: /Detector
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
            if (model == null || string.IsNullOrWhiteSpace(model.MessageText))
            {
                ModelState.AddModelError("MessageText", "Please paste a message to analyze.");
                return View("Index", model);
            }

            // Lowercase for simpler checks
            string text = model.MessageText.Trim();
            string lower = text.ToLowerInvariant();

            int score = 0;
            var flags = new List<string>();

            // Expanded keywords and phishing trigger phrases with weights
            var keywordRules = new Dictionary<string, int>
            {
                // Urgency, pressure, and threats
                { "urgent", 2 },
                { "immediately", 2 },
                { "action required", 2 },
                { "immediate action required", 3 },
                { "within 24 hours", 2 },
                { "within 12 hours", 2 },
                { "last warning", 3 },
                { "final notice", 3 },
                { "deadline", 2 },
                { "limited time", 2 },
                { "time sensitive", 2 },
                { "expires today", 2 },
                { "act now", 2 },
                { "attention required", 2 },

                // Account alerts & suspension threats
                { "account suspended", 3 },
                { "account locked", 3 },
                { "account blocked", 3 },
                { "blocked", 2 },
                { "temporary suspension", 2 },
                { "unusual activity", 2 },
                { "suspicious activity", 2 },
                { "unauthorized access", 3 },
                { "security alert", 2 },
                { "compromised", 2 },
                { "reactivate account", 2 },
                { "re-activate", 2 },
                { "restore access", 2 },
                { "access restricted", 2 },

                // Verification & KYC / Identity theft
                { "verify account", 2 },
                { "verify identity", 2 },
                { "verification required", 2 },
                { "verify", 1 },
                { "kyc", 2 },
                { "kyc update", 3 },
                { "kyc verification", 3 },
                { "update kyc", 3 },
                { "pan card", 2 },
                { "aadhaar", 2 },
                { "social security", 3 },
                { "ssn", 3 },
                { "confirm your details", 2 },
                { "validate credentials", 2 },

                // Credentials / Secrets theft
                { "otp", 3 },
                { "one time password", 3 },
                { "password", 3 },
                { "passcode", 3 },
                { "pin", 3 },
                { "cvv", 3 },
                { "cvv2", 3 },
                { "security code", 3 },
                { "login credentials", 3 },
                { "reset password", 2 },
                { "change password", 2 },

                // Banking, Financial, Taxes, & Delivery scams
                { "bank", 1 },
                { "netbanking", 2 },
                { "credit card", 2 },
                { "debit card", 2 },
                { "wire transfer", 2 },
                { "bank transfer", 2 },
                { "billing problem", 2 },
                { "unpaid invoice", 2 },
                { "overdue payment", 2 },
                { "refund", 2 },
                { "tax refund", 3 },
                { "income tax refund", 3 },
                { "package delivery", 2 },
                { "parcel suspended", 2 },
                { "customs fee", 2 },
                { "delivery failed", 2 },

                // Lures, Prizes, Crypto, & Lotteries
                { "congratulations", 1 },
                { "prize", 2 },
                { "lottery", 3 },
                { "winner", 2 },
                { "you have won", 3 },
                { "claim now", 2 },
                { "claim reward", 2 },
                { "cash reward", 2 },
                { "exclusive offer", 1 },
                { "free gift", 2 },
                { "bonus reward", 2 },
                { "crypto gift", 3 },
                { "airdrop", 2 },

                // Call to actions / Deceptive links
                { "click here", 2 },
                { "tap here", 2 },
                { "click the link", 2 },
                { "open attachment", 2 },
                { "download attachment", 2 },
                { "log in here", 2 },
                { "signin link", 2 }
            };

            foreach (var kv in keywordRules)
            {
                if (lower.Contains(kv.Key))
                {
                    score += kv.Value;
                    flags.Add($"Keyword/Trigger detected: \"{kv.Key}\" (+{kv.Value})");
                }
            }

            // Suspicious link and shortener checks
            var shorteners = new[] { "bit.ly", "tinyurl.com", "t.co", "is.gd", "goo.gl", "cutt.ly", "ow.ly", "rb.gy" };
            if (lower.Contains("http://") || lower.Contains("https://") || lower.Contains("www."))
            {
                score += 2;
                flags.Add("Link/URL detected (+2)");

                if (shorteners.Any(s => lower.Contains(s)))
                {
                    score += 2;
                    flags.Add("Obfuscated or shortened link detected (+2)");
                }
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
            var moneyWords = new[] { "pay", "payment", "transfer", "send money", "bank transfer", "wire money", "crypto", "bitcoin", "gift card" };
            if (moneyWords.Any(w => lower.Contains(w)))
            {
                score += 2;
                flags.Add("Request for money/payment/transfer detected (+2)");
            }

            // OTP or password request (higher severity)
            if (lower.Contains("otp") || lower.Contains("one time password") || lower.Contains("password") || lower.Contains("pin") || lower.Contains("cvv"))
            {
                score += 3;
                flags.Add("Request for sensitive credentials (OTP/password/PIN/CVV) detected (+3)");
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
