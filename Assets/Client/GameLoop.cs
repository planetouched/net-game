using Basement.OEPFramework.UnityEngine;
using Basement.OEPFramework.UnityEngine.Loop;
using Client.Sections._Base;
using Preloader;
using UnityEngine;

namespace Client
{
    public class GameLoop : MonoBehaviour
    {
        public static readonly string EventAppPause = GEvent.GetUniqueCategory();

        private void Awake()
        {
            Application.runInBackground = true;
            
            if (!AppStart.isInit)
            {
                Loops.Init();
                AppStart.Start();
            }
        }

        #region loop

        private void OnGUI()
        {
            EngineLoopManager.Execute(Loops.LEGACY_GUI);
        }

        private void FixedUpdate()
        {
            EngineLoopManager.Execute(Loops.FIXED_UPDATE);
        }

        private void Update()
        {
            Timer.Process(Loops.TIMER, Time.deltaTime);
            EngineLoopManager.Execute(Loops.UPDATE);
        }

        private void LateUpdate()
        {
            EngineLoopManager.Execute(Loops.LATE_UPDATE);
        }

        private void OnDestroy()
        {
            SectionBase.Current.Drop();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            GEvent.Call(EventAppPause, hasFocus);
        }

        #endregion
    }
}