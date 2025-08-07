using DominoWPF.Classes;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DominoWPF
{
    public partial class AudioSettingsWindow : Window
    {
        private AudioManager _audio;
        private VoiceManager _voiceManager;


        public AudioSettingsWindow(AudioManager audio, VoiceManager voiceManager = null)
        {
            InitializeComponent();
            _audio = audio;
            _voiceManager = voiceManager;

            ChangeWindowSize();
            InitializeSliders();
        }

        public void SetPosition(Window parentWindow)
        {
            if (parentWindow != null)
            {
                this.Left = parentWindow.Left + parentWindow.Width - this.Width - 10;
                this.Top = parentWindow.Top + 10;
            }
        }

        private void ChangeWindowSize()
        {
            this.Width += 16;
            this.Height += 25;
        }

        private void InitializeSliders()
        {
            bgmVolumeSlider.Value = _audio.MasterBgmVolume * 100;
            sfxVolumeSlider.Value = _audio.MasterSfxVolume * 100;
            voiceVolumeSlider.Value = _audio.MasterVoiceVolume * 100;

            UpdateVolumeLabels();
        }

        private void UpdateVolumeLabels()
        {
            bgmVolumeLabel.Content = $"{(int)bgmVolumeSlider.Value}%";
            sfxVolumeLabel.Content = $"{(int)sfxVolumeSlider.Value}%";
            voiceVolumeLabel.Content = $"{(int)voiceVolumeSlider.Value}%";
        }

        private void BgmVolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (bgmVolumeLabel == null || _audio == null) return;

            float newVolume = (float)(e.NewValue / 100.0);
            _audio.SetBgmVolume(newVolume);
            bgmVolumeLabel.Content = $"{(int)e.NewValue}%";
        }

        private void SfxVolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sfxVolumeLabel == null || _audio == null) return;

            float newVolume = (float)(e.NewValue / 100.0);
            _audio.SetSfxVolume(newVolume);
            sfxVolumeLabel.Content = $"{(int)e.NewValue}%";

            if (newVolume > 0)
            {
                try
                {
                    _audio.PlaySfx("Sounds/hover.wav", newVolume);
                }
                catch { }
            }
        }

        private void VoiceVolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (voiceVolumeLabel == null || _audio == null) return;

            float newVolume = (float)(e.NewValue / 100.0);
            _audio.SetVoiceVolume(newVolume);
            voiceVolumeLabel.Content = $"{(int)e.NewValue}%";
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            bgmVolumeSlider.Value = 50;
            sfxVolumeSlider.Value = 100;
            voiceVolumeSlider.Value = 100;

            _audio.SetBgmVolume(0.5f);
            _audio.SetSfxVolume(1.0f);
            _audio.SetVoiceVolume(1.0f);

            try
            {
                _audio.PlaySfx("Sounds/select.wav");
            }
            catch { }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _audio.PlaySfx("Sounds/menu_exit.wav");
            }
            catch { }

            this.Close();
        }

        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
            if (_audio != null && _audio.MasterSfxVolume > 0)
            {
                try
                {
                    _audio.PlaySfx("Sounds/hover.wav", _audio.MasterSfxVolume * 0.7f);
                }
                catch { }
            }
        }

        public float GetBgmVolume() => _audio.MasterBgmVolume;
        public float GetSfxVolume() => _audio.MasterSfxVolume;
        public float GetVoiceVolume() => _audio.MasterVoiceVolume;
    }
}