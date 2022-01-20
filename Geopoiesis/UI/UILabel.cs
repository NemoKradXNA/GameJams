using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Geopoiesis.UI
{
    public class UILabel : UIBase
    {
        public SpriteFont Font { get; set; }
        public string Text { get; set; }

        public Vector2 Measure
        {
            get
            {
                if (string.IsNullOrEmpty(Text))
                    return Vector2.Zero;
                return Font.MeasureString(Text);
            }
        }

        protected Vector2 TextPosition
        {
            get
            {
                Vector2 tp = new Vector2(Position.X, Position.Y);

                tp.Y += Font.LineSpacing / 1.75f;
                tp.X += (Size.X / 2) - (Measure.X * .5f);

                return tp;
            }
        }

        public UILabel(Game game) : base(game, Point.Zero, Point.Zero) { }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);

            // Draw BG
            if (!string.IsNullOrEmpty(Text))
                _spriteBatch.DrawString(Font, Text, TextPosition, Tint);
            _spriteBatch.End();
        }
    }
}
