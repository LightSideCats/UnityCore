﻿using UnityEngine;

namespace LSCore
{
    public class DaddyWindowManager : WindowManager
    {
        protected override void RecordState() { }
    }
    
    public class DaddyCanvas : BaseWindow<DaddyCanvas>
    {
        protected override bool ActiveByDefault => true;
        protected override bool UseDefaultAnimation => false;
        protected override RectTransform Daddy => null;

        public override WindowManager Manager { get; } = new DaddyWindowManager();
    }
}