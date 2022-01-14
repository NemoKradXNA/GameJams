using Geopoiesis.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Geopoiesis.Models
{
    public class Atmosphere : DrawableGameComponent
    {
        protected ICameraService Camera { get { return Game.Services.GetService<ICameraService>(); } }

        public Color SkyColor { get; set; }
        public Color GroundColor { get; set; }
        public float SunSize { get; set; }
        public float Exposure { get; set; }
        public float AtmosphereThickness { get; set; }

        public Color ScatteringWavelength { get; set; }
        public Color RangeForScatteringWavelength { get; set; }
        public float kMIE { get; set; }
        public float Outer_radius { get; set; }
        public float Inner_radius { get; set; }
        public float lightBrightnes_Offset { get; set; }

        public ITransform Transform { get; set; }

        string _effectAsset;
        protected Effect effect;

        public Vector3 LightDirection { get; set; }

        protected Model mesh;
        protected Matrix[] transforms;
        protected Matrix meshWorld;
        protected Matrix meshWVP;

        public Atmosphere(Game game, string effectAsset) : base(game)
        {
            SkyColor = new Color(84, 107, 148, 255);
            GroundColor = new Color(105, 98, 93, 255);
            SunSize = .05f;
            Exposure = 1;
            AtmosphereThickness = 1;

            ScatteringWavelength = new Color(.65f, .57f, .475f);
            RangeForScatteringWavelength = new Color(.15f, .15f, .15f);

            kMIE = 0.0010f;
            Outer_radius = 1.025f;
            Inner_radius = 1f;
            lightBrightnes_Offset = 20;

            _effectAsset = effectAsset;

            Transform = new Transform();
        }
        protected override void LoadContent()
        {
            base.LoadContent();

            effect = Game.Content.Load<Effect>(_effectAsset);
            mesh = Game.Content.Load<Model>("Models/SkySphere");

            transforms = new Matrix[mesh.Bones.Count];
            mesh.CopyAbsoluteBoneTransformsTo(transforms);
        }

        public void SetEffectParamters()
        {
            effect.Parameters["world"].SetValue(meshWorld);
            effect.Parameters["wvp"].SetValue(meshWorld * Camera.View * Camera.Projection);

            effect.Parameters["EyePosition"].SetValue(Camera.Transform.Position);
            effect.Parameters["lightDirection"].SetValue(LightDirection);
            effect.Parameters["lightColor"].SetValue(Color.Azure.ToVector3());
            effect.Parameters["lightBrightnes"].SetValue(lightBrightnes_Offset);


            effect.Parameters["ScatteringWavelength"].SetValue(ScatteringWavelength.ToVector3());
            effect.Parameters["RangeForScatteringWavelength"].SetValue(RangeForScatteringWavelength.ToVector3());
            effect.Parameters["kMIE"].SetValue(kMIE);
            effect.Parameters["OUTER_RADIUS"].SetValue(Outer_radius);
            effect.Parameters["INNER_RADUIS"].SetValue(Inner_radius);

            effect.Parameters["_GroundColor"].SetValue(GroundColor.ToVector3());
            effect.Parameters["_SunSize"].SetValue(SunSize);
            effect.Parameters["_Exposure"].SetValue(Exposure);
            effect.Parameters["_SkyTint"].SetValue(SkyColor.ToVector4());
            effect.Parameters["_AtmosphereThickness"].SetValue(AtmosphereThickness);
        }

        public override void Draw(GameTime gameTime)
        {
            Game.GraphicsDevice.BlendState = BlendState.Additive;
            Game.GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
            Game.GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            for (int m = 0; m < mesh.Meshes.Count; m++)
            {
                if (mesh.Meshes[m].ParentBone != null)
                    meshWorld = transforms[mesh.Meshes[m].ParentBone.Index] * Transform.World;
                else
                    meshWorld = transforms[0] * Transform.World;

                for (int mp = 0; mp < mesh.Meshes[m].MeshParts.Count; mp++)
                {
                    // See if we can get a named material
                    SetEffectParamters();

                    int pCnt = effect.CurrentTechnique.Passes.Count;

                    for (int p = 0; p < pCnt; p++)
                    {
                        effect.CurrentTechnique.Passes[p].Apply();

                        Game.GraphicsDevice.SetVertexBuffer(mesh.Meshes[m].MeshParts[mp].VertexBuffer);
                        Game.GraphicsDevice.Indices = mesh.Meshes[m].MeshParts[mp].IndexBuffer;
                        Game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, mesh.Meshes[m].MeshParts[mp].VertexOffset,
                            0, mesh.Meshes[m].MeshParts[mp].PrimitiveCount);
                    }
                }
            }
        }
    }
}
