using System;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using Lockstep.Math;
using Lockstep.Util;
namespace Lockstep.Collision2D {
    public static class TransformHashCodeExtension {
        public static int GetHash(this CTransform2D val){
            return val.Pos3.GetHash() * 13
                   + val.deg.GetHash() * 31;
  
        }
    }
}
namespace Lockstep.Collision2D {
    [Serializable]
    public class CTransform2D {

        //在2D xOy下， pos的y值 就是高度值 ，不再单独设立一个y变量
        public LVector2 pos;

        public LFloat deg; //same as Unity CW deg(up) =0

        public LVector2 forward {
            // 对于3D下，forward (1,1)对应于世界的x轴和z轴，另外矩形碰撞体在XOZ平面上，所以forward也即是矩形碰撞体的 上 向量
            // 而对于2D下，forward只可能是(-1,0)和(1,0),对应于世界的x轴和y轴，此时forward不再是矩形碰撞体的上向量，而是矩形碰撞体的左向量或者右向量，(0,1)才是上向量
            get {
                LFloat s, c;
                var ccwDeg = -deg + 90;
                LMath.SinCos(out s, out c, LMath.Deg2Rad * ccwDeg);
                return new LVector2(c, LFloat.zero).normalized;
            }
            set => deg = ToDeg(value);
        }

        public static LFloat ToDeg(LVector2 value)
        {
            var ccwDeg = LMath.Atan2(value.y, value.x) * LMath.Rad2Deg;
            var deg = 90 - ccwDeg;
            return AbsDeg(deg);
        }
        public static void ToDeg_2D(LVector2 value,ref LFloat targetDegree)
        {
            if (value == LVector2.zero)
            {
                return;
            }
            var ccwDeg = LMath.Atan2(value.y, value.x) * LMath.Rad2Deg;
            var deg = 90 - ccwDeg;
            targetDegree = AbsDeg(deg);
        }

        public static LFloat TurnToward(LVector2 targetPos, LVector2 currentPos, LFloat cursDeg, LFloat turnVal,
            out bool isLessDeg){
            var toTarget = (targetPos - currentPos).normalized;
            var toDeg = CTransform2D.ToDeg(toTarget);
            return TurnToward(toDeg, cursDeg, turnVal, out isLessDeg);
        }
        public static LFloat TurnToward(LFloat toDeg, LFloat cursDeg, LFloat turnVal,
            out bool isLessDeg){
            var curDeg = CTransform2D.AbsDeg(cursDeg);
            var diff = toDeg - curDeg;
            var absDiff = LMath.Abs(diff);
            isLessDeg = absDiff < turnVal;
            if (isLessDeg) {
                return toDeg;
            }
            else {
                if (absDiff > 180) {
                    if (diff > 0) {
                        diff -= 360;
                    }
                    else {
                        diff += 360;
                    }
                }

                return curDeg + turnVal * LMath.Sign(diff);
            }
        }
        public static LFloat AbsDeg(LFloat deg){
            var rawVal = deg._val % ((LFloat) 360)._val;
            return new LFloat(true, rawVal);
        }

        public CTransform2D(){ }

        public CTransform2D(LVector2 pos) : this(pos , LFloat.zero){ }

        public CTransform2D(LVector2 pos,  LFloat deg){
            this.pos = pos;
            this.deg = deg;
        }


        public void Reset(){
            pos = LVector2.zero;
            deg = LFloat.zero;
        }

        public LVector2 TransformPoint(LVector2 point){
            return pos + TransformDirection(point);
        }

        public LVector2 TransformVector(LVector2 vec){
            return TransformDirection(vec);
        }

        public LVector2 TransformDirection(LVector2 dir){
            var x = forward;
            var y = new LVector2(0,1);
            return dir.x * x + dir.y * y;
        }

        //TEST 2D 
        //public static Transform2D operator +(CTransform2D a, CTransform2D b){
        //    return new Transform2D {pos = a.pos + b.pos, y = a.y + b.y, deg = a.deg + b.deg};
        //}
        public static Transform2D operator +(CTransform2D a, CTransform2D b)
        {
            return new Transform2D { pos = a.pos + b.pos, deg = a.deg + b.deg };
        }

        public LVector3 Pos3 {

            get => new LVector3(pos.x,pos.y,LFloat.zero);
            set
            {
                pos = new LVector2(value.x, value.y);
            }
        }

        public override string ToString(){
            return $"(deg:{deg} pos:{pos})";
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = NativeHelper.STRUCT_PACK)]
    public unsafe struct Transform2D {
        public LVector2 pos;
        public LFloat y;
        public LFloat deg;
    }
}