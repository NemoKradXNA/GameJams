using Geopoiesis.Enums;
using Geopoiesis.Interfaces;
using Geopoiesis.Managers.Coroutines;
using Geopoiesis.Models;
using Geopoiesis.Models.Planet;
using Geopoiesis.Services;
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

            moon = new MorphableSphere(Game, "Shaders/Moon", 2, 1, 1.25f, 128,geopoiesisService.Seed*2, 5, 7 );
            moon.Transform.Parent = moonAnchor;
            moon.Transform.Position = (Vector3.Left + (Vector3.Up * .25f)) * 32;
            Components.Add(moon);

            sun = new MorphableSphere(Game, "Shaders/Sun", 2, 10,16, 16,1971,3,7 );
            sun.Transform.Position = new Vector3(-1, 0, 0)  * 100;
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

            base.Initialize();

            State = SceneStateEnum.Loaded;

            geopoiesisService.StartType = StartType.G;

            geopoiesisService.OnSystemEventFired -= EventFired;
            geopoiesisService.OnSystemEventFired += EventFired;



            audioManager.PlaySong("Audio/Music/Midnight-Mist", .5f);
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
            font = Game.Content.Load<SpriteFont>("SpriteFont/font");
            logFont = Game.Content.Load<SpriteFont>("SpriteFont/logFont");
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
                //moon.Transform.Rotate(Vector3.Up + Vector3.Right, .0025f);
                atmos.Transform.Scale = (Vector3.One * 4.25f) + (Vector3.One * (planet.Radius * .8f) * DisplacementMag);

                //if (kbManager.KeyPress(Keys.F1))
                //    camera.RenderWireFrame = !camera.RenderWireFrame;

                float dmod = .01f;
                //if (kbManager.KeyDown(Keys.Q))
                //    DisplacementMag = MathHelper.Min(1, DisplacementMag + dmod);
                //if (kbManager.KeyDown(Keys.E))
                //    DisplacementMag = MathHelper.Max(0, DisplacementMag - dmod);

                //if (kbManager.KeyDown(Keys.R))
                //    geopoiesisService.WaterLevel = MathHelper.Min(1, geopoiesisService.WaterLevel + dmod);
                //if (kbManager.KeyDown(Keys.T))
                //    geopoiesisService.WaterLevel = MathHelper.Max(0, geopoiesisService.WaterLevel - dmod);

                //if (kbManager.KeyDown(Keys.F))
                //    geopoiesisService.OZone = MathHelper.Min(1, geopoiesisService.OZone + dmod);
                //if (kbManager.KeyDown(Keys.G))
                //    geopoiesisService.OZone = MathHelper.Max(0, geopoiesisService.OZone - dmod);

                //if (kbManager.KeyDown(Keys.Y))
                //    geopoiesisService.LifeLevel = MathHelper.Min(1, geopoiesisService.LifeLevel + dmod);
                //if (kbManager.KeyDown(Keys.U))
                //    geopoiesisService.LifeLevel = MathHelper.Max(0, geopoiesisService.LifeLevel - dmod);

                //if (kbManager.KeyDown(Keys.J))
                //    _MinLand = MathHelper.Min(1, _MinLand + dmod);
                //if (kbManager.KeyDown(Keys.K))
                //    _MinLand = MathHelper.Max(0, _MinLand - dmod);

                //if (kbManager.KeyDown(Keys.I))
                //    _MinHill = MathHelper.Min(1, _MinHill + dmod);
                //if (kbManager.KeyDown(Keys.O))
                //    _MinHill = MathHelper.Max(0, _MinHill - dmod);

                //if (kbManager.KeyPress(Keys.F2))
                //    planet.SetLODLevel(planet.LodLevel + 1);
                //if (kbManager.KeyPress(Keys.F3))
                //    planet.SetLODLevel(planet.LodLevel - 1);


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
                    if (msManager.PositionRect.Intersects(VolcPlus))
                    {
                        audioManager.PlaySFX("Audio/SFX/dtmf-5");
                        geopoiesisService.Volcanism = Math.Min(1, geopoiesisService.Volcanism + .01f);
                    }
                    if (msManager.PositionRect.Intersects(VolcMinus))
                    {
                        audioManager.PlaySFX("Audio/SFX/dtmf-5");
                        geopoiesisService.Volcanism = Math.Max(0, geopoiesisService.Volcanism - .01f);
                    }
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

        // Needs to be part of scene manager (thegame scene)
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

            //dpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);

            //Vector2 textPos = new Vector2(8, 8);
            //DrawSring($"Camera Transform: {camera.Transform}", textPos, Color.Gold, testFont);
            //textPos.Y += testFont.LineSpacing;

            //if (planet != null)
            //{
            //    textPos.Y += testFont.LineSpacing;
            //    foreach (string line in planet.Debug)
            //    {
            //        DrawSring(line, textPos, Color.Silver, debugFont);
            //        textPos.Y += debugFont.LineSpacing;
            //    }
            //}

            //textPos = new Vector2(GraphicsDevice.PresentationParameters.BackBufferWidth - 600, 8 + testFont.LineSpacing);

            //DrawSring($"F1 Toggle Wire Frame", textPos, Color.Gold, testFont);
            //textPos.Y += testFont.LineSpacing;
            //DrawSring($"WASD = Translate Camera", textPos, Color.Gold, testFont);
            //textPos.Y += testFont.LineSpacing;
            //DrawSring($"Q/E = Displacement [{DisplacementMag}]", textPos, Color.Gold, testFont);
            //textPos.Y += testFont.LineSpacing;
            //DrawSring($"R/T = _MinDeepSeaDepth [{_MinDeepSeaDepth}]", textPos, Color.Gold, testFont);
            //textPos.Y += testFont.LineSpacing;
            //DrawSring($"F/G = _MinSeaDepth [{_MinSeaDepth}]", textPos, Color.Gold, testFont);
            //textPos.Y += testFont.LineSpacing;
            //DrawSring($"Y/U = _MinShoreDepth [{_MinShoreDepth}]", textPos, Color.Gold, testFont);
            //textPos.Y += testFont.LineSpacing;
            //DrawSring($"J/K = _MinLand [{_MinLand}]", textPos, Color.Gold, testFont);
            //textPos.Y += testFont.LineSpacing;
            //DrawSring($"I/O = _MinHill [{_MinHill}]", textPos, Color.Gold, testFont);
            //textPos.Y += testFont.LineSpacing;
            //if (planet.Generated)
            //{
            //    DrawSring($"F2/F3 = LOD [{planet.LodLevel + 1}] ({planet.LodSizes[planet.LodLevel]})", textPos, Color.Gold, testFont);
            //    textPos.Y += testFont.LineSpacing;
            //}

            //DrawSring($"Time: [{geopoiesisService.Years,0:###,###,###,0} years]", textPos, Color.Gold, testFont);
            //textPos.Y += testFont.LineSpacing;
            //DrawSring($"Epoch: [{geopoiesisService.CurrentEpoch}]", textPos, Color.Gold, testFont);
            //textPos.Y += testFont.LineSpacing;


            // Render planet cube map.


            //_spriteBatch.End();
            
            if (hudBorder == null)
            {
                gameFont = Game.Content.Load<SpriteFont>("SpriteFont/GameFont");
                float a = .75f;
                hudBorder = CreateBox(Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height, new Rectangle(1,1,1,1), Color.Transparent, new Color(1, 1, 1, a));
                starBox = CreateBox(256, 256, new Rectangle(1, 1, 1, 1), new Color(0, 0, 0, a), new Color(1, 1, 1, .75f));
                pixel = CreateBox(1, 1, new Rectangle(1, 1, 1, 1), Color.Transparent, new Color(1, 1, 1, .75f));
                statBox = CreateBox(256, font.LineSpacing,  new Rectangle(1,1,1,1), new Color(0, 0, 0, a), new Color(1, 1, 1, .75f));

                logBox = CreateBox(512, 780, new Rectangle(1, 1, 1, 1), new Color(0, 0, 0, a), new Color(1, 1, 1, .75f));
            }
            
            Vector2 screeCenter = new Vector2(Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height) * .5f;

            // HUD

            Vector2 p = screeCenter;
            string str;
            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);

            _spriteBatch.Draw(hudBorder, new Rectangle(0, 0, hudBorder.Width, hudBorder.Height), hudColor);
            _spriteBatch.Draw(starBox, new Rectangle(0, 0, 256, 256), hudColor);
            _spriteBatch.Draw(Game.Content.Load<Texture2D>($"Textures/Stars/{geopoiesisService.StartType}"), new Rectangle(64, 42, 120, 120), Color.White);
            _spriteBatch.Draw(pixel, new Rectangle(0, 200, 256, 1), hudColor );

            str = $"{geopoiesisService.StartType} - Class Star";
            p = new Vector2(128, 256 - (font.LineSpacing * 1f));
            p -= font.MeasureString(str) * .5f;
            _spriteBatch.DrawString(font, str, p, hudColor);

            int l = 32;
            str = "H2O:";
            p = new Vector2(256 + 8 , 8);
            _spriteBatch.DrawString(font, str, p, hudColor);
            _spriteBatch.Draw(statBox, new Rectangle((int)p.X + 64 + l, (int)p.Y, 256, font.LineSpacing), hudColor);

            // Calc stat Value
            SetStatTexutre(geopoiesisService.WaterLevel,Color.DarkBlue, Color.LightBlue);
            _spriteBatch.Draw(statValue, new Rectangle((int)p.X + 64 + 1 + l, (int)p.Y + 1, 256-1, font.LineSpacing-1), Color.White);
            str = $"{geopoiesisService.WaterLevel * 100, 0:000}";
            Vector2 pp = new Vector2(p.X + 64 + 128 + l, p.Y + font.LineSpacing *.5f);
            pp -= font.MeasureString(str) * .5f;
            _spriteBatch.DrawString(font, str, pp, hudColor);

            str = "O3:";
            p.Y += font.LineSpacing;
            _spriteBatch.DrawString(font, str, p, hudColor);
            _spriteBatch.Draw(statBox, new Rectangle((int)p.X + 64 + l, (int)p.Y, 256, font.LineSpacing), hudColor);

            // Calc stat Value
            SetStatTexutre(geopoiesisService.OZone, Color.DarkSlateGray, Color.LightSkyBlue);
            _spriteBatch.Draw(statValue, new Rectangle((int)p.X + 64 + 1 + l, (int)p.Y + 1, 256 - 1, font.LineSpacing - 1), Color.White);
            str = $"{geopoiesisService.OZone * 100,0:000}";
            pp = new Vector2(p.X + 64 + 128 + l, p.Y + font.LineSpacing * .5f);
            pp -= font.MeasureString(str) * .5f;
            _spriteBatch.DrawString(font, str, pp, hudColor);


            str = "Life:";
            p.Y += font.LineSpacing;
            _spriteBatch.DrawString(font, str, p, hudColor);
            _spriteBatch.Draw(statBox, new Rectangle((int)p.X + 64 + l, (int)p.Y, 256, font.LineSpacing), hudColor);

            // Calc stat Value
            SetStatTexutre(geopoiesisService.LifeLevel, Color.Firebrick, Color.ForestGreen);
            _spriteBatch.Draw(statValue, new Rectangle((int)p.X + 64 + 1 + l, (int)p.Y + 1, 256 - 1, font.LineSpacing - 1), Color.White);
            str = $"{geopoiesisService.LifeLevel * 100,0:000}";
            pp = new Vector2(p.X + 64 + 128 + l, p.Y + font.LineSpacing * .5f);
            pp -= font.MeasureString(str) * .5f;
            _spriteBatch.DrawString(font, str, pp, hudColor);

            str = "AU:";
            p.Y += font.LineSpacing;
            _spriteBatch.DrawString(font, str, p, hudColor);
            _spriteBatch.Draw(statBox, new Rectangle((int)p.X + 64 + l, (int)p.Y, 256, font.LineSpacing), hudColor);

            // Calc stat Value
            SetStatTexutre(geopoiesisService.DistanceFromStar/10f, Color.Gold, Color.White);
            _spriteBatch.Draw(statValue, new Rectangle((int)p.X + 64 + 1 + l, (int)p.Y + 1, 256 - 1, font.LineSpacing - 1), Color.White);
            str = $"{geopoiesisService.DistanceFromStar,0:0.0} AU";
            pp = new Vector2(p.X + 64 + 128 + l, p.Y + font.LineSpacing * .5f);
            pp -= font.MeasureString(str) * .5f;
            _spriteBatch.DrawString(font, str, pp, hudColor);

            str = "Temp:";
            p.Y += font.LineSpacing;
            _spriteBatch.DrawString(font, str, p, hudColor);
            _spriteBatch.Draw(statBox, new Rectangle((int)p.X + 64 + l, (int)p.Y, 256, font.LineSpacing), hudColor);

            // Calc stat Value
            SetStatTexutre(geopoiesisService.SurfaceTemp / 10f, Color.Gold, Color.White);
            _spriteBatch.Draw(statValue, new Rectangle((int)p.X + 64 + 1 + l, (int)p.Y + 1, 256 - 1, font.LineSpacing - 1), Color.White);
            str = $"{geopoiesisService.SurfaceTemp,0:00} c";
            pp = new Vector2(p.X + 64 + 128 + l, p.Y + font.LineSpacing * .5f);
            pp -= font.MeasureString(str) * .5f;
            _spriteBatch.DrawString(font, str, pp, hudColor);

            // Epoch and years..
            str = $"{$"[Epoch: {geopoiesisService.CurrentEpoch}]", 0:10}";
            p = screeCenter;
            p -= font.MeasureString(str) * .5f;
            p.Y = font.LineSpacing;
            _spriteBatch.DrawString(font, str, p, hudColor);

            str = $"{$"{geopoiesisService.Years,0:###,###,###,0} years", 0:-100}";
            p = new Vector2(Game.GraphicsDevice.Viewport.Width-64,0);
            p -= font.MeasureString(str);
            p.Y = font.LineSpacing;
            _spriteBatch.DrawString(font, str, p, hudColor);


            
            
            p = screeCenter;
            if (!planet.Generated)
            {                
                str = $"Planet - {planet.GenerationString}";
                p -= gameFont.MeasureString(str) * .5f;
                
                _spriteBatch.DrawString(gameFont, str, p + shaddowOffset, shadowColor);
                _spriteBatch.DrawString(gameFont, str, p, hudColor);
            }

            if (!moon.Generated)
            {
                p = screeCenter;
                p.Y += gameFont.LineSpacing;
                str = $"Moon - {moon.GenerationString}";
                p -= gameFont.MeasureString(str) * .5f;

                _spriteBatch.DrawString(gameFont, str, p + shaddowOffset, shadowColor);
                _spriteBatch.DrawString(gameFont, str, p, hudColor);
            }

            if (!sun.Generated)
            {
                p = screeCenter;
                p.Y += gameFont.LineSpacing * 2;
                str = $"Sun - {sun.GenerationString}";
                p -= gameFont.MeasureString(str) * .5f;

                _spriteBatch.DrawString(gameFont, str, p + shaddowOffset, shadowColor);
                _spriteBatch.DrawString(gameFont, str, p, hudColor);
            }

            // Draw Event Logger
            Rectangle eventLogRect = new Rectangle(0, 300, logBox.Width, logBox.Height);
            str = "Events:-";
            _spriteBatch.DrawString(font, str, new Vector2(8, 300 - font.LineSpacing), hudColor);
            _spriteBatch.Draw(logBox, eventLogRect, hudColor);

            _spriteBatch.End();

            Rectangle orgRect = _spriteBatch.GraphicsDevice.ScissorRectangle;
            Rectangle eventLogRectCull = new Rectangle(eventLogRect.X + 1, eventLogRect.Y + 1, eventLogRect.Width - 2, eventLogRect.Height - 2);
            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.DepthRead, new RasterizerState() { ScissorTestEnable = true, });
            _spriteBatch.GraphicsDevice.ScissorRectangle = eventLogRectCull;
            p = new Vector2(12, eventLogRect.Y + logFont.LineSpacing*.5f);
            for (int e = geopoiesisService.LoggedEvents.Count - 1; e >= 0; e--)
            {
                SystemEvent thisEvt = geopoiesisService.LoggedEvents[e];
                _spriteBatch.DrawString(logFont, $"[{thisEvt.Title}] - {thisEvt.YearArrives, 0:###,###,##0} years", p, thisEvt.TitleColor);
                p.Y += logFont.LineSpacing;
                _spriteBatch.DrawString(logFont, thisEvt.Description, p, thisEvt.TextColor);
                p.Y += logFont.LineSpacing;
            }

            
            _spriteBatch.End();

            //Controls.
            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);

            // Volcanism
            p = new Vector2(256 + 8, 12 + font.LineSpacing*5);
            buttonBox = CreateBox(32, 32, new Rectangle(2, 2, 2, 2), new Color(.2f, .5f, .2f, 1), hudColor);
            VolcPlus = new Rectangle((int)p.X, (int)p.Y, buttonBox.Width, buttonBox.Height);
            _spriteBatch.Draw(buttonBox, VolcPlus,Color.White);
            str = "+";
            p = (p + (new Vector2(buttonBox.Width,buttonBox.Height) * .5f)) - font.MeasureString(str) * .5f;
            _spriteBatch.DrawString(font, str, p, hudColor);
            str = $"Volcanism [{geopoiesisService.Volcanism * 100, 0:000}]";
            p.X += 32 + (150 - (font.MeasureString(str).X * .5f));
            _spriteBatch.DrawString(font, str, p, hudColor);
            p.X = 621;// testFont.MeasureString(str).X + 16;
            VolcMinus = new Rectangle((int)p.X, (int)p.Y, buttonBox.Width, buttonBox.Height);
            _spriteBatch.Draw(buttonBox, VolcMinus, Color.White);
            str = "-";
            p = (p + (new Vector2(buttonBox.Width, buttonBox.Height) * .5f)) - font.MeasureString(str) * .5f;
            _spriteBatch.DrawString(font, str, p, hudColor);

            // Quakes
            p = new Vector2(256 + 8, 18 + font.LineSpacing * 6);
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
            p = new Vector2(256 + 8, 24 + font.LineSpacing * 7);
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

        SpriteFont gameFont;
        Texture2D hudBorder;
        Texture2D starBox;
        Texture2D logBox;
        Texture2D pixel;
        Texture2D buttonBox;
        Texture2D quitBox;

        Texture2D statBox;
        Texture2D statValue;

        Rectangle VolcPlus;
        Rectangle VolcMinus;

        Rectangle QuakePlus;
        Rectangle QuakeMinus;

        Rectangle DistPlus;
        Rectangle DistMinus;

        Rectangle QuitRect;

        protected void SetStatTexutre(float v, Color min, Color max)
        {
            statValue = new Texture2D(Game.GraphicsDevice, 128, 1);
            Color[] c = new Color[128];

            for (int x = 0; x < 128; x++)
            {
                Color col = Color.Transparent;
                float t = x / 128f;

                if(t<= v)
                    col = Color.Lerp(min, max, t/v);

                c[x] = col;
            }

            statValue.SetData(c);
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
