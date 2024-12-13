using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace live.videosdk
{
    public sealed class VideoSurface : MonoBehaviour
    {
        private IUser _participant;
        private Renderer objectRenderer;
        private RawImage uiImage;
        private Texture2D _videoTexture;
        private VideoSurfaceType _rendertype;

        public event Action<string> OnStreamEnableCallback;
        public event Action<string> OnStreamDisableCallback;

        public string Id
        {
            get
            {
                if (_participant != null)
                {
                    return _participant.ParticipantId;
                }
                return null;
            }
        }
        public bool IsLocal
        {
            get
            {
                if (_participant != null)
                {
                    return _participant.IsLocal;
                }
                return false;
            }
        }

        public string ParticipantName
        {
            get
            {
                if (_participant != null)
                {
                    return _participant.ParticipantName;
                }
                return null;
            }
        }

        public bool CamEnabled
        {
            get
            {
                if (_participant != null)
                {
                    return _participant.CamEnabled;
                }
                return false;
            }
        }

        public bool MicEnabled
        {
            get
            {
                if (_participant != null)
                {
                    return _participant.MicEnabled;
                }
                return false;
            }
        }

        private void Awake()
        {
            objectRenderer = GetComponentInChildren<Renderer>();
            uiImage = GetComponentInChildren<RawImage>();
            _videoTexture = new Texture2D(720, 480, TextureFormat.RGBA32, false);
        }

        public void SetVideoSurfaceType(VideoSurfaceType type)
        {
            _rendertype = type;
        }

        private void SetTexture(Texture2D texture)
        {
            switch (_rendertype)
            {
                case VideoSurfaceType.RawImage:
                    {
                        // Assign the texture to the UI RawImage
                        uiImage.texture = texture;
                        break;
                    }
                case VideoSurfaceType.Renderer:
                    {
                        // Assign the texture to the 3D object's material
                        objectRenderer.material.mainTexture = texture;
                        break;
                    }
            }
        }
        private void RemoveTexture()
        {
            switch (_rendertype)
            {
                case VideoSurfaceType.RawImage:
                    {
                        // Assign the texture to the UI RawImage
                        uiImage.texture = null;
                        break;
                    }
                case VideoSurfaceType.Renderer:
                    {
                        // Assign the texture to the 3D object's material
                        objectRenderer.material.mainTexture = null;
                        break;
                    }
            }
        }

        public void SetParticipant(IParticipant participantData)
        {
            if (_participant != null)
            {
                UnRegisterParticipantCallback();
            }
            _participant = Meeting.GetParticipantById(participantData.ParticipantId);
            if (_participant == null)
            {
                Debug.LogError($"Invalid Participant Id: {participantData.ParticipantId}. No such participant exist");
                return;
            }

        }

        public void SetEnable(bool status)
        {
            switch (status)
            {
                case true:
                    RegisterParticipantCallback();
                    break;
                case false:
                    RemoveTexture();
                    UnRegisterParticipantCallback();
                    break;
            }
        }

        private void RegisterParticipantCallback()
        {
            if (_participant == null) return;
            _participant.OnStreamDisabledCallaback += OnStreamDisabled;
            _participant.OnStreamEnabledCallaback += OnStreamEnabled;
            _participant.OnParticipantLeftCallback += OnParticipantLeft;
            RegisterVideoFrameCallbacks();
        }

        private void UnRegisterParticipantCallback()
        {
            if (_participant == null) return;
            _participant.OnStreamDisabledCallaback -= OnStreamDisabled;
            _participant.OnStreamEnabledCallaback -= OnStreamEnabled;
            _participant.OnParticipantLeftCallback -= OnParticipantLeft;
            UnRegisterVideoFrameCallbacks();
        }

        private void UnRegisterVideoFrameCallbacks()
        {
            _participant.OnVideoFrameReceivedCallback -= OnVideoFrameReceived;

        }
        private void RegisterVideoFrameCallbacks()
        {
            _participant.OnVideoFrameReceivedCallback += OnVideoFrameReceived;
        }

        private void OnParticipantLeft()
        {
            RemoveTexture();
            UnRegisterParticipantCallback();
            _participant = null;
        }

        private void OnStreamEnabled(string kind)
        {
            OnStreamEnableCallback?.Invoke(kind);
        }

        private void OnStreamDisabled(string kind)
        {
            if (kind.Equals("video"))
            {
                RemoveTexture();
            }

            OnStreamDisableCallback?.Invoke(kind);
        }

        private void OnVideoFrameReceived(byte[] videoStream)
        {
            if (_videoTexture != null)
            {
                _videoTexture.LoadImage(videoStream);
                SetTexture(_videoTexture);
            }
        }

        public void SetVideo(bool status)
        {
            if (_participant == null) return;

            if (!IsLocal)
            {
                Debug.LogError($"{name} participantId {Id} is not your local participant");
                return;
            }
            _participant.ToggleWebCam(status);
        }



        public void SetAudio(bool status)
        {
            if (_participant == null) return;

            if (!IsLocal)
            {
                Debug.LogError($"{name} participantId {Id} is not your local participant");
                return;
            }

            _participant.ToggleMic(status);
        }

        private void OnDestroy()
        {
            UnRegisterParticipantCallback();
        }


    }

    public enum VideoSurfaceType
    {
        RawImage,
        Renderer
    }

}