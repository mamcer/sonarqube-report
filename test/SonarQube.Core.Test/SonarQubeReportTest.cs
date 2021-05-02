using System;
using Xunit;
using SonarQube.Core;

namespace SonarQube.Core.Test
{
    public class SonarQubeReportTest
    {
        [Fact]
        public void ConstructorShouldSetEmptyContent()
        {
            // Arrange
            var sonarQubeReport = new SonarQubeReport(string.Empty);
            string content = "non-empty";

            // Act            
            content = sonarQubeReport.GetContent();

            // Assert
            Assert.Empty(content);
        }

        [Theory]
        [InlineData(0, "0 seconds")]
        [InlineData(1, "1 seconds")]
        [InlineData(10, "10 seconds")]
        [InlineData(59, "59 seconds")]
        public void GetTimeUnitWithLessThan60SecondsShouldReturnSeconds(int seconds, string expected)
        {
            // Arrange
            var sonarQubeReport = new SonarQubeReport(string.Empty);
            string time;

            // Act
            time = sonarQubeReport.GetTimeUnit(seconds);

            // Assert
            Assert.Equal(expected, time);
        }

        [Theory]
        [InlineData(60, "1 minutes")]
        [InlineData(1800, "30 minutes")]
        [InlineData(3559, "59 minutes")]
        public void GetTimeUnitWithLessThan1hourShouldReturnMinutes(int seconds, string expected)
        {
            // Arrange
            var sonarQubeReport = new SonarQubeReport(string.Empty);
            string time;

            // Act
            time = sonarQubeReport.GetTimeUnit(seconds);

            // Assert
            Assert.Equal(expected, time);
        }

        [Theory]
        [InlineData(3600, "1 hours")]
        [InlineData(14400, "4 hours")]
        [InlineData(25200, "7 hours")]
        [InlineData(28759, "7 hours")]
        public void GetTimeUnitWithLessThan8hourShouldReturnHours(int seconds, string expected)
        {
            // Arrange
            var sonarQubeReport = new SonarQubeReport(string.Empty);
            string time;

            // Act
            time = sonarQubeReport.GetTimeUnit(seconds);

            // Assert
            Assert.Equal(expected, time);
        }

        [Theory]
        [InlineData(28800, "1 days")]
        [InlineData(144000, "5 days")]
        [InlineData(2880000, "100 days")]
        public void GetTimeUnitWithMoreThan8hourShouldReturndays(int seconds, string expected)
        {
            // Arrange
            var sonarQubeReport = new SonarQubeReport(string.Empty);
            string time;

            // Act
            time = sonarQubeReport.GetTimeUnit(seconds);

            // Assert
            Assert.Equal(expected, time);
        }
    }
}
