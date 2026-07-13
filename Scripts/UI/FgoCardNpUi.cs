// using Fgo.Scripts.Singletons;
// using Godot;
// using MegaCrit.Sts2.Core.Combat;
// using MegaCrit.Sts2.Core.Models;
// using MegaCrit.Sts2.Core.Nodes.Cards;
// using STS2RitsuLib.Scaffolding.Godot.NodeAttachments;
//
// namespace Fgo.Scripts.UI;
//
// public sealed partial class FgoCardNpUi : Control
// {
//     private Label _npLabel = null!;
//     private ColorRect _progressFill = null!;
//     private int _lastNp = -1;
//
//     private const int NpMax = 300;
//
//     public static void Initialize()
//     {
//         ModNodeAttachmentRegistry
//             .For(Entry.ModId)
//             .RegisterReadyChild<NCard, FgoCardNpUi>(
//                 "card_np_ui",
//                 static _ => new FgoCardNpUi(),
//                 static (_, _) => { },
//                 new NodeAttachmentOptions
//                 {
//                     Name = "FgoCardNpUi",
//                     DuplicatePolicy = NodeAttachmentDuplicatePolicy.ReuseExistingByName,
//                     AttachParentSelector = static parent =>
//                         parent is NCard { Body: { } body } ? body : parent,
//                 });
//     }
//
//     public override void _Ready()
//     {
//         MouseFilter = MouseFilterEnum.Ignore;
//
//         SetAnchorsPreset(LayoutPreset.TopWide);
//         OffsetLeft = 16;
//         OffsetRight = -16;
//         OffsetTop = 480;
//         OffsetBottom = 420;
//
//         // Background
//         var bg = new ColorRect
//         {
//             MouseFilter = MouseFilterEnum.Ignore,
//             Color = new Color(0, 0, 0, 0.65f),
//         };
//         bg.SetAnchorsPreset(LayoutPreset.FullRect);
//         AddChild(bg);
//
//         // Progress fill
//         _progressFill = new ColorRect
//         {
//             MouseFilter = MouseFilterEnum.Ignore,
//             Color = new Color(0.2f, 0.5f, 1f, 0.85f),
//         };
//         _progressFill.SetAnchorsPreset(LayoutPreset.LeftWide);
//         AddChild(_progressFill);
//
//         // NP text
//         _npLabel = new Label
//         {
//             MouseFilter = MouseFilterEnum.Ignore,
//             Text = "0",
//             HorizontalAlignment = HorizontalAlignment.Center,
//             VerticalAlignment = VerticalAlignment.Center,
//         };
//         _npLabel.SetAnchorsPreset(LayoutPreset.FullRect);
//         _npLabel.AddThemeFontSizeOverride("font_size", 24);
//         _npLabel.AddThemeColorOverride("font_color", Colors.White);
//         _npLabel.AddThemeColorOverride("font_outline_color", Colors.Black);
//         _npLabel.AddThemeConstantOverride("outline_size", 3);
//         AddChild(_npLabel);
//
//         Visible = false;
//     }
//
//     public override void _Process(double delta)
//     {
//         // Only show in combat
//         if (CombatManager.Instance == null ||
//             (!CombatManager.Instance.IsInProgress && !CombatManager.Instance.IsStarting))
//         {
//             if (Visible)
//                 Visible = false;
//             return;
//         }
//
//         var resources = ModelDb.Singleton<FgoPlayerResources>();
//         if (resources == null)
//         {
//             Visible = false;
//             return;
//         }
//
//         var np = resources.Np;
//         if (np == _lastNp)
//             return;
//         _lastNp = np;
//
//         _npLabel.Text = np.ToString();
//
//         // Update progress bar fill
//         var progress = Mathf.Clamp((float)np / NpMax, 0f, 1f);
//         var barWidth = Size.X * progress;
//         _progressFill.Size = new Vector2(barWidth, Size.Y);
//
//         // Color based on NP level
//         _progressFill.Color = np switch
//         {
//             >= 300 => new Color(1f, 0.84f, 0f, 0.9f),       // Gold
//             >= 200 => new Color(1f, 0.6f, 0.2f, 0.9f),       // Orange
//             >= 100 => new Color(1f, 0.85f, 0.3f, 0.85f),     // Yellow-gold
//             _ => new Color(0.2f, 0.5f, 1f, 0.85f)            // Blue
//         };
//
//         Visible = true;
//     }
// }
