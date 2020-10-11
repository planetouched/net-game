namespace Shared.Messages
{
    public struct WorldSnapshot
    {
        public uint snapshotNum { get; set; }
        public int snapshotSize { get; set; } 
        public byte[] data { get; set; }
    }
}