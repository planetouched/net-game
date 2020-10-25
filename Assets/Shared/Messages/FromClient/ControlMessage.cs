using LiteNetLib.Utils;
using Shared.Enums;
using Shared.Messages._Base;

namespace Shared.Messages.FromClient
{
    public enum ControlMessageActions
    {
        Forward = 0,
        Backward = 1,
        Left = 2,
        Right = 3,
        MouseButton0 = 4,
        MouseButton1 = 5
    }
    
    public class ControlMessage : MessageBase
    {
        public float deltaTime { get; set; }
        public float serverTime { get; set; }
        public float sensitivity { get; set; }

        public float mouseX { get; set; }
        public float mouseY { get; set; }
        
        private int _keys;

        public bool mouseButton0
        {
            get => CheckBit(ControlMessageActions.MouseButton0);
            set => SetBit(ControlMessageActions.MouseButton0, value);
        }
        
        public bool mouseButton1
        {
            get => CheckBit(ControlMessageActions.MouseButton1);
            set => SetBit(ControlMessageActions.MouseButton1, value);
        }
        
        public bool forward
        {
            get => CheckBit(ControlMessageActions.Forward);
            set => SetBit(ControlMessageActions.Forward, value);
        }

        public bool backward
        {
            get => CheckBit(ControlMessageActions.Backward);
            set => SetBit(ControlMessageActions.Backward, value);
        }

        public bool left
        {
            get => CheckBit(ControlMessageActions.Left);
            set => SetBit(ControlMessageActions.Left, value);
        }

        public bool right
        {
            get => CheckBit(ControlMessageActions.Right);
            set => SetBit(ControlMessageActions.Right, value);
        }

        private bool CheckBit(ControlMessageActions action)
        {
            int mask = 1 << (int) action;
            int result = _keys & mask;
            return result == mask;
        }

        private void SetBit(ControlMessageActions action, bool set)
        {
            int mask = 1 << (int) action;

            if (set)
                _keys |= mask;
            else
                _keys ^= mask;
        }

        public ControlMessage() : base(MessageIds.PlayerControl)
        {
        }

        public override NetDataWriter Serialize(NetDataWriter netDataWriter)
        {
            WriteHeader(netDataWriter);
            netDataWriter.Put(deltaTime);
            netDataWriter.Put(serverTime);
            netDataWriter.Put(sensitivity);
            netDataWriter.Put(_keys);
            netDataWriter.Put(mouseX);
            netDataWriter.Put(mouseY);
            
            return netDataWriter;
        }

        public override void Deserialize(NetDataReader netDataReader)
        {
            ReadHeader(netDataReader);
            deltaTime = netDataReader.GetFloat();
            serverTime = netDataReader.GetFloat();
            sensitivity = netDataReader.GetFloat();
            _keys = netDataReader.GetInt();
            mouseX = netDataReader.GetFloat();
            mouseY = netDataReader.GetFloat();
        }
    }
}