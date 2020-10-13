namespace Shared.Simulations
{
    public interface ISimulation
    {
        void Start();
        void Stop();
        void Process();
    }
}