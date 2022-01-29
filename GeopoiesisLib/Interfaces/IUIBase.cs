using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Geopoiesis.Interfaces
{
    public interface IUIBase
    {
        Point Position { get; set; }
        Point Size { get; set; }
        Rectangle Rectangle { get; }
        Color Tint { get; set; }

    }
}
