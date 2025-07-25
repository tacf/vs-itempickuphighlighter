using System.Drawing;
using System.Numerics;
using ConfigLib;
using ImGuiNET;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace ItemPickupHighlighter;

/// <summary>
/// Provides ConfigLib integration for ItemPickupHighlighter mod.
/// </summary>
public class ConfigLibCompatibility
{

    /// <summary>
    /// Registers the ItemPickupHighlighter config screen with ConfigLib.
    /// </summary>
    public ConfigLibCompatibility(ICoreAPI api)
    {
        api.ModLoader.GetModSystem<ConfigLibModSystem>().RegisterCustomConfig(ItemPickupHighlighterModSystem.ModId, (id, buttons) =>
        {
            if (buttons.Save) api.StoreModConfig<ModConfig>(ModConfig.Instance, ItemPickupHighlighterModSystem.ConfigFileName);
            if (buttons.Restore) ModConfig.Instance = api.LoadModConfig<ModConfig>(ItemPickupHighlighterModSystem.ConfigFileName);
            if (buttons.Defaults) ModConfig.Instance = new();
            BuildSettings(ModConfig.Instance, id);
        });
    }

    private void BuildSettings(ModConfig config, string id)
    {
        if (ImGui.CollapsingHeader("Highlighter Behavior"))
        {
            ImGui.Indent();

            // Highlight Distance
            ImGui.Text("Highlight distance (blocks).");
            ImGui.PushItemWidth(200);
            var highlightDistance = config.HighlightDistance;
            if (ImGui.SliderInt("##HighlightDistance", ref highlightDistance, 2, 50, "%d"))
            {
                config.HighlightDistance = highlightDistance;
            }
            ImGui.PopItemWidth();
            ImGui.TextDisabled("How far away items can be highlighted.");

            ImGui.Separator();

            // Continuous Mode
            var continuousMode = config.HighlightContinousMode;
            if (ImGui.Checkbox("Continuous Highlight Mode", ref continuousMode))
            {
                config.HighlightContinousMode = continuousMode;
            }
            ImGui.TextDisabled("If enabled, items are highlighted continuously without pressing the hotkey.");

            ImGui.Unindent();
        }

        if (ImGui.CollapsingHeader("Highlighter Appearance"))
        {
            ImGui.Indent();

            // Highlight Color
            ImGui.Text("Highlight color.");
            ImGui.PushItemWidth(200);
            var highlightColor = new Vector4(ColorUtil.ToRGBAFloats(config.HighlightColor)) ;
            if (ImGui.ColorPicker4("##HighlightColor", ref highlightColor))
            {
                config.HighlightColor = ColorUtil.FromRGBADoubles(new double[] { highlightColor.X, highlightColor.Y, highlightColor.Z, highlightColor.W });
            }
            ImGui.PopItemWidth();
            ImGui.TextDisabled("The color of the highlight.");
        }
    }
} 