using System;

namespace SonarQube.Core.Api
{
    public class Period
    {
        public int Id { get; set; }

        public string Value { get; set; }

        public bool BestValue { get; set; }
    }
}