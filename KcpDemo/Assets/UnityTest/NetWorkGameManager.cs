using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Assets.UnityTest.KCPTest;
using Unity.Multiplayer.Playmode;
using UnityEngine;

public class NetWorkGameManager : MonoBehaviour
{
    KCPPlayer myPlayer;
    KCPPlayer remotePlayer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debuger.EnableLog = true;
        if (CurrentPlayer.ReadOnlyTags()[0] == "player1")
        {
            print("Player1初始化");
            myPlayer = new KCPPlayer();
            myPlayer.Init("Player1", 12345, 12346);

        }
        if (CurrentPlayer.ReadOnlyTags()[0] == "player2")
        {
            print("Player2初始化");
            myPlayer = new KCPPlayer();
            myPlayer.Init("Player2", 12346, 12345);
        }
    }

    void Update()
    {

        if (CurrentPlayer.ReadOnlyTags()[0] == "player1")
        {
            GameObject cube = GameObject.Find("Player1");
            if (Input.GetKey(KeyCode.W))
            {
                Vector3 move = new Vector3(0f, 0.1f, 0f);
                myPlayer.SendMessage(move);
                cube.transform.Translate(move);
            }
            if (Input.GetKey(KeyCode.A))
            {
                Vector3 move = new Vector3(-0.1f, 0f, 0f);
                myPlayer.SendMessage(move);
                cube.transform.Translate(move);
            }
            if (Input.GetKey(KeyCode.S))
            {
                Vector3 move = new Vector3(0f, -0.1f, 0f);
                myPlayer.SendMessage(move);
                cube.transform.Translate(move);
            }
            if (Input.GetKey(KeyCode.D))
            {
                Vector3 move = new Vector3(0.1f, 0f, 0);
                myPlayer.SendMessage(move);
                cube.transform.Translate(move);
            }
        }

        if (CurrentPlayer.ReadOnlyTags()[0] == "player2")
        {
            GameObject cube = GameObject.Find("Player2");
            if (Input.GetKey(KeyCode.UpArrow))
            {
                Vector3 move = new Vector3(0f, 0.1f, 0f);
                myPlayer.SendMessage(move);
                cube.transform.Translate(move);
            }
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                Vector3 move = new Vector3(-0.1f, 0f, 0f);
                myPlayer.SendMessage(move);
                cube.transform.Translate(move);
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                Vector3 move = new Vector3(0f, -0.1f, 0f);
                myPlayer.SendMessage(move);
                cube.transform.Translate(move);
            }
            if (Input.GetKey(KeyCode.RightArrow))
            {
                Vector3 move = new Vector3(0.1f, 0f, 0);
                myPlayer.SendMessage(move);
                cube.transform.Translate(move);
            }
        }

        myPlayer.OnUpdate();


    }

    //void OnGUI()
    //{
    //    if (GUILayout.Button("Player1 SendMessage"))
    //    {
    //        myPlayer.SendMessage();
    //    }

    //    //if (GUILayout.Button("Player2 SendMessage"))
    //    //{
    //    //    p2.SendMessage();
    //    //}
    //}
}
