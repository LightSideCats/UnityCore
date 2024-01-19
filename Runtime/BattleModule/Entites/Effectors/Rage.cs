﻿using System;
using Battle.Data.Components;
using LSCore.LevelSystem;
using DG.Tweening;
using LSCore.Async;
using UnityEngine;
using Move = Battle.Data.Components.MoveComponent;

namespace Battle.Data
{
    [Serializable]
    internal class Rage : BaseEffector
    {
        private const string Name = nameof(Rage);
        
        private float duration;
        private float damageBuff;
        private float moveSpeedBuff;
        protected override bool NeedFindOpponent => false;
        [SerializeField] private float buffDuration = 0.1f;

        protected override void OnInit()
        {
            duration = GetValue<HealthGP>();
            damageBuff = GetValue<DamageGP>() / 100;
            moveSpeedBuff = GetValue<MoveSpeedGP>() / 100;
        }

        protected override void OnApply()
        {
            radiusRenderer.color = new Color(0.68f, 0.17f, 1f, 0.39f);

            Wait.Run(duration, _ =>
            {
                foreach (var target in findTargetComponent.FindAll(radius))
                {
                    target.Get<Move>().Buffs.Set(Name, moveSpeedBuff, buffDuration);
                    target.Get<AutoAttackComponent>().Buffs.Set(Name, damageBuff, buffDuration);
                }
            }).OnComplete(() =>
            {
                radiusRenderer.gameObject.SetActive(false);
                isApplied = false;
            });
        }
    }
}