﻿#if UNITY_EDITOR
using System.Collections.Generic;
using System.Reflection;
using LSCore.Extensions.Unity;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using UnityToolbarExtender;
using UnityEngine.UI;

namespace LSCore
{
    [CustomEditor(typeof(LSImage), true)]
    [CanEditMultipleObjects]
    public class LSImageEditor : ImageEditor
    {
        SerializedProperty rotateId;
        SerializedProperty invert;
        SerializedProperty gradient;
        SerializedProperty angle;
        SerializedProperty gradientStart;
        SerializedProperty gradientEnd;

        SerializedProperty m_Sprite;
        SerializedProperty m_Type;
        SerializedProperty m_PreserveAspect;
        SerializedProperty m_UseSpriteMesh;
        FieldInfo m_bIsDriven;
        private LSImage image;
        private RectTransform rect;
        private bool isDragging;
        private bool isEditing;

        protected override void OnEnable()
        {
            base.OnEnable();

            m_Sprite = serializedObject.FindProperty("m_Sprite");
            m_Type = serializedObject.FindProperty("m_Type");
            m_PreserveAspect = serializedObject.FindProperty("m_PreserveAspect");
            m_UseSpriteMesh = serializedObject.FindProperty("m_UseSpriteMesh");
            rotateId = serializedObject.FindProperty("rotateId");
            invert = serializedObject.FindProperty("invert");
            gradient = serializedObject.FindProperty("gradient");
            angle = serializedObject.FindProperty("angle");
            gradientStart = serializedObject.FindProperty("gradientStart");
            gradientEnd = serializedObject.FindProperty("gradientEnd");
            var type = GetType().BaseType;
            m_bIsDriven = type.GetField("m_bIsDriven", BindingFlags.Instance | BindingFlags.NonPublic);
            image = (LSImage)target;
            rect = image.GetComponent<RectTransform>();
            isEditing = false;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            //m_bIsDriven.SetValue(this, (rect.drivenByObject as Slider)?.fillRect == rect);

            SpriteGUI();
            EditorGUILayout.PropertyField(gradient);
            EditorGUILayout.PropertyField(invert);
            
            if (image.Gradient.Count > 1)
            {
                angle.floatValue = EditorGUILayout.Slider("Angle", angle.floatValue, 0, 360);
                gradientStart.floatValue = EditorGUILayout.Slider("Gradient Start", gradientStart.floatValue, -1, 1);
                gradientEnd.floatValue = EditorGUILayout.Slider("Gradient End", gradientEnd.floatValue, -1, 1);
            }
            else
            {
                isEditing = false;
            }

            EditorGUILayout.PropertyField(m_Material);
            RaycastControlsGUI();
            MaskableControlsGUI();

            TypeGUI();
            SetShowNativeSize(false);
            if (EditorGUILayout.BeginFadeGroup(m_ShowNativeSize.faded))
            {
                EditorGUI.indentLevel++;

                if ((Image.Type)m_Type.enumValueIndex == Image.Type.Simple)
                    EditorGUILayout.PropertyField(m_UseSpriteMesh);

                EditorGUILayout.PropertyField(m_PreserveAspect);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFadeGroup();
            NativeSizeButtonGUI();

            DrawRotateButton();

            serializedObject.ApplyModifiedProperties();
        }

        void SetShowNativeSize(bool instant)
        {
            Image.Type type = (Image.Type)m_Type.enumValueIndex;
            bool showNativeSize = (type == Image.Type.Simple || type == Image.Type.Filled) &&
                                  m_Sprite.objectReferenceValue != null;
            base.SetShowNativeSize(showNativeSize, instant);
        }

        protected virtual void DrawRotateButton()
        {
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();

            for (int i = 0; i < 4; i++)
            {
                var targetAngle = i * 90;
                if (GUILayout.Button($"{targetAngle}°", GUILayout.Height(30)) && rotateId.intValue != i)
                {
                    rotateId.intValue = i;
                    image.SetVerticesDirty();
                }
            }

            GUILayout.EndHorizontal();
        }

        [MenuItem("GameObject/LSCore/Image")]
        private static void CreateButton()
        {
            new GameObject("LSImage").AddComponent<LSImage>();
        }
    }

}
#endif