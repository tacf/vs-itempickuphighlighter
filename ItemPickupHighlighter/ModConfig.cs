using Vintagestory.API.MathTools;

namespace ItemPickupHighlighter
{
    class ModConfig
    {
        public static ModConfig Instance { get; set; } = new ModConfig();
        
        /// <summary>
        /// Highlight distance.
        /// </summary>
        public int HighlightDistance { get { return _highlightDistance; } set { _highlightDistance = value >= 2 ? value : 2; } }
        private int _highlightDistance = 10;

        /// <summary>
        /// Highlight Continous Mode.
        /// </summary>
        private bool _continousMode = false;
        public bool HighlightContinousMode { get { return _continousMode; } set { _continousMode = value; } }

        /// <summary>
        /// Highlight Color.
        /// </summary>
        public int HighlightColor { get; set; } = ColorUtil.WhiteArgb;

        /// <summary>
        /// Enabled/Disable Feature
        /// </summary>
        private bool _showItemNames = false;
        public bool ShowItemNames { get { return _showItemNames; } set { _showItemNames = value; } }
        
        /// <summary>
        /// Enabled/Disable Highlight for Projectiles
        /// </summary>
        private bool _highlightProjectiles = true;
        public bool HighlightProjectiles { get { return _highlightProjectiles; } set { _highlightProjectiles = value; } }
        
        /// <summary>
        /// Enabled/Disable Highlight for Items
        /// </summary>
        private bool _highlightItems = true;
        public bool HighlightItems { get { return _highlightItems; } set { _highlightItems = value; } }
    }
}