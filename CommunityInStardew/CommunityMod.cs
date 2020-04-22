using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using PyTK.CustomTV;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using CommunityInStardew.Framework;
using CommunityInStardew.Music;
using CommunityInStardew.Television;
using CommunityInStardew.UI;
using StardewValley;
using StardewValley.Objects;
using StardewValley.Menus;

namespace CommunityInStardew
{
    public class CommunityMod : Mod
    {
        const int HOME_TIME_INTERVAL = 14000;

        public static IModHelper ModHelper;
        public static IMonitor ModMonitor;

        private bool _shouldEnable => Context.IsWorldReady && Context.IsMainPlayer;

        
        private readonly TimeHelper _timeHelper = new TimeHelper();
        private SlowTimeIcon _icon;
        public static MusicManager MusicManager;

        /// <summary>Backing field for <see cref="TickInterval"/>.</summary>
        private int _tickInterval;

        /// <summary>The number of seconds per 10-game-minutes to apply.</summary>
        private int TickInterval
        {
            get => _tickInterval;
            set => _tickInterval = Math.Max(value, 0);
        }

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            ModHelper = helper;
            ModMonitor = Monitor;

            helper.Events.Player.Warped += Player_Warped;
            helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
            helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            this._timeHelper.WhenTickProgressChanged(this.OnTickProgressed);

            TvShow.LoadShows();
            
            MusicManager = new MusicManager();
        }

        private void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            // get the internal asset key for the map file
            string mapAssetKey = this.Helper.Content.GetActualAssetKey("Content/Assets/GreendaleTest.tbin", ContentSource.ModFolder);

            // add the location
            GameLocation location = new GameLocation(mapAssetKey, "GreendaleTest") { IsOutdoors = false, IsFarm = false };
            Game1.locations.Add(location);
        }

        private void GameLoop_DayStarted(object sender, DayStartedEventArgs e)
        {
            UpdateLocationTickLength(Game1.currentLocation);
        }

        private double ScaleTickProgress(double progress, int tickInterval)
        {
            return progress * _timeHelper.CurrentDefaultTickInterval / tickInterval;
        }

        private void GameLoop_UpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            _timeHelper.Update();
        }

        private void OnTickProgressed(object sender, TickProgressChangedEventArgs e)
        {
            if (!this._shouldEnable)
                return;

            if (this.TickInterval == 0)
                this.TickInterval = this._timeHelper.CurrentDefaultTickInterval;

            if (e.TimeChanged)
                this._timeHelper.TickProgress = this.ScaleTickProgress(this._timeHelper.TickProgress, this.TickInterval);
            else
                this._timeHelper.TickProgress = e.PreviousProgress + this.ScaleTickProgress(e.NewProgress - e.PreviousProgress, this.TickInterval);
        }

        private void Player_Warped(object sender, WarpedEventArgs e)
        {
            if (e.IsLocalPlayer)
            {
                if(e.NewLocation.Name == "GreendaleTest")
                {
                    MusicManager.Play("daybreak");
                }
                else
                {
                    MusicManager.Stop();
                }

                UpdateLocationTickLength(e.NewLocation);
            }
        }

        private void UpdateLocationTickLength(GameLocation location)
        {
            if (location.Name == "FarmHouse")
            {
                _icon.Toggle(true);
                this.TickInterval = HOME_TIME_INTERVAL;
                Game1.hudMessages.Add(new HUDMessage("The great wheel of time slows...", 2) { timeLeft = 1000 });
            }
            else
            {
                _icon.Toggle(false);
                TickInterval = this._timeHelper.CurrentDefaultTickInterval;
            }
        }
    }
}
