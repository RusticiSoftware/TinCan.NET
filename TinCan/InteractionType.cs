using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TinCan
{
    public sealed class InteractionType
    {
        private const string choice = "choice";
        private const string sequencing = "sequencing";
        private const string likert = "likert";
        private const string matching = "matching";
        private const string performance = "performance";
        private const string truefalse = "true-false";
        private const string fillin = "fill-in";
        private const string numeric = "numeric";
        private const string other = "other";


        public static readonly InteractionType Choice = new InteractionType("choice");
        public static readonly InteractionType Sequencing = new InteractionType("sequencing");
        public static readonly InteractionType Likert = new InteractionType("likert");
        public static readonly InteractionType Matching = new InteractionType("matching");
        public static readonly InteractionType Performance = new InteractionType("performance");
        public static readonly InteractionType TrueFalse = new InteractionType("true-false");
        public static readonly InteractionType FillIn = new InteractionType("fill-in");
        public static readonly InteractionType Numeric = new InteractionType("numeric");
        public static readonly InteractionType Other = new InteractionType("other");


        private InteractionType(string value)
        {
            Value = value;
        }

        public static InteractionType FromValue(string value)
        {
            switch (value)
            {
                case choice:
                    return Choice;

                case sequencing:
                    return Sequencing;

                case likert:
                    return Likert;

                case matching:
                    return Matching;

                case performance:
                    return Performance;

                case truefalse:
                    return TrueFalse;

                case fillin:
                    return FillIn;

                case numeric:
                    return Numeric;

                case other:
                    return Other;

                default:
                    return null;

            }
        }

        public string Value { get; private set; }
    }
}
