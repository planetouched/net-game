using System.Numerics;
using Shared.Messages.FromClient;
using Shared.Utils;

namespace Shared.Game.Entities._Base
{
    public abstract class SharedPlayerBase : SharedEntityBase, ISharedPlayer
    {
        public uint lastMessageNum { get; protected set; }
        public abstract void AddControlMessage(ControlMessage message);
        
        protected void Movement(ControlMessage message)
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

            rotation += new Vector3(message.mouseY, message.mouseX, 0) * message.sensitivity;
            rotation = MathUtil.ClampTo180(rotation);

            var q = Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathUtil.ToRadians(rotation.Y));
            position += Vector3.Transform(movement * SharedSettings.BaseSpeed * message.deltaTime, q);
        }
    }
}