using System;
using System.Collections.Generic;
using System.Linq;
using SonarQube.Core;
using SonarQube.Core.Api;

namespace SonarQube.Application
{
    public class MetricService
    {
        private SonarQubeApi _sonarQubeApi;
        private string _metricUri;
        private string _projectKey;
        private List<SonarQubeMeasure> _measures;

        public MetricService(SonarQubeApi sonarQubeApi, string metricUri, string[] metrics, string projectKey)
        {
            _sonarQubeApi = sonarQubeApi;
            _metricUri = metricUri;
            _projectKey = projectKey;
            _measures = new List<SonarQubeMeasure>(metrics.Length);
            foreach (var metric in metrics)
            {
                _measures.Add(
                             new SonarQubeMeasure
                             {
                                 Metric = metric,
                                 Value = null
                             }
                );
            }
        }

        public IEnumerable<SonarQubeMeasure> GetMeasures()
        {
            GetValuesFromApi();
            return _measures.ToList();
        }

        private void GetValuesFromApi()
        {
            if (_measures[0].Value == null)
            {
                foreach (var measure in _measures)
                {
                    var metricInfo = _sonarQubeApi.ExecuteRequest<Result>(string.Format(_metricUri, _projectKey, measure.Metric));

                    if (metricInfo.Component.Measures != null && metricInfo.Component.Measures.Length > 0)
                    {
                        if (string.IsNullOrEmpty(metricInfo.Component.Measures[0].Value) && metricInfo.Component.Measures[0].Periods != null && metricInfo.Component.Measures[0].Periods.Length > 0)
                        {
                            measure.Value = metricInfo.Component.Measures[0].Periods[0].Value;
                        }
                        else
                        {
                            measure.Value = metricInfo.Component.Measures[0].Value;
                        }
                    }
                    else
                    {
                        measure.Value = string.Empty;
                    }
                }
            }
        }
    }
}