using LiteNetLib.Utils;
using Shared.Enums;
using Shared.Messages._Base;

namespace Shared.Messages.FromServer
{
    public class WorldSnapshotMessage : MessageBase
    {
        public float serverTime { get; private set; }
        public uint snapshotNum { get; private set; }
        public byte[] worldData { get; private set; }

        public WorldSnapshotMessage()
        {
        }

        public WorldSnapshotMessage(
            uint snapshotNum,
            NetDataWriter worldDataWriter,
            float serverTime
            
        ) : base(MessageIds.WorldSnapshot)
        {
            this.serverTime = serverTime;
            this.snapshotNum = snapshotNum;
            var size = worldDataWriter.Length;
            worldData = new byte[size];
            var netDataReader = new NetDataReader(worldDataWriter);
            netDataReader.GetBytes(worldData, size);
        }

        public override NetDataWriter Serialize(NetDataWriter netDataWriter)
        {
            WriteHeader(netDataWriter);
            netDataWriter.Put(snapshotNum);
            netDataWriter.Put(serverTime);
            netDataWriter.PutBytesWithLength(worldData);

            return netDataWriter;
        }

        public override void Deserialize(NetDataReader netDataReader)
        {
            ReadHeader(netDataReader);
            snapshotNum = netDataReader.GetUInt();
            serverTime = netDataReader.GetFloat();
            worldData = netDataReader.GetBytesWithLength();
        }
    }
}