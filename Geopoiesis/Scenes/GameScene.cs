using Geopoiesis.Enums;
using Geopoiesis.Interfaces;
using Geopoiesis.Managers.Coroutines;
using Geopoiesis.Models;
using Geopoiesis.Models.Planet;
using Geopoiesis.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Geopoiesis.Scenes
{
    public class GameScene : SceneBase
    {
        

        SpriteBatch _spriteBatch;
        SpriteFont testFont;
        SpriteFont debugFont;
        PlanetGeometry planet;
        Atmosphere atmos;
        ParticleEmitter pet;
        Cube cube;

        protected float DisplacementMag = .5f;
        float _MinDeepSeaDepth = .07f;
        float _MinSeaDepth = .4f;
        float _MinShoreDepth = .42f;
        float _MinLand = .55f;
        float _MinHill = .7f;

        Dictionary<float, int> LODSettings = new Dictionary<float, int>()
        {
            { 500, 7 },
            { 300, 6 },
            { 200, 5 },
            { 100, 4 },
            { 50, 3 },
            { 25, 2 },
        };


        Vector3 ld = Vector3.Left;

        public GameScene(Game game, string name) : base(game, name) { }

        public override void Initialize()
        {
            SkyBox skyBox = new SkyBox(Game, "Shaders/SkyBox");
            Components.Add(skyBox);


            //cube = new Cube(this, "Shaders/basic");
            //cube.Transform.Position = new Vector3(-3, 0, -10);
            //Components.Add(cube);

            planet = new PlanetGeometry(Game, "Shaders/PlanetSplatMap");
            planet.Transform.Position = new Vector3(0, 0, 0);
            Components.Add(planet);

            atmos = new Atmosphere(Game, "Shaders/AtmosShader");
            atmos.Transform.Scale = Vector3.One * 4.25f;
            //atmos.Transform.Parent = planet.Transform;
            Components.Add(atmos);

            // Make sure all particle emitters are added last...
            pet = new ParticleEmitter(Game);
            //Components.Add(pet);

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

            State = SceneStateEnum.Open;

        }

        protected override void LoadContent()
        {
            base.LoadContent();
            _spriteBatch = new SpriteBatch(Game.GraphicsDevice);
            testFont = Game.Content.Load<SpriteFont>("SpriteFont/font");
            debugFont = Game.Content.Load<SpriteFont>("SpriteFont/debugfont");
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
            float translateSpeed = 2f;
            float rotateSpeed = 1f;

            translateSpeed *= (float)gameTime.ElapsedGameTime.TotalSeconds;
            rotateSpeed *= (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Test moving the camera about.
            if (kbManager.KeyDown(Keys.W))
                camera.Transform.Translate(Vector3.Forward * translateSpeed);
            if (kbManager.KeyDown(Keys.S))
                camera.Transform.Translate(Vector3.Backward * translateSpeed);
            if (kbManager.KeyDown(Keys.A))
                camera.Transform.Translate(Vector3.Left * translateSpeed);
            if (kbManager.KeyDown(Keys.D))
                camera.Transform.Translate(Vector3.Right * translateSpeed);

            if (kbManager.KeyDown(Keys.Down))
                camera.Transform.Rotate(Vector3.Left, rotateSpeed);
            if (kbManager.KeyDown(Keys.Up))
                camera.Transform.Rotate(Vector3.Right, rotateSpeed);
            if (kbManager.KeyDown(Keys.Right))
                camera.Transform.Rotate(Vector3.Down, rotateSpeed);
            if (kbManager.KeyDown(Keys.Left))
                camera.Transform.Rotate(Vector3.Up, rotateSpeed);

            //planet.Transform.Rotate(Vector3.Up, .001f);
            atmos.Transform.Scale = (Vector3.One * 4.25f) + (Vector3.One * (planet.Radius * .8f) * DisplacementMag);

            if (kbManager.KeyPress(Keys.F1))
                camera.RenderWireFrame = !camera.RenderWireFrame;

            float dmod = .01f;
            if (kbManager.KeyDown(Keys.Q))
                DisplacementMag = MathHelper.Min(1, DisplacementMag + dmod);
            if (kbManager.KeyDown(Keys.E))
                DisplacementMag = MathHelper.Max(0, DisplacementMag - dmod);

            if (kbManager.KeyDown(Keys.R))
                _MinDeepSeaDepth = MathHelper.Min(1, _MinDeepSeaDepth + dmod);
            if (kbManager.KeyDown(Keys.T))
                _MinDeepSeaDepth = MathHelper.Max(0, _MinDeepSeaDepth - dmod);

            if (kbManager.KeyDown(Keys.F))
                _MinSeaDepth = MathHelper.Min(1, _MinSeaDepth + dmod);
            if (kbManager.KeyDown(Keys.G))
                _MinSeaDepth = MathHelper.Max(0, _MinSeaDepth - dmod);

            if (kbManager.KeyDown(Keys.Y))
                _MinShoreDepth = MathHelper.Min(1, _MinShoreDepth + dmod);
            if (kbManager.KeyDown(Keys.U))
                _MinShoreDepth = MathHelper.Max(0, _MinShoreDepth - dmod);

            if (kbManager.KeyDown(Keys.J))
                _MinLand = MathHelper.Min(1, _MinLand + dmod);
            if (kbManager.KeyDown(Keys.K))
                _MinLand = MathHelper.Max(0, _MinLand - dmod);

            if (kbManager.KeyDown(Keys.I))
                _MinHill = MathHelper.Min(1, _MinHill + dmod);
            if (kbManager.KeyDown(Keys.O))
                _MinHill = MathHelper.Max(0, _MinHill - dmod);

            if (kbManager.KeyPress(Keys.F2))
                planet.SetLODLevel(planet.LodLevel + 1);
            if (kbManager.KeyPress(Keys.F3))
                planet.SetLODLevel(planet.LodLevel - 1);


            if (planet.effect != null)
            {
                // Change values in the planet's shader.
                if (planet.effect.Parameters["colorRamp"] != null) // Or generate this procedurally. 
                    planet.effect.Parameters["colorRamp"].SetValue(Game.Content.Load<Texture2D>("Textures/Ramp1"));

                if (planet.effect.Parameters["displacemntMag"] != null)
                    planet.effect.Parameters["displacemntMag"].SetValue(DisplacementMag);

                if (planet.effect.Parameters["_MinDeepSeaDepth"] != null)
                    planet.effect.Parameters["_MinDeepSeaDepth"].SetValue(_MinDeepSeaDepth);

                if (planet.effect.Parameters["_MinSeaDepth"] != null)
                    planet.effect.Parameters["_MinSeaDepth"].SetValue(_MinSeaDepth);

                if (planet.effect.Parameters["_MinShoreDepth"] != null)
                    planet.effect.Parameters["_MinShoreDepth"].SetValue(_MinShoreDepth);

                if (planet.effect.Parameters["_MinLand"] != null)
                    planet.effect.Parameters["_MinLand"].SetValue(_MinLand);

                if (planet.effect.Parameters["_MinHill"] != null)
                    planet.effect.Parameters["_MinHill"].SetValue(_MinHill);
            }

            planet.LightDirection = ld;
            atmos.LightDirection = ld;

            base.Update(gameTime);
        }

        IEnumerator WaitForPlanetBuild()
        {
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

            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);

            Vector2 textPos = new Vector2(8, 8);
            DrawSring($"Camera Transform: {camera.Transform}", textPos, Color.Gold, testFont);
            textPos.Y += testFont.LineSpacing;

            if (planet != null)
            {
                textPos.Y += testFont.LineSpacing;
                foreach (string line in planet.Debug)
                {
                    DrawSring(line, textPos, Color.Silver, debugFont);
                    textPos.Y += debugFont.LineSpacing;
                }
            }

            textPos = new Vector2(GraphicsDevice.PresentationParameters.BackBufferWidth - 600, 8 + testFont.LineSpacing);

            DrawSring($"F1 Toggle Wire Frame", textPos, Color.Gold, testFont);
            textPos.Y += testFont.LineSpacing;
            DrawSring($"WASD = Translate Camera", textPos, Color.Gold, testFont);
            textPos.Y += testFont.LineSpacing;
            DrawSring($"Q/E = Displacement [{DisplacementMag}]", textPos, Color.Gold, testFont);
            textPos.Y += testFont.LineSpacing;
            DrawSring($"R/T = _MinDeepSeaDepth [{_MinDeepSeaDepth}]", textPos, Color.Gold, testFont);
            textPos.Y += testFont.LineSpacing;
            DrawSring($"F/G = _MinSeaDepth [{_MinSeaDepth}]", textPos, Color.Gold, testFont);
            textPos.Y += testFont.LineSpacing;
            DrawSring($"Y/U = _MinShoreDepth [{_MinShoreDepth}]", textPos, Color.Gold, testFont);
            textPos.Y += testFont.LineSpacing;
            DrawSring($"J/K = _MinLand [{_MinLand}]", textPos, Color.Gold, testFont);
            textPos.Y += testFont.LineSpacing;
            DrawSring($"I/O = _MinHill [{_MinHill}]", textPos, Color.Gold, testFont);
            textPos.Y += testFont.LineSpacing;
            if (planet.Generated)
            {
                DrawSring($"F2/F3 = LOD [{planet.LodLevel + 1}] ({planet.LodSizes[planet.LodLevel]})", textPos, Color.Gold, testFont);
                textPos.Y += testFont.LineSpacing;
            }

            DrawSring($"Time: [{geopoiesisService.Years,0:###,###,###,0} years]", textPos, Color.Gold, testFont);
            textPos.Y += testFont.LineSpacing;
            DrawSring($"Epoch: [{geopoiesisService.CurrentEpoch}]", textPos, Color.Gold, testFont);
            textPos.Y += testFont.LineSpacing;


            // Render planet cube map.


            _spriteBatch.End();

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.RasterizerState = currentRasterizerState;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
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
            State = SceneStateEnum.Closed;
        }
    }
}
