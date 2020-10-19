using System;
using LiteNetLib.Utils;
using Shared.Enums;
using Shared.Messages._Base;
using Shared.Messages.FromClient;
using Shared.Messages.FromServer;

namespace Shared.Factories
{
    public static class MessageFactory
    {
        public static T Create<T>(MessageIds messageId) where T : IMessage
        {
            IMessage message = null;

            switch (messageId)
            {
                //client 
                case MessageIds.EnterGame:
                    message = new EnterGameMessage();
                    break;
                case MessageIds.PlayerControl:
                    message = new ControlMessage();
                    break;

                //server
                case MessageIds.ConnectAccepted:
                    message = new EnterGameAcceptedMessage();
                    break;
                case MessageIds.WorldSnapshot:
                    message = new WorldSnapshotMessage();
                    break;
            }

            if (message != null)
            {
                return (T) message;
            }

            throw new Exception("packetID not implemented");
        }

        public static IMessage Create(NetDataReader netDataReader)
        {
            var messageId = (MessageIds) netDataReader.PeekByte();
            var message = Create<IMessage>(messageId);

            message.Deserialize(netDataReader);
            return message;
        }
    }
}