using System;
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
                case MessageIds.Connect:
                    message = new ConnectMessage();
                    break;
                case MessageIds.Disconnect:
                    message = new DisconnectMessage();
                    break;
                case MessageIds.PlayerControl:
                    message = new ControlMessage();
                    break;

                //server
                case MessageIds.ConnectAccepted:
                    message = new ConnectAcceptedMessage();
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

        public static IMessage Create(byte[] data)
        {
            var messageId = MessageBase.GetMessageId(data);
            var message = Create<IMessage>(messageId);

            int offset = 0;
            message.Deserialize(ref offset, data);
            return message;
        }
    }
}