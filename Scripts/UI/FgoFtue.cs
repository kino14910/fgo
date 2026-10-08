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
///     <para>
///         <b>为什么用 Profile 而不是 Global：</b>
///         需求是「新存档需要显示教程，显示一次后不再显示」。
///         <c>SaveScope.Profile</c> 只针对主界面选中的那个存档位（Profile）独立——存档 A 看过，
///         换到新建的存档 B 就又会弹一次；同一存档内看过一次之后永不再弹。
///         这正是教程原文对 Profile 的描述：「在存档 A 里解锁的东西，换到新建的存档 B 就没有了」。
///     </para>
///     <para>
///         <b>为什么不用 <c>SaveManager.Instance.SeenFtue</c>：</b>
///         原版这个方法在设置里关掉「教程/tutorial」后会<b>一律返回 true</b>，
///         于是 mod 的 FTUE 永远看不到玩家、等于功能失效。自己存 bool 才能真正做到
///         「不受教程开关影响，且按存档判断」。
///     </para>
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
    ///     <para>
    ///         <b>为什么不能并进 <c>Entry</c> 已有的那个 using 块</b>：
    ///         那里用的是 <c>GetRunSavedDataStore</c>，本槽要用 <c>GetDataStore</c>，
    ///         两者是<b>不同</b>的 store 实例；而同一个 store 内重复 <c>Register</c> 同一个 key
    ///         会抛 <c>Data key 'x' is already registered</c>。所以单开一个 using 块。
    ///     </para>
    ///     <para>
    ///         <b>Profile 槽的初始化比注册晚，这是设计如此</b>：注册时 profile 路径还没就绪，
    ///         <c>ModDataStore</c> 会把 <c>Initialize</c>/<c>Load</c> 推迟，
    ///         直到 <c>SaveManager.InitProfileId</c> 的patch 触发
    ///         <c>InitializeProfileScoped</c>。所以此处注册完<b>不能</b>立刻读写——
    ///         见 <see cref="TryGetProgress" />。
    ///     </para>
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
    ///     <para>
    ///         <b>不能直接 <c>store.Get</c>：</b>
    ///         <c>ModDataStore.GetEntry</c> 的「懒初始化」分支只对
    ///         <c>Scope == SaveScope.Global</c> 生效；Profile 槽若尚未初始化，
    ///         会走到 <c>RegisteredDataEntry.Data</c> 而直接抛
    ///         <c>InvalidOperationException("Data entry '...' is not initialized")</c>。
    ///         所以必须先看 <see cref="ModDataStore.IsProfileInitialized" />。
    ///     </para>
    ///     <para>
    ///         战斗中的触发点（NP 满）必然发生在 profile 就绪之后，正常不会走到这个 false 分支；
    ///         留着是为了防御「模组加载顺序异常」时不把异常抛进战斗逻辑。
    ///     </para>
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
    /// <para>
    ///         会自行跳过以下情况，避免打断玩家或与其他 modal 抢屏：
    ///     </para>
    ///     <list type="bullet">
    ///         <item>Profile 数据尚未就绪（见 <see cref="TryGetProgress" />）</item>
    ///         <item>当前存档已看过（<see cref="FgoFtueProgress.SeenNpBarFtue" /> 为 true）</item>
    ///         <item>不在局内/ 已有 modal 打开（<c>NModalContainer.Add</c> 在有 modal 时只 warn 并静默丢弃）</item>
    ///         <item>本地化缺失（避免弹一个空白框）</item>
    ///     </list>
    ///     <para>
    ///         <b>时机说明</b>：不是 <c>CombatStartingEvent</c>，而是
    ///         <c>canUse</c> 的 false→true 边沿（见 <c>FgoNpBar.OnNpChanged</c>）——
    ///         需求就是「宝具条满了之后出现提示」，等到满的那一刻弹才指得准。
    ///     </para>
    ///     <para>
    ///         <b>目标矩形</b>：NP 条挂在 <c>NCreatureStateDisplay</c> 下、跟着 HP 条走，
    ///         运行期位置随分辨率与玩家位置变化，所以由调用方传入按钮的屏幕矩形，
    ///         弹窗据此摆位、箭头据此指向。
    ///     </para>
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