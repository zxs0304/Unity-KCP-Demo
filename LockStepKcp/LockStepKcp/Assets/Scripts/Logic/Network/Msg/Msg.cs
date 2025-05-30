using System;
using Lockstep.Network;
using Lockstep.Logging;
using Lockstep.Math;
using Lockstep.Serialization;
using IMessage = Lockstep.Network.IMessage;

namespace Lockstep.Logic {
    public class FrameInput : BaseFormater {
        public int tick;
        public PlayerInput[] inputs;

        public override void Serialize(Serializer writer){
            writer.Write(tick);
            writer.Write(inputs);
        }

        public override void Deserialize(Deserializer reader){
            tick = reader.ReadInt32();
            inputs = reader.ReadArray(inputs);
        }

        public FrameInput Clone(){
            var tThis = this;
            var val = new FrameInput() {tick = tThis.tick};
            if (tThis.inputs == null) return val;
            val.inputs = new PlayerInput[tThis.inputs.Length];
            for (int i = 0; i < val.inputs.Length; i++) {
                val.inputs[i] = tThis.inputs[i].Clone();
            }

            return val;
        }
    }


    [Serializable]
    public class PlayerServerInfo : BaseFormater {
        public string name;
        public int Id;
        public int localId;
        public LVector3 initPos;
        public LFloat initDeg;
        public int PrefabId;


        public override void Serialize(Serializer writer){
            writer.Write(initPos);
            writer.Write(initDeg);
            writer.Write(PrefabId);
        }

        public override void Deserialize(Deserializer reader){
            initPos = reader.ReadLVector3();
            initDeg = reader.ReadLFloat();
            PrefabId = reader.ReadInt32();
        }

        //other infos...
    }

    public class PlayerInput : BaseFormater {
        public LVector2 mousePos;
        public LVector2 inputUV;
        public bool wantExit;
        public int skillId;
        public bool isJump;

        public override void Serialize(Serializer writer){
            writer.Write(mousePos);
            writer.Write(inputUV);
            writer.Write(wantExit);
            writer.Write(skillId);
            writer.Write(isJump);
        }

        public override void Deserialize(Deserializer reader){
            mousePos = reader.ReadLVector2();
            inputUV = reader.ReadLVector2();
            wantExit = reader.ReadBoolean();
            skillId = reader.ReadInt32();
            isJump = reader.ReadBoolean();
        }

        public PlayerInput Clone(){
            var tThis = this;
            return new PlayerInput() {
                mousePos = tThis.mousePos,
                inputUV = tThis.inputUV,
                wantExit = tThis.wantExit,
                skillId = tThis.skillId,
                isJump = tThis.isJump,
            };
        }
    }
    
}


namespace Lockstep.Logic {
    public enum EMsgType {
        JoinRoom,
        QuitRoom,
        PlayerInput,
        FrameInput,
        StartGame,
        HashCode
    }

    public class Msg_HashCode : BaseFormater, IMessage {
        public ushort opcode { get; set; } = (ushort) EMsgType.HashCode;
        public int tick;
        public int hash;

        public override void Serialize(Serializer writer){
            writer.Write(tick);
            writer.Write(hash);
        }

        public override void Deserialize(Deserializer reader){
            tick = reader.ReadInt32();
            hash = reader.ReadInt32();
        }
    }

    public class Msg_JoinRoom : BaseFormater, IMessage {
        public ushort opcode { get; set; } = (ushort) EMsgType.JoinRoom;
        public string name;

        public override void Serialize(Serializer writer){
            writer.Write(name);
        }

        public override void Deserialize(Deserializer reader){
            name = reader.ReadString();
        }
    }

    public class Msg_QuitRoom : BaseFormater, IMessage {
        public ushort opcode { get; set; } = (ushort) EMsgType.QuitRoom;
        public int val;

        public override void Serialize(Serializer writer){
            writer.Write(val);
        }

        public override void Deserialize(Deserializer reader){
            val = reader.ReadInt32();
        }
    }


    public class Msg_PlayerInput : BaseFormater, IMessage {
        public ushort opcode { get; set; } = (ushort) EMsgType.PlayerInput;
        public int tick;
        public PlayerInput input;

        public override void Serialize(Serializer writer){
            writer.Write(tick);
            writer.Write(input);
        }

        public override void Deserialize(Deserializer reader){
            tick = reader.ReadInt32();
            input = reader.ReadRef(ref input);
        }
    }

    public class Msg_StartGame : BaseFormater, IMessage {
        public ushort opcode { get; set; } = (ushort) EMsgType.StartGame;
        public int mapId;
        public int localPlayerId;
        public PlayerServerInfo[] playerInfos;

        public override void Serialize(Serializer writer){
            writer.Write(mapId);
            writer.Write(localPlayerId);
            writer.Write(playerInfos);
        }

        public override void Deserialize(Deserializer reader){
            mapId = reader.ReadInt32();
            localPlayerId = reader.ReadInt32();
            playerInfos = reader.ReadArray(playerInfos);
        }
    }

    public class Msg_FrameInput : BaseFormater, IMessage {
        public ushort opcode { get; set; } = (ushort) EMsgType.FrameInput;
        public FrameInput input;

        public override void Serialize(Serializer writer){
            writer.Write(input);
        }

        public override void Deserialize(Deserializer reader){
            input = reader.ReadRef(ref input);
        }
    }
}