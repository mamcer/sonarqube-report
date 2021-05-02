namespace SonarQube.Core.Api
{
    public class IssueResult
    {
        public Issue[] Issues { get; set; }

        public IssueComponent[] Components { get; set; }
    }
}