using System.Collections.Generic;
using LiteNetLib.Utils;
using Server.Entities;
using Shared.Enums;
using Shared.Messages._Base;
using Shared.Messages.FromClient;

namespace Shared.Messages.FromServer
{
    public class WorldSnapshotMessage : MessageBase
    {
        public Dictionary<ServerPlayer, List<ControlMessage>> messages { get; } = new Dictionary<ServerPlayer, List<ControlMessage>>();
        public float deltaTime { get; private set; }
        public float serverTime { get; private set; }
        public uint snapshotNum { get; private set; }
        public byte[] worldData { get; private set; }

        public WorldSnapshotMessage()
        {
        }

        public WorldSnapshotMessage(
            uint snapshotNum,
            NetDataWriter worldDataWriter,
            float deltaTime,
            float serverTime
            
        ) : base(MessageIds.WorldSnapshot)
        {
            this.serverTime = serverTime;
            this.deltaTime = deltaTime;
            this.snapshotNum = snapshotNum;
            var size = worldDataWriter.Length;
            worldData = new byte[size];
            var netDataReader = new NetDataReader(worldDataWriter);
            netDataReader.GetBytes(worldData, size);
        }

        public void AddControlMessage(ServerPlayer player, ControlMessage message)
        {
            if (messages.TryGetValue(player, out var list))
            {
                list.Add(message);
            }
            else
            {
                messages.Add(player, new List<ControlMessage> {message});
            }
        }

        public override NetDataWriter Serialize(NetDataWriter netDataWriter)
        {
            WriteHeader(netDataWriter);
            netDataWriter.Put(snapshotNum);
            netDataWriter.Put(deltaTime);
            netDataWriter.Put(serverTime);
            netDataWriter.PutBytesWithLength(worldData);

            return netDataWriter;
        }

        public override void Deserialize(NetDataReader netDataReader)
        {
            ReadHeader(netDataReader);
            snapshotNum = netDataReader.GetUInt();
            deltaTime = netDataReader.GetFloat();
            serverTime = netDataReader.GetFloat();
            worldData = netDataReader.GetBytesWithLength();
        }
    }
}