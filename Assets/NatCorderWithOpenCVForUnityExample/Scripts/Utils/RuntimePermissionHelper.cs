using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NatCorderWithOpenCVForUnityExample
{
    public class RuntimePermissionHelper
    {
        private RuntimePermissionHelper ()
        {
        }

        private static AndroidJavaObject GetActivity ()
        {
            using (var UnityPlayer = new AndroidJavaClass ("com.unity3d.player.UnityPlayer")) {
                return UnityPlayer.GetStatic<AndroidJavaObject> ("currentActivity");
            }
        }

        private static bool IsAndroidMOrGreater ()
        {
            using (var VERSION = new AndroidJavaClass ("android.os.Build$VERSION")) {
                return VERSION.GetStatic<int> ("SDK_INT") >= 23;
            }
        }

        public static bool HasPermission (string permission)
        {
            if (IsAndroidMOrGreater ()) {
                using (var activity = GetActivity ()) {
                    return activity.Call<int> ("checkSelfPermission", permission) == 0;
                }
            }

            return true;
        }

        public static bool ShouldShowRequestPermissionRationale (string permission)
        {
            if (IsAndroidMOrGreater ()) {
                using (var activity = GetActivity ()) {
                    return activity.Call<bool> ("shouldShowRequestPermissionRationale", permission);
                }
            }

            return false;
        }

        public static void RequestPermission (string[] permissiions)
        {
            if (IsAndroidMOrGreater ()) {
                using (var activity = GetActivity ()) {
                    activity.Call ("requestPermissions", permissiions, 0);
                }
            }
        }
    }
}