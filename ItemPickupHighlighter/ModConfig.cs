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
        public bool HighlightContinousMode { get; set; } = false;

        /// <summary>
        /// Highlight Color.
        /// </summary>
        public int HighlightColor { get; set; } = ColorUtil.WhiteArgb;
    }
}