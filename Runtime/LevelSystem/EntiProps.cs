﻿using System;
using System.Collections.Generic;
using LSCore.LevelSystem;
using LSCore.ConfigModule;
using Newtonsoft.Json;

namespace LSCore.LevelSystem
{
    public class EntiProps : BaseConfig<EntiProps>
    {
        [Serializable]
        public class PropsByEntityId : Dictionary<string, Props> { }

        [Serializable]
        public class Props : Dictionary<string, Prop>
        {
            public float GetValue<T>() where T : BaseGameProperty
            {
                return this[typeof(T).Name].Value[FloatAndPercent.ValueKey];
            }
        }
        
        [JsonProperty("props")] private PropsByEntityId props = new();

        public static PropsByEntityId ByName => Config.props;
        public static Props GetProps(string entityId) => Config.props[entityId];
        public static void Clear() => Config.props.Clear();
    }
}