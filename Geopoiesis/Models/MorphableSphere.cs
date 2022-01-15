using Geopoiesis.Interfaces;
using Geopoiesis.Managers.Coroutines;
using Geopoiesis.Models.Planet;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Geopoiesis.Models
{
    public class MorphableSphere : GeometryBase
    {
        protected ICoroutineService coroutineService { get { return Game.Services.GetService<ICoroutineService>(); } }
        public TextureCube CubeHeightMap { get; set; }
        public TextureCube CubeNormalMap { get; set; }
        public TextureCube CubeSplatMap { get; set; }
        public List<Texture2D> FaceTextures { get; set; }

        public List<Texture2D> NoiseMaps { get; set; }

        protected List<Vector3> FaceNormals = new List<Vector3>() { Vector3.Backward, Vector3.Up, Vector3.Left, Vector3.Right, Vector3.Forward, Vector3.Down };

        protected List<IPlanetFace> Faces = new List<IPlanetFace>() { null, null, null, null, null, null };

        public Vector3 LightDirection { get; set; }

        public int FaceDimensions = 2;
        public float Radius = 2;
        public float NoiseMod = 1.25f;
        public int CubeSize = 32;
        public int LodLevel = 8;
        public int MaxLodLevel = 8;

        public bool Generated;

        public string GenerationString;

        protected List<MeshData> lodMeshData = new List<MeshData>();
        public List<int> LodSizes = new List<int>();

        protected int _seed;

        public MorphableSphere(Game game, string effectAsset, int faceDimensions = 2, float radius = 2, float noiseMod = 1, int cubeSize = 32, int seed = 1971, int lodLevel = 8, int maxLod = 8) : base(game, effectAsset)
        {
            _seed = seed;
            LodLevel = lodLevel;

            FaceDimensions = faceDimensions;
            Radius = radius;
            NoiseMod = noiseMod;
            CubeSize = cubeSize;
            _seed = seed;
            LodLevel = lodLevel;
            MaxLodLevel = maxLod;

            coroutineService.StartCoroutine(GenerateFaces());
        }

        protected virtual  IEnumerator GenerateLodFaces()
        {
            Generated = false;
            float prog = 0;
            for (int l = MaxLodLevel; l >= 0; l--)
            {
                MeshData thisMeshData = new MeshData();

                for (int idx = 0; idx < 6; idx++)
                {
                    yield return new WaitForEndOfFrame(Game);

                    GenerationString = $"Generating [{ (prog++/ ((MaxLodLevel+1) * 6f))* 100, 0:000} %]";

                    Vector3 n = FaceNormals[idx];
                    Texture2D FaceMap = null;

                    Faces[idx] = new PlanetFace(Game, Transform.Position, n, FaceDimensions, Radius, NoiseMod, CubeSize, FaceMap, _seed);

                    Faces[idx].Seed = _seed;

                    yield return coroutineService.StartCoroutine(Faces[idx].BuildMesh());

                    thisMeshData.Combine(Faces[idx].meshData);


                    if (l == MaxLodLevel)
                    {
                        FaceTextures.Add(Faces[idx].faceHeightMap);
                        NoiseMaps[idx] = Faces[idx].faceHeightMap;

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
                LodSizes.Insert(0, FaceDimensions);
                FaceDimensions *= 2;
            }
            Generated = true;
        }

        public void SetLODLevel(int lvl)
        {
            if (lodMeshData.Count > lvl)
            {
                //LodLevel = Math.Min(MaxLodLevel - 1, lvl);
                LodLevel = Math.Max(0, Math.Min(MaxLodLevel, lvl));
                meshData = lodMeshData[LodLevel];
                SetVertexBuffer();
            }
        }

        public virtual IEnumerator GenerateFaces()
        {
            CubeHeightMap = new TextureCube(Game.GraphicsDevice, CubeSize, false, SurfaceFormat.Color);
            CubeNormalMap = new TextureCube(Game.GraphicsDevice, CubeSize, false, SurfaceFormat.Color);
            CubeSplatMap = new TextureCube(Game.GraphicsDevice, CubeSize, false, SurfaceFormat.Color);

            if (FaceTextures == null)
                FaceTextures = new List<Texture2D>();

            FaceTextures.Clear();

            meshData = new MeshData();

            yield return new WaitForEndOfFrame(Game);

            if (NoiseMaps == null || NoiseMaps.Count == 0)
                NoiseMaps = new List<Texture2D>() { null, null, null, null, null, null };

            yield return coroutineService.StartCoroutine(GenerateLodFaces());


            meshData = lodMeshData[LodLevel];

            CalculateTangents();
            SetVertexBuffer();
        }

        public override void Draw(GameTime gameTime)
        {
            if (effect.Parameters["heightTexture"] != null)
                effect.Parameters["heightTexture"].SetValue(CubeHeightMap);

            if (effect.Parameters["splatTexture"] != null)
                effect.Parameters["splatTexture"].SetValue(CubeSplatMap);

            if (effect.Parameters["normalTexture"] != null)
                effect.Parameters["normalTexture"].SetValue(CubeNormalMap);

            if (effect.Parameters["lightDirection"] != null)
                effect.Parameters["lightDirection"].SetValue(LightDirection);


            base.Draw(gameTime);
        }
    }
}
