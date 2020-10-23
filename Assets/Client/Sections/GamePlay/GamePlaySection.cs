using Basement.OEPFramework.Futures;
using Client.Loggers;
using Client.Sections._Base;
using Client.Simulations;
using Server.Simulations;
using UnityEngine;
using Logger = Shared.Loggers.Logger;

namespace Client.Sections.GamePlay
{
    public class GamePlaySection : SectionBase
    {
        private ClientSimulation _clientSimulation;
        private ServerSimulation _serverSimulation;
        
        protected override void Init()
        {
            Logger.SetLogger(new UnityLogger());
            Logger.Log("start");
        }

        private void OnGUI()
        {
            if (GUI.Button(new Rect(0, 0, 100, 100), "Run server"))
            {
                _serverSimulation = new ServerSimulation(12345, 1);
                _serverSimulation.StartSimulation();
            }
            
            if (GUI.Button(new Rect(0, 200, 100, 100), "Stop server"))
            {
                _serverSimulation.StopSimulation();
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