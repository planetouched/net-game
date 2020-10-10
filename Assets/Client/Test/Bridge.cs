using Client.Game;
using Server.Game;

namespace Client.Test
{
    public static class Bridge
    {
        public static ClientSimulation clientSimulation { get; private set; }
        public static ServerSimulation serverSimulation { get; private set; }
        
        public static void SetSimulations(ClientSimulation clientSimulation, ServerSimulation serverSimulation)
        {
            Bridge.clientSimulation = clientSimulation;
            Bridge.serverSimulation = serverSimulation;
        }
    }
}