﻿using Attributes;
using LSCore;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Animatable
{
    public class AnimatableCanvas : SingleService<AnimatableCanvas>
    {
        private static Canvas canvas;
        private static Camera Cam => canvas.worldCamera;
        
        [ColoredField, SerializeField] private AnimText animText;
        [ColoredField, SerializeField] private HealthBar healthBar;
        [ColoredField, SerializeField] private HealthBar opponentHealthBar;
        [ColoredField, SerializeField] private Loader loader;

        public static Transform SpawnPoint => Instance.transform;
        internal static AnimText AnimText => Instance.animText;
        internal static HealthBar HealthBar => Instance.healthBar;
        internal static HealthBar OpponentHealthBar => Instance.opponentHealthBar;
        internal static Loader Loader => Instance.loader;
        
        protected override void Init()
        {
            base.Init();
            DontDestroyOnLoad(this);
            animText.Init();
            canvas = GetComponent<Canvas>();
            canvas.worldCamera = Camera.main;
            canvas.sortingOrder = 10;
            
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        protected override void DeInit()
        {
            base.DeInit();
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            canvas.worldCamera = Camera.main;
            Clean();
        }

        internal static Vector3 GetLocalPosition(Vector3 worldPos)
        {
            var targetLocalPosByCam = Cam.transform.InverseTransformPoint(worldPos);
            targetLocalPosByCam /= canvas.transform.lossyScale.x;
            targetLocalPosByCam.z = 0;
            return targetLocalPosByCam;
        }
        
        public static void Clean()
        {
            var instance = Instance;
            for (int i = 1; i < instance.transform.childCount; i++)
            {
                Destroy(instance.transform.GetChild(i).gameObject);
            }
        }
    }
}