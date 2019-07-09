using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Content.Res;
using System.IO;
using Fleck;
using QuestomAssets;
using BeatOn.ClientModels;
using Newtonsoft.Json;
using System.Net.NetworkInformation;
using BeatOn.Core;

namespace BeatOn
{
    public class WebServer : IDisposable
    {
        private const int HTTP_PORT = 50000;
        private const int WEBSOCKET_PORT = 50001;
        private HttpListener _listener;
        private bool _started = false;
        private AssetManager _assetManager;
        private string _baseAssetPath;
        private WebSocketServer _webSocket;
        private List<IWebSocketConnection> _wsClients = new List<IWebSocketConnection>();

        private Dictionary<MessageType, IMessageHandler> _messageHandlers = new Dictionary<MessageType, IMessageHandler>();

        public void AddMessageHandler(MessageType type, IMessageHandler handler)
        {
            _messageHandlers.Add(type, handler);
        }

        public bool IsRunning { get
            {
                //probably should check the web socket along with this, but they get created/killed at the same times
                return (_listener != null && _listener.IsListening);
            }
        }
        public int Port { get; } = HTTP_PORT;

        private string _ip = null;
        public string IP
        {
            get
            {
                if (_ip == null)
                {
                    var interfaces = NetworkInterface.GetAllNetworkInterfaces().Where(x => x.NetworkInterfaceType != NetworkInterfaceType.Loopback && x.OperationalStatus == OperationalStatus.Up).ToList();
                    if (interfaces.Count() < 1)
                    {
                        Log.LogErr("No active network interface found");
                        _ip = "localhost";
                        return "localhost";
                    }
                    else if (interfaces.Count() > 1)
                    {
                        Log.LogErr("More than one active interface?  Picking the first one");
                    }
                    var addr = interfaces.First().GetIPProperties().UnicastAddresses.Where(x => x.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).FirstOrDefault()?.Address?.ToString();
                    if (string.IsNullOrEmpty(addr))
                    {
                        _ip = "localhost";
                    }
                    else
                    {
                        _ip = addr;
                    }
                }
                return _ip;
            }
        }
        public string ListeningOnUrl
        {
            get
            {
                
                if (string.IsNullOrWhiteSpace(IP))
                    return "";
                else
                    return $"http://{IP}:{Port}/";
            }
        }

        public string WebSocketUrl
        {
            get
            {
                if (string.IsNullOrWhiteSpace(IP))
                    return "";
                else
                    return $"ws://{IP}:{WEBSOCKET_PORT}/";
            }
        }

        public SimpleRouter Router { get; private set; } = new SimpleRouter();

        public WebServer(AssetManager assetManager, string baseAssetPath)
        {
            ServicePointManager.DefaultConnectionLimit = 1000;
            _baseAssetPath = baseAssetPath;
            _assetManager = assetManager;
        }

        public bool Started { get => _started; }

        public void Stop()
        {
            Log.LogMsg("Stopping web wever...");
            _started = false;
            try
            {
                _listener.Stop();
                _listener.Close();
            }
            catch
            { }
            _listener = null;

            lock (_wsClients)
            {
                try
                {
                    _wsClients.ForEach(x => x.Close());
                }
                catch
                { }
                _wsClients.Clear();
            }

            try
            {
                _webSocket.Dispose();
                _webSocket = null;
            }
            catch
            { }
            
        }

        public void Start()
        {
            Log.LogMsg("Starting web server...");
            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://*:{Port}/");
            try
            {
                _listener.IgnoreWriteExceptions = true;
                _listener.Start();
                _started = true;
                Log.LogMsg($"HttpListener started on port {Port}");
            //    break;
            }
            catch (Exception ex)
            {
                Log.LogMsg($"Unable to start HttpListener on port {Port}", ex);
                _listener = null;
            }
            //}
            if (_listener == null)
                throw new Exception("Unable to start http listener!");
            StartWebSocketServer();
            _listener.BeginGetContext(HandleRequest, null);
        }

        private void StartWebSocketServer()
        {
            try
            {                
                _webSocket = new WebSocketServer($"ws://0.0.0.0:{WEBSOCKET_PORT}");
                _webSocket.ListenerSocket.NoDelay = true;
                _webSocket.RestartAfterListenError = true;
                _webSocket.Start(socket =>
                {
                    socket.OnOpen = () =>
                    {
                        Log.LogMsg("Client connected.");
                        lock(_wsClients)
                        {
                            _wsClients.Add(socket);
                        }
                    };
                    socket.OnClose = () =>
                    {
                        Log.LogMsg("Client disconnected.");
                        lock(_wsClients)
                        {
                            _wsClients.Remove(socket);
                        }
                    };
                    socket.OnMessage = message =>
                    {
                        HandleWebSocketMessage(message);
                    };
                });
                Log.LogMsg($"Started web socket server on port {WEBSOCKET_PORT}");
            }
            catch (Exception ex)
            {
                Log.LogMsg($"Unable to start WebSocketServer on port {WEBSOCKET_PORT}", ex);
            }
        }

        public void HandleWebSocketMessage(string message)
        {
            try
            {
                var msg = JsonConvert.DeserializeObject<MessageBase>(message);
                if (msg == null)
                    throw new Exception("Unable to deserialize message");
                if (_messageHandlers.ContainsKey(msg.Type))
                    _messageHandlers[msg.Type].HandleMessage(msg);
                else
                    Log.LogErr($"No message handler for type {msg.Type}");
            }
            catch (Exception ex)
            {
                Log.LogErr($"Excepction handling websocket message {message}", ex);
            }
        }

        public void SendMessage(ClientModels.MessageBase message)
        {
            lock (_wsClients)
            {
                if (_wsClients.Count < 1)
                {
                   // Log.LogErr("Websocket message sent from server, but no clients are connected.");
                    return;
                }
                string msgString = JsonConvert.SerializeObject(message);
                byte[] msg = System.Text.Encoding.UTF8.GetBytes(msgString);
                _wsClients.ForEach(x =>
                {
                   try
                   {
                       x.Send(msg);
                   }
                   catch (Exception ex)
                   {
                     //  Log.LogErr($"Exception sending web socket message to {x.ConnectionInfo.ClientIpAddress}", ex);
                   }
                });
            }
            
            
        }

        private void HandleRequest(IAsyncResult iar)
        {
            try
            {
                 var context = _listener.EndGetContext(iar);
                if (_started)
                    _listener.BeginGetContext(HandleRequest, null);
                var method = context.Request.HttpMethod;
                var uriPath = context.Request.Url.AbsolutePath;
                context.Response.AppendHeader("Access-Control-Allow-Origin", "*");
                if (method == "OPTIONS")
                {
                    HandleOptions(context);
                }
                else if (uriPath.StartsWith("/host/"))
                {
                    var path = uriPath.TrimStart('/');
                    path = path.Substring(path.IndexOf('/') + 1);
                    var route = Router.FindRoute(method, path);
                    if (route == null)
                    {
                        context.Response.StatusCode = 404;
                    }
                    else
                    {
                        route.HandleRequest(context);
                    }
                }
                else
                {

                    if (method == "GET")
                    {
                        HandleGet(context);
                    }
                    else if (method == "POST")
                    {
                        HandlePost(context);
                    }
                    else
                    {
                        Log.LogErr($"Got unsupported request for method {context.Request.HttpMethod}");
                        context.Response.StatusCode = 405;
                    }
                }
                try
                {
                    context.Response.Close();
                }
                catch { }
            }
            catch (Exception ex)
            {
                Log.LogErr($"Error in HandleRequest", ex);
            }
        }

        private void HandlePost(HttpListenerContext context)
        {

        }

        private void HandleOptions(HttpListenerContext context)
        {
            context.Response.AppendHeader("Access-Control-Allow-Methods", "POST, PUT, GET, OPTIONS, DELETE");
            context.Response.AppendHeader("Access-Control-Max-Age", "86400");
            context.Response.AppendHeader("Access-Control-Allow-Headers", "Content-Type, Access-Control-Allow-Headers, Authorization, X-Requested-With");
            context.Response.Ok();
            
        }

        private void HandleGet(HttpListenerContext context)
        {
            var req = context.Request;
            var resp = context.Response;
            var uriPath = req.Url.AbsolutePath;
            
            //give the root default index.html
            if (uriPath == "/")
                uriPath = "/index.html";

            try
            {
                using (Stream assetStream = _assetManager.Open(_baseAssetPath + uriPath))
                {
                    resp.ContentType = MimeMap.GetMimeType(uriPath);
                    assetStream.CopyTo(resp.OutputStream);
                }                
            }
            catch
            {
                //if not found, serve up index
                try
                {
                    using (Stream assetStream = _assetManager.Open(_baseAssetPath + "/index.html"))
                    {
                        resp.ContentType = MimeMap.GetMimeType("index.html");
                        assetStream.CopyTo(resp.OutputStream);
                    }
                } catch (Exception ex2) { 
                    Log.LogErr($"Error handling request for path {uriPath}", ex2);
                    resp.StatusCode = 404;
                    return;
                }
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    try
                    {
                        Stop();
                    }
                    catch
                    { }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~WebServer()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}