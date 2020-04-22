using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PyTK.CustomTV;
using StardewModdingAPI;
using CommunityInStardew.Music;
using StardewValley;
using StardewValley.Objects;
using System.IO;
using Newtonsoft.Json;

namespace CommunityInStardew.Television
{
    public class TvShow
    {
        private static Dictionary<string, TvShow> _shows = new Dictionary<string, TvShow>();

        private TvShowConfig _config;
        private Texture2D _spriteSheet;

        private bool _showAdded = false;
        private bool _showToday = false;
        private TvScript _todaysScript;

        public TvShow(TvShowConfig config)
        {
            _config = config;

            _spriteSheet = CommunityMod.ModHelper.Content.Load<Texture2D>(_config.TexturePath);

            CommunityMod.ModHelper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
            CommunityMod.ModHelper.Events.GameLoop.TimeChanged += GameLoop_TimeChanged;
        }

        private void GameLoop_TimeChanged(object sender, StardewModdingAPI.Events.TimeChangedEventArgs e)
        {
            CheckIfShowActiveAtCurrentTime();
        }

        private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            CheckIfShowHappensToday();
            CheckIfShowActiveAtCurrentTime();
        }

        private void CheckIfShowHappensToday()
        {
            var r = new Random(Game1.Date.TotalDays);
            double roll = r.NextDouble();
            _showToday = roll < 0.9;
            _todaysScript = _config.Scripts[r.Next(0, _config.Scripts.Length)];
            CommunityMod.ModMonitor.Log($"Show Today? {_showToday} {roll}", LogLevel.Info);
        }

        private void CheckIfShowActiveAtCurrentTime()
        {
            if (_showToday)
            {
                if (Game1.timeOfDay < _todaysScript.EndTime && Game1.timeOfDay >= _todaysScript.StartTime)
                {
                    if (!_showAdded)
                    {
                        CustomTVMod.addChannel(_config.ShowId, _config.ShowTitle, BeginShow);
                        _showAdded = true;
                    }
                }
                else if (_showAdded)
                {
                    CustomTVMod.removeChannel(_config.ShowId);
                    _showAdded = false;
                }
            }
        }

        private void BeginShow(TV tv, TemporaryAnimatedSprite sprite, Farmer who, string answer)
        {
            ContinueShow(tv, _todaysScript, 0);
        }

        private void ContinueShow(TV tv, TvScript script, int index)
        {
            ScriptPage page = script.Pages[index];

            TemporaryAnimatedSprite BackgroundSprite = new TemporaryAnimatedSprite()
            {
                paused = true,
                texture = _spriteSheet,
                sourceRect = new Rectangle(page.X, page.Y, 42, 28),
                interval = 9999f,
                animationLength = 999999,
                totalNumberOfLoops = 999999,
                position = tv.getScreenPosition(),
                flicker = false,
                flipped = false,
                layerDepth = (float)((tv.boundingBox.Bottom - 1) / 10000.0 + 9.99999974737875E-06),
                alphaFade = 0f,
                color = Color.White,
                scale = tv.getScreenSizeModifier(),
                scaleChange = 0.0f,
                rotation = 0.0f,
                rotationChange = 0.0f,
                local = false
            };

            if (index == script.Pages.Length - 1)
            {
                CustomTVMod.showProgram(BackgroundSprite, script.Pages[index].Text, CustomTVMod.endProgram, BackgroundSprite);
            }
            else
            {
                CustomTVMod.showProgram(BackgroundSprite, script.Pages[index].Text, () => ContinueShow(tv, script, ++index), BackgroundSprite);
            }
        }

        public static void LoadShows()
        {
            var dir = new DirectoryInfo(Path.Combine(CommunityMod.ModHelper.DirectoryPath, @"Content\Shows"));
            foreach (var file in dir.GetFiles("*.json"))
            {
                using (var streamReader = new StreamReader(file.OpenRead()))
                {
                    var config = JsonConvert.DeserializeObject<TvShowConfig>(streamReader.ReadToEnd());
                    _shows.Add(config.ShowTitle, new TvShow(config));
                }
            }
        }

        public static TvShow GetShow(string showName)
        {
            return _shows[showName];
        }
    }
}
