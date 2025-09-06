using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;

namespace ItemPickupHighlighter.Behaviors
{

    public class EntityItemBehaviorNameTag : EntityBehavior, IRenderer
    {
        protected LoadedTexture nameTagTexture = null;
        protected bool showNameTagOnlyWhenTargeted = false;
        protected int _RenderRange = 999;
        protected int totalNearby;
        private protected EntityItemBehaviorNameTag parentBehavior;

        ICoreClientAPI capi;


        /// <summary>
        /// The display name for the entity.
        /// </summary>
        public string DisplayName
        {
            get
            {
                //if (capi != null && TriggeredNameReveal && !IsNameRevealedFor(capi.World.Player.PlayerUID)) return UnrevealedDisplayName;
                return entity.GetName();
            }
        }

        public string UnrevealedDisplayName { get; set; }

        /// <summary>
        /// Whether or not to show the nametag constantly or only when being looked at.
        /// </summary>
        public bool ShowOnlyWhenTargeted
        {
            get { return entity.WatchedAttributes.GetTreeAttribute("nametag")?.GetBool("showtagonlywhentargeted") == true; }
            set { entity.WatchedAttributes.GetTreeAttribute("nametag")?.SetBool("showtagonlywhentargeted", value); }
        }


        public int RenderRange
        {
            get { return _RenderRange; }
            set { _RenderRange = value; }
        }

        public double RenderOrder => 1;

        public EntityItemBehaviorNameTag(EntityItem entity) : base(entity)
        {
            capi = entity.World.Api as ICoreClientAPI;
            if (capi != null)
            {
                capi.Event.RegisterRenderer(this, EnumRenderStage.Ortho, "itemnametag");
            }
            UpdateDisplayText();
        }


        public void OnRenderFrame(float deltaTime, EnumRenderStage stage)
        {
            // If parentBehavior is defined then some other entity is displaying the nametag
            if (parentBehavior != null) return;

            IRenderAPI rapi = capi.Render;

            Vec3d tagPos = new Vec3d(entity.Pos.X, entity.Pos.Y, entity.Pos.Z);

            Vec3d pos = MatrixToolsd.Project(tagPos, rapi.PerspectiveProjectionMat, rapi.PerspectiveViewMat, rapi.FrameWidth, rapi.FrameHeight);

            // Z negative seems to indicate that the name tag is behind us \o/
            if (pos.Z < 0) return;

            float scale = 4f / Math.Max(1, (float)pos.Z);

            float cappedScale = Math.Min(1f, scale);
            if (cappedScale > 0.75f) cappedScale = 0.75f + (cappedScale - 0.75f) / 2;

            float offY = 0;

            if (nameTagTexture != null && (!ShowOnlyWhenTargeted || capi.World.Player.CurrentEntitySelection?.Entity == entity))
            {
                float posx = (float)pos.X - cappedScale * nameTagTexture.Width / 2;
                float posy = rapi.FrameHeight - (float)pos.Y - (nameTagTexture.Height * Math.Max(0, cappedScale));

                rapi.Render2DTexture(
                    nameTagTexture.TextureId, posx, posy, cappedScale * nameTagTexture.Width, cappedScale * nameTagTexture.Height, 20
                );

                offY += nameTagTexture.Height;
            }
        }

        public void SetParentBehavior(ref EntityItemBehaviorNameTag behavior)
        {
            parentBehavior = behavior;
        }

        public bool HasParentBehavior()
        {
            return parentBehavior != null;
        }

        public void AggregateNearby(int itemStackSize)
        {
            totalNearby += itemStackSize;
            UpdateDisplayText();
        }

        public void Dispose()
        {
            if (nameTagTexture != null)
            {
                nameTagTexture.Dispose();
                nameTagTexture = null;
            }

            if (capi != null)
            {
                capi.Event.UnregisterRenderer(this, EnumRenderStage.Ortho);
            }
        }



        public override void OnEntityDeath(DamageSource damageSourceForDeath)
        {
            Dispose();
        }
        public override void OnEntityDespawn(EntityDespawnData despawn)
        {
            Dispose();
        }

        public override string PropertyName()
        {
            return "itemdisplayname";
        }

        private void UpdateDisplayText()
        {
            var ent = entity as EntityItem;
            var displayName = ent.Itemstack.GetName();
            var quantity = ent.Itemstack.StackSize + totalNearby;
            if (quantity > 1)
            {
                displayName = quantity.ToString() + "x " + displayName;
            }

            var bgColor = GuiStyle.DialogLightBgColor;
            bgColor[3] = 0.25; // Reduce alpha channel of default color
            nameTagTexture?.Dispose();
            nameTagTexture = capi.Gui.TextTexture.GenUnscaledTextTexture(
                displayName,
                CairoFont.WhiteMediumText().WithColor(ColorUtil.WhiteArgbDouble),
                new TextBackground()
                {
                    FillColor = bgColor,
                    Padding = 3,
                    Radius = GuiStyle.ElementBGRadius,
                    Shade = true,
                }
            );
        }
    }
}
