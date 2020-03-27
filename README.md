# NatCorder With OpenCVForUnity Example

* An example of a video recording app by using NatCorder and OpenCVForUnity.
* An example of native sharing and save to the camera roll using NatShare API.


## Environment
* Anddroid (Pixel) / iOS (iPhone8, iPhone6s)
* Unity >= 2018.3+ (Unity 2019.2 or higher is recommended due to [the Unity issue](https://issuetracker.unity3d.com/issues/android-video-player-cannot-play-files-located-in-the-persistent-data-directory-on-android-10?_ga=2.235187912.870386860.1577555830-195736471.1541757609))
* Scripting backend MONO / IL2CPP
* [NatCorder - Video Recording API](https://assetstore.unity.com/packages/tools/integration/natcorder-video-recording-api-102645?aid=1011l4ehR) 1.7.1+ 
* [NatShare - Mobile Sharing API](https://assetstore.unity.com/packages/tools/integration/natshare-mobile-sharing-api-117705?aid=1011l4ehR) 1.2.2+ 
* [OpenCV for Unity](https://assetstore.unity.com/packages/tools/integration/opencv-for-unity-21088?aid=1011l4ehR) 2.3.8+ 


## Demo
* Android [NatCorderWithOpenCVForUnityExample.apk](https://github.com/EnoxSoftware/NatCorderWithOpenCVForUnityExample/releases)


## Note
* [Using HEIF or HEVC media on Apple devices](https://support.apple.com/en-us/HT207022)


## Setup
1. Download the latest release unitypackage. [NatCorderWithOpenCVForUnityExample.unitypackage](https://github.com/EnoxSoftware/NatCorderWithOpenCVForUnityExample/releases)
1. Create a new project. (NatCorderWithOpenCVForUnityExample)
1. Import NatCorder.
1. Import NatShare.
1. Import OpenCVForUnity.
    * Setup the OpenCVForUnity. (Tools > OpenCV for Unity > Set Plugin Import Settings)
1. Import the NatCorderWithOpenCVForUnityExample.unitypackage.
1. Change the "Minimum API Level" to 24 or higher in the "Player Settings (Androd)" Inspector.
1. Change the "Target minimum iOS Version" to 11 or higher in the "Player Settings (iOS)" Inspector.
    * Set the reason for accessing the camera in "cameraUsageDescription".
    * Set the reason for accessing the microphone in "microphoneUsageDescription".
1. Add the "Assets/NatCorderWithOpenCVForUnityExample/*.unity" files to the "Scenes In Build" list in the "Build Settings" window.
1. Build and Deploy to Android and iOS.


## Android Instructions
Build requires Android SDK Platform 29 or higher.


## iOS Instructions
After building an Xcode project from Unity, add the following keys to the `Info.plist` file with a good description:
- `NSPhotoLibraryUsageDescription`
- `NSPhotoLibraryAddUsageDescription`


## ScreenShot
![screenshot01.jpg](screenshot01.jpg) 

