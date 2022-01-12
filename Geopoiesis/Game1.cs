using Geopoiesis.Interfaces;
using Geopoiesis.Managers.Coroutines;
using Geopoiesis.Models;
using Geopoiesis.Models.Planet;
using Geopoiesis.Services;
using Geopoiesis.Services.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// geopoiesis
///
/// geo = earth, poiesis = making
///
/// "
///     second, there is the principle of gaian geopoiesis,
///     a global principle of self-organization, that trumps the
///     interests of individuals and species.
/// "
/// Technology and the Contested Meanings of Sustainability by Aidan Davison
///
/// In Geopoiesis, you have a bare lump of rock that you need to form into a planet that can sustain life.
/// 
/// Current thinking, Start with a ball of mud, select a start to orbit, select it's distance and speed, result gives a starting live rating.
/// Object if the game to get life to be sentient and have a tech level high enough to leave the planet.
/// Life goes through epochs, starting with microscopic life (thinking of Conway's game of life for this bit 
/// [https://en.wikipedia.org/wiki/Conway%27s_Game_of_Life] rendered as small cubes if at all), to plant (giving rise to more oxygen, or any other
/// exhaust chemical, that then gives rise to animal life, then early sophonts [https://en.wiktionary.org/wiki/sophont], when then move up through tech levels.
/// 
/// While our player en devours to improve life, there will be random events that will "challenge" them, meteor showers, cosmic rays, 
/// depending on the star they chose, sun flares, alien visitors, these events will be both problematic and benificial to the player.
/// 
/// Game time will be 1000 years a second, and can be slowed to 100 years a second if they like.
/// 
/// Should the player reach 0 life on their rock, they get up to a maximum of 2 panspermia comets [https://en.wikipedia.org/wiki/Panspermia] that will give them a life boost.
/// In fact, they may get one for free as an event which could either bring aid, or deadly viruses to the world...
/// 
/// </summary>
namespace Geopoiesis
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        protected INoiseService noiseService;
        protected ICoroutineService coroutineService;
        protected IInputStateHandler inputHandlerService;
        protected IKeyboardStateManager kbManager;
        protected ICameraService camera;

        protected GeopoiesisService geopoiesisService;

        protected RasterizerState initialRasterizerState;
        protected RasterizerState currentRasterizerState;

        SpriteFont testFont;
        SpriteFont debugFont;
        PlanetGeometry planet;
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


        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _graphics.GraphicsProfile = GraphicsProfile.HiDef;


            //Window.AllowUserResizing = true;
            //Window.IsBorderless = false;
            //_graphics.IsFullScreen = true;
            _graphics.PreferredBackBufferHeight = 1080;
            _graphics.PreferredBackBufferWidth = 1920;
            _graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            noiseService = new KeijiroPerlinService(this);
            coroutineService = new CoroutineService(this);
            geopoiesisService = new GeopoiesisService(this);
            kbManager = new KeyboardStateManager(this);
            inputHandlerService = new InputHandlerService(this, kbManager);

            SkyBox skyBox = new SkyBox(this, "Shaders/SkyBox");
            Components.Add(skyBox);

            camera = new CameraService(this,0.1f,20000f);
            camera.ClearColor = Color.Black;
            camera.Transform.Position = new Vector3(0, 0, 500);


            //cube = new Cube(this, "Shaders/basic");
            //cube.Transform.Position = new Vector3(-3, 0, -10);
            //Components.Add(cube);

            planet = new PlanetGeometry(this, "Shaders/ShaderColor");
            planet.Transform.Position = new Vector3(0, 0, 0);
            
            Components.Add(planet);

            Atmosphere atmos = new Atmosphere(this, "Shaders/AtmosShader");
            atmos.Transform.Scale = Vector3.One * 4.25f;
            //atmos.Transform.Parent = planet.Transform;
            Components.Add(atmos);

            // Make sure all particle emitters are added last...
            pet = new ParticleEmitter(this);
            //Components.Add(pet);

            base.Initialize();

            initialRasterizerState = GraphicsDevice.RasterizerState;
            currentRasterizerState = initialRasterizerState;

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

                t = Content.Load<Texture2D>($"Textures/Particles/flare{i}");

                pet.AddParticle(v, Vector3.One * .25f, t, Color.White);
            }

            coroutineService.StartCoroutine(WaitForPlanetBuild());
            coroutineService.StartCoroutine(StartGame());
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            testFont = Content.Load<SpriteFont>("SpriteFont/font");
            debugFont = Content.Load<SpriteFont>("SpriteFont/debugfont");
        }

        protected override void Update(GameTime gameTime)
        {
            if (kbManager.KeyPress(Keys.Escape))
                Exit();

            float translateSpeed = .1f;
            float rotateSpeed = .01f;

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

            planet.Transform.Rotate(Vector3.Up, .001f);
            

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


            // Change values in the planet's shader.
            if (planet.effect.Parameters["colorRamp"] != null) // Or generate this procedurally. 
                planet.effect.Parameters["colorRamp"].SetValue(Content.Load<Texture2D>("Textures/Ramp1"));

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

            inputHandlerService.PreUpdate(gameTime);
            base.Update(gameTime);
        }

        IEnumerator WaitForPlanetBuild()
        {
            while (!planet.Generated)
            {
                yield return new WaitForEndOfFrame(this);
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
                
                yield return new WaitForEndOfFrame(this);

                foreach (float key in LODSettings.Keys) 
                {
                    if (d <= key && planet.LodLevel != LODSettings[key])
                        planet.SetLODLevel(LODSettings[key]);
                }
                

                if (d - distToPlanetTarget < .025f)
                    break;
            }
            yield return new WaitForEndOfFrame(this);
        }

        // Needs to be part of scene manager (thegame scene)
        IEnumerator StartGame()
        {
            while (!planet.Generated)
            {
                yield return new WaitForEndOfFrame(this);
            }

            geopoiesisService.StartTheMarchOfTime();
        }

        protected override void Draw(GameTime gameTime)
        {
            currentRasterizerState = GraphicsDevice.RasterizerState;

            if (camera.RenderWireFrame)
                currentRasterizerState = new RasterizerState() { FillMode = FillMode.WireFrame };
            else
                currentRasterizerState = new RasterizerState() { FillMode = FillMode.Solid };


            GraphicsDevice.Clear(camera.ClearColor);

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
                DrawSring($"F2/F3 = LOD [{planet.LodLevel+1}] ({planet.LodSizes[planet.LodLevel]})", textPos, Color.Gold, testFont);
                textPos.Y += testFont.LineSpacing;
            }

            DrawSring($"Time: [{geopoiesisService.Years, 0:###,###,###,0} years]", textPos, Color.Gold, testFont);
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
            _spriteBatch.DrawString(font, msg, position + new Vector2(-1,1), Color.Black);
            _spriteBatch.DrawString(font, msg, position, color);
        }

        protected override void EndDraw()
        {
            base.EndDraw();

            coroutineService.UpdateEndFrame(null);
        }


    }
}
