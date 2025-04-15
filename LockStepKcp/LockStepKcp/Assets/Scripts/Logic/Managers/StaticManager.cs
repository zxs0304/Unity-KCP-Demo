using System.Collections.Generic;
using Lockstep.Logic;
using Lockstep.Math;
using UnityEngine;
using Debug = Lockstep.Logging.Debug;

namespace LockstepTutorial
{
    public class StaticManager : UnityBaseManager
    {
        public static StaticManager Instance { get; private set; }

        public override void DoAwake()
        {
            Instance = this;
        }

        public override void DoStart()
        {

        }

        public override void DoUpdate(LFloat deltaTime)
        {

        }


        public static BaseEntity InstantiateEntity(int prefabId, Entity owner, LVector3 position)
        {
            var prefab = ResourceManager.LoadPrefab(prefabId);
            var config = ResourceManager.GetBulletConfig(prefabId);
            Debug.Trace("CreateBullet");
            var bullet = new Bullet(owner);
            var obj = UnityEntityService.CreateEntity(bullet, prefabId, position, prefab, config);
            return bullet;
        }
    }
}