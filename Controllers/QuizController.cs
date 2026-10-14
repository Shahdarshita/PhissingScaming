using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using PhissingScaming.Models;

namespace PhissingScaming.Controllers
{
    public class QuizController : Controller
    {
        // Build the quiz questions (simple in-memory list for each request)
        private List<QuizQuestion> GetQuestions()
        {
            return new List<QuizQuestion>
            {
                new QuizQuestion
                {
                    Id = 1,
                    Question = "You receive an SMS saying your bank account will be blocked in 10 minutes unless you click a link. What should you do?",
                    Options = new List<string>
                    {
                        "Click the link immediately to prevent blocking",
                        "Ignore and verify via official bank app/website or call official number",
                        "Reply with your OTP to confirm identity",
                        "Share the message with friends"
                    },
                    CorrectOptionIndex = 1,
                    Explanation = "Banks will not ask for OTP via SMS links. Verify using official channels."
                },
                new QuizQuestion
                {
                    Id = 2,
                    Question = "An unknown person asks for your OTP. Is this safe?",
                    Options = new List<string>
                    {
                        "Yes, if they say it's urgent",
                        "No, never share OTP with anyone",
                        "Only share the last two digits",
                        "Ask them why you need to share"
                    },
                    CorrectOptionIndex = 1,
                    Explanation = "OTP is private; sharing it gives access to your accounts."
                },
                new QuizQuestion
                {
                    Id = 3,
                    Question = "An email contains a link whose domain looks different from the official company website. What could this indicate?",
                    Options = new List<string>
                    {
                        "A legitimate internal redirect",
                        "Possible phishing (spoofed domain)",
                        "Automatic safe tracking",
                        "It's always safe"
                    },
                    CorrectOptionIndex = 1,
                    Explanation = "Mismatch in domain is a common phishing sign."
                },
                new QuizQuestion
                {
                    Id = 4,
                    Question = "Someone calls claiming to be from technical support and asks you to install remote-access software. What should you do?",
                    Options = new List<string>
                    {
                        "Install and give access immediately",
                        "Refuse and verify caller through official support channels",
                        "Give temporary access while watching",
                        "Share your account password"
                    },
                    CorrectOptionIndex = 1,
                    Explanation = "Do not give remote access unless verified through official support workflows."
                },
                new QuizQuestion
                {
                    Id = 5,
                    Question = "You receive a QR code asking you to scan it to receive a refund. What should you check first?",
                    Options = new List<string>
                    {
                        "Scan immediately to claim refund",
                        "Check sender authenticity and inspect the URL before interacting",
                        "Share QR with friends to help",
                        "Only scan using public Wi-Fi"
                    },
                    CorrectOptionIndex = 1,
                    Explanation = "QR codes can direct to malicious URLs; verify source first."
                }
            };
        }

        // GET: /Quiz/Index
        [HttpGet]
        public IActionResult Index()
        {
            var questions = GetQuestions();
            return View(questions);
        }

        // POST: /Quiz/Index
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(Microsoft.AspNetCore.Http.IFormCollection form)
        {
            var questions = GetQuestions();
            int score = 0;
            var results = new List<(QuizQuestion Question, int? SelectedIndex, bool IsCorrect)>();

            foreach (var q in questions)
            {
                string key = $"q_{q.Id}";
                if (form.ContainsKey(key) && int.TryParse(form[key], out int selected))
                {
                    bool isCorrect = selected == q.CorrectOptionIndex;
                    if (isCorrect) score++;
                    results.Add((q, selected, isCorrect));
                }
                else
                {
                    results.Add((q, null, false));
                }
            }

            // Prepare simple final message
            string finalMessage = score >= 4 ? "Good awareness!" : "Keep learning about online scams.";

            ViewBag.Score = score;
            ViewBag.Total = questions.Count;
            ViewBag.Results = results;
            ViewBag.FinalMessage = finalMessage;

            return View(questions);
        }
    }
}
