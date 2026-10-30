using System;
using System.Reflection;

namespace GameMod.Toolkit
{
    internal sealed class BuildTargetAdapter
    {
        private readonly StructuredLog _log;

        public BuildTargetAdapter(StructuredLog log)
        {
            _log = log;
            Current = BuildProbeResult.Unknown;
        }

        public BuildProbeResult Current { get; private set; }

        public BuildProbeResult Detect()
        {
            var assembly = FindAssemblyCSharp();
            if (assembly == null)
            {
                Current = BuildProbeResult.Unknown;
                _log.Warn("BUILD", "ASSEMBLY", "Assembly-CSharp not found in current AppDomain.");
                return Current;
            }

            var hasFullTypes =
                HasType(assembly, "BeautifulGlitch.NavigationManager") &&
                HasType(assembly, "BeautifulGlitch.GameEventView") &&
                HasType(assembly, "MonsterCamp.GameAnnouncementsOverlay");

            var hasDemoTypes =
                HasType(assembly, "PrepareGameManager") &&
                HasType(assembly, "MapNavigation") &&
                HasType(assembly, "EventManager");

            if (hasFullTypes)
            {
                Current = new BuildProbeResult(
                    "full",
                    assembly.GetName().Name,
                    "BeautifulGlitch.NavigationManager",
                    "BeautifulGlitch.GameSession",
                    "BeautifulGlitch.GameEventView",
                    "BeautifulGlitch.PlayerTurnOverlay",
                    "BeautifulGlitch.StatsManager");
                return Current;
            }

            if (hasDemoTypes)
            {
                Current = new BuildProbeResult(
                    "demo",
                    assembly.GetName().Name,
                    "MapNavigation",
                    "PrepareGameManager",
                    "EventManager",
                    "GameManager",
                    "StatsManager");
                return Current;
            }

            Current = new BuildProbeResult(
                "unknown",
                assembly.GetName().Name,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty);
            return Current;
        }

        private static Assembly FindAssemblyCSharp()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (string.Equals(assembly.GetName().Name, "Assembly-CSharp", StringComparison.Ordinal))
                {
                    return assembly;
                }
            }

            return null;
        }

        private static bool HasType(Assembly assembly, string fullName)
        {
            return assembly.GetType(fullName, throwOnError: false) != null;
        }
    }

    internal sealed class BuildProbeResult
    {
        public static readonly BuildProbeResult Unknown = new BuildProbeResult("unknown", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);

        public BuildProbeResult(string targetName, string assemblyName, string focusTypeName, string nameTypeName, string eventTypeName, string turnTypeName, string statsTypeName)
        {
            TargetName = targetName;
            AssemblyName = assemblyName;
            FocusTypeName = focusTypeName;
            NameTypeName = nameTypeName;
            EventTypeName = eventTypeName;
            TurnTypeName = turnTypeName;
            StatsTypeName = statsTypeName;
        }

        public string TargetName { get; }
        public string AssemblyName { get; }
        public string FocusTypeName { get; }
        public string NameTypeName { get; }
        public string EventTypeName { get; }
        public string TurnTypeName { get; }
        public string StatsTypeName { get; }
    }
}
