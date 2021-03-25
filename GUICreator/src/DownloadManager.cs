namespace Oxide.Plugins
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public partial class GUICreator
    {
        private class DownloadManager
        {
            private Queue<Request> Requests = new Queue<Request>();
            private bool loading = false;

            public void Enqueue(Request request)
            {
                Requests.Enqueue(request);
                if(!loading)
                {
                    ServerMgr.Instance.StartCoroutine(Process());
                }
            }

            private IEnumerator Process()
            {
                if(loading || Requests.Count == 0)
                {
                    yield break;
                }
                loading = true;

                Request request = Requests.Dequeue();
                request.ProcessDownload(() =>
                {
                    loading = false;
                    ServerMgr.Instance.StartCoroutine(Process());
                });
            }

            public class Request
            {
                public string SafeName { get; set; }
                public string Url { get; set; }
                public int ImgSizeX { get; set; }
                public int ImgSizeY { get; set; }
                public Action Callback { get; set; }

                public void ProcessDownload(Action finishCallback)
                {
                    var il = new GameObject("WebObject").AddComponent<ImageLoader>();
                    il.StartCoroutine(il.DownloadImage(Url, (b) =>
                    {
                        PluginInstance.ImageLibrary.Call("AddImageData", SafeName, b, (ulong)0, Callback);
                        finishCallback();
#if CoroutineDEBUG
                    PluginInstance.Puts($"completed processing image download: {SafeName}");
#endif

                    }, ImgSizeX, ImgSizeY, finishCallback));
                }
            }
        }            
    }
}