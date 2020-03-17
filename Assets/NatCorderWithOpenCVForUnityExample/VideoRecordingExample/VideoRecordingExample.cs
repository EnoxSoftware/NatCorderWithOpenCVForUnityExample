using NatCorder;
using NatCorder.Clocks;
using NatCorder.Inputs;
using NatShare;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.UnityUtils.Helper;
using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

namespace NatCorderWithOpenCVForUnityExample
{
    /// <summary>
    /// VideoRecording Example
    /// </summary>
    [RequireComponent(typeof(WebCamTextureToMatHelper), typeof(AudioSource), typeof(VideoPlayer))]
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

        [Space(20)]
        [Header("Recording")]

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

        [Header("Microphone")]

        /// <summary>
        /// Determines if record microphone audio.
        /// </summary>
        public bool recordMicrophoneAudio;

        /// <summary>
        /// The record microphone audio toggle.
        /// </summary>
        public Toggle recordMicrophoneAudioToggle;

        /// <summary>
        /// The microphone frequency.
        /// </summary>
        public MicrophoneFrequencyPreset microphoneFrequency = MicrophoneFrequencyPreset._48000;

        /// <summary>
        /// The microphone frequency.
        /// </summary>
        public Dropdown microphoneFrequencyDropdown;

        [Space(20)]

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

        IMediaRecorder videoRecorder;

        AudioSource microphoneSource;

        AudioInput audioInput;

        IClock recordingClock;

        CancellationTokenSource cancellationTokenSource;

        const int MAX_RECORDING_TIME = 10; // Seconds

        string videoPath = "";

        VideoPlayer videoPlayer;

        bool isVideoPlaying;

        bool isVideoRecording;

        bool isFinishWriting;

        int frameCount;

        int recordEveryNthFrame;

        int recordingWidth;
        int recordingHeight;
        int videoFramerate;
        int audioSampleRate;
        int audioChannelCount;
        int videoBitrate;
        float frameDuration;

        ComicFilter comicFilter;

        string exampleTitle = "";
        string exampleSceneTitle = "";
        string settingInfo1 = "";
        string settingInfo2 = "";
        string settingInfoGIF = "";
        string settingInfoJPG = "";
        Scalar textColor = new Scalar(255, 255, 255, 255);
        Point textPos = new Point();

        /// <summary>
        /// The FPS monitor.
        /// </summary>
        FpsMonitor fpsMonitor;

        // Use this for initialization
        void Start()
        {
            exampleTitle = "[NatCorderWithOpenCVForUnity Example] (" + NatCorderWithOpenCVForUnityExample.GetNatCorderVersion() + ")";
            exampleSceneTitle = "- Video Recording Example";

            fpsMonitor = GetComponent<FpsMonitor>();

            webCamTextureToMatHelper = gameObject.GetComponent<WebCamTextureToMatHelper>();
            int width, height;
            Dimensions(requestedResolution, out width, out height);
            webCamTextureToMatHelper.requestedWidth = width;
            webCamTextureToMatHelper.requestedHeight = height;

#if UNITY_ANDROID && !UNITY_EDITOR
            // Avoids the front camera low light issue that occurs in only some Android devices (e.g. Google Pixel, Pixel2).
            webCamTextureToMatHelper.avoidAndroidFrontCameraLowLightIssue = true;
#endif
            webCamTextureToMatHelper.Initialize();

            microphoneSource = gameObject.GetComponent<AudioSource>();

            videoPlayer = gameObject.GetComponent<VideoPlayer>();

            comicFilter = new ComicFilter();

            // Update GUI state
            requestedResolutionDropdown.value = (int)requestedResolution;
            containerDropdown.value = (int)container;
            string[] enumNames = System.Enum.GetNames(typeof(MicrophoneFrequencyPreset));
            int index = Array.IndexOf(enumNames, microphoneFrequency.ToString());
            microphoneFrequencyDropdown.value = index;
            applyComicFilterToggle.isOn = applyComicFilter;
            recordMicrophoneAudioToggle.isOn = recordMicrophoneAudio;
        }

        /// <summary>
        /// Raises the webcam texture to mat helper initialized event.
        /// </summary>
        public void OnWebCamTextureToMatHelperInitialized()
        {
            Debug.Log("OnWebCamTextureToMatHelperInitialized");

            Mat webCamTextureMat = webCamTextureToMatHelper.GetMat();

            texture = new Texture2D(webCamTextureMat.cols(), webCamTextureMat.rows(), TextureFormat.RGBA32, false);

            gameObject.GetComponent<Renderer>().material.mainTexture = texture;

            gameObject.transform.localScale = new Vector3(webCamTextureMat.cols(), webCamTextureMat.rows(), 1);
            Debug.Log("Screen.width " + Screen.width + " Screen.height " + Screen.height + " Screen.orientation " + Screen.orientation);

            if (fpsMonitor != null)
            {
                fpsMonitor.Add("width", webCamTextureToMatHelper.GetWidth().ToString());
                fpsMonitor.Add("height", webCamTextureToMatHelper.GetHeight().ToString());
                fpsMonitor.Add("isFrontFacing", webCamTextureToMatHelper.IsFrontFacing().ToString());
                fpsMonitor.Add("rotate90Degree", webCamTextureToMatHelper.rotate90Degree.ToString());
                fpsMonitor.Add("flipVertical", webCamTextureToMatHelper.flipVertical.ToString());
                fpsMonitor.Add("flipHorizontal", webCamTextureToMatHelper.flipHorizontal.ToString());
                fpsMonitor.Add("orientation", Screen.orientation.ToString());
            }


            float width = webCamTextureMat.width();
            float height = webCamTextureMat.height();

            float widthScale = (float)Screen.width / width;
            float heightScale = (float)Screen.height / height;
            if (widthScale < heightScale)
            {
                Camera.main.orthographicSize = (width * (float)Screen.height / (float)Screen.width) / 2;
            }
            else
            {
                Camera.main.orthographicSize = height / 2;
            }
        }

        /// <summary>
        /// Raises the webcam texture to mat helper disposed event.
        /// </summary>
        public void OnWebCamTextureToMatHelperDisposed()
        {
            Debug.Log("OnWebCamTextureToMatHelperDisposed");

            CancelRecording();
            StopVideo();

            if (texture != null)
            {
                Texture2D.Destroy(texture);
                texture = null;
            }
        }

        /// <summary>
        /// Raises the webcam texture to mat helper error occurred event.
        /// </summary>
        /// <param name="errorCode">Error code.</param>
        public void OnWebCamTextureToMatHelperErrorOccurred(WebCamTextureToMatHelper.ErrorCode errorCode)
        {
            Debug.Log("OnWebCamTextureToMatHelperErrorOccurred " + errorCode);
        }

        // Update is called once per frame
        void Update()
        {
            if (webCamTextureToMatHelper.IsPlaying() && webCamTextureToMatHelper.DidUpdateThisFrame())
            {

                Mat rgbaMat = webCamTextureToMatHelper.GetMat();

                if (applyComicFilter)
                    comicFilter.Process(rgbaMat, rgbaMat);

                if (isVideoRecording && !isFinishWriting)
                {
                    textPos.x = 5;
                    textPos.y = rgbaMat.rows() - 70;
                    Imgproc.putText(rgbaMat, exampleTitle, textPos, Imgproc.FONT_HERSHEY_SIMPLEX, 0.5, textColor, 1, Imgproc.LINE_AA, false);
                    textPos.y = rgbaMat.rows() - 50;
                    Imgproc.putText(rgbaMat, exampleSceneTitle, textPos, Imgproc.FONT_HERSHEY_SIMPLEX, 0.5, textColor, 1, Imgproc.LINE_AA, false);
                    if (container == ContainerPreset.MP4 || container == ContainerPreset.HEVC)
                    {
                        textPos.y = rgbaMat.rows() - 30;
                        Imgproc.putText(rgbaMat, settingInfo1, textPos, Imgproc.FONT_HERSHEY_SIMPLEX, 0.5, textColor, 1, Imgproc.LINE_AA, false);
                        textPos.y = rgbaMat.rows() - 10;
                        Imgproc.putText(rgbaMat, settingInfo2, textPos, Imgproc.FONT_HERSHEY_SIMPLEX, 0.5, textColor, 1, Imgproc.LINE_AA, false);
                    }
                    else if (container == ContainerPreset.GIF)
                    {
                        textPos.y = rgbaMat.rows() - 30;
                        Imgproc.putText(rgbaMat, settingInfoGIF, textPos, Imgproc.FONT_HERSHEY_SIMPLEX, 0.5, textColor, 1, Imgproc.LINE_AA, false);
                    }
                    else if (container == ContainerPreset.JPG)
                    {
                        textPos.y = rgbaMat.rows() - 30;
                        Imgproc.putText(rgbaMat, settingInfoJPG, textPos, Imgproc.FONT_HERSHEY_SIMPLEX, 0.5, textColor, 1, Imgproc.LINE_AA, false);
                    }
                }

                // Restore the coordinate system of the image by OpenCV's Flip function.
                Utils.fastMatToTexture2D(rgbaMat, texture);

                // Record frames
                if (videoRecorder != null && (isVideoRecording && !isFinishWriting) && frameCount++ % recordEveryNthFrame == 0)
                {
                    videoRecorder.CommitFrame((IntPtr)rgbaMat.dataAddr(), recordingClock.timestamp);
                }
            }

            if (isVideoPlaying && videoPlayer.isPlaying)
            {
                gameObject.GetComponent<Renderer>().sharedMaterial.mainTexture = videoPlayer.texture;
            }
        }

        private async Task StartRecording()
        {
            if (isVideoPlaying || isVideoRecording || isFinishWriting)
                return;

            Debug.Log("StartRecording ()");

            // First make sure recording microphone is only on MP4 or HEVC
            recordMicrophoneAudio = recordMicrophoneAudioToggle.isOn;
            recordMicrophoneAudio &= (container == ContainerPreset.MP4 || container == ContainerPreset.HEVC);
            // Create recording configurations
            recordingWidth = webCamTextureToMatHelper.GetWidth();
            recordingHeight = webCamTextureToMatHelper.GetHeight();
            videoFramerate = 30;
            audioSampleRate = recordMicrophoneAudio ? AudioSettings.outputSampleRate : 0;
            audioChannelCount = recordMicrophoneAudio ? (int)AudioSettings.speakerMode : 0;
            videoBitrate = (int)(960 * 540 * 11.4f);
            frameDuration = 0.1f;

            // Create video recorder
            recordingClock = new RealtimeClock();
            if (container == ContainerPreset.MP4)
            {
                videoRecorder = new MP4Recorder(
                    recordingWidth,
                    recordingHeight,
                    videoFramerate,
                    audioSampleRate,
                    audioChannelCount
                );
                recordEveryNthFrame = 1;
            }
            else if (container == ContainerPreset.HEVC)
            {
                videoRecorder = new HEVCRecorder(
                    recordingWidth,
                    recordingHeight,
                    videoFramerate,
                    audioSampleRate,
                    audioChannelCount
                );
                recordEveryNthFrame = 1;
            }
            else if (container == ContainerPreset.GIF)
            {
                videoRecorder = new GIFRecorder(
                    recordingWidth,
                    recordingHeight,
                    frameDuration
                );
                recordEveryNthFrame = 5;
            }
            else if (container == ContainerPreset.JPG) // macOS and Windows platform only.
            {
                videoRecorder = new JPGRecorder(
                    recordingWidth,
                    recordingHeight
                );
                recordEveryNthFrame = 5;
            }
            frameCount = 0;



            // Start recording
            isVideoRecording = true;

            HideAllVideoUI();
            recordVideoButton.interactable = true;
            recordVideoButton.GetComponentInChildren<UnityEngine.UI.Text>().color = Color.red;

            CreateSettingInfo();

            // Start microphone and create audio input
            if (recordMicrophoneAudio)
            {
                await StartMicrophone();
                audioInput = new AudioInput(videoRecorder, recordingClock, microphoneSource, true);
            }

            // Start countdown
            cancellationTokenSource = new CancellationTokenSource();
            try
            {
                Debug.Log("Countdown start.");
                await CountdownAsync(
                    sec =>
                    {
                        string str = "Recording";
                        for (int i = 0; i < sec; i++)
                        {
                            str += ".";
                        }

                        if (fpsMonitor != null) fpsMonitor.consoleText = str;

                    }, MAX_RECORDING_TIME, cancellationTokenSource.Token);
                Debug.Log("Countdown end.");
            }
            catch (OperationCanceledException e)
            {
                if (e.CancellationToken == cancellationTokenSource.Token)
                {
                    Debug.Log("Countdown canceled.");
                }
            }
            cancellationTokenSource.Dispose();
            cancellationTokenSource = null;

            if (this != null && isActiveAndEnabled)
                await FinishRecording();
        }

        private void CancelRecording()
        {
            if (!isVideoRecording || isFinishWriting)
                return;

            if (cancellationTokenSource != null)
                cancellationTokenSource.Cancel(true);
        }

        private async Task FinishRecording()
        {
            if (!isVideoRecording || isFinishWriting)
                return;

            // Stop the microphone if we used it for recording
            if (recordMicrophoneAudio)
            {
                StopMicrophone();
                audioInput.Dispose();
            }

            if (fpsMonitor != null) fpsMonitor.consoleText = "FinishWriting...";

            // Stop recording
            isFinishWriting = true;
            try
            {
                var path = await videoRecorder.FinishWriting();
                videoPath = path;
                Debug.Log("Saved recording to: " + videoPath);
                savePathInputField.text = videoPath;
            }
            catch (ApplicationException e)
            {
                Debug.Log(e.Message);
                savePathInputField.text = e.Message;
            }
            isFinishWriting = false;

            if (fpsMonitor != null) fpsMonitor.consoleText = "";

            ShowAllVideoUI();
            recordVideoButton.GetComponentInChildren<UnityEngine.UI.Text>().color = Color.black;

            isVideoRecording = false;
        }

        private Task<bool> StartMicrophone()
        {
            var task = new TaskCompletionSource<bool>();
            StartCoroutine(CreateMicrophone(granted =>
            {
                microphoneSource.Play();
                task.SetResult(granted);
            }));

            return task.Task;
        }

        private IEnumerator CreateMicrophone(Action<bool> completionHandler)
        {
            // Create a microphone clip
            microphoneSource.loop = true;
            microphoneSource.bypassEffects =
            microphoneSource.bypassListenerEffects = false;
            microphoneSource.clip = Microphone.Start(null, true, MAX_RECORDING_TIME, (int)microphoneFrequency);
            yield return new WaitUntil(() => Microphone.GetPosition(null) > 0);
            completionHandler(true);
        }

        private void StopMicrophone()
        {
            // Stop microphone
            microphoneSource.Stop();
            Microphone.End(null);
        }

        private async Task CountdownAsync(Action<int> countdownHandler, int sec = 10, CancellationToken cancellationToken = default(CancellationToken))
        {
            for (int i = sec; i > 0; i--)
            {
                cancellationToken.ThrowIfCancellationRequested();
                countdownHandler(i);
                await Task.Delay(1000, cancellationToken);
            }
            cancellationToken.ThrowIfCancellationRequested();
            countdownHandler(0);
        }


        private void PlayVideo(string path)
        {
            if (isVideoPlaying || isVideoRecording || isFinishWriting || string.IsNullOrEmpty(path))
                return;

            Debug.Log("PlayVideo ()");

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
            videoPlayer.EnableAudioTrack(0, true);
            videoPlayer.SetTargetAudioSource(0, microphoneSource);

            videoPlayer.prepareCompleted += PrepareCompleted;
            videoPlayer.loopPointReached += EndReached;

            videoPlayer.Prepare();

            HideAllVideoUI();
        }

        private void PrepareCompleted(VideoPlayer vp)
        {
            Debug.Log("PrepareCompleted ()");

            vp.prepareCompleted -= PrepareCompleted;

            vp.Play();

            webCamTextureToMatHelper.Pause();
        }

        private void EndReached(VideoPlayer vp)
        {
            Debug.Log("EndReached ()");

            StopVideo();
        }

        private void StopVideo()
        {
            if (!isVideoPlaying)
                return;

            Debug.Log("StopVideo ()");

            videoPlayer.loopPointReached -= EndReached;

            if (videoPlayer.isPlaying)
                videoPlayer.Stop();

            isVideoPlaying = false;

            if (this != null && isActiveAndEnabled)
            {
                gameObject.GetComponent<Renderer>().sharedMaterial.mainTexture = texture;
                webCamTextureToMatHelper.Play();
                ShowAllVideoUI();
            }
        }

        private void ShowAllVideoUI()
        {
            requestedResolutionDropdown.interactable = true;
            containerDropdown.interactable = true;
            microphoneFrequencyDropdown.interactable = true;
            applyComicFilterToggle.interactable = true;
            recordMicrophoneAudioToggle.interactable = true;
            recordVideoButton.interactable = true;
            savePathInputField.interactable = true;
            playVideoButton.interactable = true;
            playVideoFullScreenButton.interactable = true;
            shareButton.interactable = true;
            saveToCameraRollButton.interactable = true;
        }

        private void HideAllVideoUI()
        {
            requestedResolutionDropdown.interactable = false;
            containerDropdown.interactable = false;
            microphoneFrequencyDropdown.interactable = false;
            applyComicFilterToggle.interactable = false;
            recordMicrophoneAudioToggle.interactable = false;
            recordVideoButton.interactable = false;
            savePathInputField.interactable = false;
            playVideoButton.interactable = false;
            playVideoFullScreenButton.interactable = false;
            shareButton.interactable = false;
            saveToCameraRollButton.interactable = false;
        }

        private void CreateSettingInfo()
        {
            settingInfo1 = "- [" + container + "] SIZE:" + recordingWidth + "x" + recordingHeight + " FPS:" + videoFramerate;
            settingInfo2 = "- ASR:" + audioSampleRate + " ACh:" + audioChannelCount + " VBR:" + videoBitrate + " MicFreq:" + (int)microphoneFrequency;
            settingInfoGIF = "- [" + container + "] SIZE:" + recordingWidth + "x" + recordingHeight + " FrameDur:" + frameDuration;
            settingInfoJPG = "- [" + container + "] SIZE:" + recordingWidth + "x" + recordingHeight;
        }

        /// <summary>
        /// Raises the destroy event.
        /// </summary>
        void OnDestroy()
        {
            webCamTextureToMatHelper.Dispose();

            if (comicFilter != null)
                comicFilter.Dispose();
        }

        /// <summary>
        /// Raises the back button click event.
        /// </summary>
        public void OnBackButtonClick()
        {
            SceneManager.LoadScene("NatCorderWithOpenCVForUnityExample");
        }

        /// <summary>
        /// Raises the play button click event.
        /// </summary>
        public void OnPlayButtonClick()
        {
            webCamTextureToMatHelper.Play();
        }

        /// <summary>
        /// Raises the pause button click event.
        /// </summary>
        public void OnPauseButtonClick()
        {
            webCamTextureToMatHelper.Pause();
        }

        /// <summary>
        /// Raises the stop button click event.
        /// </summary>
        public void OnStopButtonClick()
        {
            webCamTextureToMatHelper.Stop();
        }

        /// <summary>
        /// Raises the change camera button click event.
        /// </summary>
        public void OnChangeCameraButtonClick()
        {
            webCamTextureToMatHelper.requestedIsFrontFacing = !webCamTextureToMatHelper.IsFrontFacing();
        }

        /// <summary>
        /// Raises the requested resolution dropdown value changed event.
        /// </summary>
        public void OnRequestedResolutionDropdownValueChanged(int result)
        {
            if ((int)requestedResolution != result)
            {
                requestedResolution = (ResolutionPreset)result;

                int width, height;
                Dimensions(requestedResolution, out width, out height);

                webCamTextureToMatHelper.Initialize(width, height);
            }
        }

        /// <summary>
        /// Raises the container dropdown value changed event.
        /// </summary>
        public void OnContainerDropdownValueChanged(int result)
        {
#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_EDITOR_WIN || UNITY_EDITOR_OSX)
            if ((ContainerPreset)(result) == ContainerPreset.JPG)
            {
                containerDropdown.value = (int)container;
                return;
            }
#endif

            if ((int)container != result)
            {
                container = (ContainerPreset)(result);
            }
        }

        /// <summary>
        /// Raises the microphone frequency dropdown value changed event.
        /// </summary>
        public void OnMicrophoneFrequencyDropdownValueChanged(int result)
        {
            string[] enumNames = Enum.GetNames(typeof(MicrophoneFrequencyPreset));
            int value = (int)System.Enum.Parse(typeof(MicrophoneFrequencyPreset), enumNames[result], true);

            if ((int)microphoneFrequency != value)
            {
                microphoneFrequency = (MicrophoneFrequencyPreset)value;
            }
        }

        /// <summary>
        /// Raises the apply comic filter toggle value changed event.
        /// </summary>
        public void OnApplyComicFilterToggleValueChanged()
        {
            if (applyComicFilter != applyComicFilterToggle.isOn)
            {
                applyComicFilter = applyComicFilterToggle.isOn;
            }
        }

        /// <summary>
        /// Raises the record microphone audio toggle value changed event.
        /// </summary>
        public void OnRecordMicrophoneAudioToggleValueChanged()
        {
            if (recordMicrophoneAudio != recordMicrophoneAudioToggle.isOn)
            {
                recordMicrophoneAudio = recordMicrophoneAudioToggle.isOn;
            }
        }

        /// <summary>
        /// Raises the record video button click event.
        /// </summary>
        public async void OnRecordVideoButtonClick()
        {
            Debug.Log("OnRecordVideoButtonClick ()");

            if (isVideoPlaying)
                return;

            if (!isVideoRecording && !isFinishWriting)
            {
                await StartRecording();
            }
            else
            {
                CancelRecording();
            }
        }

        /// <summary>
        /// Raises the play video button click event.
        /// </summary>
        public void OnPlayVideoButtonClick()
        {
            Debug.Log("OnPlayVideoButtonClick ()");

            if (isVideoPlaying || isVideoRecording || isFinishWriting || string.IsNullOrEmpty(videoPath))
                return;

            if (System.IO.Path.GetExtension(videoPath) == ".gif")
            {
                Debug.LogWarning("GIF format video playback is not supported.");
                return;
            }
            if (System.IO.Path.GetExtension(videoPath) == "")
            {
                Debug.LogWarning("JPG format video playback is not supported.");
                return;
            }

            // Playback the video
            var prefix = Application.platform == RuntimePlatform.IPhonePlayer ? "file://" : "";
            PlayVideo(prefix + videoPath);
        }

        /// <summary>
        /// Raises the play video full screen button click event.
        /// </summary>
        public void OnPlayVideoFullScreenButtonClick()
        {
            Debug.Log("OnPlayVideoFullScreenButtonClick ()");

            if (isVideoPlaying || isVideoRecording || isFinishWriting || string.IsNullOrEmpty(videoPath))
                return;

            // Playback the video
#if UNITY_EDITOR
            UnityEditor.EditorUtility.OpenWithDefaultApp(videoPath);
#elif UNITY_ANDROID || UNITY_IOS
            var prefix = Application.platform == RuntimePlatform.IPhonePlayer ? "file://" : "";
            Handheld.PlayFullScreenMovie(prefix + videoPath);
#else
            Debug.LogWarning("Full-screen video playback is not supported on this platform.");
#endif
        }

        /// <summary>
        /// Raises the share button click event.
        /// </summary>
        public void OnShareButtonClick()
        {
            Debug.Log("OnShareButtonClick ()");

            if (isVideoPlaying || isVideoRecording || isFinishWriting || string.IsNullOrEmpty(videoPath))
                return;

            using (var payload = new SharePayload("NatCorderWithOpenCVForUnityExample",
                completionHandler: () =>
                {
                    Debug.Log("User shared video!");
                }
            ))
            {
                payload.AddText("User shared video!");
                payload.AddMedia(videoPath);
            }
        }

        /// <summary>
        /// Raises the save to camera roll button click event.
        /// </summary>
        public void OnSaveToCameraRollButtonClick()
        {
            Debug.Log("OnSaveToCameraRollButtonClick ()");

            if (isVideoPlaying || isVideoRecording || isFinishWriting || string.IsNullOrEmpty(videoPath))
                return;

            using (var payload = new SavePayload("NatCorderWithOpenCVForUnityExample",
                completionHandler: () =>
                {
                    Debug.Log("User saved video to camera roll!");
                }
            ))
            {
                payload.AddMedia(videoPath);
            }
        }

        public enum ResolutionPreset
        {
            Lowest,
            _640x480,
            _1280x720,
            _1920x1080,
            Highest,
        }

        private void Dimensions(ResolutionPreset preset, out int width, out int height)
        {
            switch (preset)
            {
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
            MP4,
            HEVC,
            GIF,
            JPG,
        }

        public enum MicrophoneFrequencyPreset
        {
            _16000 = 16000,
            _24000 = 24000,
            _32000 = 32000,
            _44100 = 44100,
            _48000 = 48000,
        }
    }
}