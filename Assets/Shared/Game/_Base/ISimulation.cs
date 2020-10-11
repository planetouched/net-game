namespace Shared.Game._Base
{
    public interface ISimulation
    {
        void Start();
        void Stop();
        void Process();
    }
}