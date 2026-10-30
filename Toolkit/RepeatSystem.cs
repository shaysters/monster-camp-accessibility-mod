namespace GameMod.Toolkit
{
    internal sealed class RepeatSystem
    {
        private SpeechMessage _last;

        public void Remember(SpeechMessage message)
        {
            _last = message;
        }

        public bool TryGetLast(out SpeechMessage message)
        {
            message = _last;
            return !string.IsNullOrWhiteSpace(message.Text);
        }
    }
}
