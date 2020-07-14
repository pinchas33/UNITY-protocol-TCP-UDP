﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class publisher : MonoBehaviour
{
    UDP.UDPSocket socket_server = new UDP.UDPSocket();
    UDP.UDPSocket socket_client1 = new UDP.UDPSocket();
    UDP.UDPSocket socket_clinet2 = new UDP.UDPSocket();
    Queue<PrimitiveType> tospawn = new Queue<PrimitiveType>();
    Vector3 POSITION = new Vector3();
    string mes;

    private void Awake() {
        socket_server.Server("127.0.0.1", 8888);
        socket_server.OnMessageRead += ProcessMessage;
        socket_client1.Client("127.0.0.1", 8888);
        socket_clinet2.Client("127.0.0.1", 8888);
    }
    
    private void Start() {
        // socket_client1.Send("0");
    }

    private void Update() {

        if (Input.GetKeyDown(KeyCode.A))
        {
            socket_client1.Send("0$1,2,3");
            socket_clinet2.Send("3$1,2,5");
        }

        try
        {
            Debug.Log(mes);
            PrimitiveType prim = tospawn.Dequeue();
            GameObject a = GameObject.CreatePrimitive(prim);
            mes.Replace(mes.Substring(0, mes.IndexOf('$')), "");    
            POSITION.x = int.Parse(mes.Substring(0, mes.IndexOf(',')));
            mes.Replace(mes.Substring(0, mes.IndexOf(',')), "");
            POSITION.y = int.Parse(mes.Substring(0, mes.IndexOf(',')));
            mes.Replace(mes.Substring(0, mes.IndexOf(',')), "");
            POSITION.z = int.Parse(mes.Substring(0, mes.IndexOf(',')));
            a.transform.position = POSITION;
        }
        catch (InvalidOperationException)
        {
            Debug.Log("no shapes to spawn");
        }
    }

    private void ProcessMessage(byte[] message){
        mes = System.Text.Encoding.UTF8.GetString(message);
        int idxOfShape = int.Parse(mes.Substring(0, mes.IndexOf('$')));


        string[] shapes = {"Sphere", "Cube", "Cylinder", "Capsule"};
        PrimitiveType[] types = {PrimitiveType.Sphere, PrimitiveType.Cube, PrimitiveType.Cylinder, PrimitiveType.Capsule};
        tospawn.Enqueue(types[idxOfShape]);
    }
}
