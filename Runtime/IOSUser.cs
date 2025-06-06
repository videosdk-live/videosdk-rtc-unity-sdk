using Newtonsoft.Json;
using System;
using UnityEngine;

namespace live.videosdk
{
#if UNITY_IOS
    internal class IOSUser : IUser
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
        public event Action<int, int> OnTexureSizeChangedCallback;

        private IMeetingControlls _meetControlls;
        private IVideoSDKDTO _videoSdkDto;
        public IOSUser(IParticipant participantData, IMeetingControlls meetControlls, IVideoSDKDTO videoSDK)
        {
            if (participantData != null)
            {
                IsLocal = participantData.IsLocal;
                ParticipantId = participantData.Id;
                ParticipantName = participantData.Name;
                _meetControlls = meetControlls;
                _videoSdkDto = videoSDK;
                RegiesterCallBacks();
            }

        }

        private void RegiesterCallBacks()
        {
            IOSParticipantCallback.Instance.SubscribeToStreamEnabled(OnStreamEnable);
            IOSParticipantCallback.Instance.SubscribeToStreamDisabled(OnStreamDisable);
            IOSParticipantCallback.Instance.SubscribeToFrameReceived(OnVideoFrameReceive);
            IOSParticipantCallback.Instance.SubscribeToPauseStream(OnStreamPaused);
            IOSParticipantCallback.Instance.SubscribeToResumeStream(OnStreamResumed);
        }

        private void UnRegisterCallBacks()
        {
            IOSParticipantCallback.Instance.UnsubscribeFromStreamEnabled(OnStreamEnable);
            IOSParticipantCallback.Instance.UnsubscribeFromStreamDisabled(OnStreamDisable);
            IOSParticipantCallback.Instance.UnsubscribeFromFrameReceived(OnVideoFrameReceive);
            IOSParticipantCallback.Instance.UnsubscribeFromPauseStream(OnStreamPaused);
            IOSParticipantCallback.Instance.UnsubscribeFromResumeStream(OnStreamResumed);
        }

        public void OnParticipantLeft()
        {
            _meetControlls = null;
            OnParticipantLeftCallback?.Invoke();
            UnRegisterCallBacks();
        }

        private void OnStreamEnable(string id, string kind)
        {
            if (!id.Equals(ParticipantId)) return;
            _videoSdkDto.SendDTO("INFO", $"StreamEnabled:- Kind: {kind} Id: {id} ParticipantName: {ParticipantName}");
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

        private void OnStreamDisable(string id, string kind)
        {
            if (!id.Equals(ParticipantId)) return;
            _videoSdkDto.SendDTO("INFO", $"StreamDisable:- Kind: {kind} Id: {id} ParticipantName: {ParticipantName}");
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

        private void OnStreamResumed(string id, string kind)
        {
            if (!id.Equals(ParticipantId)) return;
            _videoSdkDto.SendDTO("INFO", $"StreamResumed:- Kind: {kind} Id: {id} ParticipantName: {ParticipantName}");
            RunOnUnityMainThread(() =>
            {
                if (Enum.TryParse(kind, true, out StreamKind streamKind))
                {
                    OnStreamResumedCallaback?.Invoke(streamKind);

                }
            });
        }

        private void OnStreamPaused(string id, string kind)
        {
            if (!id.Equals(ParticipantId)) return;
            _videoSdkDto.SendDTO("INFO", $"StreamPaused:- Kind: {kind} Id: {id} ParticipantName: {ParticipantName}");
            RunOnUnityMainThread(() =>
            {
                if (Enum.TryParse(kind, true, out StreamKind streamKind))
                {
                    OnStreamPausedCallaback?.Invoke(streamKind);
                }
            });
        }


        public void RunOnUnityMainThread(Action action)
        {
            if (action != null)
            {
                MainThreadDispatcher.Instance.Enqueue(action);
            }
        }

        #region CallToNative
        public void ToggleWebCam(bool status, CustomVideoStream customVideoStream)
        {
            if (_meetControlls == null)
            {
                Debug.LogError("It seems you don't have active meet instance, please join meet first");
                return;
            }

            if (customVideoStream == null && status)
            {
                customVideoStream = new CustomVideoStream(VideoEncoderConfig.h90p_w160p);
            }

            string customVideoStreamStr = JsonConvert.SerializeObject(customVideoStream);

            _meetControlls.ToggleWebCam(IsLocal, status, ParticipantId, customVideoStreamStr);
        }
        public void ToggleMic(bool status)
        {
            if (_meetControlls == null)
            {
                Debug.LogError("It seems you don't have active meet instance, please join meet first");
                return;
            }
            _meetControlls.ToggleMic(IsLocal, status, ParticipantId);
        }

        public void Remove()
        {
            if (_meetControlls == null)
            {
                Debug.LogError("It seems you don't have active meet instance, please join meet first");
                return;
            }
            _meetControlls.Remove(ParticipantId);
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

    }

#endif
}