using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;

namespace SonarQube.Core
{
    public class SonarQubeReport
    {
        private string _templateFilePath;
        private string _content;

        public SonarQubeReport(string templateFilePath)
        {
            _templateFilePath = templateFilePath;
            _content = string.Empty;
        }

        public void ApplyParameters(StringDictionary parameters)
        {
            using (StreamReader sr = new StreamReader(_templateFilePath))
            {
                _content = sr.ReadToEnd();

                if (parameters != null)
                {
                    foreach (string key in parameters.Keys)
                    {
                        _content = _content.Replace(key, parameters[key]);
                    }
                }
            }
        }

        public void UpdateMillisecondUnit(StringDictionary parameters, string[] keys)
        {
            foreach (var key in keys)
            {
                if (parameters.ContainsKey(key))
                {
                    parameters[key] = GetTimeUnit(Convert.ToInt32(parameters[key]) / 1000);
                }
            }
        }

        public void UpdateMinuteUnit(StringDictionary parameters, string[] keys)
        {
            foreach (var key in keys)
            {
                if (parameters.ContainsKey(key))
                {
                    parameters[key] = GetTimeUnit(Convert.ToInt32(parameters[key]) * 60);
                }
            }
        }

        public string GetTimeUnit(int seconds)
        {
            decimal minutes = seconds / 60;
            if (minutes >= 1)
            {
                if (minutes >= 60)
                {
                    decimal hours = minutes / 60;
                    if (hours > 1)
                    {
                        if (hours >= 8)
                        {
                            decimal days = decimal.Round(hours / 8, MidpointRounding.AwayFromZero);

                            return $"{days} days";
                        }
                    }

                    return $"{Math.Floor(hours)} hours";
                }

                return $"{minutes} minutes";
            }

            return $"{seconds} seconds";
        }

        public void AddNewIssuesSection(string[] labels, List<List<string>> values)
        {
            var result = @"<p>None</p>";
            if (values.Count > 0)
            {
                result = @"<table class=""table"" style=""font-size:small"">
                                <thead>
                                    <tr>";
                foreach (var label in labels)
                {
                    result += $"<th>{label}</th>";
                }
                result += @"</tr>
                                </thead>
                                <tbody>";
                foreach (var line in values)
                {
                    result += "<tr>";
                    foreach (var value in line)
                    {
                        result += $"<td>{value}</td>";
                    }
                    result += "</tr>";
                }

                result += @"</tbody>
                            </table>";
            }

            _content = _content.Replace("{{new_issues}}", result);
        }

        public string GetContent()
        {
            return _content;
        }
    }
}