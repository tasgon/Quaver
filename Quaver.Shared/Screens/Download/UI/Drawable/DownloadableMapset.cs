/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Newtonsoft.Json.Linq;
using Quaver.API.Enums;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens.Select.UI.Banner;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI;
using Wobble.Graphics.UI.Buttons;
using Wobble.Input;
using Color = Microsoft.Xna.Framework.Color;
using ColorHelper = Quaver.Shared.Helpers.ColorHelper;

namespace Quaver.Shared.Screens.Download.UI.Drawable
{
    public class DownloadableMapset : Button
    {
        /// <summary>
        ///     The instance of the download for this mapset.
        /// </summary>
        public MapsetDownload Download { get; private set; }

        /// <summary>
        /// </summary>
        private DownloadScrollContainer Container { get; }

        /// <summary>
        /// </summary>
        public int MapsetId { get; }

        /// <summary>
        ///     The mapset that this represents.
        /// </summary>
        public JToken Mapset { get; }

        /// <summary>
        /// </summary>
        private Sprite Banner { get; }

        /// <summary>
        /// </summary>
        private SpriteTextBitmap Artist { get; }

        /// <summary>
        /// </summary>
        private SpriteTextBitmap Title { get; }

        /// <summary>
        /// </summary>
        private SpriteTextBitmap Modes { get;  }

        /// <summary>
        /// </summary>
        private SpriteTextBitmap Creator { get; }

        /// <summary>
        ///     Displays the progress of the download.
        /// </summary>
        private ProgressBar Progress { get; }

        /// <summary>
        ///     Displays if the mapset is already owned.
        /// </summary>
        private SpriteTextBitmap AlreadyOwned { get; }

        /// <summary>
        /// </summary>
        public static int HEIGHT { get; } = 82;

        /// <summary>
        ///     Sees if the mapset is already owned.
        /// </summary>
        private bool IsAlreadyOwned { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public DownloadableMapset(DownloadScrollContainer container, JToken mapset)
        {
            Container = container;
            Mapset = mapset;
            MapsetId = (int) Mapset["id"];

            Size = new ScalableVector2(container.Width - 2, HEIGHT);
            Image = UserInterface.DownloadItem;

            Banner = new Sprite
            {
                Parent = this,
                X = 0,
                Size = new ScalableVector2(900 / 3.6f, Height),
                Alignment = Alignment.MidLeft,
                Alpha = 0
            };

            AlreadyOwned = new SpriteTextBitmap(FontsBitmap.GothamRegular, "Already Owned")
            {
                Parent = Banner,
                Alignment = Alignment.MidCenter,
                UsePreviousSpriteBatchOptions = true,
                FontSize = 14
            };

            // Check if the mapset is already owned.
            var set = MapDatabaseCache.FindSet(MapsetId);
            IsAlreadyOwned = set != null;
            AlreadyOwned.Alpha = IsAlreadyOwned ? 1 : 0;

            Title = new SpriteTextBitmap(FontsBitmap.GothamRegular, $"{Mapset["title"]}")
            {
                Parent = this,
                X = Banner.X + Banner.Width + 15,
                Y = 10,
                Alpha = IsAlreadyOwned ? 0.65f : 1,
                FontSize = 16
            };

            Artist = new SpriteTextBitmap(FontsBitmap.GothamRegular, $"{Mapset["artist"]}")
            {
                Parent = this,
                X = Title.X,
                Y = Title.Y + Title.Height + 5,
                Alpha = IsAlreadyOwned ? 0.65f : 1,
                FontSize = 14
            };

            var gameModes = Mapset["game_modes"].ToList();
            var modes = new List<string>();

            if (gameModes.Contains(1))
                modes.Add("4K");

            if (gameModes.Contains(2))
                modes.Add("7K");

            Modes = new SpriteTextBitmap(FontsBitmap.GothamRegular, string.Join(" & ", modes) + " | ")
            {
                Parent = this,
                X = Artist.X,
                Y = Artist.Y + Artist.Height + 5,
                Alpha = IsAlreadyOwned ? 0.65f : 1,
                FontSize = 14
            };

            Creator = new SpriteTextBitmap(FontsBitmap.GothamRegular, $"Created By: {Mapset["creator_username"]}")
            {
                Parent = this,
                Alignment = Alignment.BotRight,
                Position = new ScalableVector2(-10, -5),
                Alpha = IsAlreadyOwned ? 0.65f : 1,
                FontSize = 14
            };

            var lowestDiff = Mapset["difficulty_range"].ToList().Min();
            var highestDiff = Mapset["difficulty_range"].ToList().Max();

            // ReSharper disable once ObjectCreationAsStatement
            var low = new SpriteTextBitmap(FontsBitmap.GothamRegular, $"{lowestDiff:0.00}")
            {
                Parent = this,
                X = Modes.X + Modes.Width + 2,
                Y = Modes.Y,
                Alpha = IsAlreadyOwned ? 0.65f : 1,
                FontSize = 14,
                Tint = ColorHelper.DifficultyToColor((float) lowestDiff)
            };

            if (lowestDiff != highestDiff)
            {
                var high = new SpriteTextBitmap(FontsBitmap.GothamRegular, $" - {highestDiff:0.00}")
                {
                    Parent = this,
                    X = low.X + low.Width + 2,
                    Y = Modes.Y,
                    Alpha = IsAlreadyOwned ? 0.65f : 1,
                    FontSize = 14,
                    Tint = ColorHelper.DifficultyToColor((float) highestDiff)
                };
            }
            var badge = new BannerRankedStatus
            {
                Parent = this,
                Alignment = Alignment.TopRight,
                Position = new ScalableVector2(-10, 5),
                Alpha = IsAlreadyOwned ? 0.65f : 1,
            };

            var screen = (DownloadScreen) container.View.Screen;
            badge.UpdateMap(new Map { RankedStatus = screen.CurrentRankedStatus});
            FetchMapsetBanner();

            // ReSharper disable once ObjectCreationAsStatement
            new Sprite()
            {
                Parent = this,
                Alignment = Alignment.BotLeft,
                Size = new ScalableVector2(Width, 1),
                Alpha = 0.65f
            };

            Progress = new ProgressBar(new Vector2(Width - Banner.Width, Height), 0, 100, 0, Color.Transparent, Colors.MainAccent)
            {
                Parent = this,
                X = Banner.X + Banner.Width,
                ActiveBar =
                {
                    UsePreviousSpriteBatchOptions = true,
                    Alpha = 0.60f
                },
            };

            Clicked += (sender, args) => OnClicked();
        }

        /// <summary>
        ///     Called when
        /// </summary>
        public void OnClicked()
        {
            var screen = (DownloadScreen) Container.View.Screen;

            // On the very first click of this mapset, set the bindables value
            // so that the download status can be aware of this clicked on mapset.
            if (screen.SelectedMapset.Value != this)
            {
                screen.SelectedMapset.Value = this;
                return;
            }

            if (Download != null)
            {
                NotificationManager.Show(NotificationLevel.Error, "This mapset is already downloading!");
                return;
            }

            screen.SelectedMapset.Value = null;

            // Begin downloading the map.
            Download = MapsetDownloadManager.Download(Mapset);

            if (Download == null)
                return;

            // Update the progress bar
            Download.Progress.ValueChanged += (o, e) => Progress.Bindable.Value = e.Value.ProgressPercentage;

            // Handle download completions.
            Download.Completed.ValueChanged += (o, e) =>
            {
                if (e.Value.Cancelled)
                    NotificationManager.Show(NotificationLevel.Info, $"Cancelled download for mapset: {MapsetId}!");
                else if (e.Value.Error == null)
                {
                    NotificationManager.Show(NotificationLevel.Success, $"Downloaded: {Mapset["artist"]} - {Mapset["title"]}!");

                    // Animate it off-screen
                    MoveToX(-Width, Easing.OutQuint, 400);

                    // Remove the mapset from the list and destroy it.
                    Container.Mapsets.Remove(this);
                    ThreadScheduler.RunAfter(Destroy, 450);

                    Container.RealignMapsets();
                }
                else
                    NotificationManager.Show(NotificationLevel.Error, $"An error has occurred while downloading: {MapsetId}.");
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            Alpha = MathHelper.Lerp(Alpha, IsHovered ? 0.30f : 0.60f, (float) Math.Min(gameTime.ElapsedGameTime.TotalMilliseconds / 60, 1));
            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            if (RectangleF.Intersect(ScreenRectangle, Container.ScreenRectangle).IsEmpty)
                return;

            base.Draw(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        ///     In this case, we only want buttons to be clickable if they're in the bounds of the scroll container.
        /// </summary>
        /// <returns></returns>
        protected override bool IsMouseInClickArea()
        {
            var newRect = RectangleF.Intersect(ScreenRectangle, Container.ScreenRectangle);
            return GraphicsHelper.RectangleContains(newRect, MouseManager.CurrentState.Position);
        }

        /// <summary>
        ///
        /// </summary>
        private void FetchMapsetBanner() => Task.Run(async () =>
        {
            Banner.Image = await ImageDownloader.DownloadMapsetBanner(MapsetId);
            Banner.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, 0, IsAlreadyOwned ? 0.45f : 1, 300));
        });
    }
}
