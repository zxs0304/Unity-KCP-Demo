using System.Collections.Generic;
using System.Linq;
using Lockstep.Math;
using UnityEngine;

namespace LockstepTutorial {
    public class PingMono : UnityEngine.MonoBehaviour {
        private float _guiTimer;
        public List<float> delays => GameManager.Delays;

        private void Update(){
            if (delays == null) return;
            _guiTimer += Time.deltaTime;
            if (_guiTimer > 0.5f) {
                _guiTimer = 0;
                GameManager.PingVal = (int) (delays.Sum() * 1000 / LMath.Max(delays.Count, 1));
                delays.Clear();
            }
        }

        private void OnGUI(){
            // 创建一个 GUIStyle 对象
            GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
            // 设置字体大小
            labelStyle.fontSize = 24;  // 你可以根据需要调整大小
            // 设置字体颜色
            labelStyle.normal.textColor = Color.red;  // 你可以选择任何颜色
            GUI.Label(new Rect(0, 0, 100, 100), $"Ping: {GameManager.PingVal}ms",labelStyle);
        }
    }
}