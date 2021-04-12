﻿using Examine.Search;

namespace Examine.Search
{
    public struct ExamineValue : IExamineValue
    {
        public ExamineValue(Examineness vagueness, string value)
            : this(vagueness, value, 1)
        {
        }

        public ExamineValue(Examineness vagueness, string value, float level)
        {
            this.Examineness = vagueness;
            this.Value = value;
            this.Level = level;
        }

        public Examineness Examineness { get; }

        public string Value { get; }

        public float Level { get; }

    }
}
