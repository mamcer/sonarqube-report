using System;

namespace SonarQube.Core.Api
{
    public class Measure
    {
        public string Metric { get; set; }

        public string Value { get; set; }

        public bool BestValue { get; set; }

        public Period[] Periods { get; set; }
    }
}