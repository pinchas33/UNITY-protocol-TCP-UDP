using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Collections;

// ADD DELAY FOR EACH SEND TO AVOID TRYING TO SEND BEFORE CONNECTION IS ESTABLISHED

namespace TCP
{
    public class TCPSocket : MonoBehaviour
    {
        private TcpListener listener;
        private TcpClient client;
        public NetworkStream networkStream;
        private const int bufSize = 8 * 1024;
        private State state = new State();
        private EndPoint epFrom = new IPEndPoint(IPAddress.Any, 0);
        private AsyncCallback recv = null;
        public delegate void ProcessMessage(byte[] m);
        public ProcessMessage OnMessageRead;
        public bool IsConnected = false;

        public class State
        {
            public byte[] buffer = new byte[bufSize];
        }

        public void Server(string ip, int port)
        {
            listener = new TcpListener(IPAddress.Parse(ip), port);
            listener.Start();
            listener.BeginAcceptTcpClient(OnClientConnected, null);
        }


        public void Client(string ip, int port)
        {
            client = new TcpClient(AddressFamily.InterNetwork);
            client.BeginConnect(IPAddress.Parse(ip), port, OnClientConnection, null);
        }

        // WHEN SERVER RECEIVES CONNECTION
        void OnClientConnected(IAsyncResult ar)
        {
            IsConnected = true;
            client = listener.EndAcceptTcpClient(ar);
            networkStream = client.GetStream();
            listener.BeginAcceptTcpClient(OnClientConnected, null);
            Receive();
        }

        // WHEN CLIENT SUCCESSFULLY CONNECTS
        void OnClientConnection(IAsyncResult ar){
            IsConnected = true;
            networkStream = client.GetStream();
            Receive();
        }

        public void Send(string text)
        {
            byte[] data = Encoding.ASCII.GetBytes(text);     
            networkStream.BeginWrite(data, 0, data.Length, (ar) =>
            {
                State so = (State)ar.AsyncState;
                networkStream.EndWrite(ar);
            }, state);
        }

        public void Send(byte[] data)
        {
            networkStream.BeginWrite(data, 0, data.Length, (ar) =>
            {
                State so = (State)ar.AsyncState;
                networkStream.EndWrite(ar);
            }, state);
        }

        private void Receive()
        {
            byte[] buffer = new byte[client.ReceiveBufferSize];
            networkStream.BeginRead(state.buffer, 0, bufSize, recv = (ar) =>
            {
                State so = (State)ar.AsyncState;
                int bytes = networkStream.EndRead(ar);
                networkStream.BeginRead(so.buffer, 0, bufSize, recv, so);
                OnMessageRead(so.buffer);
            }, state);
        }

        private void OnApplicationQuit()
        {
            if(client != null){ 
                client.Close();
            }

            if(listener != null){
                listener.Stop();
            }
        }
    }
}
