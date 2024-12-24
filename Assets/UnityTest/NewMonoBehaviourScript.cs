using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse("172.16.174.44"),8080 );
        socket.Bind(ipPoint);
        //接收数据
        //byte[] receiveBytes = new byte[1024];
        //int receiveNum = socket.Receive(receiveBytes);
        //print("收到服务端发来的消息：" + Encoding.UTF8.GetString(receiveBytes, 0, receiveNum));

    }


}
