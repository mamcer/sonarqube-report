using System;
using Xunit;
using SonarQube.Application;
using SonarQube.Core;
using SonarQube.Core.Api;
using Moq;

namespace SonarQube.Application.Test
{
    public class ProjectStatusServiceTest
    {
        [Fact]
        public void LastAnalysisDateShouldReturnValueIfNotNull()
        {
            // Arrange
            var sonarQubeApiMock = new Mock<SonarQubeApi>(null, null, null);
            sonarQubeApiMock.Setup(m => m.ExecuteRequest<ProjectStatusResult>(It.IsAny<string>())).Returns(
                new ProjectStatusResult
                {
                    ProjectStatus = new ProjectStatus { Periods = new[] { new ProjectPeriod { Date = DateTime.Today } } }
                }
                );
            var projectStatusService = new ProjectStatusService(sonarQubeApiMock.Object, string.Empty, string.Empty, string.Empty);
            string periodDate;
            DateTime cstTime = TimeZoneInfo.ConvertTimeToUtc(DateTime.Today);
            string expected = $"{cstTime.ToString("MMMM dd, yyyy, hh:mm tt", new System.Globalization.CultureInfo("en-US"))} (UTC)";
            DateTime dateNow = DateTime.Now;

            // Act
            periodDate = projectStatusService.PeriodDateString;

            // Assert
            Assert.Equal(expected, periodDate);
        }
    }
}