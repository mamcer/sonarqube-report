using System;
using System.Collections.Generic;
using System.Linq;
using SonarQube.Core;
using SonarQube.Core.Api;

namespace SonarQube.Application
{
    public class IssueService
    {
        private SonarQubeApi _sonarQubeApi;
        private string _metricUri;
        private List<SonarQubeIssue> _issues;

        public IssueService(SonarQubeApi sonarQubeApi, string issueUri, string createdAfter, string projectKey)
        {
            _sonarQubeApi = sonarQubeApi;
            _metricUri = string.Format(issueUri, createdAfter, projectKey);
            _issues = new List<SonarQubeIssue>();
        }

        public IEnumerable<SonarQubeIssue> GetIssues()
        {
            if(_issues == null || _issues.Count() == 0)
            {
                GetValuesFromApi();
                SortIssues();
            }

            return _issues;
        }

        private void SortIssues()
        {
            var types = new string[] {"bug","vulnerability","code_smell"};
            var severities = new string[] {"blocker", "critical", "major", "minor", "info"};
            var result = new List<SonarQubeIssue>();
            foreach(var type in types)
            {
                foreach(var severity in severities)
                {
                    var issues = _issues.Where(i => i.Type.ToLowerInvariant() == type && i.Severity.ToLowerInvariant() == severity).ToList();
                    result.AddRange(issues);
                }
            }

            _issues = result;
        }

        private void GetValuesFromApi()
        {
            var lastResult = _sonarQubeApi.ExecuteRequest<IssueResult>(_metricUri);
            foreach(var issue in lastResult.Issues)
            {
                var sonarQubeIssue = new SonarQubeIssue
                {
                    Severity = issue.Severity,
                    Line = issue.Line,
                    Message = issue.Message,
                    Effort = issue.Effort,
                    Type = issue.Type,
                    ComponentId = issue.Component,
                    SubProjectId = issue.SubProject
                };

                _issues.Add(sonarQubeIssue);

                foreach(var sqissue in _issues)
                {
                    var component = lastResult.Components.Where(c => c.Key == sqissue.ComponentId).FirstOrDefault();
                    sqissue.FileName = component.Name;

                    component = lastResult.Components.Where(c => c.Key == sqissue.SubProjectId).FirstOrDefault();
                    sqissue.ProjectName = component.Name;
                }
            }
        }
    }
}