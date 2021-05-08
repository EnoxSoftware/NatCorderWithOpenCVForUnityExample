# NatCorder With OpenCVForUnity Example

* An example of a video recording app by using NatCorder and OpenCVForUnity.
* An example of native sharing and save to the camera roll using NatShare API.


## Environment
* Anddroid (Pixel) / iOS (iPhoneSE)
* Unity >= 2019.4.26f1+
* Scripting backend MONO / IL2CPP
* [NatCorder - Video Recording API](https://assetstore.unity.com/packages/tools/integration/natcorder-video-recording-api-102645?aid=1011l4ehR) 1.8.0+ 
* [NatShare - Mobile Sharing API](https://github.com/natsuite/NatShare) 1.2.5+ 
* [OpenCV for Unity](https://assetstore.unity.com/packages/tools/integration/opencv-for-unity-21088?aid=1011l4ehR) 2.4.4+ 


## Demo
* Android [NatCorderWithOpenCVForUnityExample.apk](https://github.com/EnoxSoftware/NatCorderWithOpenCVForUnityExample/releases)


## Note
* [Using HEIF or HEVC media on Apple devices](https://support.apple.com/en-us/HT207022)


## Setup
1. Download the latest release unitypackage. [NatCorderWithOpenCVForUnityExample.unitypackage](https://github.com/EnoxSoftware/NatCorderWithOpenCVForUnityExample/releases)
1. Create a new project. (NatCorderWithOpenCVForUnityExample)
1. Import NatCorder.
1. Import NatShare. (NatShare should be installed using the Unity Package Manager. See [README](https://github.com/natsuite/NatShare))
1. Import OpenCVForUnity.
    * Setup the OpenCVForUnity. (Tools > OpenCV for Unity > Set Plugin Import Settings)
1. Import the NatCorderWithOpenCVForUnityExample.unitypackage.
1. Change the "Minimum API Level" to 24 or higher in the "Player Settings (Androd)" Inspector.
1. Change the "Target minimum iOS Version" to 13 or higher in the "Player Settings (iOS)" Inspector.
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

