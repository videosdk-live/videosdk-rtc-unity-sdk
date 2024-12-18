using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;
using Newtonsoft.Json;
namespace live.videosdk
{
    public sealed class Meeting : MonoBehaviour
    {
        private static Meeting _instance;
        public const string packageName = "live.videosdk.unity.android.VideoSDKUnityPlugin";
        private IApiCaller _apiCaller;
        public const string _meetingUri = "https://api.videosdk.live/v2/rooms";
        private static readonly Dictionary<string, IUser> _participantsDict = new Dictionary<string, IUser>();
        public static string _meetingId { get; private set; }

        #region Callbacks For User
        public event Action<string> OnCreateMeetingIdCallback;
        public event Action<string> OnCreateMeetingIdFailedCallback;
        public event Action<IParticipant> OnParticipantJoinedCallback;
        public event Action<IParticipant> OnParticipantLeftCallback;
        public event Action<string> OnMeetingStateChangedCallback;
        public event Action<Error> OnErrorCallback;
        #endregion

#if UNITY_ANDROID
        private AndroidJavaObject _applicationContext;
        private AndroidJavaClass _pluginClass;
        private AndroidJavaObject _currentActivity;
        private IMeetingActivity _meetingActivity;

        private void OnPermissionGranted(string permissionName)
        {
            if (Permission.HasUserAuthorizedPermission(Permission.Microphone) && Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                InitializeAndroidComponent();
                InitializeVideoSDK();
                return;
            }
            RequestForPermission(Permission.Microphone);

        }

        private void OnPermissionDenied(string permissionName)
        {
            Debug.LogError($"VideoSDK can't Initialize {permissionName} Denied");

        }

        private void OnPermissionDeniedAndDontAskAgain(string permissionName)
        {
            Debug.LogError($"VideoSDK can't Initialize {permissionName} Denied And DontAskAgain");
        }


        private void RequestForPermission(string permission)
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                if (Permission.HasUserAuthorizedPermission(Permission.Microphone) && Permission.HasUserAuthorizedPermission(Permission.Camera))
                {
                    // The user authorized use of the microphone.
                    OnPermissionGranted("");
                }
                else
                {
                    var callbacks = new PermissionCallbacks();
                    callbacks.PermissionDenied += OnPermissionDenied;
                    callbacks.PermissionGranted += OnPermissionGranted;
                    callbacks.PermissionDeniedAndDontAskAgain += OnPermissionDeniedAndDontAskAgain;
                    Permission.RequestUserPermission(permission, callbacks);
                }
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

        private void Awake()
        {
            Application.targetFrameRate = 60;
            //RequestForPermission(Permission.Camera);
        }

        public void Initialize()
        {
            _ = MainThreadDispatcher.Instance;

            InitializeAndroidComponent();
            InitializeVideoSDK();
        }


        public void InitializeVideoSDK()
        {

            if (Application.platform == RuntimePlatform.Android)
            {
                string result = _pluginClass?.CallStatic<string>("init", _applicationContext);
                if (!result.Equals("Success"))
                {
                    throw new InvalidOperationException(result);
                }

                InitializeDependecy();
                Debug.Log("Video SDK Initialized successfully!");
                return;
            }
            Debug.Log("Video SDK doesn't support for this platform.");
        }

        private void InitializeDependecy()
        {
            _apiCaller = new ApiCaller();
            _meetingActivity = new MeetingActivity(_pluginClass, _currentActivity);

            RegisterMeetCallBacks();

        }

        private void RegisterMeetCallBacks()
        {
            _meetingActivity.OnMeetingJoinedCallback += OnMeetingJoined;
            _meetingActivity.OnMeetingLeftCallback += OnMeetingLeft;
            _meetingActivity.OnParticipantJoinedCallback += OnPraticipantJoin;
            _meetingActivity.OnParticipantLeftCallback += OnPraticipantLeft;
            _meetingActivity.OnMeetingStateChangedCallback += OnMeetingStateChanged;
            _meetingActivity.OnErrorCallback += OnError;
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
            }
            StartCoroutine(CreateMeetingIdCoroutine(token, OnCreateMeetingId));
        }

        private void OnCreateMeetingId(string meetId)
        {
            RunOnUnityMainThread(() =>
            {
                OnCreateMeetingIdCallback?.Invoke(meetId);
            });
        }

        private IEnumerator CreateMeetingIdCoroutine(string token, Action<string> OnCreateMeeting)
        {
            var task = _apiCaller.CallApi(_meetingUri, token);
            while (!task.IsCompleted)
            {
                yield return null; // Wait for the task to complete
            }
            if (task.IsFaulted)
            {
                OnCreateMeetingIdFailedCallback?.Invoke(task.Exception.Message);
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

            _meetingActivity?.JoinMeeting(token, meetingId, name, micEnabled, camEnabled, participantId);
        }

        public void Leave()
        {
            _meetingActivity?.LeaveMeeting();
        }

        #region NativeCallBacks
        private void OnMeetingJoined(string meetingId, string Id, string name, bool isLocal)
        {
            _meetingId = meetingId;
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
            var participantData = new Participant(Id, name, isLocal);
            _participantsDict[Id] = new User(participantData, _pluginClass);

            RunOnUnityMainThread(() =>
            {
                OnParticipantJoinedCallback?.Invoke(participantData);
            });
            
        }
        private void OnPraticipantLeft(string Id, string name, bool isLocal)
        {
            if (_participantsDict.TryGetValue(Id, out IUser participant))
            {
                participant.OnParticipantLeft();
                _participantsDict.Remove(Id);
            }

            RunOnUnityMainThread(() =>
            {
                OnParticipantLeftCallback?.Invoke(new Participant(Id, name, isLocal));
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

