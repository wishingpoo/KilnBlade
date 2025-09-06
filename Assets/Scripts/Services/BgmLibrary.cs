using UnityEngine;

namespace RPG.Services
{
    [CreateAssetMenu(fileName = "BgmLibrary", menuName = "Audio/BGM Library")]
    public class BgmLibrary : ScriptableObject
    {
        [System.Serializable]
        public class BgmEntry
        {
            public string key;
            public AudioClip clip;
        }

        public BgmEntry[] entries;

        public AudioClip GetClip(string key)
        {
            foreach (var entry in entries)
            {
                if (entry.key == key)
                    return entry.clip;
            }
            Debug.LogWarning($"BgmLibrary: No clip found for key '{key}'");
            return null;
        }
    }
}
