using Assets.Script;
using Facebook.Unity;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Splash : MonoBehaviour
{
    public GameObject PanelContent;

	void Awake()
    {
        PanelContent.SetActive(false);

        AppLog.LogLine("Splash: Initializing FB, first try");
        FacebookUtil.InitializeFacebook(FacebookInitCallback);
	}

    private void FacebookInitCallback()
    {
        AppLog.LogLine("Splash: FB init callback, IsInitialized = {0}, IsLoggedIn = {1}", FB.IsInitialized, FB.IsLoggedIn);

        bool isLoggedIn = FB.IsInitialized && FB.IsLoggedIn;
        if (!isLoggedIn)
        {
            AppLog.LogLine("Splash: Showing splash screen");
            ShowSplash();
            return;
        }
    }

    public void OnLoginButtonClick()
    {
        if (!FB.IsInitialized)
        {
            AppLog.LogLine("Splash: Error, login button clicked but FB not initialized");
            return;
        }

        DoLogin();
    }

    private void DoLogin()
    {
        FacebookUtil.LoginFacebook(FacebookLoginCallback);
    }

    private void OnFacebookLoggedIn()
    {
        FB.ActivateApp();
        SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
    }

    private void FacebookLoginCallback(ILoginResult result)
    {
        AppLog.LogLine("Splash: FB login callback, IsLoggedIn = {0}, error = {1}", FB.IsLoggedIn, result.Error);

        if (result.Error == null && FB.IsLoggedIn)
            OnFacebookLoggedIn();
    }

    private void ShowSplash()
    {
        PanelContent.SetActive(true);
    }

    private void OnApplicationPause(bool pause)
    {
        AppLog.LogLine("Splash: OnApplicationPause, pause = {0}", pause);

        bool resumed = !pause;
        if (resumed)
        {
        }
    }

    void Update()
    {
		
	}
}
