using Basement.OEPFramework.Futures;
using Basement.OEPFramework.UnityEngine;
using Client.Loggers;
using Client.Sections._Base;
using Client.Simulations;
using Server;
using Server.Simulations;
using Shared.Loggers;
using UnityEngine;

namespace Client.Sections.GamePlay
{
    public class GamePlaySection : SectionBase
    {
        private ClientSimulation _clientSimulation;
        private ServerSimulation _serverSimulation;
        private Timer _serverTickTimer;
        
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
            if (GUI.Button(new Rect(0, 0, 100, 100), "Run server") && _serverSimulation == null)
            {
                var tickDelay = 1 / (float) ServerSettings.TicksCount;
                _serverTickTimer = Timer.CreateRealtime(tickDelay, Server_Tick, null);
                _serverSimulation = new ServerSimulation(12345);
                _serverSimulation.CreateWorld();
                _serverSimulation.StartSimulation();
            }
            
            if (GUI.Button(new Rect(0, 200, 100, 100), "Stop server") && _serverSimulation != null)
            {
                _serverSimulation.Drop();
                _serverTickTimer.Drop();
                _serverSimulation = null;
            }

            if (GUI.Button(new Rect(300, 0, 100, 100), "Run client") && _clientSimulation == null)
            {
                //_clientSimulation = new ClientSimulation("95.216.192.107", 12345);
                _clientSimulation = new ClientSimulation("localhost", 12345);
                _clientSimulation.StartSimulation();
            }
            
            if (GUI.Button(new Rect(300, 200, 100, 100), "Stop client") && _clientSimulation != null)
            {
                _clientSimulation.Drop();
                _clientSimulation = null;
            }
        }

        private void Server_Tick()
        {
            _serverSimulation.ProcessSimulation();
        }

        public override IFuture Drop()
        {
            _serverSimulation?.StopSimulation();
            _clientSimulation?.Drop();
            _serverTickTimer?.Drop();
            return null;
        }
    }
}