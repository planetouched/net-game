namespace Shared.Simulations
{
    public interface ISimulation
    {
        void StartSimulation();
        void StopSimulation();
        void ProcessSimulation();
    }
}