using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Geopoiesis.Interfaces
{
    public interface ICoroutineService
    {
        List<ICoroutine> Coroutines { get; }

        void Update(GameTime gameTime);

        void UpdateEndFrame(GameTime gameTime);

        ICoroutine StartCoroutine(IEnumerator routine);

        void StopCoroutine(IEnumerator coroutine);

        ICoroutine StartCoroutine(ICoroutine coroutine);
        void StopCoroutine(ICoroutine coroutine);
    }
    public interface ICoroutine
    {
        ICoroutineService CoroutineManager { get; }
        IEnumerator Routine { get; set; }
        ICoroutine WaitForCoroutine { get; set; }
        bool Finished { get; set; }
    }

    public interface IWaitCoroutine { }
}
