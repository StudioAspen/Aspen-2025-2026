using UnityEngine;

namespace CharonsCorner.Runtime
{
    public static class SaveManager
    {
        public static SaveStore SettingsStore { get; private set; }
        public static SaveStore GameStore { get; private set; }

        static SaveManager()
        {
            InitializeSaveStores();
        }

        private static void InitializeSaveStores()
        {
            SettingsStore = new SaveStore("Settings", false);
            GameStore = new SaveStore("Data", true);
        }
    }
}
