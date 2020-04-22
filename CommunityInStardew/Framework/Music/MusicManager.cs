using Microsoft.Xna.Framework.Audio;
using NAudio.Wave;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunityInStardew.Music
{
    public class MusicManager
    {
        public SoundEffectInstance CurrentSong;

        private Dictionary<string, SoundEffectInstance> _songs = new Dictionary<string, SoundEffectInstance>();
        private const string MUSIC_FOLDER_NAME = @"Mods\CommunityInStardew\Content\Assets\music";

        public MusicManager()
        {
            CommunityMod.ModHelper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;

            LoadMusicFiles();
        }

        private void GameLoop_UpdateTicked(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
        {
            if (this.CurrentSong != null && this.CurrentSong.State == SoundState.Playing
                && Game1.currentSong != null && !Game1.currentSong.Name.ToLower().Contains("ambient"))
            {
                Game1.currentSong.Stop(AudioStopOptions.Immediate);
                Game1.currentSong.Stop(AudioStopOptions.AsAuthored);
            }
        }

        private void LoadMusicFiles()
        {
            DirectoryInfo songFolder = new DirectoryInfo(MUSIC_FOLDER_NAME);

            foreach (FileInfo file in songFolder.GetFiles())
            {
                string name = Path.GetFileNameWithoutExtension(file.Name);
                SoundEffect effect = null;

                using (Stream waveFileStream = File.OpenRead(file.FullName))
                {
                    switch (file.Extension)
                    {
                        case ".wav":
                            effect = SoundEffect.FromStream(waveFileStream);
                            break;

                        case ".mp3":
                            using (Mp3FileReader reader = new Mp3FileReader(waveFileStream))
                            using (WaveStream pcmStream = WaveFormatConversionStream.CreatePcmStream(reader))
                            {
                                string tempPath = Path.Combine(songFolder.FullName, $"{name}.wav");

                                WaveFileWriter.CreateWaveFile(tempPath, pcmStream);
                                using (Stream tempStream = File.OpenRead(tempPath))
                                    effect = SoundEffect.FromStream(tempStream);
                                File.Delete(tempPath);
                            }
                            break;

                        default:
                            CommunityMod.ModMonitor.Log($"Unsupported file extension {file.Extension}.", LogLevel.Warn);
                            break;
                    }
                }

                SoundEffectInstance instance = effect.CreateInstance();
                this._songs.Add(name, instance);
            }
        }

        public void Play(string songKey)
        {
            if (!this._songs.TryGetValue(songKey, out SoundEffectInstance song))
            {
                CommunityMod.ModMonitor.Log($"Unable to load song by the key {songKey}", LogLevel.Error);
                return;
            }

            this.CurrentSong = song;
            this.CurrentSong.Volume = 0.2f;
            this.CurrentSong.Play();
        }

        public void Stop()
        {
            this.CurrentSong?.Stop(true);
        }
    }
}
