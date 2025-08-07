using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;
using static System.Net.WebRequestMethods;

namespace DominoWPF.Classes
{
    public class VoiceManager
    {
        private readonly string _baseFolder;
        private readonly Dictionary<int, Dictionary<string, List<string>>> _voiceLines;
        private readonly Random _rng;
        private AudioManager _audio;

        public VoiceManager(AudioManager audio, string baseFolder = "Sounds")
        {
            _audio = audio;
            _baseFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, baseFolder);
            _voiceLines = new Dictionary<int, Dictionary<string, List<string>>>();
            _rng = new Random();
            LoadVoiceLines();
        }

        private void LoadVoiceLines()
        {
            if (!Directory.Exists(_baseFolder))
            {
                //MessageBox.Show($"[VoiceManager] Base folder '{_baseFolder}' not found.");
                return;
            }

            var playerDirs = Directory.GetDirectories(_baseFolder);
            //MessageBox.Show($"[VoiceManager] Found {playerDirs.Length} player folders.");

            foreach (var playerDir in playerDirs)
            {
                string folderName = Path.GetFileName(playerDir);

                if (!folderName.StartsWith("player"))
                {
                    //MessageBox.Show($"[VoiceManager] Skipped unknown folder '{folderName}'.");
                    continue;
                }

                if (!int.TryParse(folderName.Replace("player", ""), out int folderPlayerNumber))
                {
                    //MessageBox.Show($"[VoiceManager] Invalid player folder '{folderName}'.");
                    continue;
                }

                int playerIndex = folderPlayerNumber - 1;
                if (playerIndex < 0)
                {
                    //MessageBox.Show($"[VoiceManager] Ignoring invalid player index: {playerIndex}");
                    continue;
                }

                var voiceTypes = new Dictionary<string, List<string>>();

                foreach (var typeDir in Directory.GetDirectories(playerDir))
                {
                    string voiceType = Path.GetFileName(typeDir);
                    var files = Directory.GetFiles(typeDir, "*.*")
                        .Where(f => f.EndsWith(".wav") || f.EndsWith(".mp3") || f.EndsWith(".aiff"))
                        .ToList();
                    //MessageBox.Show($"  -> Type '{voiceType}' has {files.Count} file(s).");
                    voiceTypes[voiceType] = files;
                }

                _voiceLines[playerIndex] = voiceTypes;
            }
        }

        public void PlayRandom(int playerIndex, string voiceType, float volume = -1f)
        {
            if (_voiceLines.TryGetValue(playerIndex, out var voiceTypeDict))
            {
                if (voiceTypeDict.TryGetValue(voiceType, out var voiceFiles) && voiceFiles.Count > 0)
                {
                    var randomFile = voiceFiles[_rng.Next(voiceFiles.Count)];
                    //MessageBox.Show($"[VoiceManager] Playing random voice line: {randomFile}");

                    // Use the AudioManager's voice volume if no specific volume is provided
                    _audio.PlayVoice(randomFile, volume);
                }
                //else
                //{
                //    MessageBox.Show($"[VoiceManager] No voice lines found for player {playerIndex}, type '{voiceType}'.");
                //}
            }
            //else
            //{
            //    MessageBox.Show($"[VoiceManager] No voice lines found for player index {playerIndex}.");
            //}
        }

        public bool HasVoiceLines(int playerIndex, string voiceType)
        {
            return _voiceLines.TryGetValue(playerIndex, out var voiceTypeDict) &&
                   voiceTypeDict.TryGetValue(voiceType, out var voiceFiles) &&
                   voiceFiles.Count > 0;
        }

        public int GetVoiceLineCount(int playerIndex, string voiceType)
        {
            if (_voiceLines.TryGetValue(playerIndex, out var voiceTypeDict) &&
                voiceTypeDict.TryGetValue(voiceType, out var voiceFiles))
            {
                return voiceFiles.Count;
            }
            return 0;
        }
    }
}