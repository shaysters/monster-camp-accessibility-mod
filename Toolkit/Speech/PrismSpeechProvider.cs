using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace GameMod.Toolkit.Speech
{
    // Prism (https://github.com/ethindp/prism) picks the best backend: a running screen
    // reader (NVDA, JAWS, ...) first, otherwise Windows TTS (SAPI / OneCore).
    // Minimal P/Invoke wrapper over Prism's C API; the prismatoid .NET bindings need a
    // newer runtime than the game's Mono. prism.dll (32-bit) is loaded by full path first
    // so the [DllImport] calls resolve to that already-loaded module.
    internal sealed class PrismSpeechProvider : ISpeechProvider
    {
        private const string PrismLib = "prism.dll";
        private const int PrismOk = 0;
        private const ulong SupportsOutput = 1UL << 5;
        private const ulong SupportsStop = 1UL << 7;

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr LoadLibraryW(string lpFileName);

        [DllImport(PrismLib, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr prism_init(IntPtr cfg);

        [DllImport(PrismLib, CallingConvention = CallingConvention.Cdecl)]
        private static extern void prism_shutdown(IntPtr ctx);

        [DllImport(PrismLib, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr prism_registry_acquire_best(IntPtr ctx);

        [DllImport(PrismLib, CallingConvention = CallingConvention.Cdecl)]
        private static extern void prism_backend_free(IntPtr backend);

        [DllImport(PrismLib, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr prism_backend_name(IntPtr backend);

        [DllImport(PrismLib, CallingConvention = CallingConvention.Cdecl)]
        private static extern ulong prism_backend_get_features(IntPtr backend);

        [DllImport(PrismLib, CallingConvention = CallingConvention.Cdecl)]
        private static extern int prism_backend_speak(IntPtr backend, byte[] text, [MarshalAs(UnmanagedType.I1)] bool interrupt);

        [DllImport(PrismLib, CallingConvention = CallingConvention.Cdecl)]
        private static extern int prism_backend_output(IntPtr backend, byte[] text, [MarshalAs(UnmanagedType.I1)] bool interrupt);

        [DllImport(PrismLib, CallingConvention = CallingConvention.Cdecl)]
        private static extern int prism_backend_stop(IntPtr backend);

        [DllImport(PrismLib, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr prism_error_string(int error);

        private readonly StructuredLog _log;
        private IntPtr _ctx;
        private IntPtr _backend;
        private ulong _features;

        public PrismSpeechProvider(StructuredLog log)
        {
            _log = log;
        }

        public string Name => "Prism";

        public bool Initialize()
        {
            var baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var prismPath = Path.Combine(baseDir ?? string.Empty, PrismLib);

            if (!File.Exists(prismPath))
            {
                _log.Warn("SPEECH", "PRISM", "Prism DLL missing.", ("path", prismPath));
                return false;
            }

            if (LoadLibraryW(prismPath) == IntPtr.Zero)
            {
                _log.Warn("SPEECH", "PRISM", "Prism DLL failed to load.", ("path", prismPath), ("error", Marshal.GetLastWin32Error().ToString()));
                return false;
            }

            _ctx = prism_init(IntPtr.Zero);
            if (_ctx == IntPtr.Zero)
            {
                _log.Warn("SPEECH", "PRISM", "prism_init failed.");
                return false;
            }

            _backend = prism_registry_acquire_best(_ctx);
            if (_backend == IntPtr.Zero)
            {
                _log.Warn("SPEECH", "PRISM", "No Prism speech backend available.");
                prism_shutdown(_ctx);
                _ctx = IntPtr.Zero;
                return false;
            }

            _features = prism_backend_get_features(_backend);
            _log.Info("SPEECH", "PRISM", "Prism backend selected.", ("backend", PtrToUtf8(prism_backend_name(_backend)) ?? "unknown"));
            return true;
        }

        public void Speak(string text, bool interrupt)
        {
            if (_backend == IntPtr.Zero || string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            var utf8 = ToUtf8(text);
            // Output = speech + braille on screen readers that support it.
            var result = (_features & SupportsOutput) != 0
                ? prism_backend_output(_backend, utf8, interrupt)
                : prism_backend_speak(_backend, utf8, interrupt);
            if (result != PrismOk)
            {
                _log.Warn("SPEECH", "PRISM", "Prism speak failed.", ("error", ErrorString(result)));
            }
        }

        public void Stop()
        {
            if (_backend == IntPtr.Zero || (_features & SupportsStop) == 0)
            {
                return;
            }

            prism_backend_stop(_backend);
        }

        public void Dispose()
        {
            if (_backend != IntPtr.Zero)
            {
                prism_backend_free(_backend);
                _backend = IntPtr.Zero;
            }

            if (_ctx != IntPtr.Zero)
            {
                prism_shutdown(_ctx);
                _ctx = IntPtr.Zero;
            }
        }

        private static byte[] ToUtf8(string text)
        {
            var bytes = new byte[Encoding.UTF8.GetByteCount(text) + 1];
            Encoding.UTF8.GetBytes(text, 0, text.Length, bytes, 0);
            return bytes;
        }

        private static string PtrToUtf8(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero)
            {
                return null;
            }

            var length = 0;
            while (Marshal.ReadByte(ptr, length) != 0)
            {
                length++;
            }

            var bytes = new byte[length];
            Marshal.Copy(ptr, bytes, 0, length);
            return Encoding.UTF8.GetString(bytes);
        }

        private static string ErrorString(int error)
        {
            return PtrToUtf8(prism_error_string(error)) ?? error.ToString();
        }
    }
}
