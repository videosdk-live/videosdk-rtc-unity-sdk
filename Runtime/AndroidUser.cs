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
        private IMeetingControlls _meetControlls;
        public AndroidUser(Participant participantData, IMeetingControlls meetControlls)
        {
            if (participantData != null)
            {
                IsLocal = participantData.IsLocal;
                ParticipantId = participantData.ParticipantId;
                ParticipantName = participantData.Name;
                _meetControlls = meetControlls;
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


        #endregion

    }
#endif
}