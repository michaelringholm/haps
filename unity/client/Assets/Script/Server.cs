using UnityEngine;
using System;
using Commands;
using UnityEngine.Networking;
using System.Collections;
using System.Linq;

public interface IServer
{
    IEnumerator RequestDonees(Action<string> cb);
    IEnumerator CreateUser(string info, Action<string> cb);
    IEnumerator RequestSequence(string userId, Action<string> cb);
    IEnumerator ClaimWin(string userId, string latestSequence, string doneeId, Action<string> cb);
}

public class Server : IServer
{
//    const string ServerAddress = "localhost";
    const string ServerAddress = "https://tihe48ll6k.execute-api.eu-central-1.amazonaws.com/Prod/";

    private float requestStartTime;
    private Action<bool> OnSuccess;

    public Server(Action<bool> onSuccess)
    {
        OnSuccess = onSuccess;
    }

    private IEnumerator SendWithRetry(string request, Action<string> cb, bool first = false)
    {
        while (true)
        {
            var webReq = UnityWebRequest.Get(string.Format("http://{0}/{1}", ServerAddress, request));
            yield return webReq.Send();

            if (webReq.isNetworkError)
            {
                Debug.Log("Web request error: " + webReq.error);
                OnSuccess(false);
                yield return new WaitForSeconds(1);
            }
            else
            {
                string s = webReq.downloadHandler.text;
                var lines = s.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                var line = lines.Where(aa => aa.StartsWith("a:")).FirstOrDefault().Trim();
                line = line.Substring(2); // skip 'a:'
                float requestTotalTime = Time.realtimeSinceStartup - requestStartTime;
                Debug.LogFormat("Response ({0}ms): {1}", Mathf.RoundToInt(requestTotalTime * 1000), s);

                OnSuccess(true);
                cb(line);
                break;
            }
        }
    }

    public IEnumerator CreateUser(string info, Action<string> cb)
    {
        var cmd = new CreateUser
        {
            info = info
        };

        var str = JsonUtility.ToJson(cmd);
        yield return SendWithRetry(str, cb, true);
    }

    public IEnumerator RequestDonees(Action<string> cb)
    {
        throw new NotImplementedException();
    }

    public IEnumerator RequestSequence(string userId, Action<string> cb)
    {
        var cmd = new RequestSequence();
        cmd.uid = userId;
        var str = JsonUtility.ToJson(cmd);
        yield return SendWithRetry(str, cb, true);
    }

    public IEnumerator ClaimWin(string userId, string latestSequence, string doneeId, Action<string> cb)
    {
        var cmd = new ClaimWin();
        cmd.uid = userId;
        cmd.seq = latestSequence;
        cmd.donId = doneeId;
        var str = JsonUtility.ToJson(cmd);
        yield return SendWithRetry(str, cb, true);
    }
}
