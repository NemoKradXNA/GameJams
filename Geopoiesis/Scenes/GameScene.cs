using Geopoiesis.Enums;
using Geopoiesis.Interfaces;
using Geopoiesis.Managers.Coroutines;
using Geopoiesis.Models;
using Geopoiesis.Models.Planet;
using Geopoiesis.Services;
using Geopoiesis.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Geopoiesis.Scenes
{
    public class GameScene : SceneBase
    {
        SpriteBatch _spriteBatch;
        SpriteFont font;
        SpriteFont logFont;

        PlanetGeometry planet;
        ITransform moonAnchor;
        MorphableSphere moon;
        MorphableSphere sun;
        Atmosphere atmos;
        ParticleEmitter pet;
        Cube cube;

        protected float DisplacementMag = .15f;
        float _MinDeepSeaDepth = .07f;
        float _MinSeaDepth = .4f;
        float _MinShoreDepth = .42f;
        float _MinLand = .55f;
        float _MinHill = .8f;

        Color hudColor = Color.LimeGreen;
        Color shadowColor = Color.DarkGreen;
        Vector2 shaddowOffset = new Vector2(-2, 2);

        Dictionary<float, int> LODSettings = new Dictionary<float, int>()
        {
            { 500, 7 },
            { 300, 6 },
            { 200, 5 },
            { 100, 4 },
            { 50, 3 },
            { 25, 2 },
        };


        Vector3 ld = new Vector3(-1, .25f, .25f);

        SpriteFont gameFont;
        //Texture2D hudBorder;
        //Texture2D starBox;
        Texture2D logBox;
        //Texture2D pixel;
        Texture2D buttonBox;
        Texture2D quitBox;

        //Rectangle VolcPlus;
        //Rectangle VolcMinus;

        Rectangle QuakePlus;
        Rectangle QuakeMinus;

        Rectangle DistPlus;
        Rectangle DistMinus;

        Rectangle QuitRect;

        UIImage imgHudBorder;
        UIStarBox starBox;
        UIStatBox statH2O;
        UIStatBox statO3;
        UIStatBox statLife;
        UIStatBox statAU;
        UIStatBox statTemp;
        UILabel lblEpoch;
        UILabel lblTime;

        UILabel lblGeneratingPlanet;
        UILabel lblGeneratingMoon;
        UILabel lblGeneratingSun;

        UIList lstLogger;

        UINumericUpDown nudVolcanism;

        public GameScene(Game game, string name) : base(game, name) { }

        public override void Initialize()
        {
            SkyBox skyBox = new SkyBox(Game, "Shaders/SkyBox");
            Components.Add(skyBox);

            planet = new PlanetGeometry(Game, "Shaders/PlanetSplatMap");
            planet.Transform.Position = new Vector3(0, 0, 0);
            moonAnchor = new Transform();
            moonAnchor.Position = planet.Transform.Position;
            Components.Add(planet);

            moon = new MorphableSphere(Game, "Shaders/Moon", 2, 1, 1.25f, 128, geopoiesisService.Seed * 2, 5, 7);
            moon.Transform.Parent = moonAnchor;
            moon.Transform.Position = (Vector3.Left + (Vector3.Up * .25f)) * 32;
            Components.Add(moon);

            sun = new MorphableSphere(Game, "Shaders/Sun", 2, 10, 16, 16, 1971, 3, 7);
            sun.Transform.Position = new Vector3(-1, 0, 0) * 100;
            Components.Add(sun);

            atmos = new Atmosphere(Game, "Shaders/AtmosShader");
            atmos.Transform.Scale = Vector3.One * 4.25f;
            Components.Add(atmos);

            // Make sure all particle emitters are added last...
            pet = new ParticleEmitter(Game);
            Components.Add(pet);

            // particle test..
            int totalParticles = 1000;
            int volumeSize = 100;
            Random rnd = new Random(1971);
            Vector3 v = Vector3.Zero;
            Texture2D t = null;


            for (int p = 0; p < totalParticles; p++)
            {

                float x = rnd.Next(-volumeSize / 2, volumeSize / 2);
                float y = rnd.Next(-volumeSize / 2, volumeSize / 2);
                float z = rnd.Next(-volumeSize / 2, volumeSize / 2);

                v = new Vector3(x, y, z);
                int i = rnd.Next(6);

                t = Game.Content.Load<Texture2D>($"Textures/Particles/flare{i}");

                pet.AddParticle(v, Vector3.One * .25f, t, Color.White);
            }

            coroutineService.StartCoroutine(WaitForPlanetBuild());
            coroutineService.StartCoroutine(StartGame());

            geopoiesisService.StartType = StartType.G;

            font = Game.Content.Load<SpriteFont>("SpriteFont/font");
            gameFont = Game.Content.Load<SpriteFont>("SpriteFont/GameFont");
            logFont = Game.Content.Load<SpriteFont>("SpriteFont/logFont");

            Point screenSize = new Point(Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height);
            Point screenCenter = new Point(Game.GraphicsDevice.Viewport.Width / 2, Game.GraphicsDevice.Viewport.Height / 2);
            Rectangle border1Pixel = new Rectangle(1, 1, 1, 1);
            float a = .75f;
            Color BorderColor = new Color(1, 1, 1, a);
            Color BGBlackColor = new Color(0, 0, 0, a);

            imgHudBorder = new UIImage(Game, Point.Zero, screenSize);
            imgHudBorder.Texture = CreateBox(imgHudBorder.Size.X, imgHudBorder.Size.Y, border1Pixel, Color.Transparent, BorderColor);
            imgHudBorder.Tint = hudColor;
            Components.Add(imgHudBorder);

            starBox = new UIStarBox(Game, Point.Zero, new Point(256));
            starBox.Texture = CreateBox(starBox.Size.X, starBox.Size.Y, border1Pixel, BGBlackColor, BorderColor);
            starBox.BarTexture = CreateBox(starBox.Size.X, 1, border1Pixel, Color.Transparent, BorderColor);
            starBox.StarTexture = Game.Content.Load<Texture2D>($"Textures/Stars/{geopoiesisService.StartType}");
            starBox.Tint = hudColor;
            starBox.Text = $"{geopoiesisService.StartType} - Class Star";
            starBox.Font = font;
            Components.Add(starBox);


            statH2O = new UIStatBox(Game, new Point(264, 8), new Point(288, font.LineSpacing));
            statH2O.Tint = hudColor;
            statH2O.Texture = CreateBox(statH2O.Size.X, statH2O.Size.Y, border1Pixel, BGBlackColor, BorderColor);
            statH2O.Font = font;
            statH2O.Text = "H2O:";
            statH2O.ColorHigh = Color.LightBlue;
            statH2O.ColorLow = Color.DarkBlue;

            Components.Add(statH2O);

            statO3 = new UIStatBox(Game, new Point(264, 8 + font.LineSpacing), new Point(288, font.LineSpacing));
            statO3.Tint = hudColor;
            statO3.Texture = CreateBox(statH2O.Size.X, statH2O.Size.Y, border1Pixel, BGBlackColor, BorderColor);
            statO3.Font = font;
            statO3.Text = "O3:";
            statO3.ColorHigh = Color.LightSkyBlue;
            statO3.ColorLow = Color.DarkSlateGray;

            Components.Add(statO3);

            statLife = new UIStatBox(Game, new Point(264, 8 + (font.LineSpacing * 2)), new Point(288, font.LineSpacing));
            statLife.Tint = hudColor;
            statLife.Texture = CreateBox(statH2O.Size.X, statH2O.Size.Y, border1Pixel, BGBlackColor, BorderColor);
            statLife.Font = font;
            statLife.Text = "Life:";
            statLife.ColorHigh = Color.ForestGreen;
            statLife.ColorLow = Color.Firebrick;

            Components.Add(statLife);

            statAU = new UIStatBox(Game, new Point(264, 8 + (font.LineSpacing * 3)), new Point(288, font.LineSpacing));
            statAU.Tint = hudColor;
            statAU.Texture = CreateBox(statH2O.Size.X, statH2O.Size.Y, border1Pixel, BGBlackColor, BorderColor);
            statAU.Font = font;
            statAU.Text = "AU:";
            statAU.ColorHigh = Color.White;
            statAU.ColorLow = Color.Gold;
            statAU.MaxValue = 10;
            statAU.Format = "{0,0:0.0} AU";

            Components.Add(statAU);

            statTemp = new UIStatBox(Game, new Point(264, 8 + (font.LineSpacing * 4)), new Point(288, font.LineSpacing));
            statTemp.Tint = hudColor;
            statTemp.Texture = CreateBox(statH2O.Size.X, statH2O.Size.Y, border1Pixel, BGBlackColor, BorderColor);
            statTemp.Font = font;
            statTemp.Text = "Temp:";
            statTemp.ColorHigh = Color.Red;
            statTemp.ColorLow = Color.Azure;
            statTemp.Format = "{0,0:00.00} c";
            statTemp.MaxValue = 10;

            Components.Add(statTemp);

            lblEpoch = new UILabel(Game);
            lblEpoch.Tint = hudColor;
            lblEpoch.Position = screenCenter;
            lblEpoch.Font = gameFont;

            Components.Add(lblEpoch);

            lblTime = new UILabel(Game);
            lblTime.Tint = hudColor;
            lblTime.Position = screenCenter;
            lblTime.Font = gameFont;

            Components.Add(lblTime);

            lblGeneratingPlanet = new UILabel(Game);
            lblGeneratingPlanet.Font = gameFont;
            lblGeneratingPlanet.ShadowColor = shadowColor;
            lblGeneratingPlanet.ShadowOffset = shaddowOffset;
            lblGeneratingPlanet.Tint = Color.RosyBrown;

            Components.Add(lblGeneratingPlanet);

            lblGeneratingMoon = new UILabel(Game);
            lblGeneratingMoon.Font = gameFont;
            lblGeneratingMoon.ShadowColor = shadowColor;
            lblGeneratingMoon.ShadowOffset = shaddowOffset;
            lblGeneratingMoon.Tint = Color.Silver;

            Components.Add(lblGeneratingMoon);

            lblGeneratingSun = new UILabel(Game);
            lblGeneratingSun.Font = gameFont;
            lblGeneratingSun.ShadowColor = shadowColor;
            lblGeneratingSun.ShadowOffset = shaddowOffset;
            lblGeneratingSun.Tint = Color.Gold;

            Components.Add(lblGeneratingSun);

            lstLogger = new UIList(Game, new Point(0, 300), new Point(512, 780));
            lstLogger.Title = "Events:-";
            lstLogger.TitleFont = font;
            lstLogger.ListFont = logFont;
            lstLogger.Tint = hudColor;
            lstLogger.ListBackgroundTexture = CreateBox(lstLogger.Size.X, lstLogger.Size.Y, border1Pixel, BGBlackColor, BorderColor);
            lstLogger.SystemEventsList = geopoiesisService.LoggedEvents;

            Components.Add(lstLogger);

            Rectangle border2Pixel = new Rectangle(2, 2, 2, 2);
            Color buttonBGTint = new Color(.2f, .5f, .2f, 1);

            nudVolcanism = new UINumericUpDown(Game, new Point(256 + 8, 16 + font.LineSpacing * 5), new Point(389, 32));
            
            nudVolcanism.Tint = Color.Red;
            nudVolcanism.ButtonTexture = CreateBox(32, 32, border2Pixel, buttonBGTint, nudVolcanism.Tint);
            nudVolcanism.ButtonFont = font;
            nudVolcanism.LabelFont = font;
            nudVolcanism.Format = "Volcanism [{0:0}]";
            nudVolcanism.ButtonTextColor = nudVolcanism.Tint;

            nudVolcanism.OnUpMouseClick += nudUpClick;
            nudVolcanism.OnDownMouseClick += nudDownClick;  
            
            Components.Add(nudVolcanism);

            base.Initialize();

            State = SceneStateEnum.Loaded;

            geopoiesisService.OnSystemEventFired -= EventFired;
            geopoiesisService.OnSystemEventFired += EventFired;

            audioManager.PlaySong("Audio/Music/Midnight-Mist", .5f);
        }

        private void nudDownClick(IUIBase sender, IMouseStateManager mouseState)
        {

            audioManager.PlaySFX("Audio/SFX/dtmf-5");
            if (sender == nudVolcanism)
            {
                geopoiesisService.Volcanism = Math.Max(0, geopoiesisService.Volcanism - .01f);
            }
        }

        private void nudUpClick(IUIBase sender, IMouseStateManager mouseState)
        {

            audioManager.PlaySFX("Audio/SFX/dtmf-5");
            if (sender == nudVolcanism)
            {
                geopoiesisService.Volcanism = Math.Min(1, geopoiesisService.Volcanism + .01f);
            }
        }

        protected void EventFired(SystemEvent evt)
        {
            if(evt.beepSFX != null)
                audioManager.PlaySFX(evt.beepSFX);
            SystemEvent clone = JsonConvert.DeserializeObject<SystemEvent>(JsonConvert.SerializeObject(evt));
            clone.YearArrives = geopoiesisService.Years;
            geopoiesisService.LoggedEvents.Add(clone);
        }

        protected override void LoadContent()
        {
            base.LoadContent();
            _spriteBatch = new SpriteBatch(Game.GraphicsDevice);
        }

        protected override void UnloadContent()
        {

            // stop all corouties...
            coroutineService.StopCoroutine(StartGame());
            coroutineService.StopCoroutine(WaitForPlanetBuild());

            base.UnloadContent();
        }

        public override void Update(GameTime gameTime)
        {
            if (planet.Generated)
            {
                float translateSpeed = 2f;
                float rotateSpeed = 1f;

                translateSpeed *= (float)gameTime.ElapsedGameTime.TotalSeconds;
                rotateSpeed *= (float)gameTime.ElapsedGameTime.TotalSeconds;

                // Test moving the camera about.
                //if (kbManager.KeyDown(Keys.W))
                //    camera.Transform.Translate(Vector3.Forward * translateSpeed);
                //if (kbManager.KeyDown(Keys.S))
                //    camera.Transform.Translate(Vector3.Backward * translateSpeed);
                //if (kbManager.KeyDown(Keys.A))
                //    camera.Transform.Translate(Vector3.Left * translateSpeed);
                //if (kbManager.KeyDown(Keys.D))
                //    camera.Transform.Translate(Vector3.Right * translateSpeed);

                if (kbManager.KeyDown(Keys.Down))
                    camera.Transform.Rotate(Vector3.Left, rotateSpeed);
                if (kbManager.KeyDown(Keys.Up))
                    camera.Transform.Rotate(Vector3.Right, rotateSpeed);
                if (kbManager.KeyDown(Keys.Right))
                    camera.Transform.Rotate(Vector3.Down, rotateSpeed);
                if (kbManager.KeyDown(Keys.Left))
                    camera.Transform.Rotate(Vector3.Up, rotateSpeed);

                planet.Transform.Rotate(Vector3.Up, .0025f);
                moonAnchor.Rotate((Vector3.Up * .25f) + (Vector3.Forward * .1f), -.005f);
               
                atmos.Transform.Scale = (Vector3.One * 4.25f) + (Vector3.One * (planet.Radius * .8f) * DisplacementMag);

               
                if (planet.effect != null)
                {

                    if (atmos.effect.Parameters["atmos"] != null)
                        atmos.effect.Parameters["atmos"].SetValue(geopoiesisService.OZone);

                    if (planet.effect.Parameters["res"] != null)
                        planet.effect.Parameters["res"].SetValue(new Vector2(Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height));

                    if (moon.effect.Parameters["res"] != null)
                        moon.effect.Parameters["res"].SetValue(new Vector2(Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height));

                    // Change values in the planet's shader.
                    if (planet.effect.Parameters["colorRamp"] != null) // Or generate this procedurally. 
                        planet.effect.Parameters["colorRamp"].SetValue(Game.Content.Load<Texture2D>("Textures/Ramp1"));

                    if (planet.effect.Parameters["displacemntMag"] != null)
                        planet.effect.Parameters["displacemntMag"].SetValue(DisplacementMag);

                    if (moon.effect.Parameters["displacemntMag"] != null)
                        moon.effect.Parameters["displacemntMag"].SetValue(0.1f);

                    if (planet.effect.Parameters["_MinSeaDepth"] != null)
                        planet.effect.Parameters["_MinSeaDepth"].SetValue(geopoiesisService.WaterLevel);

                    if (planet.effect.Parameters["_MinShoreDepth"] != null)
                        planet.effect.Parameters["_MinShoreDepth"].SetValue(geopoiesisService.WaterLevel + .05f);

                    if (planet.effect.Parameters["_MinLand"] != null)
                        planet.effect.Parameters["_MinLand"].SetValue(geopoiesisService.LifeLevel);

                    if (planet.effect.Parameters["_MinHill"] != null)
                        planet.effect.Parameters["_MinHill"].SetValue(_MinHill);
                }

                ld = sun.Transform.Position - planet.Transform.Position;
                ld.Normalize();

                planet.LightDirection = ld;

                ld = sun.Transform.Position - atmos.Transform.Position;
                ld.Normalize();

                atmos.LightDirection = ld;

                ld = sun.Transform.Position - moon.Transform.Position;
                ld.Normalize();

                moon.LightDirection = ld;

                // Player input..
                if (msManager.LeftClicked)
                {
                    //if (msManager.PositionRect.Intersects(VolcPlus))
                    //{
                    //    audioManager.PlaySFX("Audio/SFX/dtmf-5");
                    //    geopoiesisService.Volcanism = Math.Min(1, geopoiesisService.Volcanism + .01f);
                    //}
                    //if (msManager.PositionRect.Intersects(VolcMinus))
                    //{
                    //    audioManager.PlaySFX("Audio/SFX/dtmf-5");
                    //    geopoiesisService.Volcanism = Math.Max(0, geopoiesisService.Volcanism - .01f);
                    //}
                    if (msManager.PositionRect.Intersects(QuakePlus))
                    {
                        audioManager.PlaySFX("Audio/SFX/dtmf-5");
                        geopoiesisService.Quakes = Math.Min(1, geopoiesisService.Quakes + .01f);
                    }
                    if (msManager.PositionRect.Intersects(QuakeMinus))
                    {
                        audioManager.PlaySFX("Audio/SFX/dtmf-5");
                        geopoiesisService.Quakes = Math.Max(0, geopoiesisService.Quakes - .01f);
                    }

                    if (msManager.PositionRect.Intersects(DistPlus))
                    {
                        audioManager.PlaySFX("Audio/SFX/dtmf-5");
                        geopoiesisService.DistanceFromStar = geopoiesisService.DistanceFromStar + .1f;
                    }
                    if (msManager.PositionRect.Intersects(DistMinus))
                    {
                        audioManager.PlaySFX("Audio/SFX/dtmf-5");
                        geopoiesisService.DistanceFromStar = Math.Max(.1f, geopoiesisService.DistanceFromStar - .1f);
                    }

                    if (msManager.PositionRect.Intersects(QuitRect))
                    {
                        // Save state
                        audioManager.PlaySFX("Audio/SFX/beep-07");
                        geopoiesisService.FireAnEvent(new SystemEvent() { Title = "Time Stops!!", Description = "The universe has been stored for a later date...." });
                        geopoiesisService.SaveGame();
                        sceneManager.LoadScene("mainMenu");
                    }
                }
            }
            else if (msManager.LeftClicked)
            {
                audioManager.PlaySFX("Audio/SFX/dtmf-5");
            }

            statH2O.Value = geopoiesisService.WaterLevel * 100;
            statO3.Value = geopoiesisService.OZone * 100;
            statLife.Value = geopoiesisService.LifeLevel * 100;
            statAU.Value = geopoiesisService.DistanceFromStar;
            statTemp.Value = geopoiesisService.SurfaceTemp;

            lblEpoch.Text = $"{$"[Epoch: {geopoiesisService.CurrentEpoch}]",0:10}";
            lblEpoch.Position = new Point((Game.GraphicsDevice.Viewport.Width/2)  , 28);

            lblTime.Text = $"{$"{geopoiesisService.Years,0:###,###,###,0} years",0:-100}";
            lblTime.Position = new Point((Game.GraphicsDevice.Viewport.Width - 64) - (int)lblTime.Measure.X/2, 28);

            Point screenCenter = new Point(Game.GraphicsDevice.Viewport.Width / 2, Game.GraphicsDevice.Viewport.Height / 2);

            lblGeneratingPlanet.Visible = !planet.Generated;
            if (lblGeneratingPlanet.Visible)
            {
                lblGeneratingPlanet.Text = $"Planet - {planet.GenerationString}";
                lblGeneratingPlanet.Position = screenCenter - new Point(0, lblGeneratingPlanet.Font.LineSpacing);
            }

            lblGeneratingMoon.Visible = !moon.Generated;
            if (lblGeneratingMoon.Visible)
            {
                lblGeneratingMoon.Text = $"Moon - {moon.GenerationString}";
                lblGeneratingMoon.Position = screenCenter;
            }

            lblGeneratingSun.Visible = !moon.Generated;
            if (lblGeneratingSun.Visible)
            {
                lblGeneratingSun.Text = $"Sun - {sun.GenerationString}";
                lblGeneratingSun.Position = screenCenter + new Point(0, lblGeneratingSun.Font.LineSpacing);
            }

            base.Update(gameTime);
        }

        IEnumerator WaitForPlanetBuild()
        {
            if(geopoiesisService.LoggedEvents.Count == 0)
                geopoiesisService.FireAnEvent(new SystemEvent() { Title = "Planet Forming!", Description = "There is an eon long swirl of dust and rock...." });
            else
                geopoiesisService.FireAnEvent(new SystemEvent() { Title = "Planet Re-Forming!", Description = "There was a glitch in the matrix..." });

            while (!planet.Generated)
            {
                yield return new WaitForEndOfFrame(Game);
            }

            float d = Vector3.Distance(planet.Transform.Position, camera.Transform.Position);
            float distToPlanetTarget = 10;
            float zoomLag = 1 - (distToPlanetTarget / d);
            float speed = 1;

            while (d > distToPlanetTarget)
            {
                d = Vector3.Distance(planet.Transform.Position, camera.Transform.Position);
                zoomLag = 1 - (distToPlanetTarget / d);

                Vector3 dir = planet.Transform.Position - camera.Transform.Position;
                dir.Normalize();

                camera.Transform.Translate(dir * speed * zoomLag);

                yield return new WaitForEndOfFrame(Game);

                foreach (float key in LODSettings.Keys)
                {
                    if (d <= key && planet.LodLevel != LODSettings[key])
                        planet.SetLODLevel(LODSettings[key]);
                }


                if (d - distToPlanetTarget < .025f)
                    break;
            }
            yield return new WaitForEndOfFrame(Game);

            geopoiesisService.CurrentEpoch = Epoch.PlanetFormed;

            geopoiesisService.FireAnEvent(new SystemEvent() { Title = "Planet Formed!", Description = "Dust and rock have coalesced into a planet..." });
        }

        IEnumerator StartGame()
        {
            while (!planet.Generated)
            {
                yield return new WaitForEndOfFrame(Game);
            }

            geopoiesisService.StartTheMarchOfTime();
        }

        public override void Draw(GameTime gameTime)
        {
            RasterizerState currentRasterizerState = GraphicsDevice.RasterizerState;

            base.Draw(gameTime);

            if (logBox == null)
            {
                
                float a = .75f;

                logBox = CreateBox(512, 780, new Rectangle(1, 1, 1, 1), new Color(0, 0, 0, a), new Color(1, 1, 1, .75f));
            }
            
            Vector2 screeCenter = new Vector2(Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height) * .5f;

            
            //Controls.
            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);

            // Volcanism
            string str;
            Vector2 p;
            nudVolcanism.Value = geopoiesisService.Volcanism * 100;
            //p = new Vector2(256 + 8, 16 + font.LineSpacing*5);
            //buttonBox = CreateBox(32, 32, new Rectangle(2, 2, 2, 2), new Color(.2f, .5f, .2f, 1), hudColor);
            //VolcPlus = new Rectangle((int)p.X, (int)p.Y, buttonBox.Width, buttonBox.Height);
            //_spriteBatch.Draw(buttonBox, VolcPlus,Color.White);
            //str = "+";
            //p = (p + (new Vector2(buttonBox.Width,buttonBox.Height) * .5f)) - font.MeasureString(str) * .5f;
            //_spriteBatch.DrawString(font, str, p, hudColor);
            //str = $"Volcanism [{geopoiesisService.Volcanism * 100, 0:000}]";
            //p.X += 32 + (150 - (font.MeasureString(str).X * .5f));
            //_spriteBatch.DrawString(font, str, p, hudColor);
            //p.X = 621;// testFont.MeasureString(str).X + 16;
            //VolcMinus = new Rectangle((int)p.X, (int)p.Y, buttonBox.Width, buttonBox.Height);
            //_spriteBatch.Draw(buttonBox, VolcMinus, Color.White);
            //str = "-";
            //p = (p + (new Vector2(buttonBox.Width, buttonBox.Height) * .5f)) - font.MeasureString(str) * .5f;
            //_spriteBatch.DrawString(font, str, p, hudColor);

            // Quakes
            p = new Vector2(256 + 8, 22 + font.LineSpacing * 6);
            buttonBox = CreateBox(32, 32, new Rectangle(2, 2, 2, 2), new Color(.2f, .5f, .2f, 1), hudColor);
            QuakePlus = new Rectangle((int)p.X, (int)p.Y, buttonBox.Width, buttonBox.Height);
            _spriteBatch.Draw(buttonBox, QuakePlus, Color.White);
            str = "+";
            p = (p + (new Vector2(buttonBox.Width, buttonBox.Height) * .5f)) - font.MeasureString(str) * .5f;
            _spriteBatch.DrawString(font, str, p, hudColor);
            str = $"Tectonic Shift [{geopoiesisService.Quakes * 100,0:000}]";
            p.X += 32 + (150 - (font.MeasureString(str).X * .5f));
            _spriteBatch.DrawString(font, str, p, hudColor);
            p.X = 621;// testFont.MeasureString(str).X + 16;
            QuakeMinus = new Rectangle((int)p.X, (int)p.Y, buttonBox.Width, buttonBox.Height);
            _spriteBatch.Draw(buttonBox, QuakeMinus, Color.White);
            str = "-";
            p = (p + (new Vector2(buttonBox.Width, buttonBox.Height) * .5f)) - font.MeasureString(str) * .5f;
            _spriteBatch.DrawString(font, str, p, hudColor);

            // Distance from star
            p = new Vector2(256 + 8, 28 + font.LineSpacing * 7);
            buttonBox = CreateBox(32, 32, new Rectangle(2, 2, 2, 2), new Color(.2f, .5f, .2f, 1), hudColor);
            DistPlus = new Rectangle((int)p.X, (int)p.Y, buttonBox.Width, buttonBox.Height);
            _spriteBatch.Draw(buttonBox, DistPlus, Color.White);
            str = "+";
            p = (p + (new Vector2(buttonBox.Width, buttonBox.Height) * .5f)) - font.MeasureString(str) * .5f;
            _spriteBatch.DrawString(font, str, p, hudColor);
            str = $"Distance From Star [{geopoiesisService.DistanceFromStar, 0:0.0} AU]";
            p.X += 32 + (150 - (font.MeasureString(str).X * .5f));
            _spriteBatch.DrawString(font, str, p, hudColor);
            p.X = 621;// testFont.MeasureString(str).X + 16;
            DistMinus = new Rectangle((int)p.X, (int)p.Y, buttonBox.Width, buttonBox.Height);
            _spriteBatch.Draw(buttonBox, DistMinus, Color.White);
            str = "-";
            p = (p + (new Vector2(buttonBox.Width, buttonBox.Height) * .5f)) - font.MeasureString(str) * .5f;
            _spriteBatch.DrawString(font, str, p, hudColor);


            quitBox = CreateBox(150,64, new Rectangle(2, 2, 2, 2), new Color(.1f, .5f, .1f, .75f), hudColor);
            QuitRect = new Rectangle(Game.GraphicsDevice.Viewport.Width - (quitBox.Width+8), Game.GraphicsDevice.Viewport.Height - (quitBox.Height + 8), quitBox.Width, quitBox.Height);
            _spriteBatch.Draw(quitBox, QuitRect, hudColor);

            p = new Vector2(QuitRect.X, QuitRect.Y);

            str = "Quit & Save";
            p.X += QuitRect.Width / 2 - font.MeasureString(str).X / 2;
            p.Y += font.LineSpacing * .75f;
            
            _spriteBatch.DrawString(font, str, p, hudColor);


            _spriteBatch.End();

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.RasterizerState = currentRasterizerState;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        }

        
        protected Texture2D CreateBox(int width, int height, Rectangle thickenss, Color bgColor, Color edgeColor)
        {
            return geopoiesisService.CreateBox(width, height, thickenss, bgColor, edgeColor);
        }

        protected void DrawSring(string msg, Vector2 position, Color color, SpriteFont font)
        {
            _spriteBatch.DrawString(font, msg, position + new Vector2(-1, 1), Color.Black);
            _spriteBatch.DrawString(font, msg, position, color);
        }

        public override void LoadScene()
        {
            base.LoadScene();
        }

        public override  void UnloadScene()
        {
            base.UnloadScene();
            State = SceneStateEnum.Unloaded;
        }
    }
}
