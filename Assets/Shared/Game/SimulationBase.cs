namespace Shared.Game
{
    public abstract class SimulationBase
    {
        protected SimulationBase(World world)
        {
        }

        public abstract void Start();
        public abstract void Update();
    }
}