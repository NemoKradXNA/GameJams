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
    // http://spiralgraphics.biz/packs/planet/index.htm
    public class PlanetGeometry : GeometryBase
    {
        ICoroutineService coroutineService { get { return Game.Services.GetService<ICoroutineService>(); } }
        public TextureCube CubeHeightMap { get; set; }
        public TextureCube CubeNormalMap { get; set; }
        public TextureCube CubeSplatMap { get; set; }
        public List<Texture2D> FaceTextures { get; set; }

        public List<Texture2D> NoiseMaps { get; set; }

        public List<string> Debug = new List<string>();

        List<Vector3> FaceNormals = new List<Vector3>() { Vector3.Backward, Vector3.Up, Vector3.Left, Vector3.Right, Vector3.Forward, Vector3.Down };

        protected List<IPlanetFace> Faces = new List<IPlanetFace>() { null, null, null, null, null, null };

        //public int FaceDimensions = 32;
        //public float Radius = 2;
        //public float NoiseMod = 4.8f;
        //public int CubeSize = 16;
        //public float DisplaceMesh = 0f;

        public int FaceDimensions = 2;
        public float Radius = 2;
        public float NoiseMod = 1.25f;
        public int CubeSize = 32;
        public float DisplaceMesh = 0f;
        public int LodLevel = 8;
        public int MaxLodLevel = 8;

        public bool Generated = false;

        public int Seed = 1971;

        List<MeshData> lodMeshData = new List<MeshData>();
        public List<int> LodSizes = new List<int>();

        float _lastDisplaceMesh;

        public PlanetGeometry(Game game, string effectAsset) : base(game, effectAsset)
        {
            coroutineService.StartCoroutine(GenerateFaces());
        }
        protected void WriteToDebug(string msg) // Should really be a "console" logging service...
        {
            Debug.Add(string.Format("[{0:dd-MMM-yyyy HH:mm:ss}] - {1}", DateTime.Now, msg));
        }

        protected IEnumerator GenerateLodFaces()
        {
            Generated = false;
            for (int l = MaxLodLevel; l >= 0; l--)
            {
                WriteToDebug($"--- LOD Level {MaxLodLevel-l} ---");
                MeshData thisMeshData = new MeshData();

                for (int idx = 0; idx < 6; idx++)
                {
                    Vector3 n = FaceNormals[idx];
                    Texture2D FaceMap = null;

                    //WriteToDebug($"Generating Face [{n}]");

                    //if (Faces[idx] == null)
                        Faces[idx] = new PlanetFace(Game, Transform.Position, n, FaceDimensions, Radius, NoiseMod, CubeSize, FaceMap, DisplaceMesh);

                    Faces[idx].Seed = Seed;
                    //WriteToDebug($"Building Face [{n}] mesh data...");
                    yield return coroutineService.StartCoroutine(Faces[idx].BuildMesh(Debug));

                    //WriteToDebug("Combining mesh data and maps");
                    thisMeshData.Combine(Faces[idx].meshData);


                    if (l == MaxLodLevel) 
                    {
                        FaceTextures.Add(Faces[idx].faceHeightMap);
                        NoiseMaps[idx] = Faces[idx].faceHeightMap;

                        //WriteToDebug("Generating cubemaps");
                        Color[] p = new Color[CubeSize * CubeSize];
                        Faces[idx].faceHeightMap.GetData(p);

                        Color[] np = new Color[CubeSize * CubeSize];
                        Faces[idx].faceNormalMap.GetData(np);

                        Color[] sp = new Color[CubeSize * CubeSize];
                        Faces[idx].faceSplatMap.GetData(sp);

                        if (n == Vector3.Left)
                        {
                            CubeHeightMap.SetData(CubeMapFace.NegativeX, p);
                            CubeNormalMap.SetData(CubeMapFace.NegativeX, np);
                            CubeSplatMap.SetData(CubeMapFace.NegativeX, sp);
                        }

                        if (n == Vector3.Right)
                        {
                            CubeHeightMap.SetData(CubeMapFace.PositiveX, p);
                            CubeNormalMap.SetData(CubeMapFace.PositiveX, np);
                            CubeSplatMap.SetData(CubeMapFace.PositiveX, sp);
                        }

                        if (n == Vector3.Up)
                        {
                            CubeHeightMap.SetData(CubeMapFace.PositiveY, p);
                            CubeNormalMap.SetData(CubeMapFace.PositiveY, np);
                            CubeSplatMap.SetData(CubeMapFace.PositiveY, sp);
                        }
                        if (n == Vector3.Down)
                        {
                            CubeHeightMap.SetData(CubeMapFace.NegativeY, p);
                            CubeNormalMap.SetData(CubeMapFace.NegativeY, np);
                            CubeSplatMap.SetData(CubeMapFace.NegativeY, sp);
                        }

                        if (n == Vector3.Forward)
                        {
                            CubeHeightMap.SetData(CubeMapFace.PositiveZ, p);
                            CubeNormalMap.SetData(CubeMapFace.PositiveZ, np);
                            CubeSplatMap.SetData(CubeMapFace.PositiveZ, sp);
                        }

                        if (n == Vector3.Backward)
                        {
                            CubeHeightMap.SetData(CubeMapFace.NegativeZ, p);
                            CubeNormalMap.SetData(CubeMapFace.NegativeZ, np);
                            CubeSplatMap.SetData(CubeMapFace.NegativeZ, sp);
                        }
                    }
                }
                
                lodMeshData.Insert(0, thisMeshData);
                LodSizes.Insert(0,FaceDimensions);
                FaceDimensions *= 2;
            }
            Generated = true;
        }

        public void SetLODLevel(int lvl)
        {
            //LodLevel = Math.Min(MaxLodLevel - 1, lvl);
            LodLevel = Math.Max(0, Math.Min(MaxLodLevel , lvl));
            meshData = lodMeshData[LodLevel];
            SetVertexBuffer();
        }

        public virtual IEnumerator GenerateFaces()
        {
            WriteToDebug("GenerateFaces...");

            CubeHeightMap = new TextureCube(Game.GraphicsDevice, CubeSize, false, SurfaceFormat.Color);
            CubeNormalMap = new TextureCube(Game.GraphicsDevice, CubeSize, false, SurfaceFormat.Color);
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
            yield return coroutineService.StartCoroutine(GenerateLodFaces());
            

            WriteToDebug($"Faces Generated.");

            meshData = lodMeshData[LodLevel];

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
