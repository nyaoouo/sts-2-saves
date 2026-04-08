using Godot;
using Godot.Bridge;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Nodes;
using NyMod.Saves.Bootstrap;
using NyMod.Saves.Infrastructure.Localization;

namespace NyMod.Saves;

[ModInitializer("Init")]
public class Entry
{
    public static void Init()
    {
        ServiceRegistry.Initialize();
        var harmony = new Harmony("nymod.saves");
        harmony.PatchAll();
        SaveUiLocalizationInstaller.Install();
        Log.Info("NyMod.Saves initialized");
    }
}
