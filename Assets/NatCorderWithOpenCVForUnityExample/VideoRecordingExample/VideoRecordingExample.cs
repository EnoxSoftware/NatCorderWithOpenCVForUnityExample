using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using NatCorderU.Core;
using NatShareU;
using UnityEngine.Video;

#if UNITY_5_3 || UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif
using OpenCVForUnity;

namespace NatCorderWithOpenCVForUnityExample
{
    /// <summary>
    /// VideoRecording Example
    /// </summary>
    [RequireComponent(typeof(WebCamTextureToMatHelper), typeof(AudioSource), typeof(VideoPlayer))]
    public class VideoRecordingExample : MonoBehaviour
    {
        /// <summary>
        /// The requested resolution dropdown.
        /// </summary>
        public Dropdown requestedResolutionDropdown;

        /// <summary>
        /// The requested resolution.
        /// </summary>
        public ResolutionPreset requestedResolution = ResolutionPreset._640x480;

        [Space(20)]

        /// <summary>
        /// Determines if applies the comic filter.
        /// </summary>
        public bool applyComicFilter;

        /// <summary>
        /// The apply comic filter toggle.
        /// </summary>
        public Toggle applyComicFilterToggle;

        /// <summary>
        /// Determines if record microphone audio.
        /// </summary>
        public bool recordMicrophoneAudio;

        /// <summary>
        /// The record microphone audio toggle.
        /// </summary>
        public Toggle recordMicrophoneAudioToggle;

        /// <summary>
        /// The record video button.
        /// </summary>
        public Button recordVideoButton;

        /// <summary>
        /// The save path input field.
        /// </summary>
        public InputField savePathInputField;

        /// <summary>
        /// The play video button.
        /// </summary>
        public Button playVideoButton;

        /// <summary>
        /// The play video full screen button.
        /// </summary>
        public Button playVideoFullScreenButton;

        [Space(20)]

        /// <summary>
        /// The share button.
        /// </summary>
        public Button shareButton;

        /// <summary>
        /// The save to CameraRoll button.
        /// </summary>
        public Button saveToCameraRollButton;

        /// <summary>
        /// The texture.
        /// </summary>
        Texture2D texture;

        /// <summary>
        /// The webcam texture to mat helper.
        /// </summary>
        WebCamTextureToMatHelper webCamTextureToMatHelper;

        AudioSource audioSource;

        AudioRecorder audioRecorder;

        long timestamp = 0;
        long lastTime = -1;

        const float MAX_RECORDING_TIME = 10f; // Seconds

        string videoPath = "";

        VideoPlayer videoPlayer;

        bool isVideoPlaying;

        ComicFilter comicFilter;

        /// <summary>
        /// The FPS monitor.
        /// </summary>
        FpsMonitor fpsMonitor;

        #if UNITY_ANDROID && !UNITY_EDITOR
        float rearCameraRequestedFPS;
        #endif

        // Use this for initialization
        void Start ()
        {
            fpsMonitor = GetComponent<FpsMonitor> ();

            webCamTextureToMatHelper = gameObject.GetComponent<WebCamTextureToMatHelper> ();
            int width, height;
            Dimensions (requestedResolution, out width, out height);
            webCamTextureToMatHelper.requestedWidth = width;
            webCamTextureToMatHelper.requestedHeight = height;

            #if UNITY_ANDROID && !UNITY_EDITOR
            // Set the requestedFPS parameter to avoid the problem of the WebCamTexture image becoming low light on some Android devices. (Pixel, pixel 2)
            // https://forum.unity.com/threads/android-webcamtexture-in-low-light-only-some-models.520656/
            // https://forum.unity.com/threads/released-opencv-for-unity.277080/page-33#post-3445178
            rearCameraRequestedFPS = webCamTextureToMatHelper.requestedFPS;
            if (webCamTextureToMatHelper.requestedIsFrontFacing) {                
                webCamTextureToMatHelper.requestedFPS = 15;
                webCamTextureToMatHelper.Initialize ();
            } else {
                webCamTextureToMatHelper.Initialize ();
            }
            #else
            webCamTextureToMatHelper.Initialize ();
            #endif

            audioSource = gameObject.GetComponent<AudioSource> ();

            videoPlayer = gameObject.GetComponent<VideoPlayer> ();

            comicFilter = new ComicFilter ();

            // Update GUI state
            requestedResolutionDropdown.value = (int)requestedResolution;
            applyComicFilterToggle.isOn = applyComicFilter;
            recordMicrophoneAudioToggle.isOn = recordMicrophoneAudio;
        }

        /// <summary>
        /// Raises the webcam texture to mat helper initialized event.
        /// </summary>
        public void OnWebCamTextureToMatHelperInitialized ()
        {
            Debug.Log ("OnWebCamTextureToMatHelperInitialized");

            Mat webCamTextureMat = webCamTextureToMatHelper.GetMat ();

            texture = new Texture2D (webCamTextureMat.cols (), webCamTextureMat.rows (), TextureFormat.RGBA32, false);

            gameObject.GetComponent<Renderer> ().material.mainTexture = texture;

            gameObject.transform.localScale = new Vector3 (webCamTextureMat.cols (), webCamTextureMat.rows (), 1);
            Debug.Log ("Screen.width " + Screen.width + " Screen.height " + Screen.height + " Screen.orientation " + Screen.orientation);

            if (fpsMonitor != null){
                fpsMonitor.Add ("width", webCamTextureToMatHelper.GetWidth().ToString());
                fpsMonitor.Add ("height", webCamTextureToMatHelper.GetHeight().ToString());
                fpsMonitor.Add ("isFrontFacing", webCamTextureToMatHelper.IsFrontFacing().ToString());
                fpsMonitor.Add ("rotate90Degree", webCamTextureToMatHelper.rotate90Degree.ToString());
                fpsMonitor.Add ("flipVertical", webCamTextureToMatHelper.flipVertical.ToString());
                fpsMonitor.Add ("flipHorizontal", webCamTextureToMatHelper.flipHorizontal.ToString());
                fpsMonitor.Add ("orientation", Screen.orientation.ToString());
            }

                                    
            float width = webCamTextureMat.width();
            float height = webCamTextureMat.height();
                                    
            float widthScale = (float)Screen.width / width;
            float heightScale = (float)Screen.height / height;
            if (widthScale < heightScale) {
                Camera.main.orthographicSize = (width * (float)Screen.height / (float)Screen.width) / 2;
            } else {
                Camera.main.orthographicSize = height / 2;
            }
        }

        /// <summary>
        /// Raises the webcam texture to mat helper disposed event.
        /// </summary>
        public void OnWebCamTextureToMatHelperDisposed ()
        {
            Debug.Log ("OnWebCamTextureToMatHelperDisposed");

            StopRecording ();
            StopVideo ();

            if (texture != null) {
                Texture2D.Destroy(texture);
                texture = null;
            }
        }

        /// <summary>
        /// Raises the webcam texture to mat helper error occurred event.
        /// </summary>
        /// <param name="errorCode">Error code.</param>
        public void OnWebCamTextureToMatHelperErrorOccurred (WebCamTextureToMatHelper.ErrorCode errorCode)
        {
            Debug.Log ("OnWebCamTextureToMatHelperErrorOccurred " + errorCode);
        }

        // Update is called once per frame
        void Update ()
        {
            if (webCamTextureToMatHelper.IsPlaying () && webCamTextureToMatHelper.DidUpdateThisFrame ()) {

                Mat rgbaMat = webCamTextureToMatHelper.GetMat ();

                if (applyComicFilter)
                    comicFilter.Process (rgbaMat, rgbaMat);

                if (NatCorder.IsRecording) {
                    Imgproc.putText (rgbaMat, "[NatCorder With OpenCVForUnity Example]", new Point (5, rgbaMat.rows () - 30), Core.FONT_HERSHEY_SIMPLEX, 0.5, new Scalar (255, 255, 255, 255), 1, Imgproc.LINE_AA, false);
                    Imgproc.putText (rgbaMat, "- Video Recording Example", new Point (5, rgbaMat.rows () - 10), Core.FONT_HERSHEY_SIMPLEX, 0.5, new Scalar (255, 255, 255, 255), 1, Imgproc.LINE_AA, false);
                }

                // Restore the coordinate system of the image by OpenCV's Flip function.
                Utils.fastMatToTexture2D (rgbaMat, texture);

				if (NatCorder.IsRecording)
                {					
					// Calculate time
					var frameTime = Frame.CurrentTimestamp;
					timestamp += lastTime > 0 ? frameTime - lastTime : 0;
					lastTime = frameTime;
					// Blit to recording frame
					var encoderFrame = NatCorder.AcquireFrame();
					encoderFrame.timestamp = timestamp;
				    Graphics.Blit(texture, encoderFrame);
                    NatCorder.CommitFrame (encoderFrame);
				}
            }

            if (isVideoPlaying && videoPlayer.isPlaying) {
                gameObject.GetComponent<Renderer> ().sharedMaterial.mainTexture = videoPlayer.texture;
            }
        }
            
        private void StartRecording ()
        {
            if (isVideoPlaying || NatCorder.IsRecording)
                return;

            Debug.Log ("StartRecording ()");
            if (fpsMonitor != null) {
                fpsMonitor.consoleText = "Recording";
            }   

			timestamp = 0;
			lastTime = -1;

            int recordingWidth = webCamTextureToMatHelper.GetWidth ();
            int recordingHeight = webCamTextureToMatHelper.GetHeight ();
            var configuration = new Configuration(recordingWidth, recordingHeight);
            // Start recording
            if (recordMicrophoneAudio) {
                StartMicrophone ();
                audioRecorder = audioSource.gameObject.AddComponent<AudioRecorder> ();
                audioRecorder.mute = true;
                NatCorder.StartRecording (configuration, OnVideo, audioRecorder);
            } else {
                NatCorder.StartRecording (configuration, OnVideo);
            }

            StartCoroutine ("Countdown");

            HideAllVideoUI ();
            recordVideoButton.interactable = true;
            recordVideoButton.GetComponentInChildren<UnityEngine.UI.Text>().color = Color.red;
        }

        private void StartMicrophone ()
        {
            #if !UNITY_WEBGL || UNITY_EDITOR // No `Microphone` API on WebGL :(
            // If the clip has not been set, set it now
            if (audioSource.clip == null) {
            audioSource.clip = Microphone.Start(null, true, 60, 48000);
            while (Microphone.GetPosition(null) <= 0) ;
            }            
            // Play through audio source
            audioSource.timeSamples = Microphone.GetPosition(null);
            audioSource.loop = true;
            audioSource.Play();
            #endif
        }

        private void StopRecording ()
        {
            if (!NatCorder.IsRecording)
                return;

            Debug.Log ("StopRecording ()");
            if (fpsMonitor != null) {
                fpsMonitor.consoleText = "";
            }    

            // Stop recording
            if (recordMicrophoneAudio) audioSource.Stop();
            NatCorder.StopRecording();

            StopCoroutine ("Countdown");

            ShowAllVideoUI ();
            recordVideoButton.GetComponentInChildren<UnityEngine.UI.Text>().color = Color.black;
        }

        private IEnumerator Countdown ()
        {
            float startTime = Time.time;
            while ((Time.time - startTime) < MAX_RECORDING_TIME) {

                if (fpsMonitor != null) {
                    fpsMonitor.consoleText += ".";
                }   

                yield return new WaitForSeconds(0.5f);
            }

            StopRecording ();
        }

        private void OnVideo (string path) {
            Debug.Log("Saved recording to: "+path);

            videoPath = path;

            savePathInputField.text = videoPath;
        }

        private void PlayVideo (string path)
        {
            if (isVideoPlaying || NatCorder.IsRecording || string.IsNullOrEmpty (path))
                return;

            Debug.Log("PlayVideo ()");

            isVideoPlaying = true;

            // Playback the video
            videoPlayer.renderMode = VideoRenderMode.APIOnly;
            videoPlayer.playOnAwake = false;
            videoPlayer.isLooping = false;
            audioSource.playOnAwake = false;

            videoPlayer.source = VideoSource.Url;
            videoPlayer.url = path;

            videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
            videoPlayer.controlledAudioTrackCount = 1;
            videoPlayer.EnableAudioTrack(0, true);
            videoPlayer.SetTargetAudioSource(0, audioSource);

            videoPlayer.prepareCompleted += PrepareCompleted;
            videoPlayer.loopPointReached += EndReached;

            videoPlayer.Prepare ();

            HideAllVideoUI ();
        }

        private void PrepareCompleted(VideoPlayer vp)
        {
            Debug.Log("PrepareCompleted");

            vp.prepareCompleted -= PrepareCompleted;

            vp.Play();

            webCamTextureToMatHelper.Pause ();
        }

        private void EndReached(VideoPlayer vp)
        {
            Debug.Log("EndReached");

            videoPlayer.loopPointReached -= EndReached;

            StopVideo ();
        }

        private void StopVideo ()
        {
            if (!isVideoPlaying)
                return;

            Debug.Log("StopVideo ()");

            if (videoPlayer.isPlaying)
                videoPlayer.Stop();

            gameObject.GetComponent<Renderer> ().sharedMaterial.mainTexture = texture;

            webCamTextureToMatHelper.Play ();

            isVideoPlaying = false;

            ShowAllVideoUI ();
        }

        private void ShowAllVideoUI ()
        {
            applyComicFilterToggle.interactable = true;
            recordMicrophoneAudioToggle.interactable = true;
            recordVideoButton.interactable = true;
            savePathInputField.interactable = true;
            playVideoButton.interactable = true;
            playVideoFullScreenButton.interactable = true;
            shareButton.interactable = true;
            saveToCameraRollButton.interactable = true;
        }

        private void HideAllVideoUI ()
        {
            applyComicFilterToggle.interactable = false;
            recordMicrophoneAudioToggle.interactable = false;
            recordVideoButton.interactable = false;
            savePathInputField.interactable = false;
            playVideoButton.interactable = false;
            playVideoFullScreenButton.interactable = false;
            shareButton.interactable = false;
            saveToCameraRollButton.interactable = false;
        }
    
        /// <summary>
        /// Raises the destroy event.
        /// </summary>
        void OnDestroy ()
        {
            webCamTextureToMatHelper.Dispose ();

            if (comicFilter != null)
                comicFilter.Dispose ();
        }         

        /// <summary>
        /// Raises the back button click event.
        /// </summary>
        public void OnBackButtonClick ()
        {
            #if UNITY_5_3 || UNITY_5_3_OR_NEWER
            SceneManager.LoadScene ("NatCorderWithOpenCVForUnityExample");
            #else
            Application.LoadLevel ("NatCorderWithOpenCVForUnityExample");
            #endif
        }

        /// <summary>
        /// Raises the play button click event.
        /// </summary>
        public void OnPlayButtonClick ()
        {
            webCamTextureToMatHelper.Play ();
        }

        /// <summary>
        /// Raises the pause button click event.
        /// </summary>
        public void OnPauseButtonClick ()
        {
            webCamTextureToMatHelper.Pause ();
        }

        /// <summary>
        /// Raises the stop button click event.
        /// </summary>
        public void OnStopButtonClick ()
        {
            webCamTextureToMatHelper.Stop ();
        }

        /// <summary>
        /// Raises the change camera button click event.
        /// </summary>
        public void OnChangeCameraButtonClick ()
        {
            #if UNITY_ANDROID && !UNITY_EDITOR
            if (!webCamTextureToMatHelper.IsFrontFacing ()) {
                rearCameraRequestedFPS = webCamTextureToMatHelper.requestedFPS;
                webCamTextureToMatHelper.Initialize (!webCamTextureToMatHelper.IsFrontFacing (), 15, webCamTextureToMatHelper.rotate90Degree);
            } else {                
                webCamTextureToMatHelper.Initialize (!webCamTextureToMatHelper.IsFrontFacing (), rearCameraRequestedFPS, webCamTextureToMatHelper.rotate90Degree);
            }
            #else
            webCamTextureToMatHelper.requestedIsFrontFacing = !webCamTextureToMatHelper.IsFrontFacing ();
            #endif
        }

        /// <summary>
        /// Raises the requested resolution dropdown value changed event.
        /// </summary>
        public void OnRequestedResolutionDropdownValueChanged (int result)
        {
            if ((int)requestedResolution != result) {
                requestedResolution = (ResolutionPreset)result;

                int width, height;
                Dimensions (requestedResolution, out width, out height);

                webCamTextureToMatHelper.Initialize (width, height);
            }
        }

        /// <summary>
        /// Raises the apply comic filter toggle value changed event.
        /// </summary>
        public void OnApplyComicFilterToggleValueChanged ()
        {
            if (applyComicFilter != applyComicFilterToggle.isOn) {
                applyComicFilter = applyComicFilterToggle.isOn;
            }
        }

        /// <summary>
        /// Raises the record microphone audio toggle value changed event.
        /// </summary>
        public void OnRecordMicrophoneAudioToggleValueChanged ()
        {
            if (recordMicrophoneAudio != recordMicrophoneAudioToggle.isOn) {
                recordMicrophoneAudio = recordMicrophoneAudioToggle.isOn;
            }
        }

        /// <summary>
        /// Raises the record video button click event.
        /// </summary>
        public void OnRecordVideoButtonClick ()
        {
            Debug.Log ("OnRecordVideoButtonClick ()");

            if (isVideoPlaying)
                return;

            if (NatCorder.IsRecording) {                
                StopRecording (); 
            } else {
                StartRecording ();
            }
        }

        /// <summary>
        /// Raises the play video button click event.
        /// </summary>
        public void OnPlayVideoButtonClick ()
        {
            Debug.Log ("OnPlayVideoButtonClick ()");

            if (isVideoPlaying || NatCorder.IsRecording || string.IsNullOrEmpty (videoPath))
                return;

            // Playback the video
            #if UNITY_IOS
            PlayVideo ("file://" + videoPath);
            #else
            PlayVideo (videoPath);
            #endif
        }

        /// <summary>
        /// Raises the play video full screen button click event.
        /// </summary>
        public void OnPlayVideoFullScreenButtonClick ()
        {
            Debug.Log ("OnPlayVideoFullScreenButtonClick ()");

            if (isVideoPlaying || NatCorder.IsRecording || string.IsNullOrEmpty (videoPath))
                return;

            // Playback the video
            #if UNITY_IOS
            Handheld.PlayFullScreenMovie("file://" + videoPath);
            #elif UNITY_ANDROID
            Handheld.PlayFullScreenMovie(videoPath);
            #else
            Debug.LogWarning ("Full-screen video playback is not supported on this platform.");
            #endif
        }

        /// <summary>
        /// Raises the share button click event.
        /// </summary>
        public void OnShareButtonClick ()
        {
            Debug.Log ("OnShareButtonClick ()");

            if (isVideoPlaying || NatCorder.IsRecording || string.IsNullOrEmpty (videoPath))
                return;
            
            NatShare.Share (videoPath);
        }

        /// <summary>
        /// Raises the save to camera roll button click event.
        /// </summary>
        public void OnSaveToCameraRollButtonClick ()
        {
            Debug.Log ("OnSaveToCameraRollButtonClick ()");

            if (isVideoPlaying || NatCorder.IsRecording || string.IsNullOrEmpty (videoPath))
                return;
            
            NatShare.SaveToCameraRoll (videoPath);
        }

        public enum ResolutionPreset : byte
        {
            _50x50 = 0,
            _640x480,
            _1280x720,
            _1920x1080,
            _9999x9999,
        }

        private void Dimensions (ResolutionPreset preset, out int width, out int height) {
            switch (preset) {
            case ResolutionPreset._50x50: width = 50; height = 50; break;
            case ResolutionPreset._640x480: width = 640; height = 480; break;
            case ResolutionPreset._1280x720: width = 1280; height = 720; break;
            case ResolutionPreset._1920x1080: width = 1920; height = 1080; break;
            case ResolutionPreset._9999x9999: width = 9999; height = 9999; break;
            default: width = height = 0; break;
            }
        }


        [AddComponentMenu(""), DisallowMultipleComponent]
        private sealed class AudioRecorder : MonoBehaviour, IAudioSource {

            int IAudioSource.sampleRate { get { return AudioSettings.outputSampleRate; }}
            int IAudioSource.sampleCount {
                get {
                    int sampleCount, bufferCount;
                    AudioSettings.GetDSPBufferSize(out sampleCount, out bufferCount);
                    return sampleCount;
                }
            }
            int IAudioSource.channelCount { get { return (int)AudioSettings.speakerMode; }}

            public bool mute;
            private long timestamp, lastTime = -1; // Used to support pausing and resuming

            void OnAudioFilterRead (float[] data, int channels) {
                // Calculate time
                var audioTime = Frame.CurrentTimestamp;
                timestamp += lastTime > 0 ? audioTime - lastTime : 0;
                lastTime = audioTime;
                // Send to NatCorder for encoding
                NatCorder.CommitSamples(data, timestamp);
                if (mute) Array.Clear(data, 0, data.Length);
            }

            void IDisposable.Dispose () {
                Destroy(this);
            }
        }
    }
}