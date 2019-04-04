using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;



namespace ExampleLib.Server
{

    public class Server
    {
        private class ConnectedClient
        {
            public int ID { get; }
            private TcpClient _client;
            private StreamReader _streamReader;

            public delegate void NetDataEventHandler(object sender, NetDataEventArgs e);

            public event NetDataEventHandler NetData;

            public virtual void OnNetData(NetDataEventArgs e)
            {
                NetData?.Invoke(this, e);
            }

            public class NetDataEventArgs
            {
                public NetDataEventArgs(int id, string message)
                {
                    ID = id;
                    Message = message;
                }

                public string Message { get; }
                public int ID { get; }
            }

            public ConnectedClient(int id, TcpClient client)
            {
                ID = id;
                _client = client;
            }

            public void BeginRead()
            {
                _streamReader = new StreamReader(_client.GetStream());
                _streamReader.ReadLineAsync().ContinueWith(LineRead);
            }

            private void LineRead(Task<string> obj)
            {
                OnNetData(new NetDataEventArgs(ID, obj.Result));
                _streamReader.ReadLineAsync().ContinueWith(LineRead);
            }
        }

        public Server(int numClients, int port) : this(numClients, new List<int>(new[] { port })) { }

        private CancellationTokenSource _cts;

        private readonly List<int> _portsToListen;
        private List<Thread> _listenThreads;
//        private Thread _workThread;

        private readonly Queue<int> _clientIDs;
        private readonly object _clientIDLock = new object();

        public int NumClients { get; }
        public int NumClientsConnected
        {
            get { lock (_connectedClientsModifyLock) return _connectedClients.Count; }
        }

        private readonly Dictionary<int, ConnectedClient> _connectedClients;
        private readonly object _connectedClientsModifyLock = new object();

        public Server(int numClients, List<int> portsToListen)
        {
            _portsToListen = portsToListen;
            _clientIDs = new Queue<int>(numClients);
            _connectedClients = new Dictionary<int, ConnectedClient>(numClients);

            for (int i = 0; i < numClients; i++)
            {
                _clientIDs.Enqueue(i);
            }
            NumClients = numClients;
        }

        private int GetID()
        {
            lock (_clientIDLock)
            {
                return _clientIDs.Dequeue();
            }
        }

        public void Start()
        {
            _cts = new CancellationTokenSource();
            _listenThreads = new List<Thread>(_portsToListen.Count);
            foreach (var port in _portsToListen)
            {
                var listenThread = new Thread(DoListen);
                listenThread.Start(new object[] { new TcpListener(IPAddress.Any, port), _cts.Token });
                _listenThreads.Add(listenThread);
            }
        }

        public void Stop()
        {
            _cts.Cancel();
            foreach (var thread in _listenThreads)
            {
                thread.Join(10);
                if (thread.IsAlive)
                {
                    thread.Abort();
                }
            }
        }

        private void DoListen(object state)
        {
            var listener = ((object[])state)[0] as TcpListener;
            var cToken = (CancellationToken)((object[])state)[1];
            listener.Start();
            Trace.WriteLine($"Now listening on: {listener.LocalEndpoint}");
            while (!cToken.IsCancellationRequested)
            {
                var client = listener.AcceptTcpClient();
                if (client.Connected)
                {
                    AddnewClient(client);
                }
            }
        }

        private void AddnewClient(TcpClient client)
        {
            ConnectedClient cClient = new ConnectedClient(GetID(), client);
            cClient.NetData += ClientReceiveData;
            lock (_connectedClientsModifyLock)
            {
                Trace.WriteLine($"Client[{cClient.ID}] connecting from: {client.Client.RemoteEndPoint} bound to {client.Client.LocalEndPoint}");
                _connectedClients.Add(cClient.ID, cClient);
                cClient.BeginRead();
            }
        }

        private void ClientReceiveData(object sender, ConnectedClient.NetDataEventArgs e)
        {
            
            if (string.IsNullOrEmpty(e.Message) == false)
            {
                
              Trace.WriteLine($" Client {e.ID}: {e.Message}");

            }
        }
    }
}



