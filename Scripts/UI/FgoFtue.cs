using Godot;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using STS2RitsuLib;
using STS2RitsuLib.Utils.Persistence;

namespace Fgo.Scripts.UI;

/// <summary>
///     FTUE 首见标记的持久化数据槽（<c>SaveScope.Profile</c>，<b>按存档槽位隔离</b>）。
/// </summary>
public sealed class FgoFtueProgress
{
    public const string Key = "fgo_ftue_progress";

    public const string FileName = "fgo_ftue_progress.json";

    public bool SeenNpBarFtue { get; set; }
}

/// <summary>FTUE 触发与首见标记的门面。</summary>
public static class FgoFtue
{
    private const string TitleKey = "FGO_NP_FTUE_TITLE";
    private const string DescriptionKey = "FGO_NP_FTUE_DESCRIPTION";

    private static bool _registered;

    /// <summary>
    ///     注册持久化槽。必须在 <c>Entry.Init</c> 里、走
    ///     <see cref="RitsuLibFramework.BeginModDataRegistration" /> 作用域内调用。
    /// </summary>
    public static void Register()
    {
        if (_registered)
            return;

        using (RitsuLibFramework.BeginModDataRegistration(Entry.ModId))
        {
            var store = RitsuLibFramework.GetDataStore(Entry.ModId);
            store.Register(
                FgoFtueProgress.Key,
                FgoFtueProgress.FileName,
                SaveScope.Profile,
                () => new FgoFtueProgress(),
                true
            );
        }

        _registered = true;
    }

    /// <summary>
    ///     取当前存档的首见标记；Profile 数据尚未就绪时返回 <see langword="false" />。
    /// </summary>
    private static bool TryGetProgress(out FgoFtueProgress progress)
    {
        progress = new FgoFtueProgress();

        if (!_registered)
            return false;

        var store = RitsuLibFramework.GetDataStore(Entry.ModId);

        if (!store.IsProfileInitialized)
            return false;

        progress = store.Get<FgoFtueProgress>(FgoFtueProgress.Key);
        return true;
    }

    /// <summary>
    ///     宝具条充满且可释放时调用：首次触发本局教学提示。
    /// </summary>
    /// <param name="npButtonScreenRect">NP 按钮在屏幕坐标系里的矩形。</param>
    public static void MaybeShowNpBarFtue(Rect2 npButtonScreenRect)
    {
        if (!TryGetProgress(out var progress))
            return;

        if (progress.SeenNpBarFtue)
            return;

        if (!LocString.Exists("ftues", TitleKey) ||
            !LocString.Exists("ftues", DescriptionKey))
        {
            Entry.Logger.Warn(
                $"[Fgo] NP FTUE loc keys missing ({TitleKey}/{DescriptionKey}); skipping. " +
                "Did the mod pck get rebuilt after editing ftues.json?"
            );
            return;
        }

        var modal = NModalContainer.Instance;

        if (modal == null || modal.OpenModal != null)
            return;

        var ftue = FgoNpBarFtue.Create(npButtonScreenRect);

        if (ftue == null)
            return;

        modal.Add(ftue, false);

        var store = RitsuLibFramework.GetDataStore(Entry.ModId);
        store.Modify<FgoFtueProgress>(FgoFtueProgress.Key, p => p.SeenNpBarFtue = true);
        store.Save(FgoFtueProgress.Key);

        Entry.Logger.Info("[Fgo] Showed NP bar FTUE.");
    }

    /// <summary>
    ///     重置当前存档的首见标记（调试用：等价于「重新看一次教学」）。
    /// </summary>
    public static void ResetAll()
    {
        if (!TryGetProgress(out _))
        {
            Entry.Logger.Warn("[Fgo] Cannot reset FTUE progress: profile data not ready.");
            return;
        }

        var store = RitsuLibFramework.GetDataStore(Entry.ModId);
        store.Modify<FgoFtueProgress>(
            FgoFtueProgress.Key,
            static p => p.SeenNpBarFtue = false
        );
        store.Save(FgoFtueProgress.Key);
    }
}