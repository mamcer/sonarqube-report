using System;

namespace SonarQube.Core.Api
{
    public class Component
    {
        public string Id { get; set; }

        public string Key { get; set; }

        public string Name { get; set; }

        public string Qualifier { get; set; }

        public Measure[] Measures { get; set; }
    }
}