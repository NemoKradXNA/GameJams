using Geopoiesis.Enums;
using Geopoiesis.Interfaces;
using Geopoiesis.Managers.Coroutines;
using Geopoiesis.Models;
using Geopoiesis.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Geopoiesis.Scenes
{
    public class MainMenuScene : SceneBase
    {
        SpriteBatch _spriteBatch;

        Texture2D buttonBox;

        Texture2D bg;

        Texture2D pixel;
        Texture2D fader;
        Color fadeColor = Color.Black;

        Color bgColor = new Color(.2f, .2f, .5f, .1f);
        Color edgeColor = Color.DodgerBlue ;
        Color textColor = Color.White;

        bool gameInProgress = false;

        List<Rectangle> particles = new List<Rectangle>();
        List<Color> pColor = new List<Color>();
        List<int> pSpeed = new List<int>();

        SpriteFont font;
        SpriteFont subFont;
        Random rnd;
        UIImage imgTitle;
        UILabel lblSubTitle;
        UIButton btnNewGame;
        UIButton btnContinue;
        UIButton btnQuit;
        UILabel lblVersion;

        protected string Version = "1.0.0.1";

        bool exiting;

        public MainMenuScene(Game game, string name) : base(game, name) { }

        public override void Initialize()
        {
            _spriteBatch = new SpriteBatch(Game.GraphicsDevice);
            font = Game.Content.Load<SpriteFont>("SpriteFont/font");
            subFont = Game.Content.Load<SpriteFont>("SpriteFont/logFont");
            bg = Game.Content.Load<Texture2D>("Textures/MenuBG");

            rnd = new Random(geopoiesisService.Seed);

            gameInProgress = File.Exists("save.json");


            for (int p = 0; p < 256; p++)
            {
                int x, y, w, h, s;
                x = rnd.Next(0, Game.GraphicsDevice.Viewport.Width);
                y = rnd.Next(0, Game.GraphicsDevice.Viewport.Height);

                w = rnd.Next(1, 8);
                h = rnd.Next(1, 3);

                s = rnd.Next(1, 16);
                float r, g, b, a;

                r = (float)rnd.NextDouble() + s / 16f;
                g = (float)rnd.NextDouble() + s / 16f;
                b = (float)rnd.NextDouble() + s / 16f;
                a = (float)rnd.NextDouble() + .0125f;


                particles.Add(new Rectangle(x, y, w, h));
                pColor.Add(new Color(r, g, b, a));
                pSpeed.Add(s);
            }

            pixel = new Texture2D(Game.GraphicsDevice, 1, 1);
            pixel.SetData(new Color[] { new Color(1, 1, 1, .75f) });

            fader = new Texture2D(Game.GraphicsDevice, 1, 1);
            fader.SetData(new Color[] { Color.White });

            audioManager.PlaySong("Audio/Music/Creepy-Hollow", .5f);

            buttonBox = geopoiesisService.CreateBox(512, 64, new Rectangle(1, 1, 1, 1), bgColor, edgeColor);

            Point centerScreen = new Point(Game.GraphicsDevice.Viewport.Width/2, Game.GraphicsDevice.Viewport.Height/2);
            
            imgTitle = new UIImage(Game, new Point(centerScreen.X - 512, 128), new Point(1024, 256));
            imgTitle.Texture = Game.Content.Load<Texture2D>("Textures/Logo1");
            imgTitle.Tint = Color.White;
            Components.Add(imgTitle);

            lblSubTitle = new UILabel(Game);
            lblSubTitle.Text = "Terraforming - \n\" transform (a planet) so as to resemble the earth, especially so that it can support human life.\"";
            lblSubTitle.Font = subFont;
            lblSubTitle.Tint = Color.DarkGoldenrod;
            Vector2 size = lblSubTitle.Font.MeasureString(lblSubTitle.Text);
            lblSubTitle.Size = new Point((int)size.X,(int) size.Y);
            lblSubTitle.Position = new Point(centerScreen.X - (lblSubTitle.Size.X / 2), 320);
            Components.Add(lblSubTitle);

            btnNewGame = new UIButton(Game, new Point((centerScreen.X) - buttonBox.Width / 2, 512), new Point(buttonBox.Width, buttonBox.Height));
            btnNewGame.Text = "New Game";
            btnNewGame.BackgroundTexture = buttonBox;
            btnNewGame.Tint = Color.White;
            btnNewGame.Font = font;
            btnNewGame.TextColor = textColor;
            btnNewGame.HighlightColor = Color.Aqua;
            btnNewGame.OnMouseClick += ButtonClicked;
            Components.Add(btnNewGame);

            btnContinue = new UIButton(Game, new Point((centerScreen.X) - buttonBox.Width / 2, 512 + 128), new Point(buttonBox.Width, buttonBox.Height));
            btnContinue.Text = "Continue";
            btnContinue.BackgroundTexture = buttonBox;
            btnContinue.Tint = Color.White;
            btnContinue.Font = font;
            btnContinue.TextColor = textColor;
            btnContinue.HighlightColor = Color.Aqua;
            btnContinue.OnMouseClick += ButtonClicked;
            btnContinue.Visible = false;
            Components.Add(btnContinue);

            btnQuit = new UIButton(Game, new Point((centerScreen.X) - buttonBox.Width / 2, 512 + 128), new Point(buttonBox.Width, buttonBox.Height));
            btnQuit.Text = "Quit";
            btnQuit.BackgroundTexture = buttonBox;
            btnQuit.Tint = Color.White;
            btnQuit.Font = font;
            btnQuit.TextColor = textColor;
            btnQuit.HighlightColor = Color.Aqua;
            btnQuit.OnMouseClick += ButtonClicked;
            Components.Add(btnQuit);

            lblVersion = new UILabel(Game);
            lblVersion.Text = $"Version: {Version}";
            lblVersion.Font = subFont;
            lblVersion.Tint = Color.White;
            size = lblSubTitle.Font.MeasureString(lblVersion.Text);
            lblVersion.Size = new Point((int)size.X, (int)size.Y);
            lblVersion.Position = new Point(Game.GraphicsDevice.Viewport.Width - (lblVersion.Size.X + 64), Game.GraphicsDevice.Viewport.Height - (lblVersion.Size.Y + 12));
            Components.Add(lblVersion);

            base.Initialize();            
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            for(int p=0;p<particles.Count;p++)
            {
                if(particles[p].X - pSpeed[p] > 0)
                    particles[p] = new Rectangle(particles[p].X - (pSpeed[p]), particles[p].Y, particles[p].Width, particles[p].Height);
                else
                    particles[p] = new Rectangle(Game.GraphicsDevice.Viewport.Width, rnd.Next(0,Game.GraphicsDevice.Viewport.Height), rnd.Next(1, 8), rnd.Next(1, 3));

            }

            if (gameInProgress && !btnContinue.Visible)
            {
                btnContinue.Visible = true;
                btnQuit.Position += new Point(0, 128);
            }

        }

        protected void ButtonClicked(IUIBase sender, IMouseStateManager mouseState)
        {
            if (State != SceneStateEnum.Loaded)
                return;

            btnContinue.OnMouseClick -= ButtonClicked;
            btnNewGame.OnMouseClick -= ButtonClicked;
            btnQuit.OnMouseClick -= ButtonClicked;

            audioManager.PlaySFX("Audio/SFX/beep-07");
            if (sender == btnNewGame)
            {
                geopoiesisService.reSet();
                sceneManager.LoadScene("mainGame");
            }
            else if (sender == btnContinue)
            {
                geopoiesisService.LoadGame();
                sceneManager.LoadScene("mainGame");
            }
            else if (sender == btnQuit)
            {
                exiting = true;
                State = SceneStateEnum.Unloading;
                UnloadScene();
            }
        }

        public override void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);

            _spriteBatch.Draw(bg, new Rectangle(0, 0, Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height), Color.White);
            // particles
            for (int pa = 0; pa < particles.Count; pa++)
            {
                Rectangle pos = particles[pa];
                Color color = pColor[pa];

                _spriteBatch.Draw(pixel, pos, color);
            }

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
            btnNewGame.OnMouseClick -= ButtonClicked;
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

                audioManager.MusicVolume = 1f-(a / 255f);
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

            if (exiting)
                Game.Exit();
        }
    }
}
