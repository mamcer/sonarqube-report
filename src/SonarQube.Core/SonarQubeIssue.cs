namespace SonarQube.Core
{
    public class SonarQubeIssue
    {
        // "CODE_SMELL",
        // "BUG",
        // "VULNERABILITY"
        public string Severity { get; set; }

        public string Line { get; set; }
        
        public string Message { get; set; }
        
        public string Effort { get; set; }
        
        public string Type { get; set; }
        
        public string ComponentId { get; set; }

        public string SubProjectId { get; set; }
        
        public string FileName { get; set; }
        
        public string ProjectName { get; set; }
    }
}