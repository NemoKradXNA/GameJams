using Geopoiesis.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Geopoiesis.UI
{
    public class UINumericUpDown : UIBase
    {

        public event UIMouseEvent OnDownMouseOver;
        public event UIMouseEvent OnDownMouseClick;
        public event UIMouseEvent OnUpMouseOver;
        public event UIMouseEvent OnUpMouseClick;

        protected UIButton btnUp;
        protected UILabel lblText;
        protected UIButton btnDown;

        public SpriteFont ButtonFont
        {
            get
            {
                return btnUp.Font;
            }
            set
            {
                btnUp.Font = value;
                btnDown.Font = value;
            }
        }
        public SpriteFont LabelFont
        {
            get { return lblText.Font; }
            set { lblText.Font = value; }
        }

        protected float _value;
        public float Value
        {
            get { return _value; }
            set
            {
                _value = value;

                lblText.Text = string.Format(Format, _value);
            }
        }

        public Texture2D ButtonTexture
        {
            set
            {
                btnUp.BackgroundTexture = value;
                btnDown.BackgroundTexture = value;
            }
        }

        public Color ButtonTextColor
        {
            get { return btnDown.TextColor; }
            set 
            {
                btnUp.TextColor = value;
                btnDown.TextColor = value;
            }
        }

        public Color ButtonHighlightColor
        {
            get { return btnDown.HighlightColor; }
            set
            {
                btnUp.HighlightColor = value;
                btnDown.HighlightColor = value;
            }
        }

        protected Point labelPosition
        {
            get
            {
                if (lblText.Text != null)
                {
                    Vector2 tp = Position.ToVector2();
                    Vector2 m = lblText.Font.MeasureString(lblText.Text) * .5f;

                    tp.Y += m.Y;
                    tp.X += Size.X / 2;

                    return tp.ToPoint();
                }
                else
                    return Point.Zero;
            }
        }

        public string Format { get; set; }
        public UINumericUpDown(Game game, Point position, Point size) : base(game, position, size)
        {
            btnUp = new UIButton(game, Point.Zero, new Point(32));
            lblText = new UILabel(game);
            btnDown = new UIButton(game, Point.Zero, new Point(32));

            btnDown.Text = "-";
            btnUp.Text = "+";

            ButtonHighlightColor = Color.White;
        }

        public override void Initialize()
        {
            base.Initialize();
            btnDown.Initialize();
            btnUp.Initialize();
            lblText.Initialize();

            btnDown.OnMouseOver += BtnDown_OnMouseOver;
            btnDown.OnMouseClick += BtnDown_OnMouseClick;
            btnUp.OnMouseClick += BtnUp_OnMouseClick;
            btnUp.OnMouseOver += BtnUp_OnMouseOver;
        }

        private void BtnUp_OnMouseOver(IUIBase sender, IMouseStateManager mouseState)
        {
            if (OnUpMouseOver != null)
                OnUpMouseOver(this, mouseState);
        }

        private void BtnUp_OnMouseClick(IUIBase sender, IMouseStateManager mouseState)
        {
            if (OnUpMouseClick != null)
                OnUpMouseClick(this, mouseState);
        }

        private void BtnDown_OnMouseClick(IUIBase sender, IMouseStateManager mouseState)
        {
            if (OnDownMouseClick != null)
                OnDownMouseClick(this, mouseState);
        }

        private void BtnDown_OnMouseOver(IUIBase sender, IMouseStateManager mouseState)
        {
            if (OnDownMouseOver != null)
                OnDownMouseOver(this, mouseState);
        }

        protected override void UnloadContent()
        {
            base.UnloadContent();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            btnUp.Tint = Tint;
            btnDown.Tint = Tint;
            lblText.Tint = Tint;

            btnUp.Position = Position;
            lblText.Position = labelPosition;
            btnDown.Position = Position + new Point(Size.X-btnDown.Size.X,0);

            btnUp.Update(gameTime);
            lblText.Update(gameTime);
            btnDown.Update(gameTime);
        }
        public override void Draw(GameTime gameTime)
        {
            btnUp.Draw(gameTime);
            lblText.Draw(gameTime);
            btnDown.Draw(gameTime);
        }
    }
}
