namespace RPG.Services
{
    public sealed class BgmSystem
    {
        private const string DEFAULT_BGM = "River";

        private readonly IAudioManager _audio;
        private readonly BgmLibrary _library;

        public BgmSystem(IAudioManager audio, BgmLibrary library)
        {
            _audio = audio;
            _library = library;
            
            // Just start playing the default track
            _library.LoadClip(DEFAULT_BGM, clip =>
            {
                _audio.PlayMusic(clip, 1f);
            });
        }
    }
}
