using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using DG.Tweening;
using FlipWebApps.BeautifulTransitions.Scripts.Transitions.Screen;

public class Loader : MonoBehaviour
{
    public float MinimumTimeShown = 1;

    void Start()
    {
        StartCoroutine(DoLoad());
    }

    IEnumerator DoLoad()
    {
        DOTween.Init(false, true, LogBehaviour.ErrorsOnly);

        float start = Time.time;
        var asyncLoad = SceneManager.LoadSceneAsync("scene", LoadSceneMode.Additive);
        yield return asyncLoad;

        float diff = Time.time - start;
        if (diff < MinimumTimeShown)
        {
            yield return new WaitForSecondsRealtime(MinimumTimeShown - diff);
        }

        GetComponent<WipeScreen>().InitTransitionOut();
        GetComponent<WipeScreen>().TransitionOut();
        //DOTween.Play("SplashFade");
        //DOTween.Play("SplashScale");

        yield return null;
    }

    public void AnimComplete()
    {
        var root = (GameObject)GameObject.Find("SplashRoot");
        GameObject.Destroy(root);
    }
}
