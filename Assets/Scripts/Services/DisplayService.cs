using UnityEngine;

namespace RPG.Services
{
    /// <summary>
    /// Manages display settings including resolution and fullscreen.
    /// </summary>
    public class DisplayService
    {
        public DisplayService()
        {
            InitializeDisplay();
        }

        void InitializeDisplay()
        {
            // Steam Deck resolution as default
            SetResolution(1280, 800, true);
        }

        public void SetResolution(int width, int height, bool fullscreen)
        {
            Screen.SetResolution(width, height, fullscreen);
        }
    }
}
