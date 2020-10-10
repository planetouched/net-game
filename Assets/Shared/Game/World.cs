
namespace Shared.Game
{
    public class World
    {
        public Player localPlayer { get; }
        
        public World(Player localPlayer)
        {
            this.localPlayer = localPlayer;
        }
    }
}