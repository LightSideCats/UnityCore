﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using LSCore.Extensions;
using LSCore.Extensions.Unity;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace LSCore.AnimationsModule
{
    [Serializable]
    public class Log : LSAction
    {
        public string message;
        
        public override void Invoke()
        {
            Debug.Log(message);
        }
    }
    
    [Serializable]
    public class ParticleSystemPlay : LSAction
    {
        public ParticleSystem particleSystem;
        
        public override void Invoke()
        {
            particleSystem.Stop();
            particleSystem.Play();
#if UNITY_EDITOR
            if (World.IsEditMode)
            {
                lastTime = EditorApplication.timeSinceStartup;
                EditorApplication.update -= Play;
                EditorApplication.update += Play;
            }
#endif
        }

#if UNITY_EDITOR
        private double lastTime;
        
        private void Play()
        {
            var t = EditorApplication.timeSinceStartup;
            
            if (World.IsEditMode)
            {
                if (particleSystem == null)
                {
                    EditorApplication.update -= Play;
                    return;
                }
                
                particleSystem.Simulate((float)(t - lastTime), true, false);
            }
            else
            {
                EditorApplication.update -= Play;
            }
            
            lastTime = t;
        }
#endif
    }
    
    [ExecuteAlways]
    [RequireComponent(typeof(Animation))]
    [DefaultExecutionOrder(-1000)]
    public partial class AnimationWrapper : MonoBehaviour, ISerializationCallbackReceiver
    {
        [Serializable]
        private struct Data
        {
            [Required]
            [OnValueChanged("OnClipChanged")]
            public AnimationClip clip;
            [SerializeReference] public List<Handler> handlers;

#if UNITY_EDITOR
            private void OnClipChanged()
            {
                currentInspected.FillHandlers();
            }
#endif
        }

        [SerializeField] private Data[] handlers;
        [PropertySpace(SpaceBefore = 10)]
        [SerializeReference] private List<LSAction> actions;
        private Animation animation;

        private readonly Dictionary<AnimationClip, List<Handler>> handlersByClip = new();
        private AnimationClip lastRuntimeClip;
        private AnimationClip lastClip;

        public Animation Animation => animation;
        
        private void Awake()
        {
            animation = GetComponent<Animation>();
        }
        
        public void Call(string expression)
        {
            if (int.TryParse(expression, out var ind))
            {
                actions[ind].Invoke();
            }
            else if (expression.Contains(','))
            {
                var split = expression.Split(',');

                for (int i = 0; i < split.Length; i++)
                {
                    actions[int.Parse(split[i])].Invoke();
                }
            }
            else if(expression.TryParseIndex(out var index))
            {
                actions[index].Invoke();
            }
            else if(expression.TryParseRange(out var range))
            {
                var a = actions.Range(range);

                for (int i = 0; i < a.Count; i++)
                {
                    a[i].Invoke();
                }
            }
        }

        private (AnimationClip clip, float time) GetClip()
        {
            AnimationClip clip = null;
            float time = 0;
            
#if UNITY_EDITOR
            if (World.IsEditMode)
            {
                clip = Window.animationClip;
                time = Window.time;
                goto ret;
            }
#endif
            foreach (AnimationState state in animation)
            {
                if (animation[state.name].enabled)
                {
                    clip = state.clip;
                    time = state.time;
                    break;
                }
            }

            ret:
            return (clip, time);
        }
        
        private void OnDidApplyAnimationProperties()
        {
#if UNITY_EDITOR
            if (World.IsEditMode)
            {
                Handle_Editor();
                return;
            }
#endif
            Handle();
        }

        private void Handle()
        {
            var (clip, time) = GetClip();
            var notEqual = lastRuntimeClip != clip || isPlayCalled;
            
            isPlayCalled = false;
#if UNITY_EDITOR
            TryCallEvent(clip, time);
#endif
            var currentClipHandlers = handlersByClip[clip];

            if (notEqual)
            {
                StopLastClip(lastRuntimeClip);
                
                foreach (var handler in currentClipHandlers)
                {
                    handler.Start();
                }
            }
            
            foreach (var handler in currentClipHandlers)
            {
                handler.Handle();
            }

            lastRuntimeClip = clip;
        }

        private void StopLastClip(AnimationClip clip)
        {
            if (clip != null)
            {
                if (handlersByClip.TryGetValue(clip, out var lastClipHandlers))
                {
                    foreach (var handler in lastClipHandlers)
                    {
                        handler.Stop();
                    }   
                }
            }
        }

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            FillHandlers();
        }

        private void FillHandlers()
        {
            handlersByClip.Clear();

            for (int i = 0; i < handlers.Length; i++)
            {
                var h = handlers[i];
                handlersByClip.Add(h.clip, h.handlers);
            }
        }

#if UNITY_EDITOR
        
        private float lastTime = -1;
        
        private void TryCallEvent(AnimationClip clip, float time)
        {
            if(World.IsPlaying) return;
            var events = AnimationUtility.GetAnimationEvents(clip);
            if(events.Length == 0) return;

            if (time == 0)
            {
                lastTime = -1;
            }
            
            if (time > lastTime)
            {
                for (int i = 0; i < events.Length; i++)
                {
                    var e = events[i];
                    var eTime = e.time;
                    
                    if (eTime <= lastTime) continue;

                    if (time >= eTime)
                    {
                        e.Invoke(this);
                    }
                }
            }
            else
            {
                for (int i = events.Length - 1; i >= 0; i--)
                {
                    var e = events[i];
                    var eTime = e.time;
                    
                    if (eTime >= lastTime) continue;

                    if (time <= eTime)
                    {
                        e.Invoke(this);
                    }
                }
            }

            lastTime = time;
        }
        
        private static AnimationWindow window;
        private static AnimationWindow Window => window ??= EditorWindow.GetWindow<AnimationWindow>(null, false);
        
        public AnimationWrapper()
        {
            Patchers.AnimEditor.OnSelectionChanged.Changed += OnSelectionChanged;
            Patchers.AnimEditor.previewing.Changed += OnPreviewingChanged;
            EditorApplication.update += OnEditorUpdate;
        }

        private void OnEditorUpdate()
        {
            if (this == null || EditorUtility.IsPersistent(gameObject))
            {
                EditorApplication.update -= OnEditorUpdate;
                return;
            }

            if (isAnimationCalled)
            {
                Handle();
                isAnimationCalled = false;
            }
        }

        private void OnDestroy()
        {
            Patchers.AnimEditor.OnSelectionChanged.Changed -= OnSelectionChanged;
            Patchers.AnimEditor.previewing.Changed -= OnPreviewingChanged;
            EditorApplication.update -= OnEditorUpdate;
        }

        private void OnPreviewingChanged(bool state)
        {
            if (this == null || EditorUtility.IsPersistent(gameObject))
            {
                Patchers.AnimEditor.previewing.Changed -= OnPreviewingChanged;
                return;
            }

            if (!state)
            {
                StopLastClip(lastRuntimeClip);
                lastRuntimeClip = null;
                lastTime = -1;
            }
        }
        
        private object lastAnimPlayer;
        
        private void OnSelectionChanged()
        {
            if (this == null || EditorUtility.IsPersistent(gameObject))
            {
                Patchers.AnimEditor.OnSelectionChanged.Changed -= OnSelectionChanged;
                return;
            }
            
            if(Window == null) return;
            
            var animationPlayer = LSReflection.Eval(Window, "state.activeAnimationPlayer");
            var (clip, time) = GetClip();
            
            if (animationPlayer != lastAnimPlayer || lastClip != clip)
            {
                lastTime = -1;
                lastRuntimeClip = null;
                StopLastClip(lastClip);
            }
            
            lastClip = clip;
            lastAnimPlayer = animationPlayer;
        }

        private static AnimationWrapper currentInspected;
        
        [OnInspectorGUI]
        private void OnInspectorGui()
        {
            currentInspected = this;
        }

        private bool isAnimationCalled;
        private void Handle_Editor()
        {
            isAnimationCalled = true;
        }
#endif
    }
}