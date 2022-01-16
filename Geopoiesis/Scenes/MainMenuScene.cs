using Geopoiesis.Enums;
using Geopoiesis.Managers.Coroutines;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Geopoiesis.Scenes
{
    public class MainMenuScene : SceneBase
    {
        SpriteBatch _spriteBatch;

        Texture2D buttonBox;

        Texture2D bg;

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

        bool gameInProgress = true;

        SpriteFont font;
        Random rnd;
        public MainMenuScene(Game game, string name) : base(game, name) { }

        public override void Initialize()
        {
            base.Initialize();

            _spriteBatch = new SpriteBatch(Game.GraphicsDevice);
            font = Game.Content.Load<SpriteFont>("SpriteFont/font");
            bg = Game.Content.Load<Texture2D>("Textures/MenuBG");

            rnd = new Random(geopoiesisService.Seed);

        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (msManager.PositionRect.Intersects(newGameRec))
            {
                newGameTint = buttonTint;
                if (msManager.LeftClicked)
                    sceneManager.LoadScene("mainGame");
            }
            else
                newGameTint = Color.White;

            if (msManager.PositionRect.Intersects(continueRec))
            {
                continueTint = buttonTint;
            }
            else
                continueTint = Color.White;

            if (msManager.PositionRect.Intersects(quitRec))
            {
                quitTint = buttonTint;
                if (msManager.LeftClicked)
                    Game.Exit();
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

            Vector2 p = new Vector2(Game.GraphicsDevice.Viewport.Width/2,256);

            // Title Image

            string str = "New Game";
            newGameRec = new Rectangle((int)p.X- buttonBox.Width/2, (int)p.Y, buttonBox.Width, buttonBox.Height);
            _spriteBatch.Draw(buttonBox, newGameRec, newGameTint);
            p.Y += font.LineSpacing/1.75f;
            p.X -= font.MeasureString(str).X / 2;
            _spriteBatch.DrawString(font, str, p, textColor);


            p = new Vector2(Game.GraphicsDevice.Viewport.Width / 2, 256);
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

                p = new Vector2(Game.GraphicsDevice.Viewport.Width / 2, 256);
                p.Y += 256;
            }           

            str = "Quit";
            quitRec = new Rectangle((int)p.X - buttonBox.Width / 2, (int)p.Y, buttonBox.Width, buttonBox.Height);
            _spriteBatch.Draw(buttonBox, quitRec, quitTint);
            p.Y += font.LineSpacing / 1.75f;
            p.X -= font.MeasureString(str).X / 2;
            _spriteBatch.DrawString(font, str, p, textColor);

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
