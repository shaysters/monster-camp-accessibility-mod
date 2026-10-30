using System;

namespace GameMod.Toolkit.Speech
{
    internal interface ISpeechProvider : IDisposable
    {
        string Name { get; }
        bool Initialize();
        void Speak(string text, bool interrupt);
        void Stop();
    }
}
