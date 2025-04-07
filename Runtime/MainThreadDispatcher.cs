using System;
using System.Collections.Concurrent;
using System.Threading;
using UnityEngine;

namespace live.videosdk
{
    public sealed class MainThreadDispatcher : MonoBehaviour
    {
        private static MainThreadDispatcher _instance;
        private static SynchronizationContext _mainThreadContext;
        private static readonly ConcurrentQueue<Action> _actions = new ConcurrentQueue<Action>();
        private static int _mainThreadId;

        private const int MaxActionsPerFrame = 50; // Limit actions processed per frame


        public static MainThreadDispatcher Instance
        {
            get
            {
                if (_instance == null)
                {
                    var obj = new GameObject("MainThreadDispatcher");
                    _instance = obj.AddComponent<MainThreadDispatcher>();
                    DontDestroyOnLoad(obj);
                    
                }
                return _instance;
            }
        }

        private void Awake()
        {
            //capture the main thread's Id
            _mainThreadId = Thread.CurrentThread.ManagedThreadId;
            // Capture the main thread's synchronization context
            _mainThreadContext = SynchronizationContext.Current;
        }

        /// <summary>
        /// Adds an action to the queue to be executed on the main thread.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        public void Enqueue(Action action)
        {
            if (action == null) return;

            // If already on the main thread, execute immediately.
            if (Thread.CurrentThread.ManagedThreadId == _mainThreadId)
            {
                action();
            }
            else
            {
                _actions.Enqueue(action);
            }
        }

        // <summary>
        /// Ensures the action is executed on the main thread immediately.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        public void Execute(Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            if (SynchronizationContext.Current == _mainThreadContext)
            {
                // Already on the main thread, execute directly
                action.Invoke();
            }
            else
            {
                // Post to the main thread's synchronization context
                _mainThreadContext.Post(_ => action.Invoke(), null);
            }
        }

        private void Update()
        {
            int actionsProcessed = 0;
            while (actionsProcessed < MaxActionsPerFrame && _actions.TryDequeue(out var action))
            {
                try
                {
                    action?.Invoke();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Exception in MainThreadDispatcher action: {ex}");
                }
                actionsProcessed++;
            }

            //if (_actions.Count > 0)
            //{
            //    Debug.LogWarning($"Actions remaining in queue: {_actions.Count}");
            //}
        }

        private void OnDestroy()
        {
            // Clear any pending actions to prevent memory leaks
            while (_actions.TryDequeue(out _)) { }
        }
    }
}
