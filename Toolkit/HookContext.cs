namespace GameMod.Toolkit
{
    internal static class HookContext
    {
        public static StructuredLog Log { get; private set; }
        public static SpeechRouter Speech { get; private set; }
        public static BuildProbeResult Build { get; private set; }

        public static void Initialize(StructuredLog log, SpeechRouter speech, BuildProbeResult build)
        {
            Log = log;
            Speech = speech;
            Build = build;
        }
    }
}
