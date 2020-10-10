using System.Numerics;
using System.Threading;
using Shared.Messages;
using Shared.Utils;

namespace Shared.Game
{
    public class Player
    {
        private static int _globalHash;
        private readonly int _hash;
        
        public Vector3 position { get; set; }
        public Vector3 rotation { get; set; }

        public Player(Vector3 position, Vector3 rotation)
        {
            this.position = position;
            this.rotation = rotation;
            
            _hash = Interlocked.Increment(ref _globalHash);
        }

        public void Calculate(ControlMessage message)
        {
            Vector3 movement = Vector3.Zero;

            if (message.forward)
                movement += new Vector3(0, 0, 1);

            if (message.backward)
                movement += new Vector3(0, 0, -1);

            if (message.left)
                movement += new Vector3(-1, 0, 0);

            if (message.right)
                movement += new Vector3(1, 0, 0);

            rotation += new Vector3(message.mouseY, message.mouseX, 0) * message.mouseSensitivity;
            rotation = MathUtil.ClampTo180(rotation);

            var q = Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathUtil.ToRadians(rotation.Y));
            position += Vector3.Transform(movement * SharedSettings.BaseSpeed * message.deltaTime, q);
        }

        public override int GetHashCode()
        {
            return _hash;
        }
    }
}