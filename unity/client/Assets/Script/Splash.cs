using Assets.Script;
using Facebook.Unity;
using UnityEngine;

public class Splash : MonoBehaviour
{
    public GameObject PanelContent;
    public bool SkipFacebookLogin;

	void Awake()
    {
        AppLog.LogLine("Splash.Awake()");
        PanelContent.SetActive(false);

        AppLog.LogLine("Splash: Initializing FB, first try");
        FacebookUtil.InitializeFacebook(OnFacebookInitialized);
	}

    private void OnFacebookInitialized()
    {
        AppLog.LogLine("Splash: FB, IsInitialized = {0}, IsLoggedIn = {1}", FB.IsInitialized, FB.IsLoggedIn);

        bool isLoggedIn = FB.IsInitialized && (FB.IsLoggedIn || SkipFacebookLogin);
        if (!isLoggedIn)
        {
            AppLog.LogLine("Splash: Showing splash screen");
            ShowSplash();
            return;
        }

        OnFacebookLoggedIn();
    }

    public void OnLoginButtonClick()
    {
        if (!FB.IsInitialized)
        {
            AppLog.LogLine("Splash: Error, login button clicked but FB not initialized");
            Util.LoadSplashScene();
            return;
        }

        DoLogin();
    }

    private void DoLogin()
    {
        // On mobile we will get a long-lived token. On web it will be short-lived.
        // It can be converted to long-lived with app secret and app id.
        FacebookUtil.LoginFacebook(FacebookLoginCallback);
    }

    private void OnFacebookLoggedIn()
    {
        FB.ActivateApp();
//        FB.API("/me?fields=name,email", HttpMethod.GET, Api);
        Util.LoadGameScene();
    }

    void OnFbError()
    {
        Util.LoadSplashScene();
    }

    void Api(IGraphResult res)
    {
        if (!string.IsNullOrEmpty(res.Error))
        {
            OnFbError();
            return;
        }

        AppLog.LogLine("API callback: " + res.ResultDictionary["name"]);
    }

    private void FacebookLoginCallback(ILoginResult result)
    {
        AppLog.LogLine("Splash: FB login callback, IsLoggedIn = {0}, error = {1}", FB.IsLoggedIn, result.Error);

        if (result.Error == null && FB.IsLoggedIn)
        {
            OnFacebookLoggedIn();
        }
        else
        {
            // Login error. Need to check if FB SDK shows an error or if I have to.
            // Will also get here in case of user cancel.
        }
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
