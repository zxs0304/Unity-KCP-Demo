using System.Collections.Generic;
using Lockstep.Logic;
using Lockstep.Math;
using UnityEngine;
using Debug = Lockstep.Logging.Debug;

namespace LockstepTutorial
{
    public class BulletManager : UnityBaseManager
    {
        public static BulletManager Instance { get; private set; }
        public List<Bullet> allBullets = new List<Bullet>();

        public override void DoAwake()
        {
            Instance = this;
        }

        public override void DoStart()
        {

        }

        public override void DoUpdate(LFloat deltaTime)
        {
            for (int i = allBullets.Count - 1; i >= 0; i--)
            {
                allBullets[i].DoUpdate(deltaTime);
            }
        }


        public void AddBullet(Bullet bullet)
        {
            allBullets.Add(bullet);
        }

        public void RemoveBullet(Bullet bullet)
        {
            allBullets.Remove(bullet);
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