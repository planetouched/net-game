using System.Collections;
using Basement.OEPFramework.Futures;
using Preloader;
using UnityEngine;

namespace Client.Sections._Base
{
    public abstract class SectionBase : MonoBehaviour
    {
        public static SectionBase Current { get; private set; }

        IEnumerator Start()
        {
            while (!AppStart.isInit)
            {
                yield return null;
            }

            Current = this;
            Init();
        }

        protected abstract void Init();
        public abstract IFuture Drop();
    }
}
