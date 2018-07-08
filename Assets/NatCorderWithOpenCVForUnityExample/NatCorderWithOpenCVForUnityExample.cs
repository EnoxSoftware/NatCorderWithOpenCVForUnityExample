using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

#if UNITY_5_3 || UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif

namespace NatCorderWithOpenCVForUnityExample
{
    public class NatCorderWithOpenCVForUnityExample : MonoBehaviour
    {
        public Text exampleTitle;
        public Text versionInfo;
        public ScrollRect scrollRect;
        static float verticalNormalizedPosition = 1f;

        void Awake () {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
        }

        // Use this for initialization
        void Start ()
        {
            exampleTitle.text = "NatCorderWithOpenCVForUnity Example " + Application.version;

            versionInfo.text = OpenCVForUnity.Core.NATIVE_LIBRARY_NAME + " " + OpenCVForUnity.Utils.getVersion () + " (" + OpenCVForUnity.Core.VERSION + ")";
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
            versionInfo.text +=  " ";
            #if ENABLE_MONO
            versionInfo.text +=  "Mono";
            #elif ENABLE_IL2CPP
            versionInfo.text += "IL2CPP";
            #elif ENABLE_DOTNET
            versionInfo.text += ".NET";
            #endif

            scrollRect.verticalNormalizedPosition = verticalNormalizedPosition;
        }

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
            #if UNITY_5_3 || UNITY_5_3_OR_NEWER
            SceneManager.LoadScene ("ShowSystemInfo");
            #else
            Application.LoadLevel ("ShowSystemInfo");
            #endif
        }

        public void OnShowLicenseButtonClick ()
        {
            #if UNITY_5_3 || UNITY_5_3_OR_NEWER
			SceneManager.LoadScene ("ShowLicense");
            #else
            Application.LoadLevel ("ShowLicense");
            #endif
        }

        public void OnVideoRecordingExampleButtonClick ()
        {
            #if UNITY_5_3 || UNITY_5_3_OR_NEWER
            SceneManager.LoadScene ("VideoRecordingExample");
            #else
            Application.LoadLevel ("VideoRecordingExample");
            #endif
        }
    }
}
