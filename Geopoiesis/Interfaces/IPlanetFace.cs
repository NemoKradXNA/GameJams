using Geopoiesis.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Geopoiesis.Interfaces
{
    public interface IPlanetFace
    {
        Vector3 RootPosition { get; set; }
        Vector3 Normal { get; set; }
        int Dimension { get; set; }
        float Radius { get; set; }
        float NoiseMod { get; set; }
        int CubeSize { get; set; }
        float DisplaceMesh { get; set; }

        Texture2D faceHeightMap { get; set; }
        Texture2D faceNormalMap { get; set; }
        MeshData meshData { get; set; }
        IEnumerator BuildMesh(List<string> debug);
    }
}
