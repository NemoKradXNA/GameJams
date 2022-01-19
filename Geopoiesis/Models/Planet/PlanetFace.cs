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
    public class PlanetFace : IPlanetFace
    {
        protected INoiseService noiseService { get { return _game.Services.GetService<INoiseService>(); } }
        public Vector3 RootPosition { get; set; }
        public Vector3 Normal { get; set; }
        public int Dimension { get; set; }
        public float Radius { get; set; }
        public float NoiseMod { get; set; }
        public int CubeSize { get; set; }

        public Texture2D faceHeightMap { get; set; }
        public Texture2D faceNormalMap { get; set; }
        public Texture2D faceSplatMap { get; set; }
        public MeshData meshData { get; set; }

        protected Game _game;

        protected Random rnd;

        public int Seed { get; set; }

        public PlanetFace(Game game) 
        {
            Seed = 1;
            Normal = Vector3.Backward; 
            Dimension = 20;

            _game = game;
            rnd = new Random();
        }

        public Dictionary<Vector3, Color> FaceColor = new Dictionary<Vector3, Color>()
    {
        { Vector3.Backward, new Color(.2f,.3f,.5f,1) },
        { Vector3.Up, new Color(.3f,.5f,.2f,1) },
        { Vector3.Left, new Color(1,.6f,.4f,1) },
        { Vector3.Right, new Color(.5f,.3f,.2f,1) },
        { Vector3.Forward, new Color(.4f,.6f,1,1) },
        { Vector3.Down, new Color(.6f,1,.4f,1) }
    };



        public PlanetFace(Game game, Vector3 rootPosition, Vector3 normal, int dim, float radius, float noiseMod, int cubeSize, Texture2D faceMap = null, int seed= 1971) : this(game)
        {
            faceHeightMap = faceMap;
            CubeSize = cubeSize;
            NoiseMod = noiseMod;
            RootPosition = rootPosition;
            Normal = normal;
            Dimension = dim;
            Radius = radius;
            Seed = seed;
        }



        float Get3DPerlinValue(Vector3 cubeV)
        {
            return noiseService.Noise(cubeV)
                            + (.5f * noiseService.Noise(cubeV * 2))
                            + (.25f * noiseService.Noise(cubeV * 4))
                            + (.125f * noiseService.Noise(cubeV * 8));
        }

        public static Quaternion RotateToFace(Vector3 face)
        {
            if (face == Vector3.Backward)
                return Quaternion.CreateFromAxisAngle(Vector3.Left, MathHelper.PiOver2);
            if (face == Vector3.Forward)
                return Quaternion.CreateFromAxisAngle(Vector3.Right, MathHelper.PiOver2);
            if (face == Vector3.Left)
                return Quaternion.CreateFromAxisAngle(Vector3.Backward, MathHelper.PiOver2);
            if (face == Vector3.Right)
                return Quaternion.CreateFromAxisAngle(Vector3.Forward, MathHelper.PiOver2);
            if (face == Vector3.Down)
                return Quaternion.CreateFromAxisAngle(Vector3.Left, MathHelper.Pi);

            return Quaternion.Identity;
        }

        public IEnumerator BuildMesh()
        {
            Quaternion rotation = RotateToFace(Normal);

            Vector3 cubeNormal = new Vector3(Normal.X * -1, Normal.Y * -1, Normal.Z);

            Quaternion cubeRot = RotateToFace(cubeNormal);

            meshData = new MeshData();

            float h = Dimension * .5f;
            float ch = 0;
            Color[] col;
            Color[] nc;
            Color[] sc;

            float vh;


            //if (faceHeightMap == null)
            {
                //WriteToDebug("Building face mesh data...");
                faceHeightMap = new Texture2D(_game.GraphicsDevice, CubeSize, CubeSize, false, SurfaceFormat.Color);
                faceNormalMap = new Texture2D(_game.GraphicsDevice, CubeSize, CubeSize, false, SurfaceFormat.Color);
                faceSplatMap = new Texture2D(_game.GraphicsDevice, CubeSize, CubeSize, false, SurfaceFormat.Color);

                ch = faceHeightMap.Width * .5f;
                col = new Color[faceHeightMap.Width * faceHeightMap.Height];
                vh = h - 1;

                nc = new Color[faceNormalMap.Width * faceNormalMap.Height];
                sc = new Color[faceSplatMap.Width * faceSplatMap.Height];

                float randomOffset = rnd.Next();

                Quaternion q = Quaternion.Identity;
                Vector3 v3 = Vector3.One;

                v3 = Vector3.Transform(v3, q);

                for (int x = faceHeightMap.Width / -2; x < faceHeightMap.Width / 2; x++)
                {
                    for (int y = faceHeightMap.Height / -2; y < faceHeightMap.Height / 2; y++)
                    {
                        Vector3 v = new Vector3(x, ch - 1, y) + (Vector3.One * .5f);
                        v.Normalize();
                        Vector3 cubeV = Vector3.Transform(v, cubeRot) + RootPosition;


                        int px = (int)MathHelper.Lerp(0, faceHeightMap.Width, (x + ch) / faceHeightMap.Width);
                        int py = (int)MathHelper.Lerp(0, faceHeightMap.Height, (y + ch) / faceHeightMap.Height);

                        if (Normal.Z == -1 || Normal.Y != 0)
                        {
                            py = (faceHeightMap.Height - 1) - py;
                            px = (faceHeightMap.Width - 1) - px;
                        }

                        if (Normal.X != 0)
                        {
                            int t = py;
                            py = px;
                            px = t;

                            if (Normal.X == 1)
                                px = (faceHeightMap.Width - 1) - px;
                            else
                                py = (faceHeightMap.Height - 1) - py;
                        }

                        Vector3 nV = cubeV + (Vector3.One * Seed);
                        float p = Get3DPerlinValue(nV * NoiseMod);

                        // Move p from -1 - 1 to 0 - 1
                        p = (p + 1) * .5f;

                        col[px + py * faceHeightMap.Width] = new Color(p, p, p, 1);
                    }
                }

                faceHeightMap.SetData(col);
                faceNormalMap.SetData(nc);
                faceSplatMap.SetData(sc);
            }

            vh = h - 1;

            //WriteToDebug("Building mesh data map...");

            for (float x = -h; x < h; x++)
            {
                for (float y = -h; y < h; y++)
                {
                    Vector3 v = new Vector3(x, 0, y) + (Vector3.One * .5f);

                    //int px = (int)MathHelper.Lerp(faceHeightMap.Width - 1, 0, (x + h) / Dimension);
                    //int py = (int)MathHelper.Lerp(faceHeightMap.Height - 1, 0, (y + h) / Dimension);


                    //if (Normal.Z == 1 || Normal.Y == -1)
                    //{
                    //    py = (faceHeightMap.Height - 1) - py;
                    //    px = (faceHeightMap.Width - 1) - px;
                    //}

                    //if (Normal.X != 0 || Normal.Y == 1)
                    //{
                    //    int t = py;
                    //    py = px;
                    //    px = t;

                    //    if (Normal.X == 1)
                    //        py = (faceHeightMap.Height - 1) - py;
                    //    else
                    //        px = (faceHeightMap.Width - 1) - px;

                    //}

                    v += Vector3.Up * vh;

                    v = Vector3.Transform(v, rotation);
                    v.Normalize();
                    meshData.Normals.Add(v);

                    
                    v = v * (Radius);

                    //v += RootPosition;
                    meshData.Vertices.Add(v);

                    meshData.Colors.Add(FaceColor[Normal]);

                    Vector2 uv = new Vector2(x + h, y + h) / Dimension;

                    meshData.TextCoords.Add(uv);


                }
            }

            //faceNormalMap.SetData(nc);

            //WriteToDebug("Building index map...");
            // Build triangles. 
            for (int x = 0; x < Dimension - 1; x++)
            {
                for (int y = 0; y < Dimension - 1; y++)
                {
                    meshData.Indicies.Add(((x + 1) + (y + 1) * Dimension));
                    meshData.Indicies.Add(((x + 1) + y * Dimension));
                    meshData.Indicies.Add((x + y * Dimension));

                    meshData.Indicies.Add(((x + 1) + (y + 1) * Dimension));
                    meshData.Indicies.Add((x + y * Dimension));
                    meshData.Indicies.Add((x + (y + 1) * Dimension));
                }
            }
            

            yield return new WaitForEndOfFrame(_game);

            //WriteToDebug("Face done.");
        }
    }
}
