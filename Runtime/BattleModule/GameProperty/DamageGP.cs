﻿using System;

namespace LSCore.LevelSystem
{
    [Serializable]
    public class DamageGP : FloatGameProp
    {
#if UNITY_EDITOR
        protected override string IconName => "attack-icon";
#endif
    }
}