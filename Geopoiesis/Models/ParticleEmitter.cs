using Geopoiesis.Interfaces;
using Geopoiesis.VertexType;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Geopoiesis.Models
{
    public class ParticleEmitter : DrawableGameComponent
    {
        protected ICameraService Camera { get { return Game.Services.GetService<ICameraService>(); } }

        public ITransform Transform;

        protected Effect Effect;
        public List<ITransform> Particles = new List<ITransform>();
        public Dictionary<ITransform, VertexPositionColorNormalTextureTangent[]> vertexArray = new Dictionary<ITransform, VertexPositionColorNormalTextureTangent[]>();
        public Dictionary<ITransform, Texture2D> ParticleTextures = new Dictionary<ITransform, Texture2D>();

        int[] index = new int[] { 0, 1, 2, 2, 3, 0, };    

        public ParticleEmitter(Game game) : base(game)
        {
            Transform = new Transform();
        }

        public override void Initialize()
        {

            Effect = Game.Content.Load<Effect>("Shaders/Particles/Billboard");
            base.Initialize();
        }

        public void AddParticle(Vector3 position, Vector3 scale, Texture2D texture, Color color)
        {
            

            ITransform transform = new Transform(Transform) { Position = position, Scale = scale };
            Particles.Add(transform);
            VertexPositionColorNormalTextureTangent[] vb = new VertexPositionColorNormalTextureTangent[]{
                new VertexPositionColorNormalTextureTangent(Vector3.Zero, Vector3.Forward, Vector3.Zero, new Vector2(1,1), color),
                new VertexPositionColorNormalTextureTangent(Vector3.Zero, Vector3.Forward, Vector3.Zero, new Vector2(0, 1),color),
                new VertexPositionColorNormalTextureTangent(Vector3.Zero, Vector3.Forward, Vector3.Zero, new Vector2(0,0), color),
                new VertexPositionColorNormalTextureTangent(Vector3.Zero, Vector3.Forward, Vector3.Zero, new Vector2(1, 0),color)
            };
            
            vertexArray.Add(transform, vb);

            if (texture == null)
                texture = Game.Content.Load<Texture2D>("Textures/Particles/flare3");

            ParticleTextures.Add(transform, texture);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            int pCnt = Effect.CurrentTechnique.Passes.Count;

            Game.GraphicsDevice.BlendState = BlendState.Additive;
            Game.GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;

            for (int p = 0; p < pCnt; p++)
            {
                foreach (ITransform transform in Particles)
                {
                    Effect.Parameters["world"].SetValue(transform.World);
                    Effect.Parameters["wvp"].SetValue(transform.World * Camera.View * Camera.Projection);
                    Effect.Parameters["vp"].SetValue(Camera.View * Camera.Projection);
                    Effect.Parameters["EyePosition"].SetValue(Camera.Transform.Position);
                    if (Effect.Parameters["textureMat"] != null)
                        Effect.Parameters["textureMat"].SetValue(ParticleTextures[transform]);
                    Effect.CurrentTechnique.Passes[p].Apply();

                    Game.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertexArray[transform], 0, 4, index, 0, 2);
                }
            }

            
        }
    }
}
