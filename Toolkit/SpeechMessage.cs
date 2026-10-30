namespace GameMod.Toolkit
{
    internal struct SpeechMessage
    {
        public SpeechMessage(string text, SpeechCategory category, string sourceTag, bool interrupt)
        {
            Text = text ?? string.Empty;
            Category = category;
            SourceTag = sourceTag ?? string.Empty;
            Interrupt = interrupt;
        }

        public string Text { get; }
        public SpeechCategory Category { get; }
        public string SourceTag { get; }
        public bool Interrupt { get; }
    }
}
