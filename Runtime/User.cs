using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace live.videosdk
{
    internal class User : ParticipantCallback, IUser
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
        private AndroidJavaClass _pluginClass;
        public User(Participant participantData, AndroidJavaClass pluginClass)
        {
            if (participantData != null)
            {
                IsLocal = participantData.IsLocal;
                ParticipantId = participantData.ParticipantId;
                ParticipantName = participantData.Name;
                _pluginClass = pluginClass;
                RegiesterCallBack();
            }

        }

        private void RegiesterCallBack()
        {
            _pluginClass.CallStatic("registerParticipantCallback", ParticipantId, this);
            _meetControlls = new MeetingControlls(_pluginClass);
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
            catch (FormatException ex)
            {
                Debug.LogError($"Invalid Base64 string: {ex.Message}");
            }
            catch (ArgumentNullException ex)
            {
                Debug.LogError($"Input string is null: {ex.Message}");
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
            _meetControlls.ToggleWebCam(status);
        }
        public void ToggleMic(bool status)
        {
            if (_meetControlls == null)
            {
                Debug.LogError("It seems you don't have active meet instance, please join meet first");
                return;
            }
            _meetControlls.ToggleMic(status);
        }


        #endregion

    }
}