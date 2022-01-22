using Geopoiesis.Interfaces;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Geopoiesis.Services.Input
{
    public class InputHandlerService : GameComponent, IInputStateHandler
    {
        public IKeyboardStateManager KeyboardManager { get; set; }
        /// <summary>
        /// Manager for game pad input, available on all platforms
        /// </summary>
        public IGamePadManager GamePadManager { get; set; }
        /// <summary>
        /// Manager used for mouse input, available in Windows only
        /// </summary>
        public IMouseStateManager MouseManager { get; set; }

        public InputHandlerService(Game game, IKeyboardStateManager kbm = null, IMouseStateManager msm = null,
           IGamePadManager gpm = null) : base(game)
        {
            KeyboardManager = kbm;
            GamePadManager = gpm;
            MouseManager = msm;

            Game.Services.AddService(typeof(IInputStateHandler), this);
            Game.Components.Add(this);
        }

        public override void Initialize()
        {
            base.Initialize();

            if (KeyboardManager != null)
                KeyboardManager.Initialize();

            if (GamePadManager != null)
                GamePadManager.Initialize();

            if (MouseManager != null)
                MouseManager.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            if (Game.IsActive)
            {
                if (KeyboardManager != null)
                    KeyboardManager.Update(gameTime);

                if (GamePadManager != null)
                    GamePadManager.Update(gameTime);

                if (MouseManager != null)
                    MouseManager.Update(gameTime);

                base.Update(gameTime);
            }
        }

        public void PreUpdate(GameTime gameTime)
        {
            if (Game.IsActive)
            {
                if (KeyboardManager != null)
                    KeyboardManager.PreUpdate(gameTime);

                if (GamePadManager != null)
                    GamePadManager.PreUpdate(gameTime);

                if (MouseManager != null)
                    MouseManager.PreUpdate(gameTime);
            }
        }
    }
}
