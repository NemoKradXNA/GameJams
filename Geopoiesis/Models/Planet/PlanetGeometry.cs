using Geopoiesis.Interfaces;
using Geopoiesis.Managers.Coroutines;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Geopoiesis.Models.Planet
{
    public class PlanetGeometry : GeometryBase
    {
        ICoroutineService coroutineService { get { return Game.Services.GetService<ICoroutineService>(); } }
        public TextureCube CubeHeightMap { get; set; }
        public TextureCube CubeSplatMap { get; set; }
        public List<Texture2D> FaceTextures { get; set; }

        public List<Texture2D> NoiseMaps { get; set; }

        public List<string> Debug = new List<string>();

        List<Vector3> FaceNormals = new List<Vector3>() { Vector3.Backward, Vector3.Up, Vector3.Left, Vector3.Right, Vector3.Forward, Vector3.Down };

        protected List<IPlanetFace> Faces = new List<IPlanetFace>() { null, null, null, null, null, null };

        public int FaceDimensions = 32;
        public float Radius = 2;
        public float NoiseMod = 4.8f;
        public int CubeSize = 16;
        public float DisplaceMesh = 0f;

        float _lastDisplaceMesh;

        public PlanetGeometry(Game game, string effectAsset) : base(game, effectAsset)
        {
            coroutineService.StartCoroutine(GenerateFaces());
        }
        protected void WriteToDebug(string msg) // Should really be a "console" logging service...
        {
            Debug.Add(string.Format("[{0:dd-MMM-yyyy HH:mm:ss}] - {1}", DateTime.Now, msg));
        }
        public virtual IEnumerator GenerateFaces()
        {
            WriteToDebug("GenerateFaces...");

            CubeHeightMap = new TextureCube(Game.GraphicsDevice, CubeSize, false, SurfaceFormat.Color);
            CubeSplatMap = new TextureCube(Game.GraphicsDevice, CubeSize, false, SurfaceFormat.Color);

            if (FaceTextures == null)
                FaceTextures = new List<Texture2D>();

            FaceTextures.Clear();

            meshData = new MeshData();
            WriteToDebug("Initializing Cubemaps...");

            yield return new WaitForEndOfFrame(Game);

            if (NoiseMaps == null || NoiseMaps.Count == 0)
                NoiseMaps = new List<Texture2D>() { null, null, null, null, null, null };

            WriteToDebug("Initializing NoiseMaps...");

            for (int idx = 0; idx < 6; idx++)
            {
                Vector3 n = FaceNormals[idx];
                Texture2D FaceMap = null;

                WriteToDebug($"Generating Face [{n}]");

                if (Faces[idx] == null)
                    Faces[idx] = new PlanetFace(Game, Transform.Position, n, FaceDimensions, Radius, NoiseMod, CubeSize, FaceMap, DisplaceMesh);

                WriteToDebug($"Building Face [{n}] mesh data...");
                yield return coroutineService.StartCoroutine(Faces[idx].BuildMesh(Debug));

                WriteToDebug("Combining mesh data and maps");
                meshData.Combine(Faces[idx].meshData);

                FaceTextures.Add(Faces[idx].faceHeightMap);
                NoiseMaps[idx] = Faces[idx].faceHeightMap;


                WriteToDebug("Generating cubemaps");
                Color[] p = new Color[CubeSize* CubeSize];
                Faces[idx].faceHeightMap.GetData(p);

                Color[] np = new Color[CubeSize* CubeSize];
                Faces[idx].faceNormalMap.GetData(np);

                if (n == Vector3.Left)
                {
                    CubeHeightMap.SetData(CubeMapFace.NegativeX, p);
                    CubeSplatMap.SetData(CubeMapFace.NegativeX, np);
                }
                if (n == Vector3.Right)
                {
                    CubeHeightMap.SetData(CubeMapFace.PositiveX, p);
                    CubeSplatMap.SetData(CubeMapFace.PositiveX, np);
                }

                if (n == Vector3.Up)
                {
                    CubeHeightMap.SetData(CubeMapFace.PositiveY, p);
                    CubeSplatMap.SetData(CubeMapFace.PositiveY, np);
                }
                if (n == Vector3.Down)
                {
                    CubeHeightMap.SetData(CubeMapFace.NegativeY, p);
                    CubeSplatMap.SetData(CubeMapFace.NegativeY, np);
                }

                if (n == Vector3.Forward)
                {
                    CubeHeightMap.SetData(CubeMapFace.PositiveZ, p);
                    CubeSplatMap.SetData(CubeMapFace.PositiveZ, np);
                }
                if (n == Vector3.Backward)
                {
                    CubeHeightMap.SetData(CubeMapFace.NegativeZ, p);
                    CubeSplatMap.SetData(CubeMapFace.NegativeZ, np);
                }
            }

            WriteToDebug($"Faces Generated.");

            CalculateTangents();
            SetVertexBuffer();
        }

        public override void Draw(GameTime gameTime)
        {
            if (effect.Parameters["heightTexture"] != null)
                effect.Parameters["heightTexture"].SetValue(CubeHeightMap);

            base.Draw(gameTime);
        }
    }
}
