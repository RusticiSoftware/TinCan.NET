using System;

namespace TinCan
{
    public sealed class InteractionType
    {
        private const string CHOICE = "choice";
        private const string SEQUENCING = "sequencing";
        private const string LIKERT = "likert";
        private const string MATCHING = "matching";
        private const string PERFORMANCE = "performance";
        private const string TRUEFALSE = "true-false";
        private const string FILLIN = "fill-in";
        private const string LONGFILLIN = "long-fill-in";
        private const string NUMERIC = "numeric";
        private const string OTHER = "other";

        public static readonly InteractionType Choice = new InteractionType(CHOICE);
        public static readonly InteractionType Sequencing = new InteractionType(SEQUENCING);
        public static readonly InteractionType Likert = new InteractionType(LIKERT);
        public static readonly InteractionType Matching = new InteractionType(MATCHING);
        public static readonly InteractionType Performance = new InteractionType(PERFORMANCE);
        public static readonly InteractionType TrueFalse = new InteractionType(TRUEFALSE);
        public static readonly InteractionType FillIn = new InteractionType(FILLIN);
        public static readonly InteractionType LongFillIn = new InteractionType(LONGFILLIN);
        public static readonly InteractionType Numeric = new InteractionType(NUMERIC);
        public static readonly InteractionType Other = new InteractionType(OTHER);

        private InteractionType(string value)
        {
            Value = value;
        }

        public static InteractionType FromValue(string value)
        {
            switch (value)
            {
                case CHOICE:
                    return Choice;

                case SEQUENCING:
                    return Sequencing;

                case LIKERT:
                    return Likert;

                case MATCHING:
                    return Matching;

                case PERFORMANCE:
                    return Performance;

                case TRUEFALSE:
                    return TrueFalse;

                case FILLIN:
                    return FillIn;
                    
                case LONGFILLIN:
                    return LongFillIn;

                case NUMERIC:
                    return Numeric;

                case OTHER:
                    return Other;

                default:
                    throw new ArgumentOutOfRangeException(nameof(value),
                        $"'{value}' is not a valid interactionType.");
            }
        }

        public string Value { get; private set; }
    }
}
