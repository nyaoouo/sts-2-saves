using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Logging;
using NyMod.Saves.Features.SaveArchive.Models;
using NyMod.Saves.Infrastructure.Configuration;

namespace NyMod.Saves.Features.SaveArchive.Logic;

internal sealed class SaveArchiveService
{
	private readonly SaveManagerConfigStore _configStore;
	private readonly SaveArchiveStore _archiveStore;
	private readonly SaveArchiveSummaryFactory _summaryFactory;
	private readonly RunIdentityService _runIdentityService;

	public SaveArchiveService(
		SaveManagerConfigStore configStore,
		SaveArchiveStore archiveStore,
		SaveArchiveSummaryFactory summaryFactory,
		RunIdentityService runIdentityService)
	{
		_configStore = configStore;
		_archiveStore = archiveStore;
		_summaryFactory = summaryFactory;
		_runIdentityService = runIdentityService;
	}

	public bool CaptureCurrentSnapshot(SaveArchiveKind kind, bool isMultiplayer, string? note = null)
	{
		SaveManagerConfig config = _configStore.LoadOrDefault();
		if (kind == SaveArchiveKind.Auto && isMultiplayer && !config.CaptureMultiplayerAutosaves)
		{
			Log.Info("NyMod.Saves skipped multiplayer autosave capture because config disabled it.");
			return false;
		}

		if (!_archiveStore.TryReadActivePayload(isMultiplayer, out byte[]? payloadBytes, out string? sourceFileName) || payloadBytes == null || string.IsNullOrEmpty(sourceFileName))
		{
			Log.Warn($"NyMod.Saves failed to capture {(isMultiplayer ? "multiplayer" : "singleplayer")} {kind} snapshot because the active save payload was unavailable.");
			return false;
		}

		SaveArchiveSummary summary = _summaryFactory.Create(payloadBytes);
		string runId = _runIdentityService.ResolveRunId(summary, payloadBytes, isMultiplayer);
		DateTimeOffset createdUtc = DateTimeOffset.UtcNow;
		string saveId = BuildSaveId(kind, createdUtc);
		SaveArchiveMetadata metadata = new SaveArchiveMetadata
		{
			RunId = runId,
			SaveId = saveId,
			Kind = kind,
			IsMultiplayer = isMultiplayer,
			SourceFileName = sourceFileName,
			CreatedUtc = createdUtc,
			Note = note,
			Summary = summary
		};

		_archiveStore.SaveSnapshot(metadata, payloadBytes);
		if (kind == SaveArchiveKind.Auto && config.AutosaveRetentionMode == AutosaveRetentionMode.CapLatest)
		{
			_archiveStore.TrimAutosaves(runId, isMultiplayer, config.AutosaveCapPerRun);
		}

		Log.Info($"NyMod.Saves archived {(isMultiplayer ? "multiplayer" : "singleplayer")} {kind} snapshot '{saveId}' under run '{runId}'");
		return true;
	}

	public bool CaptureManualSnapshot(bool isMultiplayer, string? note = null)
	{
		return CaptureCurrentSnapshot(SaveArchiveKind.Manual, isMultiplayer, note);
	}

	public bool PreserveCurrentRunBeforeReplacement(bool isMultiplayer, string? note = null)
	{
		return CaptureCurrentSnapshot(SaveArchiveKind.Manual, isMultiplayer, note ?? "pre_new_run");
	}

	public bool DeleteArchivedRunForCurrentActiveSave(bool isMultiplayer)
	{
		if (!TryResolveCurrentRunId(isMultiplayer, out string? runId) || string.IsNullOrEmpty(runId))
		{
			return false;
		}

		_archiveStore.DeleteRun(isMultiplayer, runId);
		Log.Info($"NyMod.Saves removed archived run '{runId}' for {(isMultiplayer ? "multiplayer" : "singleplayer")} active save cleanup");
		return true;
	}

	public bool TryGetCurrentRunId(bool isMultiplayer, out string? runId)
	{
		return TryResolveCurrentRunId(isMultiplayer, out runId);
	}

	public IReadOnlyList<RunArchiveRecord> ListRuns(bool isMultiplayer)
	{
		List<RunArchiveRecord> records = new List<RunArchiveRecord>();
		foreach (string runId in _archiveStore.LoadRunIds(isMultiplayer))
		{
			IReadOnlyList<SaveArchiveMetadata> autos = _archiveStore.LoadSnapshotsForRun(isMultiplayer, runId, SaveArchiveKind.Auto);
			IReadOnlyList<SaveArchiveMetadata> manuals = _archiveStore.LoadSnapshotsForRun(isMultiplayer, runId, SaveArchiveKind.Manual);
			SaveArchiveMetadata? latest = autos.Concat(manuals)
				.OrderByDescending(static snapshot => snapshot.CreatedUtc)
				.FirstOrDefault();

			records.Add(new RunArchiveRecord
			{
				RunId = runId,
				IsMultiplayer = isMultiplayer,
				AutoSaveCount = autos.Count,
				ManualSaveCount = manuals.Count,
				LatestSaveUtc = latest?.CreatedUtc,
				LatestSummary = latest?.Summary
			});
		}

		return records
			.OrderByDescending(static record => record.LatestSaveUtc)
			.ToList();
	}

	public IReadOnlyList<SaveArchiveMetadata> ListSnapshots(bool isMultiplayer, string runId)
	{
		return _archiveStore.LoadSnapshotsForRun(isMultiplayer, runId, SaveArchiveKind.Auto)
			.Concat(_archiveStore.LoadSnapshotsForRun(isMultiplayer, runId, SaveArchiveKind.Manual))
			.OrderByDescending(static snapshot => snapshot.CreatedUtc)
			.ToList();
	}

	public bool RestoreSnapshot(SaveArchiveMetadata metadata)
	{
		bool restored = _archiveStore.TryRestoreSnapshot(metadata);
		if (restored)
		{
			metadata.LastRestoredUtc = DateTimeOffset.UtcNow;
			Log.Info($"NyMod.Saves restored snapshot '{metadata.SaveId}' for run '{metadata.RunId}'");
		}

		return restored;
	}

	public string? BackupSnapshot(SaveArchiveMetadata metadata)
	{
		return _archiveStore.ExportSnapshot(metadata);
	}

	public string? BackupRun(bool isMultiplayer, string runId)
	{
		return _archiveStore.ExportRun(isMultiplayer, runId);
	}

	public void DeleteSnapshot(SaveArchiveMetadata metadata)
	{
		_archiveStore.DeleteSnapshot(metadata);
	}

	public void DeleteRun(bool isMultiplayer, string runId)
	{
		_archiveStore.DeleteRun(isMultiplayer, runId);
	}

	private bool TryResolveCurrentRunId(bool isMultiplayer, out string? runId)
	{
		runId = null;
		if (!_archiveStore.TryReadActivePayload(isMultiplayer, out byte[]? payloadBytes, out _) || payloadBytes == null)
		{
			return false;
		}

		SaveArchiveSummary summary = _summaryFactory.Create(payloadBytes);
		runId = _runIdentityService.ResolveRunId(summary, payloadBytes, isMultiplayer);
		return !string.IsNullOrEmpty(runId);
	}

	private static string BuildSaveId(SaveArchiveKind kind, DateTimeOffset createdUtc)
	{
		string prefix = kind == SaveArchiveKind.Auto ? "auto" : "manual";
		return $"{prefix}_{createdUtc:yyyyMMdd_HHmmss_fff}";
	}
}