using Shared.Messages.FromClient;
using Shared.Utils;
using UnityEngine;

namespace Shared.Entities
{
    public static class SharedPlayerBehaviour
    {
        public static void Movement(ref Vector3 position, ref Vector3 rotation, ControlMessage message)
        {
            Vector3 movement = Vector3.zero;

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

            var q = Quaternion.AngleAxis(rotation.y, Vector3.up);
            position +=  q * (movement * SharedSettings.BaseSpeed * message.deltaTime);
        }
    }
}