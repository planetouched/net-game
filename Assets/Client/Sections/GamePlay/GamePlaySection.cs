using Basement.OEPFramework.Futures;
using Client.Game;
using Client.Sections._Base;
using Server.Game;
using UnityEngine;

namespace Client.Sections.GamePlay
{
    public class GamePlaySection : SectionBase
    {
        private ClientSimulation _clientSimulation;
        private ServerSimulation _serverSimulation;
        
        protected override void Init()
        {
            Debug.Log("start");
        }

        private void OnGUI()
        {
            if (GUI.Button(new Rect(0, 0, 100, 100), "Run server"))
            {
                _serverSimulation = new ServerSimulation("127.0.0.1", 12345, 1);
                _serverSimulation.Start();
            }
            
            if (GUI.Button(new Rect(0, 200, 100, 100), "Stop server"))
            {
                _serverSimulation.Stop();
            }
            
            if (GUI.Button(new Rect(300, 0, 100, 100), "Run client"))
            {
                _clientSimulation = new ClientSimulation("127.0.0.1", 12345, 1);
                _clientSimulation.Start();
            }
            
            if (GUI.Button(new Rect(300, 200, 100, 100), "Stop client"))
            {
                _clientSimulation.Drop();
            }
        }

        public override IFuture Drop()
        {
            _serverSimulation?.Stop();
            _clientSimulation?.Drop();
            return null;
        }
    }
}