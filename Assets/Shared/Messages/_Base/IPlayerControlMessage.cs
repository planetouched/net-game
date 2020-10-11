namespace Shared.Messages._Base
{
    public interface IPlayerControlMessage : IControlMessage
    {
        float mouseSensitivity { get; }
        
        bool forward { get; }
        bool backward { get; }
        bool left { get; }
        bool right { get; }
        float mouseX { get; }
        float mouseY { get; }
    }
}