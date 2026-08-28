using STS2RitsuLib.Settings;
using STS2RitsuLib.Telemetry;

namespace Fgo.Scripts;

public static class FgoTelemetry
{
    private const string ApplicantId = Entry.ModId;
    private static ITelemetryClient Client = null!;

    public static void Register()
    {
        TelemetryRegistry.RegisterApplicant(new TelemetryApplicant
        {
            ApplicantId = ApplicantId,
            OwnerModId = Entry.ModId,
            DisplayName = "Fgo",
            DisplayNameText = ModSettingsText.Literal("Fgo"),

            Adapter = new PostHogTelemetryAdapter(
                "https://fgo-telemetry.1491037864qq.workers.dev",
                "proxy"
            ),

            Requests =
            [
                TelemetryRequest.BasicUsage(
                    ModSettingsText.Literal(
                        "发送版本、平台、语言和匿名安装 ID，用来估算兼容性问题范围。")),

                TelemetryRequest.ModInventory(
                    "发送已安装的模组列表。"),

                TelemetryRequest.RunHistory(
                    ModSettingsText.Literal(
                        "发送已结束跑局的原版 run-history，用来分析平衡性。"),
                    [
                        "other.mod/challenge_context"
                    ],
                    evt => !evt.IsAbandoned),

                TelemetryRequest.Diagnostics(
                    ModSettingsText.Literal(
                        "发送异常和诊断上下文，用来定位崩溃。")),

                TelemetryRequest.Custom(
                    "balance_event",
                    ModSettingsText.Literal(
                        "发送本 Mod 的平衡性事件，例如挑战选择和重掷次数。"))
            ]
        });

        Client = TelemetryApi.GetClient(ApplicantId);
    }
}