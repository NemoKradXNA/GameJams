using Geopoiesis.Enums;
using Geopoiesis.Managers.Coroutines;
using Geopoiesis.Models;
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

        Rectangle newGameRec;
        Rectangle continueRec;
        Rectangle quitRec;

        Color newGameTint = Color.White;
        Color continueTint = Color.White;
        Color quitTint = Color.White;

        Color bgColor = new Color(.2f, .2f, .5f, .1f);
        Color edgeColor = Color.DodgerBlue ;
        Color textColor = Color.White;

        Color buttonTint = Color.Aqua;

        bool gameInProgress = false;

        List<Rectangle> particles = new List<Rectangle>();
        List<Color> pColor = new List<Color>();
        List<int> pSpeed = new List<int>();

        SpriteFont font;
        SpriteFont subFont;
        Random rnd;


        protected string Version = "1.0.0.1";

        public MainMenuScene(Game game, string name) : base(game, name) { }

        public override void Initialize()
        {
            base.Initialize();

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
                pColor.Add(new Color(r,g,b,a));
                pSpeed.Add(s);
            }

            pixel = new Texture2D(Game.GraphicsDevice, 1, 1);
            pixel.SetData(new Color[] { new Color(1, 1, 1, .75f) });



            audioManager.PlaySong("Audio/Music/Creepy-Hollow", .5f);
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

            if (msManager.PositionRect.Intersects(newGameRec))
            {
                
                newGameTint = buttonTint;
                if (msManager.LeftClicked)
                {
                    audioManager.PlaySFX("Audio/SFX/beep-07");
                    sceneManager.LoadScene("mainGame");
                }
            }
            else
                newGameTint = Color.White;

            if (msManager.PositionRect.Intersects(continueRec))
            {
                continueTint = buttonTint;
                if (msManager.LeftClicked)
                {
                    audioManager.PlaySFX("Audio/SFX/beep-07");
                    geopoiesisService.LoadGame();
                    sceneManager.LoadScene("mainGame");
                }
                
            }
            else
                continueTint = Color.White;

            if (msManager.PositionRect.Intersects(quitRec))
            {
                quitTint = buttonTint;
                if (msManager.LeftClicked)
                {
                    audioManager.PlaySFX("Audio/SFX/beep-07");
                    Game.Exit();
                }
            }
            else
                quitTint = Color.White;
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            if (buttonBox == null)
                buttonBox = geopoiesisService.CreateBox(512, 64, new Rectangle(1, 1, 1, 1), bgColor, edgeColor);

            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);

            _spriteBatch.Draw(bg, new Rectangle(0, 0, Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height), Color.White);


            // particles
            for (int pa = 0; pa < particles.Count; pa++)
            {
                Rectangle pos = particles[pa];
                Color color = pColor[pa];

                _spriteBatch.Draw(pixel, pos, color);
            }


            _spriteBatch.Draw(Game.Content.Load<Texture2D>("Textures/Logo1"),new Rectangle(Game.GraphicsDevice.Viewport.Width/2 - 512 ,128,1024,256), Color.White);
            string str = "Terraforming - \n\" transform (a planet) so as to resemble the earth, especially so that it can support human life.\"";
            _spriteBatch.DrawString(subFont, str, new Vector2(Game.GraphicsDevice.Viewport.Width / 2 - (subFont.MeasureString(str).X/2), 320), Color.DarkGoldenrod);

            Vector2 p = new Vector2(Game.GraphicsDevice.Viewport.Width/2,512);

            // Title Image

            str = "New Game";
            newGameRec = new Rectangle((int)p.X- buttonBox.Width/2, (int)p.Y, buttonBox.Width, buttonBox.Height);
            _spriteBatch.Draw(buttonBox, newGameRec, newGameTint);
            p.Y += font.LineSpacing/1.75f;
            p.X -= font.MeasureString(str).X / 2;
            _spriteBatch.DrawString(font, str, p, textColor);


            p = new Vector2(Game.GraphicsDevice.Viewport.Width / 2, 512);
            p.Y += 128;
            // If there is a game in progress
            if (gameInProgress)
            {
                

                str = "Continue";
                continueRec = new Rectangle((int)p.X - buttonBox.Width / 2, (int)p.Y, buttonBox.Width, buttonBox.Height);
                _spriteBatch.Draw(buttonBox, continueRec, continueTint);
                p.Y += font.LineSpacing / 1.75f;
                p.X -= font.MeasureString(str).X / 2;
                _spriteBatch.DrawString(font, str, p, textColor);

                p = new Vector2(Game.GraphicsDevice.Viewport.Width / 2, 512);
                p.Y += 256;
            }           

            str = "Quit";
            quitRec = new Rectangle((int)p.X - buttonBox.Width / 2, (int)p.Y, buttonBox.Width, buttonBox.Height);
            _spriteBatch.Draw(buttonBox, quitRec, quitTint);
            p.Y += font.LineSpacing / 1.75f;
            p.X -= font.MeasureString(str).X / 2;
            _spriteBatch.DrawString(font, str, p, textColor);

            str = $"Version: {Version}";
            Vector2 m = subFont.MeasureString(str);
            _spriteBatch.DrawString(subFont, str, new Vector2(Game.GraphicsDevice.Viewport.Width - (m.X + 64), Game.GraphicsDevice.Viewport.Height - (m.Y + 8)), Color.White);

            _spriteBatch.End();
        }

        public override void LoadScene()
        {
            base.LoadScene();
            State = SceneStateEnum.Loaded;
        }
        public override void UnloadScene()
        {
            base.UnloadScene();
            State = SceneStateEnum.Unloaded;
        }
    }
}
