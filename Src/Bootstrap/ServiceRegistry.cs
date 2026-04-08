using System;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using NyMod.Saves.Features.SaveArchive.Logic;
using NyMod.Saves.Infrastructure.Configuration;
using NyMod.Saves.Infrastructure.Persistence;

namespace NyMod.Saves.Bootstrap;

internal static class ServiceRegistry
{
	private static bool _initialized;

	public static SaveArchiveService ArchiveService { get; private set; } = null!;

	public static void Initialize()
	{
		if (_initialized)
		{
			return;
		}

		var pathResolver = new SaveArchivePathResolver();
		var configStore = new SaveManagerConfigStore(pathResolver);
		var summaryFactory = new SaveArchiveSummaryFactory();
		var runIdentityService = new RunIdentityService();
		var archiveStore = new SaveArchiveStore(pathResolver);
		ArchiveService = new SaveArchiveService(configStore, archiveStore, summaryFactory, runIdentityService);

		try
		{
			SaveManager.Instance.Saved += OnGameSaved;
		}
		catch (Exception ex)
		{
			Log.Warn($"NyMod.Saves failed to subscribe to SaveManager.Saved: {ex.Message}");
		}

		_initialized = true;
	}

	private static void OnGameSaved()
	{
		try
		{
			bool isMultiplayer = RunManager.Instance.NetService.Type == NetGameType.Host;
			ArchiveService.CaptureCurrentSnapshot(SaveArchiveKind.Auto, isMultiplayer);
		}
		catch (Exception ex)
		{
			Log.Warn($"NyMod.Saves autosave capture failed: {ex.Message}");
		}
	}
}