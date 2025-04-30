using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace live.videosdk
{
#if UNITY_ANDROID
    internal class AndroidUser : AndroidParticipantCallback, IUser
    {
        public bool IsLocal { get; }
        public string ParticipantId { get; }
        public string ParticipantName { get; }
        public bool MicEnabled { get; private set; }
        public bool CamEnabled { get; private set; }

        public event Action<byte[]> OnVideoFrameReceivedCallback;
        public event Action<StreamKind> OnStreamEnabledCallaback;
        public event Action<StreamKind> OnStreamDisabledCallaback;
        public event Action OnParticipantLeftCallback;
        public event Action<StreamKind> OnStreamPausedCallaback;
        public event Action<StreamKind> OnStreamResumedCallaback;

        private IMeetingControlls _meetControlls;
        private IVideoSDKDTO _videoSdkDto;

        private static Dictionary<string, AndroidUser> _instances = new();


        // Delegate for the native callback
        //[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        //public delegate void FrameReceivedCallback(
        //    [MarshalAs(UnmanagedType.LPStr)] string participantId,
        //    IntPtr frameData,
        //    int frameDataSize);

        //private static FrameReceivedCallback frameCallback;

        public AndroidUser(IParticipant participantData, IMeetingControlls meetControlls, IVideoSDKDTO videoSDK)
        {
            if (participantData != null)
            {
                IsLocal = participantData.IsLocal;
                ParticipantId = participantData.ParticipantId;
                ParticipantName = participantData.Name;
                _meetControlls = meetControlls;
                _videoSdkDto = videoSDK;
                RegiesterCallBack();
                Debug.Log($"Androiduser || {ParticipantId} ");
                _instances[ParticipantId] = this;
            }

        }

        private void RegiesterCallBack()
        {
            using (var pluginClass = new AndroidJavaClass(Meeting.packageName))
            {
                pluginClass.CallStatic("registerParticipantCallback", ParticipantId, this);
            }
            //Debug.Log($"RegiesterCallBack {frameCallback == null}");
            //if (frameCallback == null)
            //{
            //    // Create the callback delegate
            //    frameCallback = new FrameReceivedCallback(OnFrameReceivedNative);

            //    IntPtr callbackPtr = Marshal.GetFunctionPointerForDelegate(frameCallback);
            //    string packageName = "live.videosdk.unity.android.VideoSDKNativePlugin";

            //    using (var pluginClass = new AndroidJavaClass(packageName))
            //    {
            //        pluginClass.CallStatic("setUnityFrameCallback", (long)callbackPtr);
            //        Debug.Log("VideoSDK Native Plugin initialized successfully");
            //    }
            //}
        }

        public void OnParticipantLeft()
        {
            _meetControlls = null;
            OnParticipantLeftCallback?.Invoke();
        }

        public override void OnStreamEnabled(string kind)
        {
            Debug.Log($"OnStreamEnabled override {kind}");
            _videoSdkDto.SendDTO("INFO", $"StreamEnabled:- Kind: {kind} Id: {ParticipantId} ParticipantName: {ParticipantName}");
            if (kind.ToLower().Equals("video"))
            {
                CamEnabled = true;
            }
            else if (kind.ToLower().Equals("audio"))
            {
                MicEnabled = true;
            }
            RunOnUnityMainThread(() =>
            {
                if (Enum.TryParse(kind, true, out StreamKind streamKind))
                {

                    OnStreamEnabledCallaback?.Invoke(streamKind);
                }
            });
        }

        public override void OnStreamDisabled(string kind)
        {
            Debug.Log($"OnStreamEnabled disable {kind}");
            _videoSdkDto.SendDTO("INFO", $"StreamDisabled:- Kind: {kind} Id: {ParticipantId} ParticipantName: {ParticipantName} ");
            if (kind.ToLower().Equals("video"))
            {
                CamEnabled = false;
            }
            else if (kind.ToLower().Equals("audio"))
            {
                MicEnabled = false;
            }
            RunOnUnityMainThread(() =>
            {
                if (Enum.TryParse(kind, true, out StreamKind streamKind))
                {

                    OnStreamDisabledCallaback?.Invoke(streamKind);
                }
            });

        }

        //public override void OnPauseStream(string kind)
        //{
        //    _videoSdkDto.SendDTO("INFO", $"PauseStream:- Kind: {kind} Id: {ParticipantId} ParticipantName: {ParticipantName} ");
        //    if (kind.ToLower().Equals("video"))
        //    {
        //        CamEnabled = false;
        //    }
        //    else if (kind.ToLower().Equals("audio"))
        //    {
        //        MicEnabled = false;
        //    }
        //    RunOnUnityMainThread(() =>
        //    {
        //        if (Enum.TryParse(kind, true, out StreamKind streamKind))
        //        {

        //            OnStreamDisabledCallaback?.Invoke(streamKind);
        //        }
        //    });

        //}

        //public override void OnResumeStream(string kind)
        //{
        //    _videoSdkDto.SendDTO("INFO", $"ResumeStream:- Kind: {kind} Id: {ParticipantId} ParticipantName: {ParticipantName} ");
        //    if (kind.ToLower().Equals("video"))
        //    {
        //        CamEnabled = false;
        //    }
        //    else if (kind.ToLower().Equals("audio"))
        //    {
        //        MicEnabled = false;
        //    }
        //    RunOnUnityMainThread(() =>
        //    {
        //        if (Enum.TryParse(kind, true, out StreamKind streamKind))
        //        {
        //            OnStreamDisabledCallaback?.Invoke(streamKind);
        //        }
        //    });

        //}

        // Native callback method
        //[AOT.MonoPInvokeCallback(typeof(FrameReceivedCallback))]
        //private static void OnFrameReceivedNative(string participantId, IntPtr frameData, int frameDataSize)
        //{
        //    try
        //    {

        //        if (!_instances.TryGetValue(participantId, out var instance) || instance.OnVideoFrameReceivedCallback == null)
        //            return;

        //        //Debug.Log($"OnFrameReceivedNative {participantId}");
        //        //Debug.Log($"height {height}   width {width}  ");

        //        // Convert the native pointer to a byte array
        //        byte[] managedArray = new byte[frameDataSize];
        //        Marshal.Copy(frameData, managedArray, 0, frameDataSize);

        //        // Invoke the event on the main thread
        //        if (instance.OnVideoFrameReceivedCallback != null)
        //        {
        //            MainThreadDispatcher.Instance.Enqueue(() =>
        //            {
        //                instance.OnVideoFrameReceivedCallback.Invoke(managedArray);
        //            });
        //        }


        //    }
        //    catch (Exception e)
        //    {
        //        Debug.LogError($"Error in native callback: {e.Message}");
        //    }
        //}


        public override void OnVideoFrameReceived(string videoStream)
        {
            try
            {
                byte[] byteArr = (Convert.FromBase64String(videoStream));
                RunOnUnityMainThread(() =>
                {
                    OnVideoFrameReceivedCallback?.Invoke(byteArr);
                });

            }
            catch (Exception ex)
            {
                Debug.LogError($"Invalid video frame data: {ex.Message}");
            }

        }



        #region CallToNative
        public void ToggleWebCam(bool status)
        {
            if (_meetControlls == null)
            {
                Debug.LogError("It seems you don't have active meet instance, please join meet first");
                return;
            }
            _meetControlls.ToggleWebCam(status, ParticipantId);
        }
        public void ToggleMic(bool status)
        {
            if (_meetControlls == null)
            {
                Debug.LogError("It seems you don't have active meet instance, please join meet first");
                return;
            }
            _meetControlls.ToggleMic(status, ParticipantId);
        }

        public void PauseStream(StreamKind kind)
        {
            if (_meetControlls == null)
            {
                Debug.LogError("It seems you don't have active meet instance, please join meet first");
                return;
            }
            _meetControlls.PauseStream(kind, ParticipantId);
        }

        public void ResumeStream(StreamKind kind)
        {
            if (_meetControlls == null)
            {
                Debug.LogError("It seems you don't have active meet instance, please join meet first");
                return;
            }
            _meetControlls.ResumeStream(kind, ParticipantId);
        }

        #endregion


        // Utility to run actions on the Unity main thread safely
        public static void RunOnUnityMainThread(Action action)
        {
            if (action != null)
            {
                MainThreadDispatcher.Instance.Enqueue(action);
            }
        }

        // Utility to run actions on the Unity main thread safely
        public static void ExecuteOnUnityMainThread(Action action)
        {
            if (action != null)
            {
                MainThreadDispatcher.Instance.Enqueue(action);
            }
        }



    }
#endif
}