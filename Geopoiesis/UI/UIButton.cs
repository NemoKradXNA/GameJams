using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Geopoiesis.UI
{
    public class UIButton : UIBase
    {

        protected bool mouseOver { get; set; }

        public Texture2D BackgroundTexture { get; set; }
        public SpriteFont Font { get; set; }
        public string Text { get; set; }

        public Color TextColor { get; set; }
        public Color HighlightColor { get; set; }

        protected Color bgColor;
        protected Color txtColor;

        public event UIMouseEvent OnMouseOver;
        public event UIMouseEvent OnMouseClick;

        protected Vector2 TextPosition
        {
            get
            {
                Vector2 tp = new Vector2(Position.X, Position.Y);

                tp.Y += Font.LineSpacing / 1.75f;
                tp.X += (Size.X/2)- (Font.MeasureString(Text).X * .5f);

                return tp;
            }
        }
        
        
        public UIButton(Game game, Point position, Point size) : base(game, position, size)
        {

        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);


            mouseOver = inputManager.MouseManager.PositionRect.Intersects(Rectangle);

            if (mouseOver)
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
            _spriteBatch.DrawString(Font, Text, TextPosition, txtColor);
            _spriteBatch.End();
        }
    }
}
