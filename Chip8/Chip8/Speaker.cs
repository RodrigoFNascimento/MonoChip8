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
            _soundEffectInstance = _soundEffect.CreateInstance();
        }

        public void Play()
        {
            if (_soundEffectInstance.State != SoundState.Playing)
                _soundEffectInstance.Play();
        }
        
        public void Stop()
        {
            _soundEffectInstance.Stop();
        }

        public void LowerVolume()
        {
            if (_soundEffectInstance.Volume - .1f >= 0f)
                _soundEffectInstance.Volume -= 0.1f;
        }

        public void RaiseVolume()
        {
            if (_soundEffectInstance.Volume + .1f <= 1f)
                _soundEffectInstance.Volume += .1f;
        }
    }
}
