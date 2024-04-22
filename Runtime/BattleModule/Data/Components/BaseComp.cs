﻿using System;
using UnityEngine;

namespace LSCore.BattleModule
{
    [Serializable]
    public abstract class BaseComp
    {
        protected CompData data;
        protected Transform transform;
        
        private bool isRunning;
        public bool IsRunning
        {
            get => isRunning;
            set
            {
                if (value == isRunning) return;
                
                isRunning = value;
                if (value)
                {
                    data.update += Update;
                    data.fixedUpdate += FixedUpdate;
                }
                else
                {
                    data.update -= Update;
                    data.fixedUpdate -= FixedUpdate;
                }
            }
        }

        public void SetIsRunning(bool value) => IsRunning = value;
        protected virtual void Update() { }
        protected virtual void FixedUpdate() { }

        public void Init(CompData data)
        {
            Register(data);
            Init();
        }

        public void Register(CompData data)
        {
            this.data = data;
            transform = data.transform;
            OnRegister();
        }

        protected void Reg<T>(T obj) where T : BaseComp
        {
            TransformDict<T>.Add(transform, obj);
            data.destroy += data.Remove<T>;
        }
        
        protected virtual void OnRegister() { }
        
        protected abstract void Init();
    }
}