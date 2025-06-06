using System;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

namespace live.videosdk
{
    public sealed class VideoSurface : MonoBehaviour
    {
        private IUser _participant;
        private Renderer _renderer;
        private RawImage _rawImage;
        private Texture2D _videoTexture;
        private VideoSurfaceType _rendertype;

        public event Action<StreamKind> OnStreamEnableCallback;
        public event Action<StreamKind> OnStreamDisableCallback;
        //public event Action<StreamKind> OnStreamPausedCallback;
        //public event Action<StreamKind> OnStreamResumedCallback;

        public string ParticipantId
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

        [SerializeField] bool flipTexture = false;
        public bool FlipTexture
        {
            get => flipTexture;
            set
            {
                if (flipTexture != value)
                {
                    flipTexture = value;
                    Flip(value);
                }
            }
        }

        private void Awake()
        {
            _renderer = GetComponentInChildren<Renderer>();
            _rawImage = GetComponentInChildren<RawImage>();
            _videoTexture = new Texture2D(960, 720, TextureFormat.RGBA32, false);
            Flip(FlipTexture);
        }

        public void SetVideoSurfaceType(VideoSurfaceType type)
        {
            _rendertype = type;
        }

        public void SetTexture(Texture2D texture)
        {
            switch (_rendertype)
            {
                case VideoSurfaceType.RawImage:
                    {
                        // Assign the texture to the UI RawImage
                        _rawImage.texture = texture;
                        break;
                    }
                case VideoSurfaceType.Renderer:
                    {
                        // Assign the texture to the 3D object's material
                        _renderer.material.mainTexture = texture;
                        break;
                    }
                case VideoSurfaceType.None:
                    break;
            }
        }

        public void SetVideoRenderer(RawImage rawImage)
        {
            SetVideoSurfaceType(VideoSurfaceType.RawImage);
            this._rawImage = rawImage;
            Flip(FlipTexture);
        }

        public void SetVideoRenderer(Renderer renderer)
        {
            SetVideoSurfaceType(VideoSurfaceType.Renderer);
            this._renderer = renderer;
            Flip(FlipTexture);
        }

        private void RemoveTexture()
        {
            switch (_rendertype)
            {
                case VideoSurfaceType.RawImage:
                    {
                        // Assign the texture to the UI RawImage
                        _rawImage.texture = null;
                        break;
                    }
                case VideoSurfaceType.Renderer:
                    {
                        // Assign the texture to the 3D object's material
                        _renderer.material.mainTexture = null;
                        break;
                    }
                case VideoSurfaceType.None:
                    break;
            }
        }

        private void Flip(bool status)
        {
            if (_rawImage != null)
            {
                _rawImage.rectTransform.localScale = status ? new Vector3(-1, 1, 1) : new Vector3(1, 1, 1);
            }
            if (_renderer != null)
            {
                _renderer.material.mainTextureScale = status ? new Vector2(-1, 1) : new Vector2(1, 1);
            }
        }

        public void SetParticipant(IParticipant participantData)
        {
            if (_participant != null)
            {
                UnRegisterParticipantCallback();
            }
            _participant = Meeting.GetParticipantById(participantData.Id);
            if (_participant == null)
            {
                Debug.LogError($"Invalid Participant Id: {participantData.Id}. No such participant exist");
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
            //_participant.OnStreamPausedCallaback +=OnStreamPaused;
            //_participant.OnStreamResumedCallaback +=OnStreamResumed;
            RegisterVideoFrameCallbacks();
        }

        private void UnRegisterParticipantCallback()
        {
            if (_participant == null) return;
            _participant.OnStreamDisabledCallaback -= OnStreamDisabled;
            _participant.OnStreamEnabledCallaback -= OnStreamEnabled;
            _participant.OnParticipantLeftCallback -= OnParticipantLeft;
            //_participant.OnStreamPausedCallaback -= OnStreamPaused;
            //_participant.OnStreamResumedCallaback -= OnStreamResumed;
            UnRegisterVideoFrameCallbacks();
        }

        private void UnRegisterVideoFrameCallbacks()
        {
            _participant.OnVideoFrameReceivedCallback -= OnVideoFrameReceived;
            _participant.OnTexureSizeChangedCallback -= OnTexureSizeChanged;

        }
        private void RegisterVideoFrameCallbacks()
        {
            _participant.OnVideoFrameReceivedCallback += OnVideoFrameReceived;
            _participant.OnTexureSizeChangedCallback += OnTexureSizeChanged;

        }

        private void OnParticipantLeft()
        {
            RemoveTexture();
            UnRegisterParticipantCallback();
            _participant = null;
        }

        private void OnStreamEnabled(StreamKind kind)
        {
            OnStreamEnableCallback?.Invoke(kind);
        }

        private void OnStreamDisabled(StreamKind kind)
        {
            if (kind == StreamKind.VIDEO)
            {
                RemoveTexture();
            }

            OnStreamDisableCallback?.Invoke(kind);
        }

        private void OnStreamPaused(StreamKind kind)
        {
            //OnStreamPausedCallback?.Invoke(kind);
        }
        private void OnStreamResumed(StreamKind kind)
        {
            //OnStreamResumedCallback?.Invoke(kind);
        }

        private void OnVideoFrameReceived(byte[] videoStream)
        {
            if (_videoTexture != null)
            {
                _videoTexture.LoadImage(videoStream);
                SetTexture(_videoTexture);
            }
        }

        private void OnTexureSizeChanged(int height, int width)
        {
            Debug.Log($"OnTexureSizeChanged {height}  {width}");
            _videoTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        }

        public void SetVideo(bool status, CustomVideoStream customVideoStream = null)
        {
            if (_participant == null) return;
            //if (!IsLocal)
            //{
            //    Debug.LogError($"{name} participantId {ParticipantId} is not your local participant");
            //    return;
            //}

            if (CamEnabled == status) return;
            _participant.ToggleWebCam(status, customVideoStream);
        }

        public void SetAudio(bool status)
        {
            if (_participant == null) return;
            //if (!IsLocal)
            //{
            //    Debug.LogError($"{name} participantId {ParticipantId} is not your local participant");
            //    return;
            //}

            if (MicEnabled == status) return;

            _participant.ToggleMic(status);
        }


        public void Remove()
        {
            if (_participant == null) return;
            _participant.Remove();
        }


        public void PauseStream(StreamKind kind)
        {
            if (_participant == null) return;
            if (IsLocal)
            {
                Debug.LogError($"{name} participantId {ParticipantId} is your local participant");
                return;
            }
            _participant.PauseStream(kind);
        }
        public void ResumeStream(StreamKind kind)
        {
            if (_participant == null) return;
            if (IsLocal)
            {
                Debug.LogError($"{name} participantId {ParticipantId} is your local participant");
                return;
            }
            _participant.ResumeStream(kind);
        }

        private void OnDestroy()
        {
            UnRegisterParticipantCallback();
        }

        #region Toggle


        #endregion

    }

    public enum VideoSurfaceType
    {
        None,
        RawImage,
        Renderer
    }

}