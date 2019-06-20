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
using Android.Util;
using Android.Content.Res;
using System.IO;

namespace BeatOn
{

  
    public class WebServer
    {
        private HttpListener _listener;
        private bool _started = false;
        private AssetManager _assetManager;
        private string _baseAssetPath;
        public int Port { get; private set; }

        public SimpleRouter Router { get; private set; } = new SimpleRouter();

        public WebServer(AssetManager assetManager, string baseAssetPath)
        {
            _baseAssetPath = baseAssetPath;
            _assetManager = assetManager;
        }

        public bool Started { get => _started; }

        public void Stop()
        {
            _started = false;
            Port = 0;
            try
            {
                _listener.Stop();
                _listener.Close();
            }
            catch
            { }
            _listener = null;
        }

        public void Start()
        {
            const int startPort = 50000;
            const int endPort = 65535;
            _listener = new HttpListener();
            int port = 0;
            for (port = startPort; port < endPort; port++)
            {
                _listener = new HttpListener();
                _listener.Prefixes.Add($"http://*:{port}/");
                try
                {
                    _listener.Start();
                    _started = true;
                    Port = port;
                    QuestomAssets.Log.LogMsg($"HttpListener started on port {port}");
                    break;
                }
                catch
                {
                    _listener = null;
                }
            }
            if (_listener == null)
                throw new Exception("Unable to start http listener!");

            _listener.BeginGetContext(HandleRequest, null);
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

                if (uriPath.StartsWith("/host/"))
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
                        route(context);
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
                    else if (method == "OPTIONS")
                    {
                        HandleOptions(context);
                    }
                    else
                    {
                        Log.Error(nameof(WebServer), $"Got unsupported request for method {context.Request.HttpMethod}");
                        context.Response.StatusCode = 405;
                    }
                }
                context.Response.Close();
            }
            catch (Exception ex)
            {
                Log.Error(nameof(WebServer), $"Error in HandleRequest: {ex.Message} {ex.StackTrace}");
                Stop();
            }
        }
        private void HandlePost(HttpListenerContext context)
        {

        }

        private void HandleOptions(HttpListenerContext context)
        {

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
            catch (Exception ex)
            {
                Log.Error(nameof(WebServer), $"Error handling request for path {uriPath}: {ex.Message} {ex.StackTrace}");
                resp.StatusCode = 404;
                return;
            }
            

            
        }

        
    }

   
}