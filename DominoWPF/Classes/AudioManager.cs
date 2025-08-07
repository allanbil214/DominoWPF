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
        private LoopStream _loopStream;

        private WaveOutEvent _sfxOutput;
        private MixingSampleProvider _sfxMixer;

        private WaveOutEvent _voiceOutput;
        private AudioFileReader _voiceReader;

        public float MasterBgmVolume { get; set; } = 0.5f;
        public float MasterSfxVolume { get; set; } = 1.0f;
        public float MasterVoiceVolume { get; set; } = 1.0f;

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

        public void PlayMusic(string path, float volume = -1f)
        {
            float actualVolume = volume >= 0 ? volume : MasterBgmVolume;

            StopMusic();
            _musicReader = new AudioFileReader(path) { Volume = actualVolume };
            _loopStream = new LoopStream(_musicReader);
            _musicOutput = new WaveOutEvent();
            _musicOutput.Init(_loopStream);
            _musicOutput.Play();
        }

        public void SetBgmVolume(float volume)
        {
            MasterBgmVolume = Math.Max(0f, Math.Min(1f, volume));

            if (_musicReader != null)
            {
                _musicReader.Volume = MasterBgmVolume;
            }
        }

        public void SetSfxVolume(float volume)
        {
            MasterSfxVolume = Math.Max(0f, Math.Min(1f, volume));
        }

        public void SetVoiceVolume(float volume)
        {
            MasterVoiceVolume = Math.Max(0f, Math.Min(1f, volume));

            if (_voiceReader != null)
            {
                _voiceReader.Volume = MasterVoiceVolume;
            }
        }

        public void StopMusic()
        {
            _musicOutput?.Stop();
            _musicOutput?.Dispose();
            _loopStream?.Dispose();
            _musicReader?.Dispose();
            _musicOutput = null;
            _loopStream = null;
            _musicReader = null;
        }

        public void PlaySfx(string path, float volume = -1f)
        {
            float actualVolume = volume >= 0 ? volume * MasterSfxVolume : MasterSfxVolume;

            if (actualVolume <= 0f) return; 

            try
            {
                var reader = new AudioFileReader(path) { Volume = actualVolume };
                var sfxProvider = reader.ToSampleProvider();
                _sfxMixer.AddMixerInput(sfxProvider);
            }
            catch (Exception)
            {

            }
        }

        public void PlayVoice(string path, float volume = -1f)
        {
            float actualVolume = volume >= 0 ? volume : MasterVoiceVolume;

            if (actualVolume <= 0f) return; 

            _voiceOutput?.Stop();
            _voiceOutput?.Dispose();
            _voiceReader?.Dispose();

            try
            {
                _voiceReader = new AudioFileReader(path) { Volume = actualVolume };
                _voiceOutput = new WaveOutEvent();
                _voiceOutput.Init(_voiceReader);
                _voiceOutput.Play();
            }
            catch (Exception)
            {

            }
        }

        public void StopVoice()
        {
            _voiceOutput?.Stop();
            _voiceOutput?.Dispose();
            _voiceReader?.Dispose();
            _voiceOutput = null;
            _voiceReader = null;
        }

        public bool IsMusicPlaying()
        {
            return _musicOutput?.PlaybackState == PlaybackState.Playing;
        }

        public bool IsVoicePlaying()
        {
            return _voiceOutput?.PlaybackState == PlaybackState.Playing;
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