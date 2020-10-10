using Basement.OEPFramework.Futures;
using Client.Game;
using Client.Sections._Base;
using Client.Test;
using Server.Game;
using Shared.Game;
using UnityEngine;
using Vector3 = System.Numerics.Vector3;

namespace Client.Sections.GamePlay
{
    public class GamePlaySection : SectionBase
    {
        private ClientSimulation _clientSimulation;
        private ServerSimulation _serverSimulation;
        
        protected override void Init()
        {
            Debug.Log("start");
            _serverSimulation = new ServerSimulation(new World(new Player(new Vector3(0, 1, 0), Vector3.Zero)));
            _clientSimulation = new ClientSimulation(new World(new Player(new Vector3(0, 1, 0), Vector3.Zero)));
            
            Bridge.SetSimulations(_clientSimulation, _serverSimulation);
            
            _serverSimulation.Start();
            _clientSimulation.Start();
        }

        public override IFuture Drop()
        {
            _clientSimulation.Drop();
            _serverSimulation.Dispose();
            return null;
        }
    }
}