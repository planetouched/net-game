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
        Right = 3
    }
    
    public class ControlMessage : MessageBase
    {
        public float deltaTime { get; set; }
        public float sensitivity { get; set; }

        public float mouseX { get; set; }
        public float mouseY { get; set; }
        
        private int _keys;

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

        public ControlMessage()
        {
        }
        
        public ControlMessage(uint objectId, int gameId) : base(0, MessageIds.PlayerControl, objectId, gameId)
        {
        }

        public void SetMessageNum(uint num)
        {
            messageNum = num;
        }
        
        public override NetDataWriter Serialize(NetDataWriter netDataWriter, bool resetBeforeWriting = true)
        {
            if (resetBeforeWriting)
                netDataWriter.Reset();
            
            WriteHeader(netDataWriter);
            netDataWriter.Put(deltaTime);
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
            sensitivity = netDataReader.GetFloat();
            _keys = netDataReader.GetInt();
            mouseX = netDataReader.GetFloat();
            mouseY = netDataReader.GetFloat();
        }
    }
}