﻿using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace B2Framework.Editor
{
    [CustomPropertyDrawer(typeof(VariableList))]
    public class VariableArrayDrawer : PropertyDrawer
    {
        private const float HORIZONTAL_GAP = 5;
        private const float VERTICAL_GAP = 5;

        private ReorderableList list;

        private ReorderableList GetList(SerializedProperty property)
        {
            if (list == null)
            {
                list = new ReorderableList(property.serializedObject, property, true, true, true, true);
                list.elementHeight = 21;
                list.drawElementCallback = DrawElement;
                list.drawHeaderCallback = DrawHeader;
                list.onAddDropdownCallback = OnAddElement;
                list.onRemoveCallback = OnRemoveElement;
                list.drawElementBackgroundCallback = DrawElementBackground;
            }
            else
            {
                list.serializedProperty = property;
            }
            return list;
        }


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = base.GetPropertyHeight(property, label) + 60;
            var variables = property.FindPropertyRelative("variables");
            for (int i = 0; i < variables.arraySize; i++)
                height += EditorGUI.GetPropertyHeight(variables.GetArrayElementAtIndex(i)) + VERTICAL_GAP;
            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var list = GetList(property.FindPropertyRelative("variables"));
            list.DoList(position);
        }

        private void OnAddElement(Rect rect, ReorderableList list)
        {
            var variables = list.serializedProperty;
            int index = variables.arraySize > 0 ? variables.arraySize : 0;
            this.DrawContextMenu(variables, index);
        }

        private void OnRemoveElement(ReorderableList list)
        {
            var variables = list.serializedProperty;
            AskRemoveVariable(variables, list.index);
        }

        private void DrawElementBackground(Rect rect, int index, bool isActive, bool isFocused)
        {
            ReorderableList.defaultBehaviours.DrawElementBackground(rect, index, isActive, false, true);
        }

        private void DrawHeader(Rect rect)
        {
            GUI.Label(rect, "Variables");
        }

        private void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            var variables = list.serializedProperty;
            if (index < 0 || index >= variables.arraySize)
                return;

            var variable = variables.GetArrayElementAtIndex(index);

            float x = rect.x;
            float y = rect.y + 2;
            float width = rect.width - 40;
            float height = rect.height;

            Rect variableRect = new Rect(x, y, width, height);
            EditorGUI.PropertyField(variableRect, variable, GUIContent.none);

            var buttonLeftRect = new Rect(variableRect.xMax + HORIZONTAL_GAP, y - 1, 18, 18);
            var buttonRightRect = new Rect(buttonLeftRect.xMax, y - 1, 18, 18);

            if (GUI.Button(buttonLeftRect, new GUIContent("+"), EditorStyles.miniButtonLeft))
            {
                DuplicateVariable(variables, index);
            }
            if (GUI.Button(buttonRightRect, new GUIContent("-"), EditorStyles.miniButtonRight))
            {
                AskRemoveVariable(variables, index);
            }
        }

        protected virtual void DrawContextMenu(SerializedProperty variables, int index)
        {
            GenericMenu menu = new GenericMenu();
            foreach (VariableEnum variableType in System.Enum.GetValues(typeof(VariableEnum)))
            {
                var type = variableType;
                menu.AddItem(new GUIContent(variableType.ToString()), false, context =>
                {
                    AddVariable(variables, index, type);
                }, null);
            }
            menu.ShowAsContext();
        }

        protected virtual void AddVariable(SerializedProperty variables, int index, VariableEnum type)
        {
            if (index < 0 || index > variables.arraySize)
                return;

            variables.serializedObject.Update();
            variables.InsertArrayElementAtIndex(index);
            SerializedProperty variableProperty = variables.GetArrayElementAtIndex(index);
            variableProperty.FindPropertyRelative("variableType").enumValueIndex = (int)type;

            variableProperty.FindPropertyRelative("name").stringValue = "";
            variableProperty.FindPropertyRelative("objectValue").objectReferenceValue = null;
            variableProperty.FindPropertyRelative("dataValue").stringValue = "";

            variables.serializedObject.ApplyModifiedProperties();
            GUI.FocusControl(null);
        }

        protected virtual void DuplicateVariable(SerializedProperty variables, int index)
        {
            if (index < 0 || index >= variables.arraySize)
                return;

            variables.serializedObject.Update();
            variables.InsertArrayElementAtIndex(index);
            SerializedProperty variableProperty = variables.GetArrayElementAtIndex(index + 1);

            variableProperty.FindPropertyRelative("name").stringValue = "";
            variableProperty.FindPropertyRelative("objectValue").objectReferenceValue = null;
            variableProperty.FindPropertyRelative("dataValue").stringValue = "";

            variables.serializedObject.ApplyModifiedProperties();
            GUI.FocusControl(null);
        }

        protected virtual void AskRemoveVariable(SerializedProperty variables, int index)
        {
            if (variables == null || index < 0 || index >= variables.arraySize)
                return;

            var variable = variables.GetArrayElementAtIndex(index);
            var name = variable.FindPropertyRelative("name").stringValue;
            if (string.IsNullOrEmpty(name))
            {
                RemoveVariable(variables, index);
                return;
            }

            if (UnityEditor.EditorUtility.DisplayDialog("Confirm delete", Utility.Text.Format("Are you sure you want to delete the item named \"{0}\"?", name), "Yes", "Cancel"))
            {
                RemoveVariable(variables, index);
            }
        }

        protected virtual void RemoveVariable(SerializedProperty variables, int index)
        {
            if (index < 0 || index >= variables.arraySize)
                return;

            variables.serializedObject.Update();
            variables.DeleteArrayElementAtIndex(index);
            variables.serializedObject.ApplyModifiedProperties();
            GUI.FocusControl(null);
        }
    }
}