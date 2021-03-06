﻿using UnityEngine;
using Facebook.Unity;
using System.Collections.Generic;

public static class FacebookUtil
{
    public static void InitializeFacebook(InitDelegate initCallback)
    {
        if (!FB.IsInitialized)
            FB.Init(initCallback, OnHideUnity);
        else
            initCallback();
    }

    // An additional benefit of asking for fewer permissions is that you might not need to submit
    // your app for Login Review.You only need to submit for Login Review if you're requesting any
    // permissions other than public_profile, user_friends and email.
    public static void LoginFacebook(FacebookDelegate<ILoginResult> loginCallback)
    {
        var perms = new List<string>() { "public_profile", "email" };
        FB.LogInWithReadPermissions(perms, loginCallback); // This is where FB dialog is shown
    }

    /// <summary>
    /// Pause game when login screen appears
    /// </summary>
    /// <param name="isGameShown">If set to <c>true</c> is game shown.</param>
    private static void OnHideUnity(bool isGameShown)
    {
        if (!isGameShown)
            Time.timeScale = 0; // Pause the game - we will need to hide
        else
            Time.timeScale = 1; // Resume the game - we're getting focus again
    }

    private static void Logout()
    {
        if (FB.IsLoggedIn)
            FB.LogOut();
    }
}
