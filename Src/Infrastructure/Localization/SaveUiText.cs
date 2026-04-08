using MegaCrit.Sts2.Core.Localization;

namespace NyMod.Saves.Infrastructure.Localization;

internal static class SaveUiText
{
	public const string Table = "sts2_saves_ui";
	public const string MainMenuTable = "main_menu_ui";
	public const string EnglishLanguage = "eng";

	public static string Get(string key)
	{
		return new LocString(Table, key).GetFormattedText();
	}

	public static string GetFromTable(string table, string key)
	{
		return new LocString(table, key).GetFormattedText();
	}

	public static string Format(string key, params (string Name, object? Value)[] variables)
	{
		return FormatFromTable(Table, key, variables);
	}

	public static string FormatFromTable(string table, string key, params (string Name, object? Value)[] variables)
	{
		var loc = new LocString(table, key);
		foreach ((string name, object? value) in variables)
		{
			loc.AddObj(name, value ?? string.Empty);
		}

		return loc.GetFormattedText();
	}

	public static string CommonYesNo(bool value)
	{
		return GetFromTable(MainMenuTable, value ? "GENERIC_POPUP.confirm" : "GENERIC_POPUP.cancel");
	}

	internal static class Keys
	{
		internal static class Common
		{
			public const string Unknown = "COMMON.unknown";
			public const string None = "COMMON.none";
			public const string Auto = "COMMON.auto";
			public const string Manual = "COMMON.manual";
			public const string SingleplayerShort = "COMMON.singleplayerShort";
			public const string MultiplayerShort = "COMMON.multiplayerShort";
		}

		internal static class PauseMenu
		{
			public const string SaveButton = "PAUSE_MENU.save";
			public const string LoadButton = "PAUSE_MENU.load";
			public const string ManualSaveCreatedTitle = "PAUSE_MENU.manualSaveCreated.title";
			public const string ManualSaveCreatedBody = "PAUSE_MENU.manualSaveCreated.body";
			public const string ManualSaveFailedTitle = "PAUSE_MENU.manualSaveFailed.title";
			public const string ManualSaveFailedBody = "PAUSE_MENU.manualSaveFailed.body";
		}

		internal static class SaveBrowser
		{
			public const string TitleSingleplayer = "SAVE_BROWSER.title.singleplayer";
			public const string TitleMultiplayer = "SAVE_BROWSER.title.multiplayer";
			public const string LoadSnapshot = "SAVE_BROWSER.action.loadSnapshot";
			public const string Backup = "SAVE_BROWSER.action.backup";
			public const string OpenFolder = "SAVE_BROWSER.action.openFolder";
			public const string EditNote = "SAVE_BROWSER.action.editNote";
			public const string DeleteSave = "SAVE_BROWSER.action.deleteSave";
			public const string DeleteRun = "SAVE_BROWSER.action.deleteRun";
			public const string Empty = "SAVE_BROWSER.empty";
			public const string SelectHint = "SAVE_BROWSER.selectHint";
			public const string GroupAuto = "SAVE_BROWSER.group.auto";
			public const string GroupManual = "SAVE_BROWSER.group.manual";
			public const string RunLabel = "SAVE_BROWSER.runLabel";
			public const string SaveLabel = "SAVE_BROWSER.saveLabel";
			public const string SaveLabelUnavailable = "SAVE_BROWSER.saveLabelUnavailable";
			public const string BackupCreated = "SAVE_BROWSER.backupCreated";
			public const string BackupFailed = "SAVE_BROWSER.backupFailed";
			public const string SnapshotDetails = "SAVE_BROWSER.snapshotDetails";
			public const string SnapshotDetailsUnavailable = "SAVE_BROWSER.snapshotDetailsUnavailable";
			public const string RunDetails = "SAVE_BROWSER.runDetails";
			public const string SnapshotNoteLine = "SAVE_BROWSER.note.snapshot";
			public const string RunNoteLine = "SAVE_BROWSER.note.run";
			public const string NoteValueNone = "SAVE_BROWSER.note.none";
			public const string NoteDialogTitleSnapshot = "SAVE_BROWSER.noteDialog.title.snapshot";
			public const string NoteDialogTitleRun = "SAVE_BROWSER.noteDialog.title.run";
			public const string NoteDialogLabelSnapshot = "SAVE_BROWSER.noteDialog.label.snapshot";
			public const string NoteDialogLabelRun = "SAVE_BROWSER.noteDialog.label.run";
		}

		internal static class Popup
		{
			public const string LoadFailedTitle = "POPUP.loadFailed.title";
			public const string RestoreFailedBody = "POPUP.loadFailed.restoreFailed.body";
			public const string SingleplayerUnreadableBody = "POPUP.loadFailed.singleplayerUnreadable.body";
			public const string MultiplayerUnreadableBody = "POPUP.loadFailed.multiplayerUnreadable.body";
			public const string MultiplayerMainMenuMissingBody = "POPUP.loadFailed.multiplayerMainMenuMissing.body";
			public const string OpenFolderFailedTitle = "POPUP.openFolderFailed.title";
			public const string OpenFolderFailedBody = "POPUP.openFolderFailed.body";
		}
	}
}