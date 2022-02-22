using Geopoiesis.Interfaces;
using Geopoiesis.Managers.Coroutines;
using Geopoiesis.Models;
using Geopoiesis.Models.Planet;
using Geopoiesis.Services;
using Geopoiesis.Scenes;
using Geopoiesis.Services.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

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
    // Music thanks to https://soundimage.org/
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;

        protected INoiseService noiseService;
        protected ICoroutineService coroutineService;
        protected IInputStateHandler inputHandlerService;
        protected IKeyboardStateManager kbManager;
        protected IMouseStateManager msManager;
        protected ICameraService camera;
        protected IAudioManager audioManager;

        protected GeopoiesisService geopoiesisService;

        protected RasterizerState initialRasterizerState;
        protected RasterizerState currentRasterizerState;

        protected BasicSceneManager sceneManager;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            //_graphics.IsFullScreen = true;
            Window.AllowUserResizing = true;
            Window.AllowAltF4 = true;
            _graphics.GraphicsProfile = GraphicsProfile.HiDef;

            _graphics.PreferredBackBufferHeight = 1080;
            _graphics.PreferredBackBufferWidth = 1920;


            _graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en");

            noiseService = new KeijiroPerlinService(this);
            coroutineService = new CoroutineService(this);
            audioManager = new AudioManagerService(this);
            geopoiesisService = new GeopoiesisService(this);
            kbManager = new KeyboardStateManager(this);
            msManager = new MouseStateManager(this);
            inputHandlerService = new InputHandlerService(this, kbManager, msManager);
            

            camera = new CameraService(this, 0.1f, 20000f);
            camera.ClearColor = Color.Black;
            camera.Transform.Position = new Vector3(0, 0, 500);

            sceneManager = new BasicSceneManager(this);

            initialRasterizerState = GraphicsDevice.RasterizerState;
            currentRasterizerState = initialRasterizerState;

            sceneManager.AddScene(new MainMenuScene(this, "mainMenu"));
            sceneManager.AddScene(new OptionsScene(this, "optionsMenu"));
            sceneManager.AddScene(new HelpScene(this, "helpMenue"));
            sceneManager.AddScene(new AudioOptionsScene(this, "audioOptions"));
            sceneManager.AddScene(new CreditsScene(this, "creditsScene"));
            sceneManager.AddScene(new GameScene(this, "mainGame"));
            sceneManager.AddScene(new PlanetTestScene(this, "testing"));
            sceneManager.AddScene(new ImageGenTest(this, "testImg"));

            base.Initialize();
        }

        protected override void LoadContent()
        {
            //_spriteBatch = new SpriteBatch(GraphicsDevice);


            //sceneManager.LoadScene("mainMenu");
            //sceneManager.LoadScene("testing");
            sceneManager.LoadScene("testImg");
        }

        protected override void Update(GameTime gameTime)
        {
            inputHandlerService.PreUpdate(gameTime);
            base.Update(gameTime);
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

            

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.RasterizerState = currentRasterizerState;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        }

        

        protected override void EndDraw()
        {
            base.EndDraw();

            coroutineService.UpdateEndFrame(null);
        }


    }
}
