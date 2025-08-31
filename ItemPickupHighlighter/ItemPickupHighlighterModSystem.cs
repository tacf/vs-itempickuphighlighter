using System;
using System.Collections.Generic;
using ItemPickupHighlighter.Behaviors;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;

namespace ItemPickupHighlighter;

public class ItemPickupHighlighterModSystem : ModSystem
{
    private ICoreClientAPI _capi;
    public override bool ShouldLoad(EnumAppSide forSide) => forSide == EnumAppSide.Client;
    public static string ConfigFileName = "ItemPickupHighlighter.json";
    public static string ModId = "ItemPickupHighlighter";
    private HashSet<Entity> _entitiesNametags = new();

    public override void Start(ICoreAPI api)
    {
        api.World.Logger.Event("started '" + ModId + "'");   
        api.RegisterEntityBehaviorClass("itemnametag", typeof(EntityItemBehaviorNameTag));
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
            HighlightNearbyItems();
    }

    private bool HighlightItems(KeyCombination hotkey)
    {
        if (ModConfig.Instance.HighlightContinousMode) return false;
        HighlightNearbyItems();
        return true;
    }

    private void HighlightNearbyItems()
    {
        var et = _capi.World.GetEntitiesAround(_capi.World.Player.Entity.SidedPos.XYZ,
            ModConfig.Instance.HighlightDistance,
            ModConfig.Instance.HighlightDistance);

        if (!_capi.World.Player.Entity.Controls.Sneak && (_entitiesNametags.Count > 0))
        {
            foreach (var en in _entitiesNametags)
            {
                if (en.HasBehavior<EntityItemBehaviorNameTag>())
                {
                    var eb = en.GetBehavior<EntityItemBehaviorNameTag>();
                    if (eb.HasParentBehavior())
                    {
                        // Remove from nearby parent tag
                        eb.AggregateNearby(-(en as EntityItem).Itemstack.StackSize);
                    }
                    en.RemoveBehavior(eb);
                    eb.Dispose();
                    _entitiesNametags.Remove(en);
                }
            }
        }

        foreach (var ent in et)
        {
            // TODO: Change logic to share the Behavior Entity object between all entities
            // Behavior Entity should keep a list of the associated entities
            if (ent is EntityItem || ent.Class.Contains("projectile", StringComparison.OrdinalIgnoreCase))
            {
                // Entity isn't already associated with a nametag
                // Entity is on the ground (has collided vertically)
                // Player is holding shift
                // We exclude projectiles from nametags for now (this needs to be worked -- code is a mess)
                if (!_entitiesNametags.Contains(ent) 
                    && ent.CollidedVertically 
                    && _capi.World.Player.Entity.Controls.Sneak
                    && !ent.Class.Contains("projectile", StringComparison.OrdinalIgnoreCase))
                {
                    // Find any close similar entities and aggregate nametag display
                    // (prevents displaying several overlapping nametags when dropping a full stacks -- game seems to split into itemstacks of 4 items)
                    var nearbySimilarItems = _capi.World.GetEntitiesAround(ent.SidedPos.XYZ, 2, 2, 
                        e => (e is EntityItem) 
                            && (e as EntityItem).Itemstack.Id == (ent as EntityItem)?.Itemstack.Id 
                            && e.HasBehavior<EntityItemBehaviorNameTag>() 
                            && !e.GetBehavior<EntityItemBehaviorNameTag>().HasParentBehavior());
                    var nearbyParentBehavior = nearbySimilarItems.Length > 0 ? nearbySimilarItems[0].GetBehavior<EntityItemBehaviorNameTag>() : null;
                        // No similar entities nearby, generate new nametag
                    if (_entitiesNametags.Add(ent) && ent.GetBehavior<EntityItemBehaviorNameTag>() == null)
                    {

                        var eb = new EntityItemBehaviorNameTag(ent as EntityItem);
                        if (nearbyParentBehavior != null)
                        {
                            nearbyParentBehavior.AggregateNearby((ent as EntityItem).Itemstack.StackSize);
                            eb.SetParentBehavior(ref nearbyParentBehavior);
                        }
                        ent.AddBehavior(eb);
                    }

                }
                if (ModConfig.Instance.HighlightContinousMode)
                {
                    // TODO: We can probably move this into the behavior entity and also share among nearby?!
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
}
