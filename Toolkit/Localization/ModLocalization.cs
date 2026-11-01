using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace GameMod.Toolkit.Localization
{
    internal static class ModLocalization
    {
        private static readonly Dictionary<string, string> English = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "accessibility_loaded", "Accessibility loaded." },
            { "make_a_choice", "Make a choice." },
            { "choose_your_character", "Choose your character." },
            { "character_selected", "Character selected." },
            { "character_selected_fmt", "Character selected. {0}" },
            { "change_name_pronouns_confirm", "You can change name, change pronouns, or confirm." },
            { "choose_what_to_pack", "Choose what to pack. Move through the items, or choose pack random items." },
            { "pack_random_items", "Pack random items." },
            { "packed_fmt", "Packed. {0}" },
            { "credits_exit", "Credits. Press Escape to go back to the main menu." },
            { "campfire_option_fmt", "Campfire option {0}" },
            { "campfire_love_log", "Love log" },
            { "seats_free_none", "No seats free." },
            { "seats_free_one", "1 seat free." },
            { "seats_free_fmt", "{0} seats free." },
            { "player_customize_fmt", "{0} player. Activate to customize." },
            { "player_1", "1 player" },
            { "player_2", "2 players" },
            { "player_3", "3 players" },
            { "player_4", "4 players" },
            { "next_page", "Next page" },
            { "previous_page", "Previous page" },
            { "previous_option", "Previous option" },
            { "next_option", "Next option" },
            { "back", "Back" },
            { "player_a", "Player A" },
            { "player_b", "Player B" },
            { "player_c", "Player C" },
            { "player_d", "Player D" },
            { "confirm_character", "Confirm character" },
            { "custom_name", "Custom name" },
            { "use_your_skills", "Use your skills" },
            { "drink_of_the_day", "Drink of the day" },
            { "mystery_drink_box", "Mystery drink box" },
            { "option_number_fmt", "Option {0}" },
            { "option_number_text_fmt", "Option {0}. {1}" },
            { "short_game_fmt", "Short game. {0}" },
            { "regular_game_fmt", "Regular game. {0}" },
            { "create_match", "Create match" },
            { "join_match", "Join match" },
            { "ready", "Ready" },
            { "add_local_player", "Add local player" },
            { "name_activate", "Name. Activate to change." },
            { "name_current_fmt", "Name. {0}. Activate to change." },
            { "choose_name", "Choose a name" },
            { "name_option", "Name option" },
            { "name_option_fmt", "Name option. {0}" },
            { "pronouns_activate", "Pronouns. Activate to change." },
            { "pronouns_current_fmt", "Pronouns. {0}. Activate to change." },
            { "choose_pronouns", "Choose pronouns." },
            { "question_prefix", "Question" },
            { "online_main", "Online. Choose create match or join match." },
            { "online_join", "Join online match. Enter a match code or choose a number." },
            { "online_waiting", "Waiting for online match." },
            { "connecting_online", "Connecting online." },
            { "player_turn_fmt", "{0}'s turn." },
            { "shop", "Shop." },
            { "shop_cash_fmt", "Shop. {0}" },
            { "costs_fmt", "Costs {0}." },
            { "affordable", "Affordable." },
            { "not_enough_money", "Not enough money." },
            { "choose_who_to_ask", "Choose who to ask." },
            { "results_prefix", "Results." },
            { "results_this_run", "This run." },
            { "results_new_secret_endings_fmt", "New secret endings {0}." },
            { "results_new_events_fmt", "New events {0}." },
            { "results_new_outcomes_fmt", "New outcomes {0}." },
            { "results_new_special_fmt", "New special stuff {0}." },
            { "results_new_memories_fmt", "Summer memories this run {0}." },
            { "results_total_memories_fmt", "Total memories {0}." },
            { "quit_confirm", "Are you sure you want to return to the main menu?" },
            { "online_lobby", "Online lobby" },
            { "online_code_announce_fmt", "Online code: {0}. Press C to hear it again." },
            { "online_code_fmt", "Online code: {0}" },
            { "online_code_none", "No online code right now." },
            { "update_available_fmt", "Monster Camp Accessibility version {0} is available. Run the installer to update." },
            { "choose_this_location_fmt", "{0}. Choose this location." },
            { "stats_unavailable", "Stats unavailable right now." },
            { "current_player", "Current player" },
            { "stat_boldness_fmt", "Boldness {0}." },
            { "stat_smarts_fmt", "Smarts {0}." },
            { "stat_creativity_fmt", "Creativity {0}." },
            { "stat_charm_fmt", "Charm {0}." },
            { "stat_fun_fmt", "Fun {0}." }
        };

        private static readonly Dictionary<string, Dictionary<string, string>> Overrides = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
        private static bool _loaded;

        public static string Get(string key)
        {
            EnsureLoaded();

            var language = DetectLanguageKey();
            if (Overrides.TryGetValue(language, out var localized) && localized.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            if (Overrides.TryGetValue("English", out var englishOverride) && englishOverride.TryGetValue(key, out var englishValue) && !string.IsNullOrWhiteSpace(englishValue))
            {
                return englishValue;
            }

            return English.TryGetValue(key, out var fallback) ? fallback : key;
        }

        public static string Format(string key, params object[] args)
        {
            return string.Format(Get(key), args);
        }

        public static string DetectLanguageKey()
        {
            try
            {
                var assembly = AppDomain.CurrentDomain.GetAssemblies();
                Assembly gameAssembly = null;
                foreach (var asm in assembly)
                {
                    if (string.Equals(asm.GetName().Name, "Assembly-CSharp", StringComparison.Ordinal))
                    {
                        gameAssembly = asm;
                        break;
                    }
                }

                if (gameAssembly != null)
                {
                    var generalManagerType = gameAssembly.GetType("BeautifulGlitch.GeneralManager", false);
                    var instanceProperty = generalManagerType?.GetProperty("Instance", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                    var generalManager = instanceProperty?.GetValue(null, null);
                    if (generalManager != null)
                    {
                        var textManagerField = generalManagerType.GetField("textManager", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        var textManager = textManagerField?.GetValue(generalManager);
                        if (textManager != null)
                        {
                            var languageField = textManager.GetType().GetField("language", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                            var languageProfile = languageField?.GetValue(textManager);
                            if (languageProfile != null)
                            {
                                var langEnumField = languageProfile.GetType().GetField("language", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                                var langEnumValue = langEnumField?.GetValue(languageProfile);
                                if (langEnumValue != null)
                                {
                                    return langEnumValue.ToString();
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
            }

            switch (Application.systemLanguage)
            {
                case SystemLanguage.French: return "French";
                case SystemLanguage.German: return "German";
                case SystemLanguage.Italian: return "Italian";
                case SystemLanguage.Japanese: return "Japanese";
                case SystemLanguage.Korean: return "Korean";
                case SystemLanguage.Russian: return "Russian";
                case SystemLanguage.Spanish: return "SpanishSpain";
                case SystemLanguage.ChineseSimplified: return "ChineseSimplified";
                case SystemLanguage.ChineseTraditional: return "ChineseTraditional";
                case SystemLanguage.Portuguese: return "PortugueseBrazil";
                default: return "English";
            }
        }

        private static void EnsureLoaded()
        {
            if (_loaded)
            {
                return;
            }

            _loaded = true;
            try
            {
                var baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
                var localizationDir = Path.Combine(baseDir, "Localization");
                if (!Directory.Exists(localizationDir))
                {
                    return;
                }

                foreach (var path in Directory.GetFiles(localizationDir, "*.lang"))
                {
                    var language = Path.GetFileNameWithoutExtension(path);
                    var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    foreach (var rawLine in File.ReadAllLines(path))
                    {
                        var line = rawLine.Trim();
                        if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
                        {
                            continue;
                        }

                        var eq = line.IndexOf('=');
                        if (eq <= 0)
                        {
                            continue;
                        }

                        var key = line.Substring(0, eq).Trim();
                        var value = line.Substring(eq + 1).Trim();
                        if (key.Length != 0)
                        {
                            map[key] = value;
                        }
                    }

                    Overrides[language] = map;
                }
            }
            catch
            {
            }
        }
    }
}
