using System;
using System.Net;
using System.Net.Sockets;
using System.Text;


namespace UDP
{
    public class UDPSocket
    {
        /*
            UDPSocket class:
            
            This script is a generic UDP socket script, using .NET's Sockets package. When creating a new  UDPSocket instance, the user can either call the 
            server or client method. A server binds the passed IP and port to the .NET socket that was made when the UDPSocket instance was made, and a client
            attempts to connect to the passed IP and port, using the created default .NET socket. Both methods then begin to listen for incoming messages.
            NOTE: Listening for incoming messages listens for any incoming messages, however when a client is instantiated, they are not bound to an IP or port,
            meaning clients are unable to receive incoming messages, they can only send messages.
            When a message is received, the OnMessageRead delegate is called, which is what should be used to parse/process incoming messages from the script
            where the instance is instantiated.
            Two of these four accept only one value, either a string or a byte array, which will be sent to the connected socket. The other two accept three values,
            two of which are the IP and port allowing for sending either a string or byte array to a specific address. Client sockets can use the Send function that
            sends to the connected socket, however server sockets cannot, because they do not connect to another socket.
        */
    
        private Socket _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        private const int bufSize = 8 * 1024;
        private State state = new State();
        private EndPoint epFrom = new IPEndPoint(IPAddress.Any, 0);
        private AsyncCallback recv = null;
        public delegate void ProcessMessage(byte[] m);
        public ProcessMessage OnMessageRead;

        public class State
        {
            public byte[] buffer = new byte[bufSize];
        }

        public void Server(string address, int port)
        {
            _socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
            _socket.Bind(new IPEndPoint(IPAddress.Parse(address), port));
            Receive();
        }

        public void Client(string address, int port)
        {
            _socket.Connect(IPAddress.Parse(address), port);
            Receive();            
        }

        public void Send(string text)
        {
            byte[] data = Encoding.ASCII.GetBytes(text);
            _socket.BeginSend(data, 0, data.Length, SocketFlags.None, (ar) =>
            {
                State so = (State)ar.AsyncState;
                int bytes = _socket.EndSend(ar);
            }, state);
        }

        public void Send(byte[] data)
        {
            _socket.BeginSend(data, 0, data.Length, SocketFlags.None, (ar) =>
            {
                State so = (State)ar.AsyncState;
                int bytes = _socket.EndSend(ar);
            }, state);
        }

        public void Send(string text, string ip, int port)
        {
            EndPoint ep = new IPEndPoint(IPAddress.Parse(ip), port);
            byte[] data = Encoding.ASCII.GetBytes(text);
            _socket.BeginSendTo(data, 0, data.Length, SocketFlags.None, ep, (ar) =>
            {
                State so = (State)ar.AsyncState;
                int bytes = _socket.EndSend(ar);
            }, state);
        }

        public void Send(byte[] data, string ip, int port)
        {
            EndPoint ep = new IPEndPoint(IPAddress.Parse(ip), port);
            _socket.BeginSendTo(data, 0, data.Length, SocketFlags.None, ep, (ar) =>
            {
                State so = (State)ar.AsyncState;
                int bytes = _socket.EndSend(ar);
            }, state);
        }

        private void Receive()
        {
            _socket.BeginReceiveFrom(state.buffer, 0, bufSize, SocketFlags.None, ref epFrom, recv = (ar) =>
            {
                State so = (State)ar.AsyncState;
                int bytes = _socket.EndReceiveFrom(ar, ref epFrom);
                _socket.BeginReceiveFrom(so.buffer, 0, bufSize, SocketFlags.None, ref epFrom, recv, so);
                OnMessageRead(so.buffer);
            }, state);
        }

        private void OnApplicationQuit() {
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Disconnect(true);
        }
    }
}