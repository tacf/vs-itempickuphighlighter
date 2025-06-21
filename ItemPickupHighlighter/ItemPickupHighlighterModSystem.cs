using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace ItemPickupHighlighter;

public class ItemPickupHighlighterModSystem : ModSystem
{
    private ICoreClientAPI _capi;
    public override bool ShouldLoad(EnumAppSide forSide) => forSide == EnumAppSide.Client;
    private readonly int _highlightRange = 10;

    public override void StartClientSide(ICoreClientAPI api)
    {
        _capi = api;
        _capi.Input.RegisterHotKey(Constants.HotKeyIdentifier, Constants.HotKeyLabel, GlKeys.F, shiftPressed: true);
        _capi.Input.SetHotKeyHandler(Constants.HotKeyIdentifier, HighlightItems);
        
        base.StartClientSide(_capi);
    }

    private bool HighlightItems(KeyCombination hotkey)
    {
        var et = _capi.World.Player.Entity.Api.World.GetEntitiesAround(_capi.World.Player.Entity.SidedPos.XYZ,
            _highlightRange,
            _highlightRange);
        foreach (var ent in et)
        {
            if (ent is EntityItem ||  ent.Class.Contains("projectile", StringComparison.OrdinalIgnoreCase))
            {
                _capi.World.SpawnParticles(new SimpleParticleProperties()
                {
                    MinPos = ent.SidedPos.XYZ,
                    Color = ColorUtil.WhiteArgb,
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

        return true;
    }
}
