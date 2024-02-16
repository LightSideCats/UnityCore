﻿using System;
using LSCore.BattleModule;
using UnityEngine;

namespace LSCore
{
    [Serializable]
    public abstract class Impact
    {
        public abstract void Apply(Transform target);
    }

    [Serializable]
    public class Damage : Impact
    {
        public int damage;
        
        public override void Apply(Transform target)
        {
            target.Get<BaseHealthComp>().TakeDamage(damage);
        }
    }
}