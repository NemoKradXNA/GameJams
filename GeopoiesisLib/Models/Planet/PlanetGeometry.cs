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
    
    //https://3dtextures.me/2021/05/28/rock-042/
    public class PlanetGeometry : MorphableSphere
    {

        public List<string> Debug = new List<string>();

        protected List<Texture2D> textures = null;


        public PlanetGeometry(Game game, string effectAsset, int faceDimensions, float noise, int mapSize, int startLod = 3, int maxLod = 8) : base(game, effectAsset, faceDimensions, 2, noise, mapSize, 1971, startLod, maxLod)
        {
        }
        protected void WriteToDebug(string msg) // Should really be a "console" logging service...
        {
            Debug.Add(string.Format("[{0:dd-MMM-yyyy HH:mm:ss}] - {1}", DateTime.Now, msg));
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


            if (textures == null)
            {
                textures = new List<Texture2D>();
                for (int t = 0; t < 4; t++)
                    textures.Add(new Texture2D(Game.GraphicsDevice, 1, 1));

                Color[] c = new Color[] { new Color(.2f, .2f, .8f, 1f) };
                textures[0].SetData(c);

                c = new Color[] { new Color(.4f, .4f, .3f, 1f) };
                textures[1].SetData(c);

                c = new Color[] { /*new Color(.8f, .4f, .3f, 1f)*/ /*new Color(162, 147, 132, 255) */ new Color(96, 85, 79, 255) };
                textures[2].SetData(c);

                c = new Color[] { new Color(.8f, .8f, 1f, 1f) };
                textures[3].SetData(c);
            }

            if (effect.Parameters["sandTexture"] != null)
                effect.Parameters["sandTexture"].SetValue(textures[0]);
            if (effect.Parameters["grassTexture"] != null)
                effect.Parameters["grassTexture"].SetValue(textures[1]);
            if (effect.Parameters["rockTexture"] != null)
                effect.Parameters["rockTexture"].SetValue(textures[2]);
            if (effect.Parameters["snowTexture"] != null)
                effect.Parameters["snowTexture"].SetValue(textures[3]);

            base.Draw(gameTime);
        }
    }
}
