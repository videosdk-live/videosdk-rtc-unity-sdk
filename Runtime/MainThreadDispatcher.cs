using System;
using System.Collections.Concurrent;
using System.Threading;
using UnityEngine;
namespace live.videosdk
{
    public sealed class MainThreadDispatcher : MonoBehaviour
    {
        private static MainThreadDispatcher _instance;
        private static readonly ConcurrentQueue<Action> _actions = new ConcurrentQueue<Action>();
        private static int _mainThreadId;

        public static MainThreadDispatcher Instance
        {
            get
            {
                if (_instance == null)
                {
                    var obj = new GameObject("MainThreadDispatcher");
                    _instance = obj.AddComponent<MainThreadDispatcher>();
                    DontDestroyOnLoad(obj);
                    _mainThreadId = Thread.CurrentThread.ManagedThreadId;
                }
                return _instance;
            }
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

        private void Update()
        {
            while (_actions.TryDequeue(out var action))
            {
                action?.Invoke();
            }
        }

        private void OnDestroy()
        {
            // Clear any pending actions to prevent memory leaks
            while (_actions.TryDequeue(out _)) { }
        }
    }
}
