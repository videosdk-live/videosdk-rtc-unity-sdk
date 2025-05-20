using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace live.videosdk
{

    public class PreMeetingController : MonoBehaviour
    {
        WebCamTexture webcamTexture;
        private RawImage rawImage;
        WebCamDevice[] devices;
        private bool isFrontFacing;

        public static Action<VideoDeviceInfo> OnSetCameraDeviceSet;

        public static Action<bool> OnStreamEnableOrDisable;

        private void Awake()
        {
            rawImage = GetComponent<RawImage>();
            devices = WebCamTexture.devices;
            OnSetCameraDeviceSet += SetCameraDevice;
            OnStreamEnableOrDisable += StreamEnableOrDisable;

            SetCameraDevice(Meeting.GetMeetingObject().selectedVideoDevice);
        }

        private void SetCameraDevice(VideoDeviceInfo selectedVideoDevice)
        {
            Debug.Log($"SetCameraDevice {gameObject.activeInHierarchy}");

            if (!gameObject.activeInHierarchy) return;
            //Debug.Log($"SetCameraDevice");
            if (webcamTexture != null)
            {
                webcamTexture.Stop();
                Destroy(webcamTexture);
            }

            bool isFrontFacing = selectedVideoDevice.facingMode == FacingMode.front;

            //if (this.isFrontFacing == isFrontFacing)
            //{
            //    Debug.Log($"Select same camera");
            //    return;
            //}

            this.isFrontFacing = isFrontFacing;

            if (devices.Length > 0)
            {
                WebCamDevice fronCamDevice = devices.FirstOrDefault(device => device.isFrontFacing == this.isFrontFacing);

                if (string.IsNullOrEmpty(fronCamDevice.name))
                {
                    Debug.LogWarning("Requested camera not found. Using default.");
                    fronCamDevice = devices[0];
                }

                string deviceName = fronCamDevice.name;
                webcamTexture = new WebCamTexture(deviceName, 600, 600);
                rawImage.texture = webcamTexture;
                webcamTexture.Play();
                // Wait until webcam is initialized to apply rotation
                StartCoroutine(ApplyRotationLater());
            }
        }

        private IEnumerator ApplyRotationLater()
        {
            Debug.Log($"ApplyRotationLater");
            // Wait a few frames for the webcam to start
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            // Handle rotation
            int rotation = webcamTexture.videoRotationAngle;

            // On many Android devices, back camera comes inverted
            if (!isFrontFacing)
            {
                Debug.Log($"ApplyRotationLater");
                rotation = (rotation + 180) % 360;
            }
            // Correct rotation
            rawImage.rectTransform.localEulerAngles = new Vector3(0, 0, rotation);

            // Mirror if front-facing
            rawImage.uvRect = isFrontFacing
                ? new Rect(1, 0, -1, 1)    // Mirror horizontally
                : new Rect(0, 0, 1, 1);    // Normal
        }

        private void StreamEnableOrDisable(bool isEnable)
        {
            Debug.Log($"StreamEnableOrDisable {isEnable}");
            if (isEnable)
            {
                if (webcamTexture != null && !webcamTexture.isPlaying)
                {
                    webcamTexture.Play();
                    Debug.Log("Camera resumed.");
                }
            }
            else if (webcamTexture != null && webcamTexture.isPlaying)
            {
                webcamTexture.Stop();
                Debug.Log("Camera stopped.");
            }
        }
    }
}