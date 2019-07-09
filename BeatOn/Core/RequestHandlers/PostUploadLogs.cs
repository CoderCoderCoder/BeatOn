using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using QuestomAssets;

namespace BeatOn.Core.RequestHandlers
{
    public class PostUploadLogs : IHandleRequest
    {
        public PostUploadLogs()
        {
        }

        public void HandleRequest(HttpListenerContext context)
        {
            var req = context.Request;
            var resp = context.Response;

            try
            {
                if (!File.Exists(Constants.SERVICE_LOGFILE))
                    throw new Exception("Log file doesn't exist!");

                string tag = null;
                foreach (string kvp in req.Url.Query.TrimStart('?').Split("&"))
                {
                    var split = kvp.Split('=');
                    if (split.Count() < 1)
                        continue;
                    if (split[0].ToLower() == "tag")
                    {
                        tag = Java.Net.URLDecoder.Decode(split[1]);
                        break;
                    }
                }
                if (string.IsNullOrWhiteSpace(tag))
                {
                    tag = "unknown";
                }
                bool success = false;
                string logstring = null;
                for (int i = 0; i < 3; i++)
                {
                    //try 3 times to open the log file
                    try
                    {
                        
                        byte[] bfr = new byte[4096];
                        using (var f = File.OpenRead(Constants.SERVICE_LOGFILE))
                        {
                            int read = 0;
                            var webreq = new HttpWebRequest(new Uri($"http://logs-01.loggly.com/bulk/{Constants.LOGGLY_TOKEN}/tag/{tag}/"));

                            webreq.ContentType = "text/plain";
                            webreq.Method = "POST";
                            var outStream = webreq.GetRequestStream();
                                
                            while ((read = f.Read(bfr, 0, bfr.Length)) > 0)
                            {
                                outStream.Write(bfr, 0, read);
                            }
                            try
                            {
                                var rsp = webreq.GetResponse() as HttpWebResponse;
                                if (rsp.StatusCode < HttpStatusCode.OK || rsp.StatusCode >= HttpStatusCode.BadRequest)
                                    throw new Exception($"Got non success response {rsp.StatusCode} from log post!");
                                success = true;
                            }
                            catch (Exception ex)
                            {
                                Log.LogErr("Exception posting log!", ex);
                            }
                        }
                        break;
                    }
                    catch
                    {
                        System.Threading.Thread.Sleep(500);
                    }
                }
                if (success)
                {
                    try
                    {
                        File.Delete(Constants.SERVICE_LOGFILE);
                    }
                    catch
                    { }
                }

                if (string.IsNullOrEmpty(logstring))
                    throw new Exception("Failed to read logfile!");
                
                resp.Ok();
            }
            catch (Exception ex)
            {
                Log.LogErr("Exception uploading logs!", ex);
                resp.StatusCode = 500;
            }
        }
    }
}