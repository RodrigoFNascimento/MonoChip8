using Microsoft.Xna.Framework.Audio;

namespace Chip8.Chip8
{
    public class Speaker
    {
        private SoundEffect _soundEffect;
        private SoundEffectInstance _soundEffectInstance;

        public void LoadSoundEffect(SoundEffect sound)
        {
            _soundEffect = sound;
        }

        public void Play()
        {
            if (_soundEffectInstance == null)
                _soundEffectInstance = _soundEffect.CreateInstance();
            if (_soundEffectInstance.State != SoundState.Playing)
                _soundEffectInstance.Play();
        }
        
        public void Stop()
        {
            if (_soundEffectInstance == null)
                _soundEffectInstance = _soundEffect.CreateInstance();
            _soundEffectInstance.Stop();
        }
    }
}
