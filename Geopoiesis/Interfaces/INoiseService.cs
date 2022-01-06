using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Geopoiesis.Interfaces
{
    public interface INoiseService
    {
        float Noise(float x);
        float Noise(float x, float y);
        float Noise(Vector2 coord);
        float Noise(float x, float y, float z);
        float Noise(Vector3 coord);
    }
}
