using UnityEngine;

namespace RPG.Services
{
    [CreateAssetMenu(fileName = "FootstepLibrary", menuName = "Audio/FootstepLibrary")]
    public class FootstepLibrary : ScriptableObject
    {
        [System.Serializable]
        public class SurfaceClips
        {
            public string SurfaceName;
            public AudioClip[] Clips;
        }

        public SurfaceClips[] Surfaces;

        /// <summary>
        /// Returns a random clip for the given surface. Returns null if surface not found or no clips.
        /// </summary>
        public AudioClip GetRandomClip(string surface)
        {
            foreach (var s in Surfaces)
            {
                if (s.SurfaceName == surface && s.Clips.Length > 0)
                {
                    int idx = Random.Range(0, s.Clips.Length);
                    return s.Clips[idx];
                }
            }
            return null;
        }
    }
}