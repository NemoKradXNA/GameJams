using Geopoiesis.Enums;
using Geopoiesis.Interfaces;
using Geopoiesis.Managers.Coroutines;
using Geopoiesis.Scenes;
using Geopoiesis.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Geopoiesis.Scenes
{
    public class HelpScene : SceneBase
    {
        SpriteBatch _spriteBatch;
        SpriteFont font;
        SpriteFont titlFont;

        Color bgColor = new Color(.2f, .2f, .5f, .1f);
        Color edgeColor = Color.DodgerBlue;
        Color textColor = Color.White;

        Texture2D fader;
        Color fadeColor = Color.Black;

       
        UIButton btnBack;
        UILabel lblTitle;

        public HelpScene(Game game, string name) : base(game, name) { }

        public override void Initialize()
        {
            _spriteBatch = new SpriteBatch(Game.GraphicsDevice);

            Point centerScreen = new Point(Game.GraphicsDevice.Viewport.Width / 2, Game.GraphicsDevice.Viewport.Height / 2);
            Texture2D buttonBox = geopoiesisService.CreateBox(512, 64, new Rectangle(1, 1, 1, 1), bgColor, edgeColor);

            font = Game.Content.Load<SpriteFont>("SpriteFont/font");
            titlFont = Game.Content.Load<SpriteFont>("SpriteFont/titleFont");

            fader = new Texture2D(Game.GraphicsDevice, 1, 1);
            fader.SetData(new Color[] { Color.White });

            lblTitle = new UILabel(Game);
            lblTitle.Text = "Help";
            lblTitle.Font = titlFont;
            lblTitle.Tint = textColor;
            Vector2 size = lblTitle.Font.MeasureString(lblTitle.Text);
            lblTitle.Size = new Point((int)size.X, (int)size.Y);
            lblTitle.Position = new Point(centerScreen.X - (lblTitle.Size.X / 2), 320);
            lblTitle.ShadowColor = edgeColor;
            lblTitle.ShadowOffset = new Vector2(-2, 2);
            Components.Add(lblTitle);

            btnBack = new UIButton(Game, new Point((centerScreen.X) - buttonBox.Width / 2, 512 + 384), new Point(buttonBox.Width, buttonBox.Height));
            btnBack.Text = "Back";
            btnBack.BackgroundTexture = buttonBox;
            btnBack.Tint = Color.White;
            btnBack.Font = font;
            btnBack.TextColor = textColor;
            btnBack.HighlightColor = Color.Aqua;
            btnBack.OnMouseClick += ButtonClicked;
            Components.Add(btnBack);

            base.Initialize();

            audioManager.PlaySong("Audio/Music/More-Sewer-Creepers_Looping", .5f);
        }
        protected void ButtonClicked(IUIBase sender, IMouseStateManager mouseState)
        {
            if (State != SceneStateEnum.Loaded)
                return;

            audioManager.PlaySFX("Audio/SFX/beep-07");

            if (sender == btnBack)
            {
                sceneManager.LoadScene("optionsMenu");
            }
        }

        public override void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);

            _spriteBatch.Draw(Game.Content.Load<Texture2D>("Textures/MenuBG"), new Rectangle(0, 0, Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height), Color.White);
            _spriteBatch.End();

            base.Draw(gameTime);

            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);

            if (State != SceneStateEnum.Loaded)
                _spriteBatch.Draw(fader, new Rectangle(0, 0, Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height), fadeColor);

            _spriteBatch.End();
        }

        public override void LoadScene()
        {
            base.LoadScene();
            coroutineService.StartCoroutine(FadeIn());
        }

        public override void UnloadScene()
        {
            base.UnloadScene();

            btnBack.OnMouseClick -= ButtonClicked;

            coroutineService.StartCoroutine(FadeOut());
        }

        IEnumerator FadeIn()
        {
            byte a = 255;
            byte fadeSpeed = 4;
            fadeColor = new Color(fadeColor.R, fadeColor.G, fadeColor.B, a);

            while (a > 0)
            {
                yield return new WaitForEndOfFrame(Game);
                a = (byte)Math.Max(0, a - fadeSpeed);
                fadeColor = new Color(fadeColor.R, fadeColor.G, fadeColor.B, a);

                audioManager.MusicVolume = 1f - (a / 255f);
            }

            State = SceneStateEnum.Loaded;
        }

        IEnumerator FadeOut()
        {
            byte a = 0;
            byte fadeSpeed = 4;
            fadeColor = new Color(fadeColor.R, fadeColor.G, fadeColor.B, a);

            while (a < 255)
            {
                yield return new WaitForEndOfFrame(Game);
                a = (byte)Math.Min(255, a + fadeSpeed);
                fadeColor = new Color(fadeColor.R, fadeColor.G, fadeColor.B, a);

                audioManager.MusicVolume = 1f - (a / 255f);
            }

            State = SceneStateEnum.Unloaded;

        }
    }
}
