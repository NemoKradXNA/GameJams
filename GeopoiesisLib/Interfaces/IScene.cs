using Geopoiesis.Enums;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Geopoiesis.Interfaces
{
    public interface IScene : IGameComponent
    {
        ISceneManager sceneManager { get; }
        string Name { get; set; }
        IScene LastScene { get; set; }

        SceneStateEnum State { get; set; }

        List<IGameComponent> Components { get; set; }

        void LoadScene();
        void UnloadScene();
    }
}
