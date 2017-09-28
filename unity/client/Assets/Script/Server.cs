using UnityEngine;
using System;
using Assets.Script;
using Commands;
using UnityEngine.Networking;
using System.Collections;
using System.Linq;

// Stateless requests on any GamePlayServer.
//    CreateUser(some platform info) (if not found on client).
//    RequestSequence(user, ) (ask MoneyServer for clearance).
//    RequestWin(user, donee, ) (Simple check, or skip entirely in v1. Send update to MoneyServer.
//    Tables: User. (id) Could skip in theory, but I want it to keep track of activity :-)

public interface IServer
{
    IEnumerator RequestDonees(Action<string> cb);
    IEnumerator CreateUser(string info, Action<string> cb);
    IEnumerator RequestSequence(string userId, Action<string> cb);
    IEnumerator ClaimWin(string userId, string latestSequence, string doneeId, Action<string> cb);
    void Stop();
}

public class Server : IServer
{
    const string ServerAddress = "localhost";
//    const string ServerAddress = "ec2-54-154-204-58.eu-west-1.compute.amazonaws.com";

    private float requestStartTime;
    private Action<bool> OnSuccess;
    private bool stop_ = false;

    public void Stop()
    {
        stop_ = true;
    }

    public Server(Action<bool> onSuccess)
    {
        OnSuccess = onSuccess;
    }

    private IEnumerator SendWithRetry(string request, Action<string> cb, bool first = false)
    {
        if (first)
            requestStartTime = Time.realtimeSinceStartup;

        while (true)
        {
            if (stop_)
                yield break;

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
        var cmd = new CreateUser();
        cmd.info = info;
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
