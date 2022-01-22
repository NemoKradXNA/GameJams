using Geopoiesis.Enums;
using Geopoiesis.Interfaces;
using Geopoiesis.Managers.Coroutines;
using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Geopoiesis.Models
{
    public class BasicSceneManager : ISceneManager
    {
        ICoroutineService coroutineService { get { return Game.Services.GetService<ICoroutineService>(); } }

        public IScene CurrentScene { get; set; }
        public SceneStateEnum CurrentSceneState
        {
            get
            {
                if (CurrentScene != null)
                    return CurrentScene.State;

                return SceneStateEnum.Unknown;
            }
        }


        public Dictionary<string,IScene> Scenes { get; set; }

        protected Game Game { get; set; }

        public BasicSceneManager(Game game)
        {
            Game = game;
            Scenes = new Dictionary<string, IScene>();

            Game.Services.AddService(typeof(ISceneManager), this);
        }

        public void AddScene(IScene scene)
        {
            Scenes.Add(scene.Name, scene);
        }

        public void LoadScene(string name)
        {
            if (Scenes.ContainsKey(name))
                coroutineService.StartCoroutine(LoadScene(Scenes[name]));
        }


        protected IEnumerator LoadScene(IScene scene)
        {
            if (CurrentScene != null)
            {
                CurrentScene.UnloadScene();

                while (CurrentScene.State != SceneStateEnum.Unloaded)
                    yield return new WaitForEndOfFrame(Game);
            }

            CurrentScene = scene;
            scene.LoadScene();
        }
    }
}
