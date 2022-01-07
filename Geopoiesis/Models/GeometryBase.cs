using Geopoiesis.Interfaces;
using Geopoiesis.Models.Planet;
using Geopoiesis.VertexType;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Geopoiesis.Models
{
    public class GeometryBase : DrawableGameComponent
    {
        protected MeshData meshData { get; set; }
        public ITransform Transform { get; set; }

        protected ICameraService Camera { get { return Game.Services.GetService<ICameraService>(); } }

        public Effect effect { get; set; }
        string _effectAsset;
        VertexPositionColorNormalTextureTangent[] vertexArray;

        public GeometryBase(Game game, string effectAsset) : base(game)
        {
            _effectAsset = effectAsset;
            Transform = new Transform(null);
        }

        public override void Initialize()
        {
            effect = Game.Content.Load<Effect>(_effectAsset);
            base.Initialize();
        }

        protected void SetVertexBuffer()
        {
            vertexArray = new VertexPositionColorNormalTextureTangent[meshData.Vertices.Count];

            if (meshData.Tangents.Count != meshData.Vertices.Count)
                CalculateTangents();

            for (int v = 0; v < meshData.Vertices.Count; v++)
                vertexArray[v] = new VertexPositionColorNormalTextureTangent(meshData.Vertices[v], meshData.Normals[v], meshData.Tangents[v], meshData.TextCoords[v], meshData.Colors[v]);
        }

        public void CalculateNormals()
        {
            meshData.Normals = new List<Vector3>();

            // clear out the normals
            foreach (Vector3 v in meshData.Vertices)
                meshData.Normals.Add(Vector3.Zero);

            // Calculate the new normals.            
            for (int i = 0; i < meshData.Indicies.Count; i += 3)
            {
                int idxA = meshData.Indicies[i];
                int idxB = meshData.Indicies[i + 1];
                int idxC = meshData.Indicies[i + 2];

                Vector3 A = meshData.Vertices[idxA];
                Vector3 B = meshData.Vertices[idxB];
                Vector3 C = meshData.Vertices[idxC];

                Vector3 p = Vector3.Cross(C - A, B - A);
                meshData.Normals[idxA] += p;
                meshData.Normals[idxB] += p;
                meshData.Normals[idxC] += p;
            }


            // Normalize
            foreach (Vector3 v in meshData.Normals)
                v.Normalize();
        }
        //https://gamedev.stackexchange.com/questions/68612/how-to-compute-tangent-and-bitangent-vectors
        //http://foundationsofgameenginedev.com/FGED2-sample.pdf
        public virtual void CalculateTangents()
        {
            int triangleCount = meshData.Indicies.Count;
            int vertexCount = meshData.Vertices.Count;

            Vector3[] tan1 = new Vector3[vertexCount];

            for (int i = 0; i < triangleCount; i += 3)
            {
                // Get the index for this triangle.
                int i1 = meshData.Indicies[i + 0];
                int i2 = meshData.Indicies[i + 1];
                int i3 = meshData.Indicies[i + 2];

                // Get the positions of each vertex.
                Vector3 v1 = meshData.Vertices[i1];
                Vector3 v2 = meshData.Vertices[i2];
                Vector3 v3 = meshData.Vertices[i3];

                // Get the texture coordinates of each vertex.
                Vector2 w1 = meshData.TextCoords[i1];
                Vector2 w2 = meshData.TextCoords[i2];
                Vector2 w3 = meshData.TextCoords[i3];

                // Calculate the vertex directions
                Vector3 vd1 = v2 - v1;
                Vector3 vd2 = v3 - v1;

                // Calculate the texture coordinates directions.
                Vector2 td1 = w2 - w1;
                Vector2 td2 = w3 - w1;

                // Calculate final direction.
                Vector3 dir = ((vd1 * td2.Y) - (vd2 * td1.Y));

                dir.Normalize();

                // Store ready to be returned in vertex order.
                tan1[i1] += dir;
                tan1[i2] += dir;
                tan1[i3] += dir;
            }

            meshData.Tangents = new List<Vector3>();

            // Populate tangents in vertex order. 
            for (int v = 0; v < vertexCount; v++)
                meshData.Tangents.Add(tan1[v]);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            if (vertexArray != null)
            {
                int pCnt = effect.CurrentTechnique.Passes.Count;

                for (int p = 0; p < pCnt; p++)
                {                  

                    effect.Parameters["world"].SetValue(Transform.World);
                    effect.Parameters["wvp"].SetValue(Transform.World * Camera.View * Camera.Projection);
                    effect.CurrentTechnique.Passes[p].Apply();

                    Game.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertexArray, 0, meshData.Vertices.Count, meshData.Indicies.ToArray(), 0, meshData.Indicies.Count / 3);

                }
            }
        }
    }
}
