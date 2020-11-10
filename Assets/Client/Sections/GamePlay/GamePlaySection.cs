using Basement.OEPFramework.Futures;
using Client.Loggers;
using Client.Sections._Base;
using Client.Simulations;
using Server.Simulations;
using Shared.Loggers;
using UnityEngine;

namespace Client.Sections.GamePlay
{
    public class GamePlaySection : SectionBase
    {
        private ClientSimulation _clientSimulation;
        private ServerSimulation _serverSimulation;
        
        protected override void Init()
        {
            Application.targetFrameRate = 144;
            Physics.autoSimulation = false;

            Log.SetLogger(new UnityLogger());
            Log.Write("start");
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.H))
            {
                Cursor.visible = !Cursor.visible;
                
                if (!Cursor.visible)
                    Cursor.lockState = CursorLockMode.Locked;
                else
                {
                    Cursor.lockState = CursorLockMode.None;
                }
            }
        }
        
        private void OnGUI()
        {
            if (GUI.Button(new Rect(0, 0, 100, 100), "Run server"))
            {
                _serverSimulation = new ServerSimulation(12345);
                _serverSimulation.StartSimulation();
            }
            
            if (GUI.Button(new Rect(0, 200, 100, 100), "Stop server"))
            {
                _serverSimulation.Drop();
            }
            
            if (GUI.Button(new Rect(300, 0, 100, 100), "Run client"))
            {
                //string ip =  new System.Net.WebClient().DownloadString("https://api.ipify.org");
                //_clientSimulation = new ClientSimulation("95.216.192.107", 12345);
                _clientSimulation = new ClientSimulation("localhost", 12345);
                _clientSimulation.StartSimulation();
            }
            
            if (GUI.Button(new Rect(300, 200, 100, 100), "Stop client"))
            {
                _clientSimulation.Drop();
            }
        }

        public override IFuture Drop()
        {
            _serverSimulation?.StopSimulation();
            _clientSimulation?.Drop();
            return null;
        }
    }
}