using System.Collections.Generic;

namespace PhissingScaming.Models
{
    // Simple quiz question model
    public class QuizQuestion
    {
        public int Id { get; set; }
        public string Question { get; set; }
        public List<string> Options { get; set; } = new List<string>();
        public int CorrectOptionIndex { get; set; } 
        public string Explanation { get; set; }
    }
}
