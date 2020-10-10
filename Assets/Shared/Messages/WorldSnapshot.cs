using System.Numerics;

namespace Shared.Messages
{
    public struct WorldSnapshot
    {
        public uint snapshotNum { get; set; }
        public uint lastMessageNum { get; set; }
        public Vector3 lastPosition { get; set; }
        public Vector3 lastRotation { get; set; }
    }
}