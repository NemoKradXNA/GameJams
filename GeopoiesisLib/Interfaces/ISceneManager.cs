using Geopoiesis.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Geopoiesis.Interfaces
{
    public interface ISceneManager
    {
        IScene CurrentScene { get; }
        Dictionary<string, IScene> Scenes { get; set; }
        void AddScene(IScene scene);
        void LoadScene(string name);

        SceneStateEnum CurrentSceneState { get; }
    }
}
