using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Geopoiesis.Models
{
    public class SkyBox : GeometryBase
    {
        public SkyBox(Game game, string effectAsset) : base(game, effectAsset)
        {
            Transform.Scale *= 10000;

            meshData = new MeshData();

            meshData.Vertices = new List<Vector3>()
            {
                new Vector3(-.5f, .5f, .5f), new Vector3(.5f, .5f, .5f), new Vector3(.5f, -.5f, .5f),new Vector3(-.5f, -.5f, .5f),
                new Vector3(-.5f, .5f, -.5f), new Vector3(.5f, .5f, -.5f), new Vector3(.5f, -.5f, -.5f), new Vector3(-.5f, -.5f, -.5f),
                new Vector3(-.5f, .5f, .5f), new Vector3(.5f, .5f, .5f), new Vector3(.5f, .5f, -.5f),new Vector3(-.5f, .5f, -.5f),
                new Vector3(-.5f, -.5f, .5f),new Vector3(.5f, -.5f, .5f),new Vector3(.5f, -.5f, -.5f),new Vector3(-.5f, -.5f, -.5f),
                new Vector3(-.5f, .5f, -.5f),new Vector3(-.5f, .5f, .5f),new Vector3(-.5f, -.5f, .5f),new Vector3(-.5f, -.5f, -.5f),
                new Vector3(.5f, .5f, -.5f),new Vector3(.5f, .5f, .5f),new Vector3(.5f, -.5f, .5f),new Vector3(.5f, -.5f, -.5f)
            };

            meshData.Normals = new List<Vector3>()
            {
                Vector3.Backward,Vector3.Backward,Vector3.Backward,Vector3.Backward,
                Vector3.Forward,Vector3.Forward,Vector3.Forward,Vector3.Forward,
                Vector3.Up,Vector3.Up,Vector3.Up,Vector3.Up,
                Vector3.Down,Vector3.Down,Vector3.Down,Vector3.Down,
                Vector3.Left,Vector3.Left,Vector3.Left,Vector3.Left,
                Vector3.Right,Vector3.Right,Vector3.Right,Vector3.Right,
            };

            meshData.Tangents = new List<Vector3>(meshData.Normals);

            meshData.TextCoords = new List<Vector2>()
            {
                new Vector2(0, 0),new Vector2(1, 0),new Vector2(1, 1),new Vector2(0, 1),
                new Vector2(0, 0),new Vector2(1, 0),new Vector2(1, 1),new Vector2(0, 1),
                new Vector2(0, 0),new Vector2(1, 0),new Vector2(1, 1),new Vector2(0, 1),
                new Vector2(0, 0),new Vector2(1, 0),new Vector2(1, 1),new Vector2(0, 1),
                new Vector2(0, 0),new Vector2(1, 0),new Vector2(1, 1),new Vector2(0, 1),
                new Vector2(0, 0),new Vector2(1, 0),new Vector2(1, 1),new Vector2(0, 1),
            };

            meshData.Colors = new List<Color>();
            for (int v = 0; v < meshData.Vertices.Count; v++)
                meshData.Colors.Add(new Color(meshData.Normals[v]));

            meshData.Indicies = new List<int>()
            {
                0, 1, 2, 2, 3, 0, // Front
                4, 7, 6, 6, 5, 4, // Back
                8, 11, 10, 10, 9, 8, // Top
                12, 13, 14, 14, 15, 12, // Bottom
                16, 17, 18, 18, 19, 16, // Left
                20, 23, 22, 22, 21, 20, // Right
            };

            // It's the same as a cue, but we want to flip the draw order as we only want to render the inside of it :)
            meshData.Indicies.Reverse();

            SetVertexBuffer();
        }

        public override void Update(GameTime gameTime)
        {
            Transform.Position = Camera.Transform.Position;
            base.Update(gameTime);
        }

        public override void SetEffectParamters()
        {
            effect.Parameters["textureMap"].SetValue(Game.Content.Load<TextureCube>("Textures/Cubemap/space"));
            effect.Parameters["EyePosition"].SetValue(Camera.Transform.Position);
            base.SetEffectParamters();
        }
    }
}
