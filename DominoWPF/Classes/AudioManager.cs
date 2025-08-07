using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave; // nuget install NAudio
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.IO;

namespace DominoWPF.Classes
{

    public class AudioManager
    {
        private WaveOutEvent _musicOutput;
        private AudioFileReader _musicReader;

        private WaveOutEvent _sfxOutput;
        private MixingSampleProvider _sfxMixer;

        private WaveOutEvent _voiceOutput;
        private AudioFileReader _voiceReader;

        public AudioManager()
        {
            _sfxMixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(44100, 2))
            {
                ReadFully = true
            };
            _sfxOutput = new WaveOutEvent();
            _sfxOutput.Init(_sfxMixer);
            _sfxOutput.Play();
        }

        public void PlayMusic(string path, float volume = 0.5f)
        {
            StopMusic();
            _musicReader = new AudioFileReader(path) { Volume = volume };
            var loop = new LoopStream(_musicReader);
            _musicOutput = new WaveOutEvent();
            _musicOutput.Init(loop);
            _musicOutput.Play();
        }

        public void StopMusic()
        {
            _musicOutput?.Stop();
            _musicOutput?.Dispose();
            _musicReader?.Dispose();
            _musicOutput = null;
            _musicReader = null;
        }

        public void PlaySfx(string path, float volume = 1.0f)
        {
            var reader = new AudioFileReader(path) { Volume = volume };
            var sfxProvider = reader.ToSampleProvider();
            _sfxMixer.AddMixerInput(sfxProvider);
        }

        public void PlayVoice(string path, float volume = 1.0f)
        {
            _voiceOutput?.Stop();
            _voiceOutput?.Dispose();
            _voiceReader?.Dispose();

            _voiceReader = new AudioFileReader(path) { Volume = volume };
            _voiceOutput = new WaveOutEvent();
            _voiceOutput.Init(_voiceReader);
            _voiceOutput.Play();
        }

        public void StopVoice()
        {
            _voiceOutput?.Stop();
            _voiceOutput?.Dispose();
            _voiceReader?.Dispose();
            _voiceOutput = null;
            _voiceReader = null;
        }

        public void Dispose()
        {
            StopMusic();
            StopVoice();
            _sfxOutput?.Stop();
            _sfxOutput?.Dispose();
        }
    }

}
