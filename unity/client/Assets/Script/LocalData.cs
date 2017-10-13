using Facebook.Unity;
using System;
using UnityEngine;

namespace Assets.Script
{
    public static class LocalData
    {
        // All keys must be prepended with deviceId + fb id. This enables multiple fb users on same device.

        public static readonly string DeviceIdKey = "DeviceId";
        public static readonly string XpKey = "xp";

        private static Int64 xp = 0;
        public static Int64 Xp
        {
            get
            {
                return xp;
            }

            set
            {
                xp = value;
            }
        }

        public static string FbAccessToken { get { return AccessToken.CurrentAccessToken.TokenString; } }
        public static string FbUserId { get { return AccessToken.CurrentAccessToken.UserId; } }
//        public static bool HasFbPermission { get { return AccessToken.CurrentAccessToken.Permissions.ToCommaSeparateList; } }

        private static string deviceId = string.Empty;
        public static string DeviceId
        {
            get
            {
                if (!string.IsNullOrEmpty(deviceId))
                    return deviceId;

                string id = PlayerPrefs.GetString(LocalData.DeviceIdKey, string.Empty);
                if (id == string.Empty)
                {
                    id = Guid.NewGuid().ToString();
                    PlayerPrefs.Save();
                }

                deviceId = id;
                return id;
            }
        }
    }
}
