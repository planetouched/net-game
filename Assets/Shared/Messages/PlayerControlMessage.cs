using Shared.Messages._Base;

namespace Shared.Messages
{
    public class PlayerControlMessage : IPlayerControlMessage
    {
        public uint objectId { get; set; }

        public uint messageNum { get; set; }
        public float deltaTime { get; set; }
        public float mouseSensitivity { get; set; }
        
        public bool forward { get; set; }
        public bool backward { get; set; }
        public bool left { get; set; }
        public bool right { get; set; }
        public float mouseX { get; set; }
        public float mouseY { get; set; }
        
        public void Clear()
        {
            forward = false;
            backward = false;
            left = false;
            right = false;
            mouseX = 0;
            mouseY = 0;
            mouseSensitivity = 0;
            messageNum = 0;
            deltaTime = 0;
        }
    }
}