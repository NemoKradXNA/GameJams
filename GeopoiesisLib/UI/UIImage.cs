using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Geopoiesis.UI
{
    public class UIImage : UIBase
    {
        public Texture2D Texture { get; set; }
        public UIImage(Game game, Point position, Point size) : base(game, position, size)
        {

        }

        public override void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);

            // Draw BG
            _spriteBatch.Draw(Texture, Rectangle, Tint);
            _spriteBatch.End();
        }
    }
}
