using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace ItemPickupHighlighter;

public class ItemPickupHighlighterModSystem : ModSystem
{
    private ICoreClientAPI _capi;
    public override bool ShouldLoad(EnumAppSide forSide) => forSide == EnumAppSide.Client;
    public static string ConfigFileName = "ItemPickupHighlighter.json";
    public static string ModId = "ItemPickupHighlighter";

    public override void Start(ICoreAPI api)
    {
        api.World.Logger.Event("started '" + ModId + "'");   
    }

        public override void StartPre(ICoreAPI api)
    {
        switch (api.Side)
        {
            case EnumAppSide.Client:
                try
                {
                    ModConfig file;
                    if ((file = api.LoadModConfig<ModConfig>(ConfigFileName)) == null)
                    {
                        api.StoreModConfig<ModConfig>(ModConfig.Instance, ConfigFileName);
                    }
                    else
                    {
                        ModConfig.Instance = file;
                    }
                }
                catch
                {
                    api.StoreModConfig<ModConfig>(ModConfig.Instance, ConfigFileName);
                }  
                break;
        }

        if (api.ModLoader.IsModEnabled("configlib"))
        {
            _ = new ConfigLibCompatibility(api);
        }
    }

    public override void StartClientSide(ICoreClientAPI api)
    {
        _capi = api;
        _capi.Input.RegisterHotKey(Constants.HotKeyIdentifier, Constants.HotKeyLabel, GlKeys.F, shiftPressed: true);
        _capi.Input.RegisterHotKey(Constants.HotKeyIdentifierToggle, Constants.HotKeyLabelToggle, GlKeys.F, shiftPressed: true, ctrlPressed: true);
        _capi.Input.SetHotKeyHandler(Constants.HotKeyIdentifier, HighlightItems);
        _capi.Input.SetHotKeyHandler(Constants.HotKeyIdentifierToggle, HighlightItemsToggle);
        _capi.Event.RegisterGameTickListener(OnGameTick, 100, 0);
        
        base.StartClientSide(_capi);
    }

    private bool HighlightItemsToggle(KeyCombination hotkey)
    {
        ModConfig.Instance.HighlightContinousMode = !ModConfig.Instance.HighlightContinousMode;
        var status = "<font weight=\"bold\" color=\"#84ff84\">Enabled</font>";
        if (!ModConfig.Instance.HighlightContinousMode)
        {
            status = "<font weight=\"bold\" color=\"#ff8484\">Disabled</font>";
        }
        _capi.TriggerChatMessage("(ItemPickupHighlighter) Continuous Mode: " + status);
        _capi.StoreModConfig<ModConfig>(ModConfig.Instance, ConfigFileName);
        return true;
    }

    private void OnGameTick(float obj)
    {
        if (ModConfig.Instance.HighlightContinousMode)
        {
            HighlightNearbyItems();
        }
    }

    private bool HighlightItems(KeyCombination hotkey)
    {
        if (ModConfig.Instance.HighlightContinousMode) return false;
        HighlightNearbyItems();
        return true;
    }

    private void HighlightNearbyItems()
    {
        var et = _capi.World.Player.Entity.Api.World.GetEntitiesAround(_capi.World.Player.Entity.SidedPos.XYZ,
            ModConfig.Instance.HighlightDistance,
            ModConfig.Instance.HighlightDistance);
        foreach (var ent in et)
        {
            if (ent is EntityItem ||  ent.Class.Contains("projectile", StringComparison.OrdinalIgnoreCase))
            {
                _capi.World.SpawnParticles(new SimpleParticleProperties()
                {
                    MinPos = ent.SidedPos.XYZ,
                    Color = ModConfig.Instance.HighlightColor,
                    MinSize = 0.1f,
                    MaxSize = 0.1f,
                    MinVelocity = new Vec3f(-0.1f, 0.5f, -0.1f),
                    AddVelocity = new Vec3f(0.1f, 1.5f, 0.1f),
                    MinQuantity = 0.7f,
                    LifeLength = 1,
                    WithTerrainCollision = false,
                    LightEmission = ColorUtil.WhiteArgb
                });
            }
        }
    }
}
