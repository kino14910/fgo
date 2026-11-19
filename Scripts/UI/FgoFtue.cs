using Godot;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using STS2RitsuLib;
using STS2RitsuLib.Data;
using STS2RitsuLib.Utils.Persistence;

namespace Fgo.Scripts.UI;

/// <summary>
///     FTUE 首见标记的持久化数据槽（<c>SaveScope.Profile</c>，<b>按存档槽位隔离</b>）。
/// </summary>
/// <remarks>
///     <b>用 Profile 而非 Global</b>：需求是「新存档显示教程、看过一次后不再显示」；Profile 按选中的存档位隔离（存档 A 看过、换新建 B 又弹），恰是教程原文语义。
///     <b>不用 <c>SaveManager.Instance.SeenFtue</c></b>：原版在关掉「教程」开关后一律返回 true，会使 mod FTUE 永不可见；自己存 bool 才不受开关影响、按存档判断。
/// </remarks>
public sealed class FgoFtueProgress
{
    /// <summary>
    ///     RitsuLib 数据槽的逻辑键。
    /// </summary>
    public const string Key = "fgo_ftue_progress";

    /// <summary>
    ///     RitsuLib 数据槽的文件名。Profile 作用域下会落到
    ///     <c>{accountBase}/{profileDir}/fgo_ftue_progress.json</c>，天然按存档分目录。
    /// </summary>
    public const string FileName = "fgo_ftue_progress.json";

    /// <summary>是否已看过「宝具条满了」教学（<b>当前存档</b>内只算一次）。</summary>
    public bool SeenNpBarFtue { get; set; }
}

/// <summary>
///     FTUE 触发与首见标记的门面。业务代码只调 <see cref="MaybeShowNpBarFtue" />。
/// </summary>
public static class FgoFtue
{
    private const string TitleKey = "FGO_NP_FTUE_TITLE";
    private const string DescriptionKey = "FGO_NP_FTUE_DESCRIPTION";

    private static bool _registered;

    /// <summary>
    ///     注册持久化槽。必须在 <c>Entry.Init</c> 里、走
    ///     <see cref="RitsuLibFramework.BeginModDataRegistration" /> 作用域内调用。
    /// </summary>
    /// <remarks>
    ///     <b>不能并进 Entry 已有的 using 块</b>：那里用 <c>GetRunSavedDataStore</c>、本槽要用 <c>GetDataStore</c>，是不同 store 实例；同 store 重复
    ///     Register 同 key 会抛 "Data key ... already registered"，故单开块。
    ///     <b>Profile 槽注册后不能立刻读写</b>：注册时 profile 路径未就绪，<c>ModDataStore</c> 把 Initialize/Load 推迟到
    ///     <c>SaveManager.InitProfileId</c> 的 patch 触发；见 <see cref="TryGetProgress" />。
    /// </remarks>
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
    /// <remarks>
    ///     <b>不能直接 <c>store.Get</c></b>：<c>ModDataStore.GetEntry</c> 的懒初始化只对 <c>SaveScope.Global</c> 生效；Profile 槽未初始化会走到
    ///     <c>RegisteredDataEntry.Data</c> 抛 "Data entry ... is not initialized"，故先判
    ///     <see cref="ModDataStore.IsProfileInitialized" />。
    ///     战斗中触发点（NP 满）必在 profile 就绪后，正常不走到 false 分支；保留是为防御模组加载顺序异常时不把异常抛进战斗逻辑。
    /// </remarks>
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
    /// <remarks>
    ///     自行跳过：Profile 未就绪（见 <see cref="TryGetProgress" />）、当前存档已看过、不在局内/已有 modal（<c>NModalContainer.Add</c> 有 modal 时只 warn
    ///     丢弃）、本地化缺失。
    ///     <b>时机</b>：用 <c>canUse</c> 的 false→true 边沿（见 <c>FgoNpBar.OnNpChanged</c>）而非 <c>CombatStartingEvent</c>
    ///     ——需求是「满了才弹」，满的那刻指得准。
    ///     <b>目标矩形</b>：NP 条挂 <c>NCreatureStateDisplay</c> 下跟 HP 条走，运行期位置随分辨率/玩家变，故由调用方传按钮屏幕矩形，弹窗据此摆位、箭头指向。
    /// </remarks>
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

        // 已有 modal 时 NModalContainer.Add 只 Log.Warn 后静默丢弃，
        // 那时既不弹窗也不该把「已看过」写掉——玩家还没看到呢。
        if (modal == null || modal.OpenModal != null)
            return;

        var ftue = FgoNpBarFtue.Create(npButtonScreenRect);

        if (ftue == null)
            return;

        modal.Add(ftue, false);

        // 只记在「当前存档」上：同一存档内不再弹，换新存档会再弹一次。
        var store = RitsuLibFramework.GetDataStore(Entry.ModId);
        store.Modify<FgoFtueProgress>(FgoFtueProgress.Key, p => p.SeenNpBarFtue = true);
        store.Save(FgoFtueProgress.Key);

        Entry.Logger.Info("[Fgo] Showed NP bar FTUE.");
    }

    /// <summary>
    ///     重置当前存档的首见标记（调试用：等价于「重新看一次教学」）。
    /// </summary>
    /// <remarks>
    ///     只影响当前存档 —— 因为槽是 <c>SaveScope.Profile</c>，别的存档本来就没标记过。
    /// </remarks>
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