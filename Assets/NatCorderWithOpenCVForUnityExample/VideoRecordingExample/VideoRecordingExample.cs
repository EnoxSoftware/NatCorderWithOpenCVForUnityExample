using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using NatShareU;
using NatCorder;
using NatCorder.Inputs;
using NatCorder.Clocks;
using OpenCVForUnity.UnityUtils.Helper;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;

namespace NatCorderWithOpenCVForUnityExample
{
    /// <summary>
    /// VideoRecording Example
    /// </summary>
    [RequireComponent (typeof(WebCamTextureToMatHelper), typeof(AudioSource), typeof(VideoPlayer))]
    public class VideoRecordingExample : MonoBehaviour
    {
        /// <summary>
        /// The requested resolution.
        /// </summary>
        public ResolutionPreset requestedResolution = ResolutionPreset._640x480;

        /// <summary>
        /// The requested resolution dropdown.
        /// </summary>
        public Dropdown requestedResolutionDropdown;

        [Space (20)]
        [Header ("Recording")]

        /// <summary>
        /// The type of container.
        /// </summary>
        public ContainerPreset container = ContainerPreset.MP4;

        /// <summary>
        /// The container dropdown.
        /// </summary>
        public Dropdown containerDropdown;

        /// <summary>
        /// Determines if applies the comic filter.
        /// </summary>
        public bool applyComicFilter;

        /// <summary>
        /// The apply comic filter toggle.
        /// </summary>
        public Toggle applyComicFilterToggle;

        [Header ("Microphone")]

        /// <summary>
        /// Determines if record microphone audio.
        /// </summary>
        public bool recordMicrophoneAudio;

        /// <summary>
        /// The record microphone audio toggle.
        /// </summary>
        public Toggle recordMicrophoneAudioToggle;

        [Space (20)]

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

        [Space (20)]

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

        IMediaRecorder videoRecorder;

        AudioSource microphoneSource;

        AudioInput audioInput;

        IClock recordingClock;

        const float MAX_RECORDING_TIME = 10f;
        // Seconds

        string videoPath = "";

        VideoPlayer videoPlayer;

        bool isVideoPlaying;

        bool isVideoRecording;

        int frameCount;

        int recordEveryNthFrame;

        ComicFilter comicFilter;

        /// <summary>
        /// The FPS monitor.
        /// </summary>
        FpsMonitor fpsMonitor;

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
            // Avoids the front camera low light issue that occurs in only some Android devices (e.g. Google Pixel, Pixel2).
            webCamTextureToMatHelper.avoidAndroidFrontCameraLowLightIssue = true;
            #endif
            webCamTextureToMatHelper.Initialize ();

            microphoneSource = gameObject.GetComponent<AudioSource> ();

            videoPlayer = gameObject.GetComponent<VideoPlayer> ();

            comicFilter = new ComicFilter ();

            // Update GUI state
            requestedResolutionDropdown.value = (int)requestedResolution;
            containerDropdown.value = (int)container - 1;
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

            if (fpsMonitor != null) {
                fpsMonitor.Add ("width", webCamTextureToMatHelper.GetWidth ().ToString ());
                fpsMonitor.Add ("height", webCamTextureToMatHelper.GetHeight ().ToString ());
                fpsMonitor.Add ("isFrontFacing", webCamTextureToMatHelper.IsFrontFacing ().ToString ());
                fpsMonitor.Add ("rotate90Degree", webCamTextureToMatHelper.rotate90Degree.ToString ());
                fpsMonitor.Add ("flipVertical", webCamTextureToMatHelper.flipVertical.ToString ());
                fpsMonitor.Add ("flipHorizontal", webCamTextureToMatHelper.flipHorizontal.ToString ());
                fpsMonitor.Add ("orientation", Screen.orientation.ToString ());
            }

                                    
            float width = webCamTextureMat.width ();
            float height = webCamTextureMat.height ();
                                    
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
                Texture2D.Destroy (texture);
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

                if (isVideoRecording) {
                    Imgproc.putText (rgbaMat, "[NatCorder With OpenCVForUnity Example]", new Point (5, rgbaMat.rows () - 30), Imgproc.FONT_HERSHEY_SIMPLEX, 0.5, new Scalar (255, 255, 255, 255), 1, Imgproc.LINE_AA, false);
                    Imgproc.putText (rgbaMat, "- Video Recording Example", new Point (5, rgbaMat.rows () - 10), Imgproc.FONT_HERSHEY_SIMPLEX, 0.5, new Scalar (255, 255, 255, 255), 1, Imgproc.LINE_AA, false);
                }

                // Restore the coordinate system of the image by OpenCV's Flip function.
                Utils.fastMatToTexture2D (rgbaMat, texture);

                // Record frames
                if (videoRecorder != null && isVideoRecording && frameCount++ % recordEveryNthFrame == 0) {
                    // Blit to recording frame
                    var frame = videoRecorder.AcquireFrame ();
                    Graphics.Blit (texture, frame);
                    videoRecorder.CommitFrame (frame, recordingClock.Timestamp);
                }
            }

            if (isVideoPlaying && videoPlayer.isPlaying) {
                gameObject.GetComponent<Renderer> ().sharedMaterial.mainTexture = videoPlayer.texture;
            }
        }

        private void StartRecording ()
        {
            if (isVideoPlaying || isVideoRecording)
                return;

            Debug.Log ("StartRecording ()");
            if (fpsMonitor != null) {
                fpsMonitor.consoleText = "Recording";
            }   
                
            // First make sure recording microphone is only on MP4
            recordMicrophoneAudio &= container == ContainerPreset.MP4;
            // Create recording configurations
            int recordingWidth = webCamTextureToMatHelper.GetWidth ();
            int recordingHeight = webCamTextureToMatHelper.GetHeight ();

            // Start recording
            recordingClock = new RealtimeClock ();
            if (container == ContainerPreset.MP4) {
                videoRecorder = new MP4Recorder (
                    recordingWidth,
                    recordingHeight,
                    30,
                    recordMicrophoneAudio ? AudioSettings.outputSampleRate : 0,
                    recordMicrophoneAudio ? (int)AudioSettings.speakerMode : 0,
                    OnVideo
                );
                recordEveryNthFrame = 1;
            } else {
                videoRecorder = new GIFRecorder (
                    recordingWidth,
                    recordingHeight,
                    0.1f,
                    OnVideo
                );
                recordEveryNthFrame = 5;
            }
            frameCount = 0;
            // Start microphone and create audio input
            if (recordMicrophoneAudio) {
                StartMicrophone ();
                audioInput = AudioInput.Create (videoRecorder, microphoneSource, recordingClock, true);
            }

            StartCoroutine ("Countdown");

            HideAllVideoUI ();
            recordVideoButton.interactable = true;
            recordVideoButton.GetComponentInChildren<UnityEngine.UI.Text> ().color = Color.red;

            isVideoRecording = true;
        }

        private void StartMicrophone ()
        {
            #if !UNITY_WEBGL || UNITY_EDITOR // No `Microphone` API on WebGL :(
            // Create a microphone clip
            microphoneSource.clip = Microphone.Start (null, true, 60, 48000);
            while (Microphone.GetPosition (null) <= 0)
                ;          
            // Play through audio source
            microphoneSource.timeSamples = Microphone.GetPosition (null);
            microphoneSource.loop = true;
            microphoneSource.Play ();
            #endif
        }

        private void StopRecording ()
        {
            if (!isVideoRecording)
                return;

            Debug.Log ("StopRecording ()");
            if (fpsMonitor != null) {
                fpsMonitor.consoleText = "";
            }    

            // Stop the microphone if we used it for recording
            if (recordMicrophoneAudio) {
                StopMicrophone ();
                audioInput.Dispose ();
            }
                
            // Stop recording
            videoRecorder.Dispose ();

            StopCoroutine ("Countdown");

            ShowAllVideoUI ();
            recordVideoButton.GetComponentInChildren<UnityEngine.UI.Text> ().color = Color.black;

            isVideoRecording = false;
        }

        private void StopMicrophone ()
        {
            #if !UNITY_WEBGL || UNITY_EDITOR
            Microphone.End (null);
            microphoneSource.Stop ();
            #endif
        }

        private IEnumerator Countdown ()
        {
            float startTime = Time.time;
            while ((Time.time - startTime) < MAX_RECORDING_TIME) {

                if (fpsMonitor != null) {
                    fpsMonitor.consoleText += ".";
                }   

                yield return new WaitForSeconds (0.5f);
            }

            StopRecording ();
        }

        private void OnVideo (string path)
        {
            Debug.Log ("Saved recording to: " + path);

            videoPath = path;

            savePathInputField.text = videoPath;
        }

        private void PlayVideo (string path)
        {
            if (isVideoPlaying || isVideoRecording || string.IsNullOrEmpty (path))
                return;

            Debug.Log ("PlayVideo ()");

            isVideoPlaying = true;

            // Playback the video
            videoPlayer.renderMode = VideoRenderMode.APIOnly;
            videoPlayer.playOnAwake = false;
            videoPlayer.isLooping = false;
            microphoneSource.playOnAwake = false;

            videoPlayer.source = VideoSource.Url;
            videoPlayer.url = path;

            videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
            videoPlayer.controlledAudioTrackCount = 1;
            videoPlayer.EnableAudioTrack (0, true);
            videoPlayer.SetTargetAudioSource (0, microphoneSource);

            videoPlayer.prepareCompleted += PrepareCompleted;
            videoPlayer.loopPointReached += EndReached;

            videoPlayer.Prepare ();

            HideAllVideoUI ();
        }

        private void PrepareCompleted (VideoPlayer vp)
        {
            Debug.Log ("PrepareCompleted");

            vp.prepareCompleted -= PrepareCompleted;

            vp.Play ();

            webCamTextureToMatHelper.Pause ();
        }

        private void EndReached (VideoPlayer vp)
        {
            Debug.Log ("EndReached");

            videoPlayer.loopPointReached -= EndReached;

            StopVideo ();
        }

        private void StopVideo ()
        {
            if (!isVideoPlaying)
                return;

            Debug.Log ("StopVideo ()");

            if (videoPlayer.isPlaying)
                videoPlayer.Stop ();

            gameObject.GetComponent<Renderer> ().sharedMaterial.mainTexture = texture;

            webCamTextureToMatHelper.Play ();

            isVideoPlaying = false;

            ShowAllVideoUI ();
        }

        private void ShowAllVideoUI ()
        {
            containerDropdown.interactable = true;
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
            containerDropdown.interactable = false;
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
            SceneManager.LoadScene ("NatCorderWithOpenCVForUnityExample");
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
            webCamTextureToMatHelper.requestedIsFrontFacing = !webCamTextureToMatHelper.IsFrontFacing ();
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
        /// Raises the container dropdown value changed event.
        /// </summary>
        public void OnContainerDropdownValueChanged (int result)
        {
            Debug.Log (result);
            if ((int)container != result + 1) {
                container = (ContainerPreset)(result + 1);
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

            if (isVideoRecording) {                
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

            if (isVideoPlaying || isVideoRecording || string.IsNullOrEmpty (videoPath))
                return;

            if (System.IO.Path.GetExtension (videoPath) == ".gif") {                
                Debug.LogWarning ("GIF format video playback is not supported.");
                return;
            }

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

            if (isVideoPlaying || isVideoRecording || string.IsNullOrEmpty (videoPath))
                return;

            // Playback the video
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.OpenWithDefaultApp (videoPath);
            #elif UNITY_IOS
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

            if (isVideoPlaying || isVideoRecording || string.IsNullOrEmpty (videoPath))
                return;
            
            NatShare.Share (videoPath,
                () => {
                    Debug.Log ("sharing is complete.");
                });
        }

        /// <summary>
        /// Raises the save to camera roll button click event.
        /// </summary>
        public void OnSaveToCameraRollButtonClick ()
        {
            Debug.Log ("OnSaveToCameraRollButtonClick ()");

            if (isVideoPlaying || isVideoRecording || string.IsNullOrEmpty (videoPath))
                return;
            
            NatShare.SaveToCameraRoll (videoPath, "NatCorderWithOpenCVForUnityExample");
        }

        public enum ResolutionPreset
        {
            Lowest,
            _640x480,
            _1280x720,
            _1920x1080,
            Highest,
        }

        private void Dimensions (ResolutionPreset preset, out int width, out int height)
        {
            switch (preset) {
            case ResolutionPreset.Lowest:
                width = height = 50;
                break;
            case ResolutionPreset._640x480:
                width = 640;
                height = 480;
                break;
            case ResolutionPreset._1920x1080:
                width = 1920;
                height = 1080;
                break;
            case ResolutionPreset.Highest:
                width = height = 9999;
                break;
            case ResolutionPreset._1280x720:
            default:
                width = 1280;
                height = 720;
                break;
            }
        }

        public enum ContainerPreset
        {
            MP4 = 1,
            GIF
        }
    }
}