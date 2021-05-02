using System;
using Xunit;
using SonarQube.Core;
using SonarQube.Core.Api;
using Moq;

namespace SonarQube.Application.Test
{
    public class LastAnalysisServiceTest
    {
        [Fact]
        public void LastAnalysisDateShouldReturnValueIfNotNull()
        {
            // Arrange
            var sonarQubeApiMock = new Mock<SonarQubeApi>(null, null, null);
            sonarQubeApiMock.Setup(m => m.ExecuteRequest<LastResult>(It.IsAny<string>())).Returns(
                new LastResult
                {
                    Analyses = new[] { new Analyses { Date = DateTime.Today }, new Analyses { Date = DateTime.Today } }
                }
                );
            var lastAnalysisService = new LastAnalysisService(sonarQubeApiMock.Object, string.Empty, string.Empty, string.Empty, string.Empty);
            string lastAnalysisDate;
            DateTime cstTime = TimeZoneInfo.ConvertTimeToUtc(DateTime.Today);
            string expected = $"{cstTime.ToString("MMMM dd, yyyy, hh:mm tt", new System.Globalization.CultureInfo("en-US"))} (UTC)";

            // Act
            lastAnalysisDate = lastAnalysisService.LastAnalysisDateString;

            // Assert
            Assert.Equal(expected, lastAnalysisDate);
        }
    }
}