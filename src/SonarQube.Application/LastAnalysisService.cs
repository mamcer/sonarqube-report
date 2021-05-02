using System;
using System.Collections.Generic;
using SonarQube.Core;
using SonarQube.Core.Api;

namespace SonarQube.Application
{
    public class LastAnalysisService
    {
        private SonarQubeApi _sonarQubeApi;
        private string _metricUri;
        private SonarQubeMeasure _lastAnalysis;
        private DateTime? _lastAnalysisDate;
        private SonarQubeMeasure _previousAnalysis;
        private DateTime? _previousAnalysisDate;

        public LastAnalysisService(SonarQubeApi sonarQubeApi, string metricUri, string lastAnalysisKey, string previousAnalysisKey, string projectKey)
        {
            _sonarQubeApi = sonarQubeApi;
            _metricUri = string.Format(metricUri, projectKey);
            _lastAnalysisDate = null;
            _previousAnalysisDate = null;

            _lastAnalysis = new SonarQubeMeasure
            {
                Metric = lastAnalysisKey,
                Value = null
            };

            _previousAnalysis = new SonarQubeMeasure
            {
                Metric = previousAnalysisKey,
                Value = null
            };
        }

        public DateTime LastAnalysisDate
        {
            get
            {
                GetValueFromApi();
                if (_lastAnalysisDate.HasValue)
                {
                    return _lastAnalysisDate.Value;
                }

                return DateTime.MinValue;
            }
        }

        public string LastAnalysisDateString
        {
            get
            {
                GetValueFromApi();
                if (_lastAnalysisDate.HasValue)
                {
                    DateTime cstTime = TimeZoneInfo.ConvertTimeToUtc(DateTime.Today);
                    _lastAnalysis.Value = $"{cstTime.ToString("MMMM dd, yyyy, hh:mm tt", new System.Globalization.CultureInfo("en-US"))} (UTC)";
                }
                else
                {
                    _lastAnalysis.Value = string.Empty;
                }

                return _lastAnalysis.Value;
            }
        }

        public DateTime PreviousAnalysisDate
        {
            get
            {
                GetValueFromApi();
                if (_previousAnalysisDate.HasValue)
                {
                    return _previousAnalysisDate.Value;
                }

                return DateTime.MinValue;
            }
        }

        public string PreviousAnalysisDateString
        {
            get
            {
                GetValueFromApi();
                if (_previousAnalysisDate.HasValue)
                {
                    try
                    {
                        TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
                        DateTime cstTime = TimeZoneInfo.ConvertTimeFromUtc(_previousAnalysisDate.Value, cstZone);
                        var utc = cstZone.IsDaylightSavingTime(cstTime) ?
                                     cstZone.DaylightName : cstZone.StandardName;
                        _previousAnalysis.Value = $"{cstTime.ToString("MMMM dd, yyyy, hh:mm tt", new System.Globalization.CultureInfo("en-US"))} ({utc})";
                    }
                    catch (TimeZoneNotFoundException)
                    {
                        _previousAnalysis.Value = $"{_previousAnalysisDate.Value.ToString("MMMM dd, yyyy, hh:mm tt", new System.Globalization.CultureInfo("en-US"))} (UTC)";
                    }
                }
                else
                {
                    _previousAnalysis.Value = string.Empty;
                }

                return _previousAnalysis.Value;
            }
        }

        public IEnumerable<SonarQubeMeasure> GetMeasures()
        {
            GetValueFromApi();
            return new List<SonarQubeMeasure>() { _lastAnalysis, _previousAnalysis };
        }

        private void GetValueFromApi()
        {
            if (!_lastAnalysisDate.HasValue)
            {
                var lastResult = _sonarQubeApi.ExecuteRequest<LastResult>(_metricUri);
                _lastAnalysisDate = lastResult.Analyses[0].Date.ToUniversalTime();
                _previousAnalysisDate = lastResult.Analyses[1].Date.ToUniversalTime();
            }

            if (_lastAnalysis.Value == null)
            {
                var lastResult = _sonarQubeApi.ExecuteRequest<LastResult>(_metricUri);
                if (lastResult.Analyses.Length > 1)
                {
                    _lastAnalysis.Value = lastResult.Analyses[0].Date.ToString("MMMM dd, yyyy, hh:mm tt", new System.Globalization.CultureInfo("en-US"));
                    _previousAnalysis.Value = lastResult.Analyses[1].Date.ToString("MMMM dd, yyyy, hh:mm tt", new System.Globalization.CultureInfo("en-US"));
                }
            }
        }
    }
}