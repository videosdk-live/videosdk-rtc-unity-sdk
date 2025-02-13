using System;
using System.Collections;
using System.Collections.Generic;
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
        public event Action<string> OnStreamEnabledCallaback;
        public event Action<string> OnStreamDisabledCallaback;
        public event Action OnParticipantLeftCallback;
        public event Action<string> OnStreamPausedCallaback;
        public event Action<string> OnStreamResumedCallaback;
        private IMeetingControlls _meetControlls;
        private IVideoSDKDTO _videoSdkDto;
        public AndroidUser(IParticipant participantData, IMeetingControlls meetControlls,IVideoSDKDTO videoSDK)
        {
            if (participantData != null)
            {
                IsLocal = participantData.IsLocal;
                ParticipantId = participantData.ParticipantId;
                ParticipantName = participantData.Name;
                _meetControlls = meetControlls;
                _videoSdkDto = videoSDK;
                RegiesterCallBack();
            }

        }

        private void RegiesterCallBack()
        {
            using (var pluginClass = new AndroidJavaClass(Meeting.packageName))
            {
                pluginClass.CallStatic("registerParticipantCallback", ParticipantId, this);
            }
        }

        public void OnParticipantLeft()
        {
            _meetControlls = null;
            OnParticipantLeftCallback?.Invoke();
        }

        public override void OnStreamEnabled(string kind)
        {
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
                OnStreamEnabledCallaback?.Invoke(kind);
            });
        }

        public override void OnStreamDisabled(string kind)
        {
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
                OnStreamDisabledCallaback?.Invoke(kind);
            });
            
        }

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
        public override void OnStreamPaused(string kind)
        {
            _videoSdkDto.SendDTO("INFO", $"StreamResumed:- Kind: {kind} Id: {ParticipantId} ParticipantName: {ParticipantName}");
            RunOnUnityMainThread(() =>
            {
                OnStreamResumedCallaback?.Invoke(kind);
            });
        }
        public override void OnStreamResumed(string kind)
        {
            _videoSdkDto.SendDTO("INFO", $"StreamPaused:- Kind: {kind} Id: {ParticipantId} ParticipantName: {ParticipantName}");
            RunOnUnityMainThread(() =>
            {
                OnStreamPausedCallaback?.Invoke(kind);
            });
        }


        #region CallToNative
        public void ToggleWebCam(bool status)
        {
            if (_meetControlls == null)
            {
                Debug.LogError("It seems you don't have active meet instance, please join meet first");
                return;
            }
            _meetControlls.ToggleWebCam(status,ParticipantId);
        }
        public void ToggleMic(bool status)
        {
            if (_meetControlls == null)
            {
                Debug.LogError("It seems you don't have active meet instance, please join meet first");
                return;
            }
            _meetControlls.ToggleMic(status,ParticipantId);
        }

        public void PauseStream(string kind)
        {
            if (_meetControlls == null)
            {
                Debug.LogError("It seems you don't have active meet instance, please join meet first");
                return;
            }
            _meetControlls.PauseStream(ParticipantId,kind);
        }

        public void ResumeStream(string kind)
        {
            if (_meetControlls == null)
            {
                Debug.LogError("It seems you don't have active meet instance, please join meet first");
                return;
            }
            _meetControlls.ResumeStream(ParticipantId, kind);
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