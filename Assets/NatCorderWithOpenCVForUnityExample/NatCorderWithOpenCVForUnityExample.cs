using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace NatCorderWithOpenCVForUnityExample
{
    public class NatCorderWithOpenCVForUnityExample : MonoBehaviour
    {
        public static string GetNatCorderVersion()
        {
            return "1.7.1";
        }

        public Text exampleTitle;
        public Text versionInfo;
        public ScrollRect scrollRect;
        private static float verticalNormalizedPosition = 1f;

        void Awake()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
        }

        // Use this for initialization
        IEnumerator Start()
        {
            exampleTitle.text = "NatCorderWithOpenCVForUnity Example " + Application.version;

            versionInfo.text = "NatCorder " + GetNatCorderVersion();
            versionInfo.text += " / " + OpenCVForUnity.CoreModule.Core.NATIVE_LIBRARY_NAME + " " + OpenCVForUnity.UnityUtils.Utils.getVersion() + " (" + OpenCVForUnity.CoreModule.Core.VERSION + ")";
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


#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
            RuntimePermissionHelper runtimePermissionHelper = GetComponent<RuntimePermissionHelper>();
            yield return runtimePermissionHelper.hasUserAuthorizedCameraPermission();
            yield return runtimePermissionHelper.hasUserAuthorizedMicrophonePermission();
            yield return runtimePermissionHelper.hasUserAuthorizedExternalStorageWritePermission();
#endif

            yield break;
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void OnScrollRectValueChanged()
        {
            verticalNormalizedPosition = scrollRect.verticalNormalizedPosition;
        }


        public void OnShowSystemInfoButtonClick()
        {
            SceneManager.LoadScene("ShowSystemInfo");
        }

        public void OnShowLicenseButtonClick()
        {
            SceneManager.LoadScene("ShowLicense");
        }

        public void OnVideoRecordingExampleButtonClick()
        {
            SceneManager.LoadScene("VideoRecordingExample");
        }
    }
}
