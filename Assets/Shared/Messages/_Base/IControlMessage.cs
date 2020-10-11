namespace Shared.Messages._Base
{
    public interface IControlMessage : IMessage
    {
        uint messageNum { get; set; }
        float deltaTime { get; set; }
    }
}