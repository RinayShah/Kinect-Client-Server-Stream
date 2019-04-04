using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Kinect;

namespace ExampleLib
{


    public class Client : IDisposable
    {

        private StreamReader _streamReader;
        private StreamWriter _streamWriter;

        private CancellationTokenSource _ctokenSource;

        public delegate void NetDataEventHandler(object sender, NetDataEventArgs e);

        public event NetDataEventHandler NetData;

        public bool IsConnected { get; private set; }

        public virtual void OnNetData(NetDataEventArgs e)
        {
            NetData?.Invoke(this, e);
        }

        public class NetDataEventArgs
        {
            public NetDataEventArgs(string message)
            {
                Message = message;
            }

            public string Message { get; }
            
        }

        public void Write(String str)
        {
            var task =_streamWriter.WriteLineAsync(str);
            if (string.CompareOrdinal(str, "exit") == 0)
            {
                task.Wait();
                _ctokenSource.Cancel();  
            }
        }


        struct ConnectData
        {
            public ConnectData(TcpClient client, CancellationToken cToken)
            {
                CToken = cToken;
                Client = client;
            }

            public TcpClient Client { get; }
            public CancellationToken CToken { get; }
        }

        public void Start(CancellationToken ctoken, string ip = "192.168.1.137", int port = 10)
        {
            _ctokenSource?.Cancel();
            _ctokenSource = new CancellationTokenSource();
            
            try
            {
                var socketForServer = new TcpClient();
                var token = CancellationTokenSource.CreateLinkedTokenSource(_ctokenSource.Token, ctoken).Token;
                socketForServer.ConnectAsync(ip, port).ContinueWith(AsyncConnect, new ConnectData(socketForServer, token), token);
                IsConnected = true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Failed to connect to server at {ip}:{port}");
                Trace.WriteLine(ex.Message);
            }
        }

        private void AsyncConnect(Task connectTask, object data)
        {
            var connectData = (ConnectData) data;

            if (connectTask.Status != TaskStatus.RanToCompletion)
            {
                Trace.Write("Failed to connect" + (connectTask.Exception == null ?  "" : connectTask.Exception.Message + Environment.NewLine));
                // Optional
                if (connectTask.Exception != null)

                throw connectTask.Exception;
                return;
            }

            var networkStream = connectData.Client.GetStream();
            _streamReader = new StreamReader(networkStream);
            _streamWriter = new StreamWriter(networkStream)
            {
                AutoFlush = true
            };


            _ctokenSource = new CancellationTokenSource();
            
            _streamReader.ReadLineAsync().ContinueWith(ContinuationAction, connectData.CToken, connectData.CToken);


            Trace.WriteLine("**This is client program who is connected to localhost on port No:10**");
        }

        private void ContinuationAction(Task<string> messageTask, object ctoken)
        {
            OnNetData(new NetDataEventArgs(messageTask.Result));
           _streamReader.ReadLineAsync().ContinueWith(ContinuationAction, ctoken, (CancellationToken)ctoken);
        }

        public void Dispose()
        {
            _streamReader?.Dispose();
            _streamWriter?.Dispose();
        }
    }
}
