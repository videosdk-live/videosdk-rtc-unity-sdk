using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;
using Newtonsoft.Json;
using System.Runtime.InteropServices;
using System.IO;

namespace live.videosdk
{
    public sealed class Meeting : MonoBehaviour
    {
        private static Meeting _instance;
        public const string packageName = "live.videosdk.unity.android.VideoSDKUnityPlugin";
        private IApiCaller _apiCaller;
        private const string _meetingUri = "https://api.videosdk.live/v2/rooms";
        private const string _customMeetingUri = "https://api.videosdk.live/v1/prebuilt/meetings/";
        private const string _authMeetingUri = "https://api.videosdk.live/v2/rooms/validate/";
        private static readonly Dictionary<string, IUser> _participantsDict = new Dictionary<string, IUser>();
        public string MeetingID { get; private set; }
        private IMeetingActivity _meetingActivity;
        private IVideoSDKDTO _videoSdkDto;
        public string[] _avaliableAudioDevicesArray;
        private const string _packageVersion = "1.2.0";
        #region Callbacks For User
        public event Action<string> OnCreateMeetingIdCallback;
        public event Action<string> OnCreateMeetingIdFailedCallback;
        private event Action<string> OnJoinMeetingFailedCallback;
        public event Action<IParticipant> OnParticipantJoinedCallback;
        public event Action<IParticipant> OnParticipantLeftCallback;
        public event Action<string> OnMeetingStateChangedCallback;
        private event Action<string, string[]> OnAudioDeviceChangedCallback;
        public event Action<Error> OnErrorCallback;

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
            if (_meetingActivity!=null)
            {
                RegisterMeetCallBacks();
            }
            
        }

        private void RegisterMeetCallBacks()
        {
            _meetingActivity.SubscribeToMeetingJoined(OnMeetingJoined);
            _meetingActivity.SubscribeToMeetingLeft(OnMeetingLeft);
            _meetingActivity.SubscribeToParticipantJoined(OnPraticipantJoin);
            _meetingActivity.SubscribeToParticipantLeft(OnPraticipantLeft);
            _meetingActivity.SubscribeToMeetingStateChanged(OnMeetingStateChanged);
            _meetingActivity.SubscribeToError(OnError);
            _meetingActivity.SubscribeToFetchAudioDevice(OnFetchAudioDevice);
            _meetingActivity.SubscribeToAudioDeviceChanged(OnAudioDeviceChanged);

            OnJoinMeetingFailedCallback += OnMeetingJoinFailed;
        }

        private void RunOnUnityMainThread(Action action)
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

        private string CombinePath(string uri,string path)
        {
            return Path.Combine(uri,path);
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
            var task = _apiCaller.CallApi(meetingUri, token,"");
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

        public void Join(string token, string meetingId, string name, bool micEnabled, bool camEnabled, string participantId = null)
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

            StartCoroutine(JoinMeetingIdCoroutine(token, meetingId, name,micEnabled,camEnabled,participantId));
        }

        private IEnumerator JoinMeetingIdCoroutine(string token, string meetingId, string name, bool micEnabled, bool camEnabled, string participantId = null)
        {
            string meetingUri = CombinePath(_customMeetingUri, meetingId);
            var task = _apiCaller.CallApi(meetingUri, token,"");
            while (!task.IsCompleted)
            {
                yield return null; // Wait for the task to complete
            }
            if (task.IsFaulted)
            {
                OnJoinMeetingFailedCallback?.Invoke(task.Exception.InnerException.Message);
                yield break;
                
            }
            _meetingActivity?.JoinMeeting(token, task.Result, name, micEnabled, camEnabled, participantId,_packageVersion);
        }

        public void Leave()
        {
            _meetingActivity?.LeaveMeeting();
        }

        public IEnumerable<string>GetAudioDevices()
        {
            return _avaliableAudioDevicesArray;
        }

        private void OnMeetingJoinFailed(string errorMessage)
        {
            throw new Exception(errorMessage);
        }


        #region NativeCallBacks
        private void OnMeetingJoined(string meetingId, string Id, string name, bool isLocal, bool enabledLogs, string logEndPoint, string jwtKey, string peerId, string sessionId)
        {
            MeetingID = meetingId;
            _videoSdkDto.Initialize(sessionId,jwtKey,meetingId,peerId,enabledLogs,logEndPoint,_packageVersion);
            OnPraticipantJoin(Id, name, isLocal);
        }
        private void OnMeetingLeft(string Id, string name, bool isLocal)
        {
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
            if(_participantsDict.ContainsKey(Id))
            {
                return;
            }
            var participantData = new Participant(Id, name, isLocal);
            _participantsDict[Id] = UserFactory.Create(participantData,MeetingControllFactory.Create(_videoSdkDto));

            RunOnUnityMainThread(() =>
            {
                OnParticipantJoinedCallback?.Invoke(participantData);
            });
            
        }
        private void OnPraticipantLeft(string Id, string name, bool isLocal)
        {
            RunOnUnityMainThread(() =>
            {
                OnParticipantLeftCallback?.Invoke(new Participant(Id, name, isLocal));

                if (_participantsDict.TryGetValue(Id, out IUser participant))
                {
                    participant.OnParticipantLeft();
                    _participantsDict.Remove(Id);
                }

            });
           
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
            RunOnUnityMainThread(() =>
            {
                OnMeetingStateChangedCallback?.Invoke(state);
            });
           
        }

        private void OnAudioDeviceChanged(string selectedDevice, string[] avaliableDeviceList)
        {
            RunOnUnityMainThread(() =>
            {
                OnAudioDeviceChangedCallback?.Invoke(selectedDevice, avaliableDeviceList);
            });
        }

        private void OnFetchAudioDevice(string[] obj)
        {
            _avaliableAudioDevicesArray = obj;
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

    }

    public interface IParticipant
    {
        string ParticipantId { get; }
        string Name { get; }
        bool IsLocal { get; }
        string ToString();
    }
    public class Participant : IParticipant
    {
        public string ParticipantId { get; }
        public string Name { get; }
        public bool IsLocal { get; }

        public Participant(string Id, string name, bool isLocal)
        {
            this.ParticipantId = Id;
            this.Name = name;
            this.IsLocal = isLocal;
        }

        public override string ToString()
        {
            return $"ParticipantId: {ParticipantId} Name: {Name} IsLocal: {IsLocal}";
        }

    }

}

