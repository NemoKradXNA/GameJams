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
    public class AudioOptionsScene : SceneBase
    {
        SpriteBatch _spriteBatch;
        SpriteFont font;
        SpriteFont titlFont;

        Color bgColor = new Color(.2f, .2f, .5f, .1f);
        Color sdrColor = Color.DodgerBlue;
        Color edgeColor = Color.DodgerBlue;
        Color textColor = Color.White;

        Texture2D fader;
        Color fadeColor = Color.Black;


        UIButton btnBack;
        UILabel lblTitle;

        UISlider sdrMasterVolume;
        UISlider sdrMusiVolume;
        UISlider sdrSFXVolume;

        UIButton btnSFXTest;

        public AudioOptionsScene(Game game, string name) : base(game, name) { }

        public override void Initialize()
        {
            _spriteBatch = new SpriteBatch(Game.GraphicsDevice);

            Point centerScreen = new Point(Game.GraphicsDevice.Viewport.Width / 2, Game.GraphicsDevice.Viewport.Height / 2);
            Texture2D buttonBox = geopoiesisService.CreateBox(512, 64, new Rectangle(1, 1, 1, 1), bgColor, edgeColor);
            Texture2D buttonBoxTest = geopoiesisService.CreateBox(128, 64, new Rectangle(1, 1, 1, 1), bgColor, edgeColor);

            font = Game.Content.Load<SpriteFont>("SpriteFont/font");
            titlFont = Game.Content.Load<SpriteFont>("SpriteFont/titleFont");

            fader = new Texture2D(Game.GraphicsDevice, 1, 1);
            fader.SetData(new Color[] { Color.White });

            UIImage imgTitle = new UIImage(Game, new Point(centerScreen.X - 512, 128), new Point(1024, 256));
            imgTitle.Texture = Game.Content.Load<Texture2D>("Textures/Logo1");
            imgTitle.Tint = Color.White;
            Components.Add(imgTitle);

            lblTitle = new UILabel(Game);
            lblTitle.Text = "Audio";
            lblTitle.Font = titlFont;
            lblTitle.Tint = textColor;
            Vector2 size = lblTitle.Font.MeasureString(lblTitle.Text);
            lblTitle.Size = new Point((int)size.X, (int)size.Y);
            lblTitle.Position = new Point(centerScreen.X - (lblTitle.Size.X / 2), 320);
            lblTitle.ShadowColor = edgeColor;
            lblTitle.ShadowOffset = new Vector2(-2, 2);
            Components.Add(lblTitle);

            // MasterVolume Slider
            int sliderWidth = 600;
            sdrMasterVolume = new UISlider(Game, new Point(centerScreen.X - sliderWidth/2, 256 + 256), new Point(sliderWidth, 32));
            sdrMasterVolume.Font = font;
            sdrMasterVolume.Label = $"Master Volume {100}%";
            sdrMasterVolume.BarTexture = geopoiesisService.CreateBox(300, 32, new Rectangle(1, 1, 1, 1), bgColor, edgeColor);
            sdrMasterVolume.SliderTexture = Game.Content.Load<Texture2D>("Textures/UI/circle");
            sdrMasterVolume.SliderColor = sdrColor;
            sdrMasterVolume.SliderHoverColor = Color.Aqua;
            Components.Add(sdrMasterVolume);

            // Music Volume Slider
            sdrMusiVolume = new UISlider(Game, new Point(centerScreen.X - sliderWidth / 2, 256 + 384), new Point(sliderWidth, 32));
            sdrMusiVolume.Font = font;
            sdrMusiVolume.Label = $"Music Volume {100}%";
            sdrMusiVolume.BarTexture = geopoiesisService.CreateBox(300, 32, new Rectangle(1, 1, 1, 1), bgColor, edgeColor);
            sdrMusiVolume.SliderTexture = Game.Content.Load<Texture2D>("Textures/UI/circle");
            sdrMusiVolume.SliderColor = sdrColor;
            sdrMusiVolume.SliderHoverColor = Color.Aqua;
            Components.Add(sdrMusiVolume);

            // SFX Slider
            sdrSFXVolume = new UISlider(Game, new Point(centerScreen.X - sliderWidth / 2, 256 + 512), new Point(sliderWidth, 32));
            sdrSFXVolume.Font = font;
            sdrSFXVolume.Label = $"SFX Volume {100}%";
            sdrSFXVolume.BarTexture = geopoiesisService.CreateBox(300, 32, new Rectangle(1, 1, 1, 1), bgColor, edgeColor);
            sdrSFXVolume.SliderTexture = Game.Content.Load<Texture2D>("Textures/UI/circle");
            sdrSFXVolume.SliderColor = sdrColor;
            sdrSFXVolume.SliderHoverColor = Color.Aqua;
            Components.Add(sdrSFXVolume);

            // SFX Test.
            btnSFXTest = new UIButton(Game, new Point((sdrSFXVolume.Position.X + sdrSFXVolume.Size.X + buttonBoxTest.Width), 256 + 512 - sdrSFXVolume.Size.Y/ 2), new Point(buttonBoxTest.Width, buttonBoxTest.Height));
            btnSFXTest.Text = "Test";
            btnSFXTest.BackgroundTexture = buttonBoxTest;
            btnSFXTest.Tint = Color.White;
            btnSFXTest.Font = font;
            btnSFXTest.TextColor = textColor;
            btnSFXTest.HighlightColor = Color.Aqua;
            btnSFXTest.OnMouseClick += ButtonClicked;
            Components.Add(btnSFXTest);

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

            sdrMasterVolume.Value = geopoiesisService.AudioSettings.MasterVolume;
            sdrMusiVolume.Value = geopoiesisService.AudioSettings.MusicVolume;
            sdrSFXVolume.Value = geopoiesisService.AudioSettings.SFXVolume;

            audioManager.PlaySong("Audio/Music/More-Sewer-Creepers_Looping", .5f);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            audioManager.MasterVolume = sdrMasterVolume.Value;
            audioManager.MusicVolume = sdrMusiVolume.Value;
            audioManager.SFXVolume = sdrSFXVolume.Value;
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
            else if (sender == btnSFXTest) { }
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

            geopoiesisService.SaveAudioSettings();

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
