using System;
using UnityEngine;

namespace live.videosdk
{
#if UNITY_IOS
    internal class IOSUser :IUser
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
        public IOSUser(Participant participantData, IMeetingControlls meetControlls)
        {
            if (participantData != null)
            {
                IsLocal = participantData.IsLocal;
                ParticipantId = participantData.ParticipantId;
                ParticipantName = participantData.Name;
                _meetControlls = meetControlls;
                RegiesterCallBacks();
            }

        }

        private void RegiesterCallBacks()
        {
            IOSParticipantCallback.Instance.SubscribeToStreamEnabled(OnStreamEnable);
            IOSParticipantCallback.Instance.SubscribeToStreamDisabled(OnStreamDisable);
            IOSParticipantCallback.Instance.SubscribeToFrameReceived(OnVideoFrameReceive);
        }

        private void UnRegisterCallBacks()
        {
            IOSParticipantCallback.Instance.UnsubscribeFromStreamEnabled(OnStreamEnable);
            IOSParticipantCallback.Instance.UnsubscribeFromStreamDisabled(OnStreamDisable);
            IOSParticipantCallback.Instance.UnsubscribeFromFrameReceived(OnVideoFrameReceive);
        }

        public void OnParticipantLeft()
        {
            _meetControlls = null;
            OnParticipantLeftCallback?.Invoke();
            UnRegisterCallBacks();
        }

        private void OnStreamEnable(string id,string kind)
        {
            if (!id.Equals(ParticipantId)) return;
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

        private void OnStreamDisable(string id, string kind)
        {
            if (!id.Equals(ParticipantId)) return;
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

        private void OnVideoFrameReceive(string id, byte[] byteArr)
        {
            if (!id.Equals(ParticipantId)) return;
            try
            {
                //byte[] byteArr = (Convert.FromBase64String(videoStream));

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

        public void RunOnUnityMainThread(Action action)
        {
            if (action != null)
            {
                MainThreadDispatcher.Instance.Enqueue(action);
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

        public void PauseStream(string kind)
        {
            if (_meetControlls == null)
            {
                Debug.LogError("It seems you don't have active meet instance, please join meet first");
                return;
            }
            _meetControlls.PauseStream(ParticipantId, kind);
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

    }

#endif
}