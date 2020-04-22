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
        public static IModHelper ModHelper;
        public static IMonitor ModMonitor;
        public static MusicManager MusicManager;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            ModHelper = helper;
            ModMonitor = Monitor;

            helper.Events.Player.Warped += Player_Warped;
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;

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
            }
        }
    }
}
