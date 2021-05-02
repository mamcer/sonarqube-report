using System;
using System.Collections.Generic;
using System.Linq;
using SonarQube.Core;
using SonarQube.Core.Api;

namespace SonarQube.Application
{
    public class ProjectStatusService
    {
        private SonarQubeApi _sonarQubeApi;
        private string _metricUri;
        private SonarQubeMeasure _measure;
        private DateTime? _periodDate;

        public ProjectStatusService(SonarQubeApi sonarQubeApi, string metricUri, string metric, string projectKey)
        {
            _sonarQubeApi = sonarQubeApi;
            _metricUri = string.Format(metricUri, projectKey);
            _measure = new SonarQubeMeasure
            {
                Metric = metric,
                Value = null
            };
        }

        public string PeriodDateString
        {
            get
            {
                GetValueFromApi();
                return _measure.Value != null ? _measure.Value : string.Empty;
            }
        }

        public IEnumerable<SonarQubeMeasure> GetMeasures()
        {
            GetValueFromApi();
            return new List<SonarQubeMeasure>() { _measure };
        }

        private void GetValueFromApi()
        {
            if (_measure.Value == null)
            {
                var projectStatusResult = _sonarQubeApi.ExecuteRequest<ProjectStatusResult>(_metricUri);
                _periodDate = projectStatusResult.ProjectStatus.Periods.FirstOrDefault()?.Date;
                if (_periodDate.HasValue)
                {
                    DateTime cstTime = TimeZoneInfo.ConvertTimeToUtc(_periodDate.Value);
                    _measure.Value = $"{cstTime.ToString("MMMM dd, yyyy, hh:mm tt", new System.Globalization.CultureInfo("en-US"))} (UTC)";
                }
                else
                {
                    _measure.Value = string.Empty;
                }
            }
        }
    }
}