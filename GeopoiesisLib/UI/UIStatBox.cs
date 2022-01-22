using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Geopoiesis.UI
{
    public class UIStatBox : UIImage
    {
        public Color ColorLow { get; set; }
        public Color ColorHigh { get; set; }
        protected float _Value = 0;
        public float Value
        {
            get
            {
                return _Value;
            }
            set
            {
                _Value = value;
                SetStatTexutre();
            }
        }

        public int MaxValue { get; set; }
        protected Texture2D statValue;

        public SpriteFont Font { get; set; }
        public string Text { get; set; }
        public string Format { get; set; }

        private Rectangle _statRect;
        protected Rectangle statRect
        {
            get
            {
                if (_statRect == null || (_statRect.X != Position.X + 101 || _statRect.Y != Position.Y + 1 || _statRect.Width != Size.X - 2 || _statRect.Height != Size.Y - 1))
                    _statRect = new Rectangle(Position.X + 101, Position.Y + 1, Size.X - 2, Size.Y - 1);

                return _statRect;
            }
        }

        private Rectangle _borderRect;
        protected Rectangle borderRect
        {
            get
            {
                if (_borderRect == null || (_borderRect.X != Position.X + 100 || _borderRect.Y != Position.Y || _borderRect.Width != Size.X || _borderRect.Height != Size.Y))
                    _borderRect = new Rectangle(Position.X + 100, Position.Y, Size.X, Size.Y);

                return _borderRect;
            }
        }

        public UIStatBox(Game game, Point position, Point size) : base(game, position, size)
        {
            Value = 0;
            ColorLow = Color.Red;
            ColorHigh = Color.LimeGreen;
            Format = "{0,0:000.00}";
            MaxValue = 100;
        }

        

        private Vector2 _labelPos;
        protected Vector2 labelPos
        {
            get
            {
                if (_labelPos.X != Position.X || _labelPos.Y != Position.Y)
                    _labelPos = new Vector2(Position.X, Position.Y);

                return _labelPos;
            }
        }

        protected string valueString
        {
            get
            {
                return string.Format(Format, Value);
            }
        }

        protected Vector2 valuePos
        {
            get
            {
                return new Vector2(statRect.X + (statRect.Width / 2) - (Font.MeasureString(valueString) / 2).X, statRect.Y);
            }
        }

        public override void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);

            // Draw our label.
            _spriteBatch.DrawString(Font, Text, labelPos, Tint);

            // Draw Stat Border.
            _spriteBatch.Draw(Texture, borderRect, Tint);

            // Draw Value;
            _spriteBatch.Draw(statValue, statRect, Color.White);

            // Draw Value Text
            _spriteBatch.DrawString(Font, valueString, valuePos, Tint);

            _spriteBatch.End();
        }

        protected void SetStatTexutre()
        {
            statValue = new Texture2D(Game.GraphicsDevice, 128, 1);
            Color[] c = new Color[128];

            float v = _Value / MaxValue;

            for (int x = 0; x < 128; x++)
            {
                Color col = Color.Transparent;
                float t = x / 128f;

                if (t <= v)
                    col = Color.Lerp(ColorLow, ColorHigh, t / v);

                c[x] = col;
            }

            statValue.SetData(c);
        }
    }
}
