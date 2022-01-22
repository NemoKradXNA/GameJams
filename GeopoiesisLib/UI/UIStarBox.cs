using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Geopoiesis.UI
{
    public class UIStarBox : UIImage
    {
        public Texture2D StarTexture { get; set; }
        public Texture2D BarTexture { get; set; }

        public SpriteFont Font { get; set; }
        public string Text { get; set; }

        protected Vector2 TextPosition
        {
            get
            {
                Vector2 tp = new Vector2(Position.X, Position.Y + Size.Y);

                tp.Y -= Font.LineSpacing * 1.4f;
                tp.X += (Size.X / 2) - (Font.MeasureString(Text).X * .5f);

                return tp;
            }
        }

        private Rectangle _starRectangle;

        protected Rectangle starRectangle
        {
            get
            {
                if (_starRectangle == null || (_starRectangle.X != Position.X || _starRectangle.Y != Position.Y || _starRectangle.Width != Size.X || _starRectangle.Height != Size.Y))
                    _starRectangle = new Rectangle(Position.X + 64, Position.Y + 42, Size.X / 2, Size.Y / 2);

                return _starRectangle;
            }
        }

        private Rectangle _barRectangle;
        protected Rectangle barRectangle
        {
            get
            {
                if (_barRectangle == null || (_barRectangle.X != Position.X || _barRectangle.Y != Position.Y || _barRectangle.Width != Size.X || _barRectangle.Height != Size.Y))
                    _barRectangle = new Rectangle(Position.X, Position.Y + (Size.X-50), BarTexture.Width, BarTexture.Height);

                return _barRectangle;
            }
        }

        public UIStarBox(Game game, Point position, Point size) : base(game, position, size)
        {
        }

        public override void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);

            // Draw BG
            _spriteBatch.Draw(Texture, Rectangle, Tint);

            // Draw bar
            _spriteBatch.Draw(BarTexture, barRectangle, Tint);

            // Draw Text
            _spriteBatch.DrawString(Font, Text, TextPosition, Tint);

            // Draw star
            _spriteBatch.Draw(StarTexture, starRectangle, Color.White);

            _spriteBatch.End();
        }
    }
}
