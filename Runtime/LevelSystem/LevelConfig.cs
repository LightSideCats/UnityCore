﻿using System.Collections.Generic;
using Battle.Data.GameProperty;
using GameCore.Attributes;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
#if UNITY_EDITOR
using Sirenix.Utilities.Editor;
#endif
using UnityEngine;

namespace Battle.Data
{
    [CreateAssetMenu(fileName = nameof(LevelConfig), menuName = "Battle/" + nameof(LevelConfig), order = 0)]
    public class LevelConfig : SerializedScriptableObject
    {
        [field: SerializeField, ReadOnly] public LevelsContainer Container { get; set; }
        public Id Id => Container.Id;

        [HideReferenceObjectPicker]
        [OdinSerialize] [ColoredField] public GameProps upgrades { get; private set; } = new();

        [OdinSerialize]
        [HideReferenceObjectPicker]
        [ListDrawerSettings(HideAddButton = true, OnTitleBarGUI = "OtherUpgradesGui")]
        public List<GroupGameProps> OtherUpgrades { get; private set; } = new();

        //[OdinSerialize] [HideReferenceObjectPicker] public Prices Prices { get; set; } = new();
        
#if UNITY_EDITOR
        public override string ToString() => name;

        private void OtherUpgradesGui()
        {
            if (SirenixEditorGUI.ToolbarButton(EditorIcons.Plus))
            {
                var allDestinationsProps = new GroupGameProps();
                OtherUpgrades.Add(allDestinationsProps);
            }
        }

        public static LevelConfig currentInspected;

        [OnInspectorInit]
        private void OnInit()
        {
            OnGui();
        }

        [OnInspectorGUI] 
        private void OnGui()
        {
            currentInspected = this;
        }
#endif
        
        public static int GetLevel(string configName)
        {
            var split = configName.Split('_');
            return int.Parse(split[1]);
        }

        public int Level => GetLevel(name);

        public void Apply()
        {
            var entiProps = EntiProps.ByName;
            var allPropsById = new Dictionary<string, HashSet<string>>();
            var allIds = new HashSet<Id> {Id};
            
            AddProps(upgrades);

            for (int i = 0; i < OtherUpgrades.Count; i++)
            {
                var upgrade = OtherUpgrades[i];
                AddProps(upgrade);
                allIds.UnionWith(upgrade.Group.Ids);
            }
            
            void AddProps(GameProps props)
            {
                foreach (var id in allIds)
                {
                    if (!allPropsById.TryGetValue(id, out var propsDict))
                    {
                        propsDict = new HashSet<string>();
                        allPropsById.Add(id, propsDict);
                    }

                    foreach (var prop in props.Props)
                    {
                        if (propsDict.Add(prop.Name))
                        {
                            if (!entiProps.TryGetValue(id, out var entiPropsDict))
                            {
                                entiPropsDict = new EntiProps.Props();
                                entiProps.Add(id, entiPropsDict);
                            }
                            
                            if (entiPropsDict.TryGetValue(prop.Name, out var propValue))
                            {
                                entiPropsDict[prop.Name] = prop.Upgrade(propValue);
                            }
                            else
                            {
                                entiPropsDict.Add(prop.Name, Prop.Copy(prop.Prop));
                            }
                        }
                    }
                }
            }
        }
    }
}