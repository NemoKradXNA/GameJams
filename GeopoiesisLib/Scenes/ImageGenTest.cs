using Geopoiesis.Interfaces;
using Geopoiesis.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Geopoiesis.Scenes
{
    public class ImageGenTest : SceneBase
    {
        SpriteBatch _spriteBatch;

        Texture2D testTexture;
        Texture3D test3D;

        Texture2D TopProfile3D;
        Texture2D FrontProfile3D;
        Texture2D SideProfile3D;

        INoiseService noiseService { get { return Game.Services.GetService<INoiseService>(); } }

        public ImageGenTest(Game game, string name) : base(game, name) { }


        public override void Initialize()
        {
            _spriteBatch = new SpriteBatch(Game.GraphicsDevice);

            int s = 128;
            testTexture = new Texture2D(Game.GraphicsDevice, s, s);
            test3D = new Texture3D(Game.GraphicsDevice, s, s, s, false, SurfaceFormat.Color);

            TopProfile3D = new Texture2D(Game.GraphicsDevice, s, s);
            FrontProfile3D = new Texture2D(Game.GraphicsDevice, s, s);
            SideProfile3D = new Texture2D(Game.GraphicsDevice, s, s);

            Color[] color = new Color[s * s];

            Color[] color3D = new Color[s * s * s];

            Color[] tColor = new Color[s * s];
            Color[] fColor = new Color[s * s];
            Color[] sColor = new Color[s * s];

            float mod = 8;

            Vector2 c = new Vector2(s, s) / 2;
            Vector3 c3d = new Vector3(s, s, s) / 2;

            // Distance D
            // Angle A arctan(x,y);
            // Core C 1 - Dist /200;
            // Arm e - D/1500 * .5  * sin((.5 * D))^.35 - A)^2 + .5 - D/1000

            //https://lup.lub.lu.se/luur/download?func=downloadFile&recordOId=8867455&fileOId=8870454

            for (int z = 0; z < s; z++)
            {
                for (int x = 0; x < s; x++)
                {
                    for (int y = 0; y < s; y++)
                    {
                        float v = Get3DPerlinValue(new Vector3((float)x / s, (float)y / s, (float)z / s) * mod);
                        v = (v + 1) * .5f;

                        Vector3 p = new Vector3(x, y, z);
                        float d = Vector3.Distance(c3d, p);
                        float a = (MathF.Atan2((p.Z - c3d.Z), (p.X - c3d.X)));
                        float core = 1f - (d / (s / 2f));

                        float dx = s / 66;
                        float e1 = (float)Math.E - (d / dx);
                        float ee1 = MathF.Pow(.5f * d, .35f);
                        float sin = MathF.Sin(ee1 - a);
                        float e2 = MathF.Pow(sin, 2);
                        float e3 = (d / (s / 10));
                        float arm = e1 * .5f * e2 + .5f - e3;

                        float density = Math.Max(0, Math.Max(core, arm));

                        v *= density;// * (1 - MathF.Pow((c3d.Z - p.Z) / 1,4));

                        Vector4 col = new Vector4(v, v, v, 1);
                        color3D[x + (y * s) + (z * s)] = new Color(col);

                        tColor[x + (z * s)] = new Color(tColor[x + (z * s)].ToVector4() + col);// * Color.Red.ToVector4());
                        fColor[x + (y * s)] = new Color(fColor[x + (y * s)].ToVector4() + col);// * Color.Gold.ToVector4());
                        sColor[y + (z * s)] = new Color(sColor[y + (z * s)].ToVector4() + col);// * Color.Blue.ToVector4());
                    }
                }
            }

            test3D.SetData(color3D);


            FrontProfile3D.SetData(fColor);
            TopProfile3D.SetData(tColor);
            SideProfile3D.SetData(sColor);

            // Distance D
            // Angle A arctan(x,y);
            // Core C 1 - Dist /200;
            // Arm e - D/1500 * .5  * sin((.5 * D))^.35 - A)^2 + .5 - D/1000

            for (int x = 0; x < s; x++)
            {
                for (int y = 0; y < s; y++)
                {
                    float v = Get3DPerlinValue(new Vector3((float)x / s, (float)y / s, 0) * mod);
                    v  = (v + 1) * .5f;
                    Vector2 p = new Vector2(x, y);

                    float d = Vector2.Distance(c, p);
                    float a = (MathF.Atan2((p.Y - c.Y),(p.X - c.X)));
                    float core = 1f - (d / (s/2f));

                    float dx = s / 66;
                    float e1 = (float)Math.E - (d / dx);
                    float ee1 = MathF.Pow(.5f * d, .35f);
                    float sin = MathF.Sin(ee1 - a);
                    float e2 = MathF.Pow(sin, 2);
                    float e3 = (d / (s/10));
                    float arm = e1 * .5f * e2  + .5f - e3;

                    float density = Math.Max(0, Math.Max(core, arm));

                    v *= density; 

                    color[x + y * s] = new Color(v, v, v, 1);
                }
            }

            testTexture.SetData(color);

            base.Initialize();
        }

        protected float Get3DPerlinValue(Vector3 cubeV)
        {
            return noiseService.Noise(cubeV)
                            + (.5f * noiseService.Noise(cubeV * 2))
                            + (.25f * noiseService.Noise(cubeV * 4))
                            + (.125f * noiseService.Noise(cubeV * 8));
        }

        public override void Draw(GameTime gameTime)
        {

            
            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);
            _spriteBatch.Draw(Game.Content.Load<Texture2D>("Textures/MenuBG"), new Rectangle(0, 0, Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height), Color.White);

            _spriteBatch.Draw(testTexture, new Rectangle(8, 8, 512, 512), Color.White);

            _spriteBatch.Draw(TopProfile3D, new Rectangle(8, 512 + 6, 512, 512), Color.White);
            _spriteBatch.Draw(FrontProfile3D, new Rectangle(512 + 16, 512 + 16, 512, 512), Color.White);
            _spriteBatch.Draw(SideProfile3D, new Rectangle((512 + 16) * 2, 512 + 16, 512, 512), Color.White);

            _spriteBatch.End();


            base.Draw(gameTime);
        }
    }
}
