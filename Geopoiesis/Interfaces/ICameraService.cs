using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Geopoiesis.Interfaces
{
    public interface ICameraService
    {
        ITransform Transform { get; set; }

        float AspectRatio { get; }
        BoundingFrustum Frustum { get; }

        Viewport Viewport { get; }

        Matrix View { get; set; }

        Matrix Projection { get; set; }

        float FieldOfView { get; set; }

        float NearClipPlane { get; set; }

        float FarClipPlane { get; set; }

        Color ClearColor { get; set; }

        bool RenderWireFrame { get; set; }
    }
}
