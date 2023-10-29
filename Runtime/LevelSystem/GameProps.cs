﻿using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace LSCore.LevelSystem
{
    [Serializable]
    public class GameProps
    {
        [field: HideReferenceObjectPicker]
        [field: ValueDropdown("AvailableProps", IsUniqueList = true)]
        [field: OdinSerialize] public HashSet<BaseGameProperty> Props { get; } = new();
        
#if UNITY_EDITOR
        private ValueDropdownList<BaseGameProperty> list;
        private IList<ValueDropdownItem<BaseGameProperty>> AvailableProps
        {
            get
            {
                list ??= new ValueDropdownList<BaseGameProperty>();
                list.Clear();
                
                foreach (var type in PropTypes)
                {
                    list.Add(type.Name, (BaseGameProperty)Activator.CreateInstance(type));
                }
                
                return list;
            }
        }
        
        protected virtual HashSet<Type> PropTypes => PropTypesByIdGroup.GetAllObjectsById(LevelConfig.currentInspected.Id);
#endif
    }

    [Serializable]
    public class GroupGameProps : GameProps
    {
        [field: SerializeField] 
        [field: ValueDropdown("Groups", IsUniqueList = true)] 
        public LevelIdGroup Group { get; private set; }

#if UNITY_EDITOR
        private IEnumerable<LevelIdGroup> Groups => LevelConfig.currentInspected.Id.GetAllGroups<LevelIdGroup>();
        protected override HashSet<Type> PropTypes
        {
            get
            {
                if (Group != null && PropTypesByIdGroup.Instance.ByKey.TryGetValue(Group, out var types))
                {
                    return types;
                }
                
                return new HashSet<Type>();
            }
        }
#endif
    }
}