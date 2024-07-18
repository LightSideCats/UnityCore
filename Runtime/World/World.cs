﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace LSCore
{
    [DefaultExecutionOrder(-1000)]
    public class World : MonoBehaviour
    {
        public static event Action ApplicationPaused;
        public static event Action Creating;
        public static event Action Created;
        public static event Action Updated;
        public static event Action Destroyed;
        private static Queue<Action> callInMainThreadQueue = new();
        private static SynchronizationContext synchronizationContext = SynchronizationContext.Current;
        private static bool isCreated;
        private static World instance;
        
        public static Camera Camera { get; private set; }
        public static bool IsPlaying { get; private set; }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            Creating?.Invoke();
            var go = new GameObject(nameof(World));
            go.hideFlags = HideFlags.HideAndDontSave;
            instance = go.AddComponent<World>();
            DontDestroyOnLoad(instance);
            IsPlaying = true;

            Created?.Invoke();
            Burger.Log("[World] Created");
        }

        private void Awake()
        {
            Camera = Camera.main;
        }

        private void Update()
        {
            Updated?.Invoke();
        }

        private void OnDestroy()
        {
            IsPlaying = false;
            Destroyed?.Invoke();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                ApplicationPaused?.Invoke();
            }
        }

        private void OnApplicationQuit()
        {
            IsPlaying = false;
            OnApplicationPause(true);
        }

        public static Coroutine BeginCoroutine(IEnumerator enumerator, Action onComplete) 
        {
            return instance.StartCoroutine(instance.StartCoroutine(enumerator, onComplete));
        }
        
        public IEnumerator StartCoroutine(IEnumerator enumerator, Action onComplete)
        {
            yield return enumerator;
            onComplete();
        }

        public static void CallInMainThread(Action action)
        {
            synchronizationContext.Post(_ => action(), null);
        }
    }
}