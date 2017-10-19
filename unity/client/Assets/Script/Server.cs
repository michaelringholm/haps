using UnityEngine;
using System;
using HapsCommands;
using UnityEngine.Networking;
using System.Collections;
using System.Linq;

public interface IServer
{
//    IEnumerator RequestDonees(Action<string> cb);
//    IEnumerator CreateUser(string info, Action<string> cb);
    IEnumerator RequestSequence(string userId, Action<ItemSequence> cb);
//    IEnumerator ClaimWin(string userId, string latestSequence, string doneeId, Action<string> cb);
}

public class Server : IServer
{
//    const string ServerAddress = "localhost";
//    const string ServerAddress = "https://tihe48ll6k.execute-api.eu-central-1.amazonaws.com/Prod/";

//    private float requestStartTime;
    private Action<bool> OnSuccess;

    public Server(Action<bool> onSuccess)
    {
        OnSuccess = onSuccess;
    }

    //    private IEnumerator SendWithRetry(string request, Action<string> cb, bool first = false)
    //    {
    //        while (true)
    //        {
    //            var webReq = UnityWebRequest.Get(string.Format("http://{0}/{1}", ServerAddress, request));
    //            yield return webReq.Send();

    //            if (webReq.isNetworkError)
    //            {
    //                Debug.Log("Web request error: " + webReq.error);
    //                OnSuccess(false);
    //                yield return new WaitForSeconds(1);
    //            }
    //            else
    //            {
    //                string s = webReq.downloadHandler.text;
    //                var lines = s.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
    //                var line = lines.Where(aa => aa.StartsWith("a:")).FirstOrDefault().Trim();
    //                line = line.Substring(2); // skip 'a:'
    //                float requestTotalTime = Time.realtimeSinceStartup - requestStartTime;
    //                Debug.LogFormat("Response ({0}ms): {1}", Mathf.RoundToInt(requestTotalTime * 1000), s);

    //                OnSuccess(true);
    //                cb(line);
    //                break;
    //            }
    //        }
    //    }

    //    public IEnumerator CreateUser(string info, Action<string> cb)
    //    {
    //        var cmd = new CreateUser
    //        {
    //            info = info
    //        };

    //        var str = JsonUtility.ToJson(cmd);
    //        yield return SendWithRetry(str, cb, true);
    //    }

    //    public IEnumerator RequestDonees(Action<string> cb)
    //    {
    //        throw new NotImplementedException();
    //    }

    public IEnumerator RequestSequence(string userId, Action<ItemSequence> cb)
    {
        //{ "fw":[],"s":-1699514571,"suc":10,"f":4294967295,"did":1,
        //"d":[{"icon":"A","ch":0.2409638554216868},{"icon":"B","ch":0.14056224899598396},{"icon":"C","ch":0.10040160642570284},{"icon":"D","ch":0.2409638554216868},{"icon":"E","ch":0.14056224899598396},{"icon":"F","ch":0.10040160642570284},{"icon":"G","ch":0.020080321285140569},{"icon":"H","ch":0.010040160642570285},{"icon":"I","ch":0.0040160642570281138},{"icon":"J","ch":0.0020080321285140569}]}
        var seq = new ItemSequence();
        seq.s = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        seq.suc = 1000;
        seq.f = 0;
        seq.did = 0;
        seq.AddItem(0, 0.00001f);
        seq.AddItem(1, 0.00001f);
        seq.AddItem(2, 0.00001f);
        seq.AddItem(3, 0.00001f);
        seq.AddItem(4, 0.00001f);
        cb(seq);

        yield break;
    }

    //    public IEnumerator ClaimWin(string userId, string latestSequence, string doneeId, Action<string> cb)
    //    {
    //        var cmd = new ClaimWin();
    //        cmd.uid = userId;
    //        cmd.seq = latestSequence;
    //        cmd.donId = doneeId;
    //        var str = JsonUtility.ToJson(cmd);
    //        yield return SendWithRetry(str, cb, true);
    //    }
}
