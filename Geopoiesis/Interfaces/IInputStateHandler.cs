using System;
using System.Collections.Generic;
using System.Text;

namespace Geopoiesis.Interfaces
{
    public interface IInputStateHandler : IInputStateManager
    {
        IKeyboardStateManager KeyboardManager { get; set; }
        IGamePadManager GamePadManager { get; set; }
        IMouseStateManager MouseManager { get; set; }
    }
}
