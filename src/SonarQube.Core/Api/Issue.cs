namespace SonarQube.Core.Api
{
    public class Issue
    {
        public string Severity { get; set; }

        public string Line { get; set; }
        
        public string Message { get; set; }
        
        public string Effort { get; set; }
        
        public string Type { get; set; }
        
        public string Component { get; set; }

        public string SubProject { get; set; }
    }
}