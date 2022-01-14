using Geopoiesis.Enums;
using Geopoiesis.Interfaces;
using Geopoiesis.Services;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Geopoiesis.Scenes
{
    public abstract class SceneBase : DrawableGameComponent, IScene
    {
        protected ICoroutineService coroutineService { get { return Game.Services.GetService<ICoroutineService>(); } }
        protected ICameraService camera { get { return Game.Services.GetService<ICameraService>(); } }
        protected IKeyboardStateManager kbManager { get { return Game.Services.GetService<IInputStateHandler>().KeyboardManager; } }
        protected GeopoiesisService geopoiesisService { get { return Game.Services.GetService<GeopoiesisService>(); } }

        public string Name { get; set; }
        public IScene LastScene { get; set; }
        public SceneStateEnum State { get; set; }

        public List<IGameComponent> Components { get; set; }

        public SceneBase(Game game, string name) : base(game)
        { 
            Name = name; 
            Components = new List<IGameComponent>(); 
        }

        public override void Initialize()
        {
            base.Initialize();

            foreach (IGameComponent component in Components)
                component.Initialize();

        }

        protected override void UnloadContent()
        {
            base.UnloadContent();

            Components.Clear();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            foreach (IGameComponent component in Components)
            {
                if (component is IUpdateable)
                    ((IUpdateable)component).Update(gameTime);
            }
        }

        public override void Draw(GameTime gameTime)
        {
            foreach (IGameComponent component in Components)
            {
                if (component is IDrawable)
                    ((IDrawable)component).Draw(gameTime);
            }

            base.Draw(gameTime);
        }

        public virtual void LoadScene()
        {
            // Load our shit up!
            Game.Components.Add(this);
        }

        public virtual void UnloadScene()
        {
            // Unload our shit!
            UnloadContent();
            Game.Components.Remove(this);
        }
    }
}
