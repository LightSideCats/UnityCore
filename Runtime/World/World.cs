﻿using System;
using System.Collections;
using UnityEngine;

namespace LSCore
{
    public class World : MonoBehaviour
    {
        public static event Action ApplicationPaused;
        public static event Action Initing;
        public static event Action Inited;
        public static event Action Updated;
        public static event Action Destroyed;
        private static bool isCreated;
        private static World instance;
        public static Camera Camera { get; private set; }
        public static bool IsPlaying { get; private set; }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            Initing?.Invoke();
            
            instance = new GameObject(nameof(World)).AddComponent<World>();
            DontDestroyOnLoad(instance);
            IsPlaying = true;

            Inited?.Invoke();
            Debug.Log("[World] Inited");
        }
        
        public static Coroutine RunCoroutine(IEnumerator enumerator) => instance.StartCoroutine(enumerator);

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
            OnApplicationPause(true);
        }
    }
}