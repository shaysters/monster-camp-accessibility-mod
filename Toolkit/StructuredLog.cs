using System;
using System.Text;
using MelonLoader;

namespace GameMod.Toolkit
{
    internal sealed class StructuredLog
    {
        private readonly MelonLogger.Instance _logger;

        public StructuredLog(MelonLogger.Instance logger)
        {
            _logger = logger;
        }

        public void Info(string area, string hook, string message, params (string Key, string Value)[] fields)
        {
            _logger.Msg(Format(area, hook, message, fields));
        }

        public void Warn(string area, string hook, string message, params (string Key, string Value)[] fields)
        {
            _logger.Warning(Format(area, hook, message, fields));
        }

        public void Error(string area, string hook, string message, params (string Key, string Value)[] fields)
        {
            _logger.Error(Format(area, hook, message, fields));
        }

        private static string Format(string area, string hook, string message, params (string Key, string Value)[] fields)
        {
            var builder = new StringBuilder();
            builder.Append("[MCA11Y][");
            builder.Append(area);
            builder.Append("][");
            builder.Append(hook);
            builder.Append("] ");
            builder.Append(message);

            if (fields == null || fields.Length == 0)
            {
                return builder.ToString();
            }

            foreach (var field in fields)
            {
                builder.Append(" | ");
                builder.Append(field.Key);
                builder.Append("=");
                builder.Append(field.Value ?? string.Empty);
            }

            return builder.ToString();
        }
    }
}
