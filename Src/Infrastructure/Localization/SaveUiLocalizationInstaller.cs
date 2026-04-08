using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;

namespace NyMod.Saves.Infrastructure.Localization;

internal static class SaveUiLocalizationInstaller
{
	private static readonly object _gate = new object();
	private static readonly Dictionary<string, Dictionary<string, string>> _cache = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
	private static bool _subscribed;

	public static void Install()
	{
		lock (_gate)
		{
			if (LocManager.Instance == null)
			{
				return;
			}

			if (!_subscribed)
			{
				LocString.SubscribeToLocaleChange(ApplyCurrentLanguage);
				_subscribed = true;
			}

			ApplyCurrentLanguage();
		}
	}

	private static void ApplyCurrentLanguage()
	{
		LocManager? locManager = LocManager.Instance;
		if (locManager == null)
		{
			return;
		}

		try
		{
			Dictionary<string, string> englishData = LoadLanguage(SaveUiText.EnglishLanguage);
			Dictionary<string, string> localizedData = locManager.Language.Equals(SaveUiText.EnglishLanguage, StringComparison.OrdinalIgnoreCase)
				? englishData
				: LoadLanguage(locManager.Language);

			LocTable englishTable = new LocTable(SaveUiText.Table, new Dictionary<string, string>(englishData, StringComparer.Ordinal));
			LocTable activeTable = locManager.Language.Equals(SaveUiText.EnglishLanguage, StringComparison.OrdinalIgnoreCase)
				? englishTable
				: new LocTable(SaveUiText.Table, new Dictionary<string, string>(localizedData, StringComparer.Ordinal), englishTable);

			Dictionary<string, LocTable> tables = Traverse.Create(locManager).Field("_tables").GetValue<Dictionary<string, LocTable>>();
			tables[SaveUiText.Table] = activeTable;
		}
		catch (Exception ex)
		{
			Log.Warn($"NyMod.Saves failed to install localization table '{SaveUiText.Table}': {ex.Message}");
		}
	}

	private static Dictionary<string, string> LoadLanguage(string language)
	{
		if (_cache.TryGetValue(language, out Dictionary<string, string>? cached))
		{
			return cached;
		}

		Assembly assembly = typeof(SaveUiLocalizationInstaller).Assembly;
		string suffix = $".localization.{language}.sts2_saves_ui.json";
		string? resourceName = assembly.GetManifestResourceNames()
			.FirstOrDefault(name => name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase));

		if (resourceName == null)
		{
			if (!language.Equals(SaveUiText.EnglishLanguage, StringComparison.OrdinalIgnoreCase))
			{
				return LoadLanguage(SaveUiText.EnglishLanguage);
			}

			throw new FileNotFoundException($"Embedded localization resource not found for language '{language}'.");
		}

		using Stream stream = assembly.GetManifestResourceStream(resourceName)
			?? throw new FileNotFoundException($"Embedded localization resource stream missing: {resourceName}");
		using StreamReader reader = new StreamReader(stream);
		string json = reader.ReadToEnd();
		Dictionary<string, string>? table = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
		if (table == null)
		{
			throw new InvalidOperationException($"Embedded localization resource '{resourceName}' deserialized to null.");
		}

		_cache[language] = table;
		return table;
	}
}

[HarmonyPatch(typeof(LocManager))]
internal static class SaveUiLocalizationInstallerHooks
{
	[HarmonyPatch(nameof(LocManager.Initialize))]
	[HarmonyPostfix]
	private static void InitializePostfix()
	{
		SaveUiLocalizationInstaller.Install();
	}
}