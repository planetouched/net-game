using Basement.OEPFramework.UnityEngine;
using Client.Entities.Weapons._Base;
using Client.Utils;
using Client.Worlds;
using UnityEngine;

namespace Client.Entities.Weapons
{
    public class ClientRailGun : ClientWeaponBase
    {
        private Timer _timer;
        
        public ClientRailGun(ClientWorld clientWorld) : base(clientWorld)
        {
        }
        
        public override void Create()
        {
            // railgun tps model
        }

        protected override void Shot(uint targetId)
        {
            var obj = Object.Instantiate(Resources.Load<GameObject>("RailGun/Prefabs/RailGun"));
            obj.transform.position = position;
            
            if (targetId == 0)
            {
                obj.transform.rotation = Quaternion.Euler(rotation);
            }
            else
            {
                var entity = clientWorld.FindEntity(targetId);
                
                if (entity != null)
                {
                    var dir = entity.current.position - position;
                    obj.transform.rotation = Quaternion.LookRotation(dir);

                    if (entity.objectId == ClientLocalPlayer.serverObjectId)
                    {
                        _timer?.Drop();
                        GameObject.Find("Canvas").transform.Find("_Damage").gameObject.SetActive(true);
                        _timer = Timer.Create(1, () =>
                        {
                            GameObject.Find("Canvas").transform.Find("_Damage").gameObject.SetActive(false);

                        }, this, true);
                    }
                }
            }
            
            obj.transform.Translate(Vector3.forward * 0.5f);
            
        }
    }
}