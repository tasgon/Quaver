using System;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Helpers;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Options.Items
{
    public class OptionsItem : ImageButton
    {
        /// <summary>
        /// </summary>
        protected SpriteTextPlus Name { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="containerWidth"></param>
        /// <param name="name"></param>
        public OptionsItem(float containerWidth, string name) : base(AssetLoader.LoadTexture2DFromFile(@"C:\users\swan\desktop\options-item-bg.png"))
        {
            Size = new ScalableVector2(containerWidth * 0.96f, 54);
            Depth = 1;

            Tint = ColorHelper.HexToColor("#242424");
            CreateName(name);

            UsePreviousSpriteBatchOptions = true;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            var alpha = IsHovered ? 1 : 0.50f;

            var dt = gameTime.ElapsedGameTime.TotalMilliseconds;
            Alpha = MathHelper.Lerp(Alpha, alpha, (float) Math.Min(dt / 20f, 1));

            base.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        /// <param name="name"></param>
        private void CreateName(string name)
        {
            Name = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), name, 21)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = 16,
                UsePreviousSpriteBatchOptions = true,
            };
        }
    }
}