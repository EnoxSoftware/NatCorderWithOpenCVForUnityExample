using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace NatCorderWithOpenCVForUnityExample
{
    public class NatCorderWithOpenCVForUnityExample : MonoBehaviour
    {
        public Text exampleTitle;
        public Text versionInfo;
        public ScrollRect scrollRect;
        static float verticalNormalizedPosition = 1f;

        void Awake ()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
        }

        // Use this for initialization
        IEnumerator Start ()
        {
            #if UNITY_ANDROID && !UNITY_EDITOR
            yield return RequestAndroidPermission ("android.permission.WRITE_EXTERNAL_STORAGE");
            yield return RequestAndroidPermission ("android.permission.CAMERA");
            yield return RequestAndroidPermission ("android.permission.RECORD_AUDIO");
            #endif

            exampleTitle.text = "NatCorderWithOpenCVForUnity Example " + Application.version;

            versionInfo.text = OpenCVForUnity.CoreModule.Core.NATIVE_LIBRARY_NAME + " " + OpenCVForUnity.UnityUtils.Utils.getVersion () + " (" + OpenCVForUnity.CoreModule.Core.VERSION + ")";
            versionInfo.text += " / UnityEditor " + Application.unityVersion;
            versionInfo.text += " / ";

            #if UNITY_EDITOR
            versionInfo.text += "Editor";
            #elif UNITY_STANDALONE_WIN
            versionInfo.text += "Windows";
            #elif UNITY_STANDALONE_OSX
            versionInfo.text += "Mac OSX";
            #elif UNITY_STANDALONE_LINUX
            versionInfo.text += "Linux";
            #elif UNITY_ANDROID
            versionInfo.text += "Android";
            #elif UNITY_IOS
            versionInfo.text += "iOS";
            #elif UNITY_WSA
            versionInfo.text += "WSA";
            #elif UNITY_WEBGL
            versionInfo.text += "WebGL";
            #endif
            versionInfo.text += " ";
            #if ENABLE_MONO
            versionInfo.text += "Mono";
            #elif ENABLE_IL2CPP
            versionInfo.text += "IL2CPP";
            #elif ENABLE_DOTNET
            versionInfo.text += ".NET";
            #endif

            scrollRect.verticalNormalizedPosition = verticalNormalizedPosition;

            yield return null;
        }

        #if UNITY_ANDROID && !UNITY_EDITOR
        private IEnumerator RequestAndroidPermission(string permission)
        {
            if (!RuntimePermissionHelper.HasPermission(permission))
            {
                if (RuntimePermissionHelper.ShouldShowRequestPermissionRationale(permission))
                {
                    RuntimePermissionHelper.RequestPermission(new string[] { permission });
                }
            }
            yield return new WaitForSeconds(0.5f);
        }
        #endif

        // Update is called once per frame
        void Update ()
        {

        }

        public void OnScrollRectValueChanged ()
        {
            verticalNormalizedPosition = scrollRect.verticalNormalizedPosition;
        }


        public void OnShowSystemInfoButtonClick ()
        {
            SceneManager.LoadScene ("ShowSystemInfo");
        }

        public void OnShowLicenseButtonClick ()
        {
            SceneManager.LoadScene ("ShowLicense");
        }

        public void OnVideoRecordingExampleButtonClick ()
        {
            SceneManager.LoadScene ("VideoRecordingExample");
        }
    }
}
