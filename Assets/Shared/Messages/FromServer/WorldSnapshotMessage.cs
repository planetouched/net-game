using System.Collections.Generic;
using LiteNetLib.Utils;
using Shared.Enums;
using Shared.Messages._Base;

namespace Shared.Messages.FromServer
{
    public class WorldSnapshotMessage : MessageBase
    {
        public readonly List<IMessage> messages = new List<IMessage>();
        public float deltaTime { get; private set; }
        public uint snapshotNum { get; private set; }
        public byte[] worldData { get; private set; }

        public WorldSnapshotMessage()
        {
        }

        public WorldSnapshotMessage(
            uint messageNum,
            MessageIds messageId,
            uint snapshotNum,
            NetDataWriter worldDataWriter,
            int gameId,
            float deltaTime
        ) : base(messageNum, messageId, 0, gameId)
        {
            this.deltaTime = deltaTime;
            this.snapshotNum = snapshotNum;
            var size = worldDataWriter.Length;
            worldData = new byte[size];
            var netDataReader = new NetDataReader(worldDataWriter);
            netDataReader.GetBytes(worldData, size);
        }

        public override NetDataWriter Serialize(NetDataWriter netDataWriter, bool resetBeforeWriting = true)
        {
            if (resetBeforeWriting)
                netDataWriter.Reset();

            WriteHeader(netDataWriter);
            netDataWriter.Put(snapshotNum);
            netDataWriter.Put(deltaTime);
            netDataWriter.PutBytesWithLength(worldData);

            return netDataWriter;
        }

        public override void Deserialize(NetDataReader netDataReader)
        {
            ReadHeader(netDataReader);
            snapshotNum = netDataReader.GetUInt();
            deltaTime = netDataReader.GetFloat();
            worldData = netDataReader.GetBytesWithLength();
        }
    }
}