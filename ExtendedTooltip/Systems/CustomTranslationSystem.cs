using Game;
using Game.SceneFlow;
using Game.UI.Localization;

using Newtonsoft.Json;

using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace ExtendedTooltip.Systems
{
	public partial class CustomTranslationSystem : GameSystemBase
	{
		public string Prefix { get; set; } = "extendedtooltip";
		private string LanguageCode { get; set; }
		public string CurrentLanguageCode => LanguageCode;
		private Dictionary<string, string> Translations { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();
			LanguageCode = GameManager.instance.localizationManager.activeLocaleId;
			Mod.Log.Info("CustomTranslationSystem created.");
			LoadCustomTranslations();
		}

		protected override void OnUpdate()
		{
		}

		public void ReloadTranslations(string locale)
		{
			LanguageCode = locale;
			Mod.Log.Info("Reloading translations.");
			LoadCustomTranslations();
		}

		private void LoadCustomTranslations()
		{
			var assembly = Assembly.GetExecutingAssembly();
			var translations = new Dictionary<string, string>();
			LoadResourceInto(translations, assembly, "ExtendedTooltip.Locale.json");

			if (!string.IsNullOrWhiteSpace(LanguageCode))
			{
				var localizedResource = $"ExtendedTooltip.Locale.{LanguageCode}.json";
				if (!LoadResourceInto(translations, assembly, localizedResource))
				{
					var neutralLanguage = LanguageCode.Split('-')[0];
					if (!neutralLanguage.Equals(LanguageCode))
					{
						LoadResourceInto(translations, assembly, $"ExtendedTooltip.Locale.{neutralLanguage}.json");
					}
				}
			}

			Translations = translations;
		}

		private static bool LoadResourceInto(Dictionary<string, string> target, Assembly assembly, string resourceName)
		{
			using var stream = assembly.GetManifestResourceStream(resourceName);
			if (stream == null)
			{
				return false;
			}

			using var reader = new StreamReader(stream);
			var resourceTranslations = JsonConvert.DeserializeObject<Dictionary<string, string>>(reader.ReadToEnd());
			if (resourceTranslations == null)
			{
				return false;
			}

			foreach (var pair in resourceTranslations)
			{
				target[pair.Key] = pair.Value;
			}

			return true;
		}

		public string GetTranslation(string key, string fallback = "Translation missing.", params string[] vars)
		{
			var dictionaryKey = $"{Prefix}.{key}";
			if (Translations == null || !Translations.ContainsKey(dictionaryKey))
			{
				return GetLocalGameTranslation(dictionaryKey, fallback, vars);
			}

			return HandleTranslationString(Translations[dictionaryKey], fallback, vars);
		}

		public string GetLocalGameTranslation(string id, string fallback = "Translation failed.", params string[] vars)
		{
			if (GameManager.instance == null || !GameManager.instance.localizationManager.activeDictionary.TryGetValue(id, out var translatedText))
			{
				return fallback;
			}

			return HandleTranslationString(translatedText, fallback, vars);
		}

		private string HandleTranslationString(string translation, string fallback, params string[] vars)
		{
			if (vars != null && vars.Length > 0)
			{
				if (vars.Length % 2 != 0)
				{
					Mod.Log.Info("Invalid amount of arguments. It must be a even number.");
					return fallback;
				}

				for (var i = 0; i < vars.Length; i += 2)
				{
					var placeholder = vars[i];
					var value = vars[i + 1];

					translation = translation.Replace("{" + placeholder + "}", value);
				}

				return translation;
			}

			return translation;
		}
	}
}
