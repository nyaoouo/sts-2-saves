using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using NyMod.Saves.Bootstrap;
using NyMod.Saves.Features.SaveBrowser.Logic;

namespace NyMod.Saves.Features.SaveBrowser.Integration;

[HarmonyPatch(typeof(NMainMenu))]
internal static class MainMenuHooks
{
	[HarmonyPatch("OnContinueButtonPressed")]
	[HarmonyPrefix]
	private static bool OnContinueButtonPressedPrefix(NMainMenu __instance)
	{
		SaveBrowserCoordinator.OpenForMainMenu(__instance, isMultiplayer: false);
		return false;
	}

	[HarmonyPatch(nameof(NMainMenu.RefreshButtons))]
	[HarmonyPostfix]
	private static void RefreshButtonsPostfix(NMainMenu __instance)
	{
		bool hasArchivedRuns = ServiceRegistry.ArchiveService.ListRuns(isMultiplayer: false).Count > 0;
		Traverse traverse = Traverse.Create(__instance);
		NMainMenuTextButton continueButton = traverse.Field("_continueButton").GetValue<NMainMenuTextButton>();
		NMainMenuTextButton abandonRunButton = traverse.Field("_abandonRunButton").GetValue<NMainMenuTextButton>();
		NMainMenuTextButton singleplayerButton = traverse.Field("_singleplayerButton").GetValue<NMainMenuTextButton>();
		continueButton.Visible = hasArchivedRuns;
		continueButton.SetEnabled(continueButton.Visible);
		singleplayerButton.Visible = true;
		abandonRunButton.Visible = false;
	}
}