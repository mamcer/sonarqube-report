using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using SonarQube.Core;
using SonarQube.Application;
using System.Linq;
using System.Collections.Generic;

namespace SonarQube.Report
{
    class Program
    {
        static void Main(string[] args)
        {
            var userName = string.Empty;
            var password = string.Empty;

            var sendEmail = true;

            if (args.Length > 1)
            {
                if (args[1].Contains(":"))
                {
                    var credentials = args[1].Split(":");
                    userName = credentials[0];
                    password = credentials[1];

                    if (args.Length > 2)
                    {
                        sendEmail = args[2] != "/noemail";
                    }
                }
                else
                {
                    sendEmail = args[1] != "/noemail";
                }
            }

            var startingTime = DateTime.Now;
            WriteLogMessage("process started...");

            if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json")))
            {
                var builder = new ConfigurationBuilder()
                                .SetBasePath(Directory.GetCurrentDirectory())
                                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);
                IConfigurationRoot configuration = builder.Build();

                var sonarQubeApiUrl = configuration["SonarQubeApiUrl"];
                var projectKey = configuration["SonarQubeProjectKey"];
                if(string.IsNullOrWhiteSpace(userName))
                {
                    userName = configuration["SonarQubeUserName"];
                    password = configuration["SonarQubePassword"];
                }
                var metrics = configuration["SonarQubeMetrics"].Split(',');
                var metricsUri = configuration["SonarQubeMetricUri"];
                var lastAnalysisUri = configuration["SonarQubeLastAnalysisUri"];
                var projectStatusUri = configuration["SonarQubeProjectStatus"];
                var issuesUri = configuration["SonarQubeIssuesUri"];
                var emailTemplateFilePath = configuration["EmailTemplateFilePath"];

                var smtpHost = configuration["SmtpHost"];
                var smtpPort = string.IsNullOrEmpty(configuration["SmtpPort"]) ? 0 : Convert.ToInt32(configuration["SmtpPort"]);
                var smtpUserName = configuration["SmtpUserName"];
                var smtpPassword = configuration["SmtpPassword"];
                var smtpTo = configuration["SmtpTo"];
                var smtpEnableSsl = string.IsNullOrEmpty(configuration["SmtpEnableSsl"]) ? false : Convert.ToBoolean(configuration["SmtpEnableSsl"]);
                var smtpUseDefaultCredentials = string.IsNullOrEmpty(configuration["SmtpUseDefaultCredentials"]) ? false : Convert.ToBoolean(configuration["SmtpUseDefaultCredentials"]);

                WriteLogMessage($"project key: {projectKey}");

                var sonarQubeApi = new SonarQubeApi(sonarQubeApiUrl, userName, password);

                var lastAnalysisService = new LastAnalysisService(sonarQubeApi, lastAnalysisUri, "last-analysis", "previous-analysis", projectKey);
                if (lastAnalysisService.LastAnalysisDate.Date == DateTime.Today.Date)
                {
                    WriteLogMessage($"last analysis date: Today [{lastAnalysisService.LastAnalysisDateString}]");
                }
                else
                {
                    WriteLogMessage($"last analysis date: {lastAnalysisService.LastAnalysisDateString}");
                }

                WriteLogMessage($"previous analysis date: {lastAnalysisService.PreviousAnalysisDateString}");

                var projectStatusService = new ProjectStatusService(sonarQubeApi, projectStatusUri, "created-after", projectKey);
                WriteLogMessage($"last period date: {projectStatusService.PeriodDateString}");

                var daysSinceLastAnalysis = lastAnalysisService.PreviousAnalysisDate.Day - DateTime.Today.Day;
                WriteLogMessage($"total days since previous analysis: {lastAnalysisService.LastAnalysisDate.Day - lastAnalysisService.PreviousAnalysisDate.Day}");
                var issueService = new IssueService(sonarQubeApi, issuesUri, DateTime.Today.AddDays(daysSinceLastAnalysis).ToString("yyyy-MM-dd"), projectKey);
                WriteLogMessage($"new issues since previous analysis: {issueService.GetIssues().Count()}");

                WriteLogMessage($"processing {metrics.Length} metrics...");
                var metricService = new MetricService(sonarQubeApi, metricsUri, metrics, projectKey);

                var parameters = new StringDictionary
                {
                    { "{{project-name}}", projectKey },
                    { "{{created-before}}", DateTime.Today.AddDays(1).ToString("yyyy-MM-dd") },
                    { "{{days_since_previous_analysis}}", (lastAnalysisService.LastAnalysisDate.Day - lastAnalysisService.PreviousAnalysisDate.Day).ToString() },
                    { "{{period-date}}", projectStatusService.PeriodDateString }
                };

                foreach (var measure in lastAnalysisService.GetMeasures())
                {
                    parameters.Add("{{" + measure.Metric + "}}", measure.Value);
                }

                foreach (var measure in projectStatusService.GetMeasures())
                {
                    parameters.Add("{{" + measure.Metric + "}}", measure.Value);
                }

                foreach (var measure in metricService.GetMeasures())
                {
                    parameters.Add("{{" + measure.Metric + "}}", measure.Value);
                }

                WriteLogMessage("generating report...");

                var sonarQubeReport = new SonarQubeReport(emailTemplateFilePath);
                var issueValues = new List<List<string>>();
                foreach (var issue in issueService.GetIssues())
                {
                    var row = new List<string>();
                    row.Add(issue.Type);
                    row.Add(issue.Severity);
                    row.Add(issue.ProjectName);
                    row.Add(issue.FileName);
                    row.Add(issue.Message);
                    row.Add(issue.Effort);
                    issueValues.Add(row);
                }
                sonarQubeReport.UpdateMinuteUnit(parameters, new[] { "{{sqale_index}}", "{{new_technical_debt}}" });
                sonarQubeReport.UpdateMillisecondUnit(parameters, new[] { "{{test_execution_time}}" });
                sonarQubeReport.ApplyParameters(parameters);
                sonarQubeReport.AddNewIssuesSection(new string[] { "Type", "Severity", "Project Name", "File Name", "Message", "Effort" }, issueValues);

                var reportsDirectory = configuration["ReportsDirectory"];
                if (!Directory.Exists(reportsDirectory))
                {
                    Directory.CreateDirectory(reportsDirectory);
                }
                var filePath = Path.Combine(reportsDirectory, DateTime.Now.ToString("yyyy-dd-MM_hh-mm-sstt", new System.Globalization.CultureInfo("en-US")) + "_report.html");
                File.WriteAllText(filePath, sonarQubeReport.GetContent());
                WriteLogMessage($"report file available in: {filePath}");

                filePath = Path.Combine(reportsDirectory, "last-report.html");
                File.WriteAllText(filePath, sonarQubeReport.GetContent());

                if (sendEmail)
                {
                    WriteLogMessage("sending email...");
                    var emailService = new EmailService(smtpHost, smtpPort, smtpUserName, smtpPassword, smtpEnableSsl, smtpUseDefaultCredentials);
                    emailService.SendEmail(smtpTo, projectKey, sonarQubeReport.GetContent());
                }
                else
                {
                    WriteLogMessage("'/noemail' flag detected. No email was sent");
                }

                var timespan = DateTime.Now - startingTime;
                WriteLogMessage($"process finished in {timespan.Hours:00}h:{timespan.Minutes:00}m:{timespan.Seconds:00}s:{timespan.Milliseconds:00}ms");
            }
            else
            {
                WriteLogMessage("error appsettings.json file not found.");
            }
        }

        private static void WriteLogMessage(string message)
        {
            Console.WriteLine(DateTime.Now.ToString("yyyy-dd-MM_HH:mm:ss") + " - " + message);
        }
    }
}