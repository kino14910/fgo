using System.Reflection;
using Fgo.Scripts.Character;
using Fgo.Scripts.UI;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using STS2RitsuLib;
using STS2RitsuLib.Content;
using STS2RitsuLib.Interop;
using STS2RitsuLib.Patching.Core;
using STS2RitsuLib.Scaffolding.Godot.NodeAttachments;

// using Fgo.Scripts.Utils;

namespace Fgo.Scripts;

[ModInitializer(nameof(Init))]
public class Entry
{
    public const string ModId = "Fgo";
    public const string ResPath = $"res://{ModId}";
    public static readonly Logger Logger = RitsuLibFramework.CreateLogger(ModId);

    public static void Init()
    {
        var assembly = Assembly.GetExecutingAssembly();
        RitsuLibFramework.EnsureGodotScriptsRegistered(assembly, Logger);
        ModTypeDiscoveryHub.RegisterModAssembly(ModId, assembly);
        RitsuLibFramework.RegisterModSettingsReflectionProvider<FgoReflectedSettings>();
        FgoEnums.Initialize(ModId);
        FgoCombatUi.Initialize();

        // Noble 卡牌图书馆筛选按钮：调用 RitsuLib 的注册 API，
        // 由 RitsuLib 的 CardLibraryCompendiumPatch 在 NCardLibrary._Ready
        // postfix 中自动构造按钮、加入 strip、连接信号、注册到 _poolFilters。
        // 详见 NoblePoolFilterRegistration 上的文档注释。
        ModContentRegistry
            .For(ModId)
            .RegisterCardLibraryCompendiumSharedPoolFilter<NobleCardPool>(
                NoblePoolFilterRegistration.StableId,
                NoblePoolFilterRegistration.IconTexturePath);

        // Noble 卡相关 patch。全部通过 Ritsu 的 patcher 模式注册
        // （IPatchMethod + CreatePatcher + ApplyRequiredPatcher），
        // 不直接用 new Harmony(...).PatchAll() —— 那样绕过了 Ritsu 的诊断/回滚机制。
        var noblePatcher = RitsuLibFramework.CreatePatcher(ModId, "noble_cards");
        noblePatcher.RegisterPatch<NobleCardHideBannerPatch>();
        noblePatcher.RegisterPatch<NobleRaritySortPatch>();
        noblePatcher.RegisterPatch<NobleTitleOutlinePatch>();
        noblePatcher.RegisterPatch<NobleRarityLockPatch>();
        noblePatcher.RegisterPatch<NoblePoolPredicatePatch>();
        RitsuLibFramework.ApplyRequiredPatcher(
            noblePatcher,
            DisableMod,
            "Noble card patcher failed; Noble pool UI features will not work.");
    }

    private static void DisableMod()
    {
        // 关键 patch 失败时调用。目前没有需要全局禁用的状态，
        // 但 Ritsu 要求传入一个 disable 回调以满足契约。
        Logger.ErrorNoTrace("[Fgo] Critical patch failed, mod disabled.");
    }
}
