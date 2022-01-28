using Geopoiesis.Enums;
using Geopoiesis.Interfaces;
using Geopoiesis.Models;
using Geopoiesis.Models.Planet;
using Geopoiesis.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Geopoiesis.Scenes
{
    public class PlanetTestScene : SceneBase
    {
        PlanetGeometry planet;
        Atmosphere atmos;


        SpriteBatch _spriteBatch;
        SpriteFont font;

        protected float DisplacementMag = .15f;
        float _MinDeepSeaDepth = .07f;
        float _MinSeaDepth = .4f;
        float _MinShoreDepth = .42f;
        float _MinLand = .55f;
        float _MinHill = .8f;

        ParticleEmitter pet;

        public PlanetTestScene(Game game, string name) : base(game, name) { }

        public override void Initialize()
        {
            SkyBox skyBox = new SkyBox(Game, "Shaders/SkyBox");
            Components.Add(skyBox);

            planet = new PlanetGeometry(Game, "Shaders/Test", 2, 1.8f, 64,3,8);
            planet.Transform.Position = new Vector3(0, 0, 0);
            Components.Add(planet);

            atmos = new Atmosphere(Game, "Shaders/AtmosShader");
            atmos.Transform.Scale = Vector3.One;
            Components.Add(atmos);

            pet = new ParticleEmitter(Game);
            Components.Add(pet);


            base.Initialize();

            camera.Transform.Position = new Vector3(0, 0, 10);


            geopoiesisService.OZone = .5f;
            geopoiesisService.WaterLevel = .5f;
            geopoiesisService.LifeLevel = .65f;

            
        }

        Texture2D face;
        Dictionary<CubeMapFace, Vector3> FaceNormals = new Dictionary<CubeMapFace, Vector3>
        {
            { CubeMapFace.PositiveZ, Vector3.Backward },
            { CubeMapFace.PositiveY,  Vector3.Up },
            { CubeMapFace.NegativeX, Vector3.Left },
            { CubeMapFace.PositiveX, Vector3.Right },
            { CubeMapFace. NegativeZ, Vector3.Forward },
            { CubeMapFace.NegativeY, Vector3.Down },
        };
        protected void AddVolcano(Vector3 p)
        {
            //CubeMapFace cubeFace = CubeMapFace.NegativeY;
            //CubeMapFace cubeFace = CubeMapFace.PositiveY;
            CubeMapFace cubeFace = CubeMapFace.PositiveZ;
            // Find a hight point on the height cube map.
            Color[] c = new Color[planet.CubeSize * planet.CubeSize];
            //planet.CubeHeightMap.GetData(CubeMapFace.PositiveZ, c);
            //planet.CubeHeightMap.GetData(CubeMapFace.NegativeZ, c);
            //planet.CubeHeightMap.GetData(CubeMapFace.NegativeX, c);
            //planet.CubeHeightMap.GetData(CubeMapFace.PositiveX, c);
            planet.CubeHeightMap.GetData(cubeFace, c);


            byte m = c.Max(c => c.R);
            Color cp = c.FirstOrDefault(c => c.R >= m);
            int idx = c.ToList().IndexOf(cp);

            c[idx] = Color.Red;
            face = new Texture2D(Game.GraphicsDevice, planet.CubeSize, planet.CubeSize);
            face.SetData(c);

            // get x/y and spherize....
            int x = idx / (planet.CubeSize-1);
            int z = idx - (planet.CubeSize * x-1);
            int y = planet.CubeSize;

             
            //z = planet.CubeSize - z;

            Vector3 n = (2 * (new Vector3(x, y, z) / planet.CubeSize)) - Vector3.One ;
            
            n = new Vector3(x, y, z) / planet.CubeSize;
            n = (2 * n) - Vector3.One;

            n.Normalize();
            //n *= 128 * .5f;
            //n += (Vector3.One * .5f);
            //n.Normalize();
            //Vector3 pd = Vector3.Transform(n, PlanetFace.RotateToFace(Vector3.Down));
            //Vector3 pd = Vector3.Transform(n, PlanetFace.RotateToFace(Vector3.Backward));
            //Vector3 pd = Vector3.Transform(n, PlanetFace.RotateToFace(Vector3.Forward));
            //Vector3 pd = Vector3.Transform(n, PlanetFace.RotateToFace(Vector3.Left));
            //Vector3 pd = Vector3.Transform(n, PlanetFace.RotateToFace(Vector3.Right));
            //Vector3 pd = Vector3.Transform(n, PlanetFace.RotateToFace(Vector3.Up));
            Vector3 pd = Vector3.Zero;

            Vector3 Normal = FaceNormals[cubeFace];
            Vector3 cubeNormal = new Vector3(Normal.X * -1, Normal.Y * -1, Normal.Z);

            Quaternion cubeRot = PlanetFace.RotateToFace(cubeNormal);

            if (cubeFace == CubeMapFace.PositiveY)
                pd = Vector3.Transform(n, PlanetFace.RotateToFace(Vector3.Up));
            else if (cubeFace == CubeMapFace.NegativeY)
                pd = Vector3.Transform(n, PlanetFace.RotateToFace(Vector3.Down));
            else if(cubeFace == CubeMapFace.PositiveZ)
                pd = Vector3.Transform(n , cubeRot);


            //pd.Normalize();
            pd *= planet.Radius + DisplacementMag;
            
            p = pd;// planet.Transform.Position + (pd) * (planet.Radius + 1);
            //pd.Normalize();
            //p = pd;
            //p = Vector3.Transform(pd, Quaternion.Inverse( planet.Transform.Rotation));
            //p = planet.Transform.Position + (p) * (planet.Radius+1);
            pet.Transform.Position = p;
            //pet.Transform.Parent = planet.Transform;
            for (int s = 0; s < 50; s++)
            {
                pet.AddParticle(p, Vector3.One, Game.Content.Load<Texture2D>("Textures/Particles/smoke0"), Color.White, false);
            }
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            _spriteBatch = new SpriteBatch(Game.GraphicsDevice);
            font = Game.Content.Load<SpriteFont>("SpriteFont/font");
        }

        public override void Update(GameTime gameTime)
        {

            if (planet.Generated)
            {
                float translateSpeed = 2f;
                float rotateSpeed = 1f;

                translateSpeed *= (float)gameTime.ElapsedGameTime.TotalSeconds;
                rotateSpeed *= (float)gameTime.ElapsedGameTime.TotalSeconds;


                if (kbManager.KeyDown(Keys.W))
                    camera.Transform.Translate(Vector3.Forward * translateSpeed);
                if (kbManager.KeyDown(Keys.S))
                    camera.Transform.Translate(Vector3.Backward * translateSpeed);
                if (kbManager.KeyDown(Keys.A))
                    camera.Transform.Translate(Vector3.Left * translateSpeed);
                if (kbManager.KeyDown(Keys.D))
                    camera.Transform.Translate(Vector3.Right * translateSpeed);

                if (kbManager.KeyDown(Keys.Down))
                    camera.Transform.Rotate(Vector3.Left, rotateSpeed);
                if (kbManager.KeyDown(Keys.Up))
                    camera.Transform.Rotate(Vector3.Right, rotateSpeed);
                if (kbManager.KeyDown(Keys.Right))
                    camera.Transform.Rotate(Vector3.Down, rotateSpeed);
                if (kbManager.KeyDown(Keys.Left))
                    camera.Transform.Rotate(Vector3.Up, rotateSpeed);

                if (kbManager.KeyPress(Keys.F1))
                    camera.RenderWireFrame = !camera.RenderWireFrame;

                float dmod = .01f;
                if (kbManager.KeyDown(Keys.Q))
                    DisplacementMag = MathHelper.Min(1, DisplacementMag + dmod);
                if (kbManager.KeyDown(Keys.E))
                    DisplacementMag = MathHelper.Max(0, DisplacementMag - dmod);

                if (kbManager.KeyDown(Keys.R))
                    geopoiesisService.WaterLevel = MathHelper.Min(1, geopoiesisService.WaterLevel + dmod);
                if (kbManager.KeyDown(Keys.T))
                    geopoiesisService.WaterLevel = MathHelper.Max(0, geopoiesisService.WaterLevel - dmod);

                if (kbManager.KeyDown(Keys.F))
                    geopoiesisService.OZone = MathHelper.Min(1, geopoiesisService.OZone + dmod);
                if (kbManager.KeyDown(Keys.G))
                    geopoiesisService.OZone = MathHelper.Max(0, geopoiesisService.OZone - dmod);

                if (kbManager.KeyDown(Keys.Y))
                    geopoiesisService.LifeLevel = MathHelper.Min(1, geopoiesisService.LifeLevel + dmod);
                if (kbManager.KeyDown(Keys.U))
                    geopoiesisService.LifeLevel = MathHelper.Max(0, geopoiesisService.LifeLevel - dmod);

                if (kbManager.KeyDown(Keys.J))
                    _MinLand = MathHelper.Min(1, _MinLand + dmod);
                if (kbManager.KeyDown(Keys.K))
                    _MinLand = MathHelper.Max(0, _MinLand - dmod);

                if (kbManager.KeyDown(Keys.I))
                    _MinHill = MathHelper.Min(1, _MinHill + dmod);
                if (kbManager.KeyDown(Keys.O))
                    _MinHill = MathHelper.Max(0, _MinHill - dmod);

                if (kbManager.KeyPress(Keys.F2))
                    planet.SetLODLevel(planet.LodLevel + 1);
                if (kbManager.KeyPress(Keys.F3))
                    planet.SetLODLevel(planet.LodLevel - 1);

                planet.LightDirection = Vector3.Left;
                atmos.LightDirection = planet.LightDirection;


                atmos.Transform.Scale = (Vector3.One * planet.Radius * 1.05f) + (Vector3.One * DisplacementMag);

                if (kbManager.KeyPress(Keys.Space))
                    AddVolcano(Vector3.Zero);

                //planet.Transform.Rotate(Vector3.Up, .0025f);

                for (int s = 0; s < pet.Particles.Count; s++)
                {
                    ITransform particle = pet.Particles[s];

                    if (pet.Active[particle] == true)
                    {
                        Vector3 v =  pet.Transform.Position - planet.Transform.Position;
                        
                        v.Normalize();

                        float x = rnd.Next(-10, 10) / 10f;
                        float y = rnd.Next(-10, 10) / 10f;
                        //v += new Vector3(x, y,0);
                        particle.Position += v * .005f;

                        float d = Vector3.Distance(particle.Position, pet.Transform.Position);
                        particle.Scale = Vector3.One * d * 1f;
                        if (d > 1)
                        {
                            pet.Active[particle] = false;
                            particle.Scale = Vector3.Zero;
                            particle.Position = pet.Transform.Position;
                        }
                    }
                    else
                    {
                        pet.Active[particle] = s == rnd.Next(0, pet.Particles.Count);
                    }
                }

                    
            }

            base.Update(gameTime);

        }

        Random rnd = new Random();

        public override void Draw(GameTime gameTime)
        {
            RasterizerState currentRasterizerState = GraphicsDevice.RasterizerState;

            if (planet.effect != null)
            {

                if (atmos.effect.Parameters["atmos"] != null)
                    atmos.effect.Parameters["atmos"].SetValue(geopoiesisService.OZone);

                if (planet.effect.Parameters["res"] != null)
                    planet.effect.Parameters["res"].SetValue(new Vector2(planet.CubeSize, planet.CubeSize));

                // Change values in the planet's shader.
                if (planet.effect.Parameters["colorRamp"] != null) // Or generate this procedurally. 
                    planet.effect.Parameters["colorRamp"].SetValue(Game.Content.Load<Texture2D>("Textures/Ramp1"));

                if (planet.effect.Parameters["displacemntMag"] != null)
                    planet.effect.Parameters["displacemntMag"].SetValue(DisplacementMag);

                if (planet.effect.Parameters["_MinSeaDepth"] != null)
                    planet.effect.Parameters["_MinSeaDepth"].SetValue(geopoiesisService.WaterLevel);

                if (planet.effect.Parameters["_MinShoreDepth"] != null)
                    planet.effect.Parameters["_MinShoreDepth"].SetValue(geopoiesisService.WaterLevel + .05f);

                if (planet.effect.Parameters["_MinLand"] != null)
                    planet.effect.Parameters["_MinLand"].SetValue(geopoiesisService.LifeLevel);

                if (planet.effect.Parameters["_MinHill"] != null)
                    planet.effect.Parameters["_MinHill"].SetValue(_MinHill);
            }

            base.Draw(gameTime);
            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);
            
            if (!planet.Generated && planet.GenerationString != null)
            {
                _spriteBatch.DrawString(font, planet.GenerationString, new Vector2(8, 8), Color.White);
            }

            if(face != null)
                _spriteBatch.Draw(face, new Rectangle(GraphicsDevice.Viewport.Width - 512, 0, 512 ,512), Color.White);

            _spriteBatch.End();

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.RasterizerState = currentRasterizerState;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        }

        public override void LoadScene()
        {
            base.LoadScene();
        }

        public override void UnloadScene()
        {
            base.UnloadScene();
            State = SceneStateEnum.Unloaded;
        }
    }
}
