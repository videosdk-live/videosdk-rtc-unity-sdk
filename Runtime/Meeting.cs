using System;
using System.Collections;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Concurrent;

namespace live.videosdk
{
    public sealed class Meeting : MonoBehaviour
    {
        public const string packageName = "live.videosdk.unity.android.VideoSDKUnityPlugin";
        private static Meeting _instance;
        private IApiCaller _apiCaller;
        private const string _meetingUri = "https://api.videosdk.live/v2/rooms";
        private const string _customMeetingUri = "https://api.videosdk.live/v1/prebuilt/meetings/";
        private const string _authMeetingUri = "https://api.videosdk.live/v2/rooms/validate/";
        private static readonly ConcurrentDictionary<string, IUser> _participantsDict = new ConcurrentDictionary<string, IUser>();
        public string MeetingID { get; private set; }
        public static MeetingState MeetingState { get { return _meetState; } }
        private static MeetingState _meetState;
        private IMeetingActivity _meetingActivity;
        private IVideoSDKDTO _videoSdkDto;
        private const string _packageVersion = "2.1.0";
        #region Callbacks For User
        public event Action<string> OnCreateMeetingIdCallback;
        public event Action<string> OnCreateMeetingIdFailedCallback;
        public event Action<IParticipant> OnParticipantJoinedCallback;
        public event Action<IParticipant> OnParticipantLeftCallback;
        public event Action<MeetingState> OnMeetingStateChangedCallback;
        public event Action<Error> OnErrorCallback;
        public event Action<string> OnSpeakerChangedCallback;
        public event Action OnCallHangupCallback;
        public event Action OnCallStartedCallback;
        public event Action OnCallRingingCallback;
        public event Action<string> OnJoinMeetingFailedCallback;
        public event Action<StreamKind> OnPausedAllStreamsCallback;
        public event Action<StreamKind> OnResumedAllStreamsCallback;

        public event Action<string, Action, Action> OnWebcamRequestedCallback;

        public event Action<string, Action, Action> OnMicRequestedCallback;

        private IUser _localParticipant;


        //public event Action<string, string> OnAvailableAudioDevicesCallback;
        public event Action<AudioDeviceInfo[], AudioDeviceInfo> OnAudioDeviceChangedCallback;

        //public event Action<string, string> OnAvailableVideoDevicesCallback;

        #endregion

#if UNITY_ANDROID
        private AndroidJavaObject _applicationContext;
        private AndroidJavaClass _pluginClass;
        private AndroidJavaObject _currentActivity;

        private void InitializeVideoSDK()
        {
            string result = _pluginClass?.CallStatic<string>("init", _applicationContext);
            if (!result.Equals("Success"))
            {
                throw new InvalidOperationException(result);
            }

        }

        private void InitializeAndroidComponent()
        {
            try
            {
                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    _currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                    _applicationContext = _currentActivity.Call<AndroidJavaObject>("getApplicationContext");
                    _pluginClass = new AndroidJavaClass(packageName);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Android Component can't be initialize due to: {ex.Message}");
            }
        }

#endif

        public static Meeting GetMeetingObject()
        {
            if (_instance == null)
            {
                GameObject obj = new GameObject("VideoSDK");
                _instance = obj.AddComponent<Meeting>();
                DontDestroyOnLoad(obj);
                _instance.Initialize();
            }
            return _instance;
        }

        private void Initialize()
        {
#if UNITY_EDITOR ||(!UNITY_ANDROID && !UNITY_IOS)
            throw new PlatformNotSupportedException("Unsupported platform. Only support for Android and iOS platforms.");
#endif

#if UNITY_ANDROID
#pragma warning disable CS0162 // Unreachable code detected
            InitializeAndroidComponent();
            InitializeVideoSDK();
#pragma warning restore CS0162 // Unreachable code detected
#endif
#pragma warning disable CS0162 // Unreachable code detected
            _ = MainThreadDispatcher.Instance;
#pragma warning restore CS0162 // Unreachable code detected
            InitializeDependecy();
        }

        private void InitializeDependecy()
        {
            _apiCaller = new ApiCaller();
            _videoSdkDto = new VideoSDKDTO(_apiCaller);
            _meetingActivity = MeetingActivityFactory.Create(_videoSdkDto);
            if (_meetingActivity != null)
            {
                RegisterCallbacks();
            }

        }

        #region Callbacks Register/DeRegister
        private void RegisterCallbacks()
        {
            RegisterMeetCallbacks();
        }
        private void UnRegisterCallbacks()
        {
            UnRegisterMeetCallbacks();
        }
        private void RegisterMeetCallbacks()
        {
            _meetingActivity.SubscribeToMeetingJoined(OnMeetingJoined);
            _meetingActivity.SubscribeToMeetingLeft(OnMeetingLeft);
            _meetingActivity.SubscribeToParticipantJoined(OnPraticipantJoin);
            _meetingActivity.SubscribeToParticipantLeft(OnPraticipantLeft);
            _meetingActivity.SubscribeToMeetingStateChanged(OnMeetingStateChanged);
            _meetingActivity.SubscribeToError(OnError);

            //_meetingActivity.SubscribeToAvailableAudioDevices(OnAvailableAudioDevices);
            _meetingActivity.SubscribeToAudioDeviceChanged(OnAudioDeviceChanged);

            //_meetingActivity.SubscribeToAvailableVideoDevices(OnAvailableVideoDevices);

            _meetingActivity.SubscribeToSpeakerChanged(OnSpeakerChanged);
            _meetingActivity.SubscribeToExternalCallStarted(OnExternalCallStarted);
            _meetingActivity.SubscribeToExternalCallRinging(OnExternalCallRinging);
            _meetingActivity.SubscribeToExternalCallHangup(OnExternalCallHangup);
            _meetingActivity.SubscribeToPausedAllStreams(OnPausedAllStreams);
            _meetingActivity.SubscribeToResumedAllStreams(OnResumedAllStreams);
            _meetingActivity.SubscribeToWebcamRequested(OnWebcamRequested);
            _meetingActivity.SubscribeToMicRequested(OnMicRequested);
        }

        private void UnRegisterMeetCallbacks()
        {
            _meetingActivity.UnsubscribeFromMeetingJoined(OnMeetingJoined);
            _meetingActivity.UnsubscribeFromMeetingLeft(OnMeetingLeft);
            _meetingActivity.UnsubscribeFromParticipantJoined(OnPraticipantJoin);
            _meetingActivity.UnsubscribeFromParticipantLeft(OnPraticipantLeft);
            _meetingActivity.UnsubscribeFromMeetingStateChanged(OnMeetingStateChanged);
            _meetingActivity.UnsubscribeFromError(OnError);

            //_meetingActivity.UnsubscribeFromAvailableAudioDevices(OnAvailableAudioDevices);
            _meetingActivity.UnsubscribeFromAudioDeviceChanged(OnAudioDeviceChanged);

            //_meetingActivity.UnsubscribeFromAvailableVideoDevices(OnAvailableVideoDevices);


            _meetingActivity.UnsubscribeFromSpeakerChanged(OnSpeakerChanged);
            _meetingActivity.UnsubscribeFromExternalCallStarted(OnExternalCallStarted);
            _meetingActivity.UnsubscribeFromExternalCallRinging(OnExternalCallRinging);
            _meetingActivity.UnsubscribeFromExternalCallHangup(OnExternalCallHangup);
            _meetingActivity.UnsubscribeFromPausedAllStreams(OnPausedAllStreams);
            _meetingActivity.UnsubscribeFromResumedAllStreams(OnResumedAllStreams);
            _meetingActivity.UnsubscribeFromWebcamRequested(OnWebcamRequested);
            _meetingActivity.UnsubscribeFromMicRequested(OnMicRequested);
        }
        #endregion

        public static void RunOnUnityMainThread(Action action)
        {
            if (action != null)
            {
                MainThreadDispatcher.Instance.Enqueue(action);
            }
        }

        public void CreateMeetingId(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                Debug.LogError("Token is empty or invalid or might have expired.");
                return;
            }
            string meetingUri = _meetingUri;
            StartCoroutine(CreateMeetingIdCoroutine(meetingUri, token, OnCreateMeetingId));
        }

        private string CombinePath(string uri, string path)
        {
            return Path.Combine(uri, path);
        }

        private void OnCreateMeetingId(string meetId)
        {
            RunOnUnityMainThread(() =>
            {
                OnCreateMeetingIdCallback?.Invoke(meetId);
            });
        }

        private IEnumerator CreateMeetingIdCoroutine(string meetingUri, string token, Action<string> OnCreateMeeting)
        {
            var task = _apiCaller.CallApi(meetingUri, token, "");
            while (!task.IsCompleted)
            {
                yield return null; // Wait for the task to complete
            }
            if (task.IsFaulted)
            {
                OnCreateMeetingIdFailedCallback?.Invoke(task.Exception.InnerException.Message);
                yield break;
            }
            _meetingActivity?.CreateMeetingId(task.Result, token, OnCreateMeeting);
        }

        public void Join(string token, string meetingId, string name, bool micEnabled, bool camEnabled, CustomVideoStream encorderConfig = null, string participantId = null)
        {
            if (string.IsNullOrEmpty(meetingId))
            {
                Debug.LogError("Invalid Join Meeting Arguments Meet-Id can't be null or empty");
                return;
            }
            if (string.IsNullOrEmpty(token))
            {
                Debug.LogError("Invalid Join Meeting Arguments Token can't be null or empty");
                return;
            }
            if (string.IsNullOrEmpty(name))
            {
                Debug.LogError("Invalid Join Meeting Arguments Name can't be null or empty");
                return;
            }
            StartCoroutine(JoinMeetingIdCoroutine(token, meetingId, name, micEnabled, camEnabled, encorderConfig, participantId));
        }

        private IEnumerator JoinMeetingIdCoroutine(string token, string meetingId, string name, bool micEnabled, bool camEnabled, CustomVideoStream encorderConfig, string participantId = null)
        {
            string meetingUri = CombinePath(_customMeetingUri, meetingId);
            var task = _apiCaller.CallApi(meetingUri, token, "");
            while (!task.IsCompleted)
            {
                yield return null; // Wait for the task to complete
            }
            if (task.IsFaulted)
            {
                OnJoinMeetingFailedCallback?.Invoke(task.Exception.InnerException.Message);
                yield break;

            }
            participantId = participantId == null ? Guid.NewGuid().ToString().Substring(0, 8) : participantId;
            _meetingActivity?.JoinMeeting(token, task.Result, name, micEnabled, camEnabled, participantId, _packageVersion, encorderConfig);
        }

        public void Leave()
        {
            _meetingActivity?.LeaveMeeting();
        }


        public AudioDeviceInfo[] GetAudioDevices()
        {
            string availableDevices = _meetingActivity?.GetAudioDevices();
            // Deserialize directly from array JSON
            AudioDeviceInfo[] availableAudioDevice = JsonConvert.DeserializeObject<AudioDeviceInfo[]>(availableDevices);
            return availableAudioDevice;
        }

        public VideoDeviceInfo[] GetVideoDevices()
        {
            string availableDevices = _meetingActivity?.GetVideoDevices();
            //Debug.Log("GetVideoDevices " + availableDevices);
            //Deserialize directly from array JSON
            VideoDeviceInfo[] availableVideoDevice = JsonConvert.DeserializeObject<VideoDeviceInfo[]>(availableDevices);

            return availableVideoDevice;
        }

        public AudioDeviceInfo GetSelectedAudioDevice()
        {
            string audioDevice = _meetingActivity?.GetSelectedAudioDevice();
            //Debug.Log("audio device " + audioDevice);
            AudioDeviceInfo selectedAudioDevice = JsonConvert.DeserializeObject<AudioDeviceInfo>(audioDevice);
            return selectedAudioDevice;
        }


        [HideInInspector] public static VideoDeviceInfo selectedVideoDevice = null;
        public VideoDeviceInfo GetSelectedVideoDevice()
        {

            if (selectedVideoDevice == null)
            {
                VideoDeviceInfo[] availableVideoDevice = GetVideoDevices();

                for (int i = 0; i < availableVideoDevice.Length; i++)
                {
                    if (availableVideoDevice[i].facingMode == FacingMode.front)
                    {
                        ChangeVideoDevice(availableVideoDevice[i]);
                        return selectedVideoDevice = availableVideoDevice[i];
                    }
                }
            }
            else
            {
                string videoDevice = _meetingActivity?.GetSelectedVideoDevice();
                VideoDeviceInfo selectedVideoDevice = JsonConvert.DeserializeObject<VideoDeviceInfo>(videoDevice);
                Meeting.selectedVideoDevice = selectedVideoDevice;
                return selectedVideoDevice;
            }

            return null;
        }

        public void ChangeAudioDevice(AudioDeviceInfo audioDevice)
        {
            _meetingActivity?.ChangeAudioDevice(audioDevice.label);
        }

        public void ChangeVideoDevice(VideoDeviceInfo videoDevice)
        {
            selectedVideoDevice = videoDevice;
            _meetingActivity?.ChangeVideoDevice(videoDevice.label);

        }
        public void PauseAllStreams(StreamKind kind)
        {
            _meetingActivity.PauseAllStreams(kind.ToString().ToLower());
        }
        public void ResumeAllStreams(StreamKind kind)
        {
            _meetingActivity.ResumeAllStreams(kind.ToString().ToLower());
        }


        #region NativeCallBacks
        private void OnMeetingJoined(string meetingId, string Id, string name, bool isLocal, bool enabledLogs, string logEndPoint, string jwtKey, string peerId, string sessionId)
        {
            if (_participantsDict.ContainsKey(Id))
            {
                return;
            }
            _videoSdkDto.SendDTO("INFO", $"MeetingJoined:- Id: {Id} IsLocal: {isLocal} ParticipantName: {name}");
            MeetingID = meetingId;
            _videoSdkDto.Initialize(sessionId, jwtKey, meetingId, peerId, enabledLogs, logEndPoint, _packageVersion);
            OnPraticipantJoin(Id, name, isLocal);
        }
        private void OnMeetingLeft(string Id, string name, bool isLocal)
        {
            _videoSdkDto.SendDTO("INFO", $"MeetingLeft:- Id: {Id} IsLocal: {isLocal} ParticipantName: {name}");
            RunOnUnityMainThread(() =>
            {
                OnParticipantLeftCallback?.Invoke(new Participant(Id, name, isLocal));



                foreach (var user in _participantsDict.Values)
                {
                    user?.OnParticipantLeft();
                }
                _participantsDict.Clear();
            });
        }

        private void OnPraticipantJoin(string Id, string name, bool isLocal)
        {
            if (_participantsDict.ContainsKey(Id))
            {
                return;
            }
            _videoSdkDto.SendDTO("INFO", $"PraticipantJoin:- Id: {Id} IsLocal: {isLocal} ParticipantName: {name}");
            var participantData = new Participant(Id, name, isLocal);

            IUser participant = UserFactory.Create(participantData, MeetingControllFactory.Create(_videoSdkDto), _videoSdkDto);

            if (isLocal) _localParticipant = participant;

            AddParticipant(Id, participant);
            RunOnUnityMainThread(() =>
            {
                OnParticipantJoinedCallback?.Invoke(participantData);
            });

        }
        private void OnPraticipantLeft(string Id, string name, bool isLocal)
        {
            _videoSdkDto.SendDTO("INFO", $"PraticipantLeft:- Id: {Id} IsLocal: {isLocal} ParticipantName: {name}");
            RunOnUnityMainThread(() =>
            {
                OnParticipantLeftCallback?.Invoke(new Participant(Id, name, isLocal));

                if (isLocal)
                {
                    _localParticipant = null;
                }

                if (_participantsDict.TryGetValue(Id, out IUser participant))
                {
                    participant.OnParticipantLeft();
                    RemoveParticipant(Id);
                }

            });

        }

        private bool AddParticipant(string key, IUser participant)
        {
            return _participantsDict.TryAdd(key, participant);
        }
        private bool RemoveParticipant(string key)
        {
            return _participantsDict.TryRemove(key, out _);
        }

        private void OnError(string jsonString)
        {
            Error error = JsonConvert.DeserializeObject<Error>(jsonString);
            RunOnUnityMainThread(() =>
            {
                OnErrorCallback?.Invoke(error);
            });

        }

        private void OnMeetingStateChanged(string state)
        {
            _videoSdkDto.SendDTO("INFO", $"MeetingStateChanged:- State: {state} ");
            RunOnUnityMainThread(() =>
            {
                if (Enum.TryParse(state, true, out _meetState))
                {
                    OnMeetingStateChangedCallback?.Invoke(_meetState);
                }

            });

        }

        private void OnSpeakerChanged(string ParticipantId)
        {
            RunOnUnityMainThread(() =>
            {
                OnSpeakerChangedCallback?.Invoke(ParticipantId);
            });

        }

        private void OnAudioDeviceChanged(string availableDevice, string selectedDevice)
        {
            RunOnUnityMainThread(() =>
            {
                //Debug.Log($"available devices {availableDevice}");
                Debug.Log($"selectedDeviceJson {selectedDevice}");

                AudioDeviceInfo[] availableAudioDevice = JsonConvert.DeserializeObject<AudioDeviceInfo[]>(availableDevice);
                AudioDeviceInfo selectedAudioDevice = JsonConvert.DeserializeObject<AudioDeviceInfo>(selectedDevice);

                OnAudioDeviceChangedCallback?.Invoke(availableAudioDevice, selectedAudioDevice);
            });
        }

        private void OnExternalCallHangup()
        {
            RunOnUnityMainThread(() =>
            {
                OnCallHangupCallback?.Invoke();
            });
        }

        private void OnExternalCallRinging()
        {
            RunOnUnityMainThread(() =>
            {
                OnCallRingingCallback?.Invoke();
            });
        }

        private void OnExternalCallStarted()
        {
            RunOnUnityMainThread(() =>
            {
                OnCallStartedCallback?.Invoke();
            });
        }

        private void OnPausedAllStreams(string kind)
        {
            RunOnUnityMainThread(() =>
            {
                if (Enum.TryParse(kind, true, out StreamKind streamKind))
                {
                    OnPausedAllStreamsCallback?.Invoke(streamKind);
                }
            });
        }

        private void OnResumedAllStreams(string kind)
        {
            RunOnUnityMainThread(() =>
            {
                if (Enum.TryParse(kind, true, out StreamKind streamKind))
                {
                    OnResumedAllStreamsCallback?.Invoke(streamKind);
                }
            });
        }
        #endregion

        internal static IUser GetParticipantById(string Id)
        {
            if (_participantsDict.TryGetValue(Id, out IUser participant))
            {
                return participant;
            }
            return null;
        }

        #region Remote Access
        StreamKind requestStream;
        private void OnWebcamRequested(string participantId, Action accept, Action reject)
        {
            requestStream = StreamKind.VIDEO;
            RunOnUnityMainThread(() =>
            {
                accept = () => Request(true);
                reject = () => Request(false);
                OnWebcamRequestedCallback?.Invoke(participantId, accept, reject);
            });
        }

        private void OnMicRequested(string participantId, Action accept, Action reject)
        {
            requestStream = StreamKind.AUDIO;
            RunOnUnityMainThread(() =>
            {
                accept = () => Request(true);
                reject = () => Request(false);
                OnMicRequestedCallback?.Invoke(participantId, accept, reject);
            });
        }

        public void Request(bool isAccept)
        {
            if (isAccept)
            {
                switch (requestStream)
                {
                    case StreamKind.AUDIO:
                        _localParticipant?.ToggleMic(true);
                        break;
                    case StreamKind.VIDEO:
                        _localParticipant?.ToggleWebCam(true, null);
                        break;
                }
            }
        }
        #endregion
    }

    public enum VideoEncoderConfig
    {
        h90p_w160p, // default ios
        h144p_w176p, // default android
        h240p_w320p,
        h360p_w640p,
        h480p_w640p,
        h720p_w960p,
        h720p_w1280p
    }
    public enum MeetingState
    {
        NONE,
        CONNECTING, //connection is in the process of being established
        CONNECTED, //connection has been successfully established
        RECONNECTING, //attempting to reconnect
        DISCONNECTED
    }

    public enum StreamKind
    {
        AUDIO,
        VIDEO
    }

    #region Model Class
    [Serializable]
    public class AudioDeviceInfo
    {
        public string label;
        public string kind;
        public string deviceId;
    }
    [Serializable]
    public class VideoDeviceInfo
    {
        public string label;
        public string kind;
        public string deviceId;
        public FacingMode facingMode;
    }


    public enum FacingMode
    {
        front, back
    }

    [SerializeField]
    public class CustomVideoStream
    {
        [JsonIgnore]
        public VideoEncoderConfig videoEncoder { get; private set; }
        public bool isMultiStream { get; private set; }
        [JsonIgnore]
        public VideoDeviceInfo videoDevice { get; private set; }

        [JsonProperty]
        private string deviceId;
        [JsonProperty]
        private string encoder;

        public CustomVideoStream(VideoEncoderConfig videoEncoder = VideoEncoderConfig.h240p_w320p, bool isMultiStream = false, VideoDeviceInfo videoDevice = null)
        {
            if (videoDevice == null) videoDevice = Meeting.selectedVideoDevice;

            this.videoEncoder = videoEncoder;
            this.isMultiStream = isMultiStream;
            this.videoDevice = videoDevice;

            if (this.videoDevice != null)
                deviceId = this.videoDevice.deviceId;

            encoder = this.videoEncoder.ToString();
        }
    }

    [System.Serializable]
    public class JoinMeetingConfig
    {
        public string token;
        public string meetingId;
        public string name;
        public bool micEnabled;
        public bool camEnabled;
        public string participantId = null;
        public string packageVersion;
        public string platform;
        public CustomVideoStream encorderConfig;

        public JoinMeetingConfig(string token, string meetingId, string name, bool micEnabled,
            bool camEnabled, string participantId, string packageVersion, string platform, CustomVideoStream encorderConfig)
        {
            this.token = token;
            this.meetingId = meetingId;
            this.name = name;
            this.micEnabled = micEnabled;
            this.camEnabled = camEnabled;
            this.participantId = participantId;
            this.packageVersion = packageVersion;
            this.platform = platform;
            this.encorderConfig = encorderConfig;
        }

    }

    #endregion


}

