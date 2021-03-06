using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Geopoiesis.UI
{
    public class UIButton : UIBase
    {

        public bool IsMouseOver { get; set; }

        public Texture2D BackgroundTexture { get; set; }
        public SpriteFont Font { get; set; }
        public string Text { get; set; }

        public Color TextColor { get; set; }
        public Color HighlightColor { get; set; }

        protected Color bgColor;
        protected Color txtColor;

        public event UIMouseEvent OnMouseOver;
        public event UIMouseEvent OnMouseClick;
        public event UIMouseEvent OnMouseDown;

        protected Vector2 TextPosition
        {
            get
            {
                if (!string.IsNullOrEmpty(Text))
                {
                    Vector2 tp = Position.ToVector2();
                    Vector2 m = Font.MeasureString(Text) * .5f;

                    tp.Y += (Size.Y / 2) - m.Y;
                    tp.X += (Size.X / 2) - m.X;

                    return tp;
                }
                return Vector2.Zero;
            }
        }


        public UIButton(Game game, Point position, Point size) : base(game, position, size) 
        {
            TextColor = Color.White;
            HighlightColor = Color.White;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);


            IsMouseOver = inputManager.MouseManager.PositionRect.Intersects(Rectangle);

            if (IsMouseOver)
            {
                // Mouse over, highlight
                bgColor = HighlightColor;
                txtColor = HighlightColor;

                if (inputManager.MouseManager.LeftClicked)
                {
                    if (OnMouseClick != null)
                        OnMouseClick(this, inputManager.MouseManager);
                }

                if (OnMouseOver != null)
                    OnMouseOver(this, inputManager.MouseManager);

                if (inputManager.MouseManager.LeftButtonDown)
                {
                    if (OnMouseDown != null)
                        OnMouseDown(this, inputManager.MouseManager);
                }
            }
            else
            {
                bgColor = Tint;
                txtColor = TextColor;
            }
        }

        public override void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);

            // Draw BG
            _spriteBatch.Draw(BackgroundTexture, Rectangle, bgColor);
            if(!string.IsNullOrEmpty(Text))
                _spriteBatch.DrawString(Font, Text, TextPosition, txtColor);
            _spriteBatch.End();
        }
    }
}
