using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using System;


namespace CommunityInStardew.UI
{
    public class SlowTimeIcon : IDisposable
    {
        private Color _color = new Color(Color.White.ToVector4());
        private ClickableTextureComponent _icon;

        public void Toggle(bool showLuckOfDay)
        {
            CommunityMod.ModHelper.Events.Player.Warped -= OnWarped;
            CommunityMod.ModHelper.Events.Display.RenderingHud -= OnRenderingHud;

            if (showLuckOfDay)
            {
                AdjustIconXToBlackBorder();
                CommunityMod.ModHelper.Events.Player.Warped += OnWarped;
                CommunityMod.ModHelper.Events.Display.RenderingHud += OnRenderingHud;
            }
        }

        public void Dispose()
        {
            Toggle(false);
        }


        /// <summary>Raised before drawing the HUD (item toolbar, clock, etc) to the screen. The vanilla HUD may be hidden at this point (e.g. because a menu is open).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnRenderingHud(object sender, RenderingHudEventArgs e)
        {
            // draw dice icon
            if (!Game1.eventUp)
            {
                Point iconPosition = GetNewIconPosition();
                _icon.bounds.X = iconPosition.X;
                _icon.bounds.Y = iconPosition.Y;
                _icon.draw(Game1.spriteBatch, _color, 1f);
            }
        }

        public Point GetNewIconPosition()
        {
            int yPos = Game1.options.zoomButtons ? 290 : 260;
            int xPosition = (int)GetWidthInPlayArea() - 134;


            return new Point(xPosition, yPos);
        }

        public int GetWidthInPlayArea()
        {
            int result = 0;

            if (Game1.isOutdoorMapSmallerThanViewport())
            {
                int right = Game1.graphics.GraphicsDevice.Viewport.TitleSafeArea.Right;
                int totalWidth = Game1.currentLocation.map.Layers[0].LayerWidth * Game1.tileSize;
                int someOtherWidth = Game1.graphics.GraphicsDevice.Viewport.TitleSafeArea.Right - totalWidth;

                result = right - someOtherWidth / 2;
            }
            else
            {
                result = Game1.graphics.GraphicsDevice.Viewport.TitleSafeArea.Right;
            }

            return result;
        }

        /// <summary>Raised after a player warps to a new location.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnWarped(object sender, WarpedEventArgs e)
        {
            // adjust icon X to black border
            if (e.IsLocalPlayer)
            {
                AdjustIconXToBlackBorder();
            }
        }

        private void AdjustIconXToBlackBorder()
        {
            _icon = new ClickableTextureComponent("",
                new Rectangle(GetWidthInPlayArea() - 134,
                    290,
                    100 * Game1.pixelZoom,
                    100 * Game1.pixelZoom),
                "",
                "",
                Game1.mouseCursors,
                new Rectangle(16, 0, 10, 16),
                Game1.pixelZoom,
                false);
        }

    }

}
