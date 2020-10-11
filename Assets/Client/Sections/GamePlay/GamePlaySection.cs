using System.Collections.Generic;
using Basement.OEPFramework.Futures;
using Client.Game;
using Client.Sections._Base;
using Client.Test;
using Server.Game;
using Server.Game.Entities;
using Shared.Game.Entities;
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
            _serverSimulation = new ServerSimulation(new ServerWorld());
            _clientSimulation = new ClientSimulation();
            
            Bridge.SetSimulations(_clientSimulation, _serverSimulation);
            
            _serverSimulation.Start();
            _clientSimulation.Start();
        }

        public override IFuture Drop()
        {
            _clientSimulation.Stop();
            _serverSimulation.Stop();
            return null;
        }
    }
}