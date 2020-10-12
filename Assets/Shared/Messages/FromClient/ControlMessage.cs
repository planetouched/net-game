using System;
using System.Threading;
using Shared.Enums;
using Shared.Factories;
using Shared.Messages._Base;
using Shared.Utils;

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
        public uint messageNum { get; set; }
        
        public float deltaTime { get; set; }
        public float sensitivity { get; set; }
        private int _keys;

        public float mouseX { get; set; }
        public float mouseY { get; set; }

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

        public ControlMessage()
        {
            messageId = (byte)MessageIds.PlayerControl;
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

        public override void Serialize(ref int offset, byte[] buffer)
        {
            WriteHeader(ref offset, buffer);
            SerializeUtil.SetUInt(messageNum, ref offset, buffer);
            SerializeUtil.SetFloat(deltaTime, ref offset, buffer);
            SerializeUtil.SetFloat(sensitivity, ref offset, buffer);
            SerializeUtil.SetInt(_keys, ref offset, buffer);
            SerializeUtil.SetFloat(mouseX, ref offset, buffer);
            SerializeUtil.SetFloat(mouseY, ref offset, buffer);
            SetMessageSize(offset, buffer);
        }

        public override void Deserialize(ref int offset, byte[] buffer)
        {
            ReadHeader(ref offset, buffer);
            
            if (MessageSize() != messageSize)
            {
                throw new Exception("MessageSize() != messageSize");
            }
            
            messageNum = SerializeUtil.GetUInt(ref offset, buffer);
            deltaTime = SerializeUtil.GetFloat(ref offset, buffer);
            sensitivity = SerializeUtil.GetFloat(ref offset, buffer);
            _keys = SerializeUtil.GetInt(ref offset, buffer);
            mouseX = SerializeUtil.GetFloat(ref offset, buffer);
            mouseY = SerializeUtil.GetFloat(ref offset, buffer);            
        }
        
        public override int MessageSize()
        {
            return HeaderSize + 24;
        }
    }
}