using System.Collections;
using Geopoiesis.Interfaces;
using Microsoft.Xna.Framework;

namespace Geopoiesis.Managers.Coroutines
{
    /// <summary>
    /// An engine Coroutine that will be used to wait for end of frame before finishing
    /// </summary>
    public class WaitForEndOfFrame : Coroutine, IWaitCoroutine
    {
        public WaitForEndOfFrame(Game game) : base(game)
        {
            Routine = routine();
            CoroutineManager.StartCoroutine(this);
        }

        IEnumerator routine()
        {
            yield break;
        }
    }
}
