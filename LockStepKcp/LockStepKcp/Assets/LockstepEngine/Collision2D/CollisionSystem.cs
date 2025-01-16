using System;
using System.Collections.Generic;
using Lockstep.Math;
using Lockstep.UnsafeCollision2D;
using Lockstep.Util;
using Random = System.Random;
using Debug = Lockstep.Logging.Debug;

namespace Lockstep.Collision2D {
    public delegate void FuncGlobalOnTriggerEvent(ColliderProxy a, ColliderProxy b, ECollisionEvent type);

    public partial class CollisionSystem : ICollisionSystem {
        public uint[] _collisionMask = new uint[32];

        public List<BoundsQuadTree> boundsTrees = new List<BoundsQuadTree>();
        public LFloat worldSize = 150.ToLFloat();
        public LFloat minNodeSize = 1.ToLFloat();
        public LFloat loosenessval = new LFloat(true, 1250);
        public LVector3 pos;

        private Dictionary<int, ColliderProxy> id2Proxy = new Dictionary<int, ColliderProxy>();
        private HashSet<long> _curPairs = new HashSet<long>();
        private HashSet<long> _prePairs = new HashSet<long>();
        public const int MaxLayerCount = 32;
        private List<ColliderProxy> tempLst = new List<ColliderProxy>();

        public FuncGlobalOnTriggerEvent funcGlobalOnTriggerEvent;

        public ColliderProxy GetCollider(int id){
            return id2Proxy.TryGetValue(id, out var proxy) ? proxy : null;
        }


        public int[] AllTypes;
        public bool[] InterestingMasks;
        public int LayerCount;

        public void DoStart(bool[] interestingMasks, int[] allTypes){
            LayerCount = allTypes.Length;
            this.InterestingMasks = interestingMasks;
            this.AllTypes = allTypes;
            //init _collisionMask//TODO read from file
            for (int i = 0; i < _collisionMask.Length; i++) {
                _collisionMask[i] = (uint) (~(1 << i));
            }

            // Initial size (metres), initial centre position, minimum node size (metres), looseness
            foreach (var type in allTypes) {
                var boundsTree = new BoundsQuadTree(worldSize, pos, minNodeSize, loosenessval);
                boundsTrees.Add(boundsTree);
            }

            BoundsQuadTree.FuncCanCollide = NeedCheck;
            BoundsQuadTree.funcOnCollide = OnQuadTreeCollision;
        }

        public BoundsQuadTree GetBoundTree(int layer){
            if (layer > boundsTrees.Count || layer < 0) return null;
            return boundsTrees[layer];
        }

        public void AddCollider(ColliderProxy collider){
            GetBoundTree(collider.LayerType).Add(collider, collider.GetBounds());
            id2Proxy[collider.Id] = collider;
        }

        public void RemoveCollider(ColliderProxy collider){
            GetBoundTree(collider.LayerType).Remove(collider);
            id2Proxy.Remove(collider.Id);
        }

        public bool Raycast(int layerType, Ray2D checkRay, out LFloat t, out int id, LFloat maxDistance){
            return GetBoundTree(layerType).Raycast(checkRay, maxDistance, out t, out id);
        }

        public bool Raycast(int layerType, Ray2D checkRay, out LFloat t, out int id){
            return GetBoundTree(layerType).Raycast(checkRay, LFloat.MaxValue, out t, out id);
        }

        public void QueryRegion(int layerType, LVector2 pos, LVector2 size, LVector2 forward, FuncCollision callback){
            Debug.Trace($"QueryRegion layerType:{layerType} pos:{pos} size:{size}  forward:{forward} ");
            //UnityEngine.Debug.Log($"QueryRegion layerType:{layerType} pos:{pos} size:{size}  forward:{forward} ");
            tempCallback = callback;
            _tempSize = size;

            // XOY平面下 的up 是 (0,1)
            _tempForward = new LVector2(0,1);
            _tempPos = pos;
            var radius = size.magnitude;
            var checkBounds = LRect.CreateRect(pos, new LVector2(radius, radius));
            //UnityEngine.Debug.Log($"checkBounds:{checkBounds} ");
            GetBoundTree(layerType).CheckCollision(ref checkBounds, _CheckRegionOBB);
        }

        public void QueryRegion(int layerType, LVector2 pos, LFloat radius, FuncCollision callback){
            Debug.Trace($"QueryRegion layerType:{layerType} pos:{pos} radius:{radius} ");
            tempCallback = callback;
            _tempPos = pos;
            _tempRadius = radius;
            GetBoundTree(layerType).CheckCollision(pos, radius, _CheckRegionCircle);
        }

        //temp attris for call back
        private FuncCollision tempCallback;
        private LVector2 _tempSize;
        private LVector2 _tempForward;
        private LVector2 _tempPos;
        private LFloat _tempRadius;

        private void _CheckRegionOBB(ColliderProxy obj){
            Debug.Trace($"ColliderProxy _CheckRegionOBB {obj.Id} trans{obj.Transform2D} col{obj.Prefab}");
            if (CollisionHelper.CheckCollision(obj.Prefab, obj.Transform2D, _tempPos, _tempSize, _tempForward)) {
                tempCallback(obj);
            }
        }

        private void _CheckRegionCircle(ColliderProxy obj){
            Debug.Trace($"ColliderProxy _CheckRegionCircle {obj.Id} trans{obj.Transform2D} col{obj.Prefab}");
            if (CollisionHelper.CheckCollision(obj.Prefab, obj.Transform2D, _tempPos, _tempRadius)) {
                tempCallback(obj);
            }
        }


        //public List<>
        public void DoUpdate(LFloat deltaTime){
            tempLst.Clear();
            //deal layer
            foreach (var pair in BoundsQuadTreeNode.obj2Node) {
                var val = pair.Key;
                if (!val.IsStatic) {
                    val.DoUpdate(deltaTime);
                    // 不应该只把移动了的物体加入tempList，应该对于所有物体都进行四叉树检测
                    //否则可能会导致：碰撞函数的触发多次或不触发
                    //原逻辑: 只对移动了的物体进行碰撞检测，NotifyCollisionEvent中调用两个物体的碰撞函数(有bug)
                    //现逻辑：所有移动和不动的物体都进行碰撞检测，NotifyCollisionEvent中只调用第一个物体的碰撞函数

                    //if (val.IsMoved) {
                    //    val.IsMoved = false;
                    //    tempLst.Add(val);
                    //}

                    tempLst.Add(val);
                }
            }

            //swap
            var temp = _prePairs;
            _prePairs = _curPairs;
            _curPairs = temp;
            _curPairs.Clear();
            ////class version 1.41ms
            Profiler.BeginSample("UpdateObj");
            foreach (var val in tempLst) {
                val.IsMoved = false;
                var bound = val.GetBounds();
                var boundsTree = GetBoundTree(val.LayerType);
                boundsTree.UpdateObj(val, bound);
            }

            Profiler.EndSample();
            ////0.32~0.42ms
            Profiler.BeginSample("CheckCollision");
            foreach (var val in tempLst) {
                val.IsMoved = false;
                var bound = val.GetBounds();
                for (int i = 0; i < LayerCount; i++) {
                    if (InterestingMasks[val.LayerType * LayerCount + i]) {
                        var boundsTree = GetBoundTree(i);
                        //原逻辑：
                        //CheckCollision只检测 当前帧中，移动了的物体是否发生了碰撞
                        //若移动了的物体发生了碰撞，那么将碰撞对 更新到 _curPairs中
                        //现逻辑：
                        //CheckCollision检测 所有的物体是否发生碰撞，并更新碰撞对 到  _curPairs中
                        boundsTree.CheckCollision(val, bound);
                    }
                }
            }

            Profiler.EndSample();
            Profiler.BeginSample("CheckLastFrameCollison");

            //原逻辑：
            // 此时_curPairs中存储的是当前帧中 (移动 且 发生碰撞的)  碰撞对
            // 从_prePairs中删除掉这些后，_prePairs中剩下的就是 ：
            // 1. (上一帧中发生了碰撞 且 这一帧移动了但没再碰撞的）碰撞对，这些应该调用exit
            // 2. (上一帧中发生了碰撞 且 这一帧没有移动的）碰撞对，这些应该调用stay

            //现逻辑：
            // 此时_curPairs中存储的是当前帧中所有 发生碰撞的 碰撞对
            // 从_prePairs中删除掉这些后，_prePairs中剩下的就只有 ：
            // 1. (上一帧中发生了碰撞 且 没再碰撞的) 碰撞对，这些应该调用exit
            // 因此不会再调用下面这个stay
            foreach (var pairId in _curPairs) {
                _prePairs.Remove(pairId);
            }

            //check stay leave event
            foreach (var idPair in _prePairs) {
                var a = GetCollider((int) (idPair >> 32));
                var b = GetCollider((int) (idPair & 0xffffffff));
                if (a == null || b == null) {
                    continue;
                }

                bool isCollided = CollisionHelper.CheckCollision
                    (a.Prefab, a.Transform2D, b.Prefab, b.Transform2D);
                if (isCollided) {
                    _curPairs.Add(idPair);
                    //现逻辑: 不会调用
                    NotifyCollisionEvent(a, b, ECollisionEvent.Stay);
                }
                else {
                    NotifyCollisionEvent(a, b, ECollisionEvent.Exit);
                }
            }

            Profiler.EndSample();
        }

        //检测 如果是同一个LayerType 则返回false
        bool NeedCheck(ColliderProxy a, ColliderProxy b){
            //var val = _collisionMask[a.LayerType];
            //var val2 = 1 << b.LayerType;
            //var needCheck = (val & val2) != 0;
            //return needCheck;

            return true;
        }

        public void OnQuadTreeCollision(ColliderProxy a, ColliderProxy b){
            var pairId = (((long)a.Id) << 32) + b.Id;
            if (_curPairs.Contains(pairId)) return;
            bool isCollided = CollisionHelper.CheckCollision
                (a.Prefab, a.Transform2D, b.Prefab, b.Transform2D);
            if (isCollided)
            {
                _curPairs.Add(pairId);
                var type = _prePairs.Contains(pairId) ? ECollisionEvent.Stay : ECollisionEvent.Enter;
                NotifyCollisionEvent(a, b, type);
            }
        }

        //为了防止多次触发碰撞函数，在NotifyCollisionEvent中，只调用a的碰撞函数
        //因为在DoUpdate执行碰撞检测player时,会执行NotifyCollisionEvent( player , enermy , ...)
        //同时在检测enermy时，也会执行NotifyCollisionEvent (enermy , player , ...)
        //如果在NotifyCollisionEvent中分别调用a 和 b的碰撞函数，那么一次碰撞，将会每人都触发两次碰撞函数
        public void NotifyCollisionEvent(ColliderProxy a, ColliderProxy b, ECollisionEvent type){
            
            //UnityEngine.Debug.Log($"{a.Entity}被发生碰撞:{type}  , event:  {a.OnTriggerEvent} trans:{a.Entity.transform} ");
            if (!a.IsStatic) {
                a.OnTriggerEvent?.Invoke(b, type);
                //TriggerEvent(a, b, type);
            }

            //if (!b.IsStatic)
            //{
            //    b.OnTriggerEvent?.Invoke(a, type);
            //    //TriggerEvent(b, a, type);
            //}

            //另一种回调碰撞函数的方式，用不到
            //funcGlobalOnTriggerEvent?.Invoke(a, b, type);
        }

        void TriggerEvent(ColliderProxy a, ColliderProxy other, ECollisionEvent type){
            switch (type) {
                case ECollisionEvent.Enter: {
                    a.OnLPTriggerEnter(other);
                    break;
                }
                case ECollisionEvent.Stay: {
                    a.OnLPTriggerStay(other);
                    break;
                }
                case ECollisionEvent.Exit: {
                    a.OnLPTriggerExit(other);
                    break;
                }
            }
        }

        public static void TriggerEvent(ILPTriggerEventHandler a, ColliderProxy other, ECollisionEvent type){
            switch (type) {
                case ECollisionEvent.Enter: {
                    a.OnLPTriggerEnter(other);
                    break;
                }
                case ECollisionEvent.Stay: {
                    a.OnLPTriggerStay(other);
                    break;
                }
                case ECollisionEvent.Exit: {
                    a.OnLPTriggerExit(other);
                    break;
                }
            }
        }

        public int ShowTreeId { get; set; }

        public void DrawGizmos(){
            var boundsTree = GetBoundTree(1);
            if (boundsTree == null) return;
#if UNITY_EDITOR
            boundsTree.DrawAllBounds(); // Draw node boundaries
            boundsTree.DrawAllObjects(); // Draw object boundaries
            boundsTree.DrawCollisionChecks(); // Draw the last *numCollisionsToSave* collision check boundaries
#endif
            // pointTree.DrawAllBounds(); // Draw node boundaries
            // pointTree.DrawAllObjects(); // Mark object positions


            var boundsTree2 = GetBoundTree(2);
            if (boundsTree2 == null) return;
#if UNITY_EDITOR
            boundsTree2.DrawAllBounds(); // Draw node boundaries
            boundsTree2.DrawAllObjects(); // Draw object boundaries
            boundsTree2.DrawCollisionChecks(); // Draw the last *numCollisionsToSave* collision check boundaries
#endif

        }
    }
}