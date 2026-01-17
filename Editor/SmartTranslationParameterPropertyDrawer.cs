using UnityEditor;
using UnityEngine;

namespace Traduio.Editor
{
    [CustomPropertyDrawer(typeof(SmartTranslationParameter))]
    public class SmartTranslationParameterPropertyDrawer : PropertyDrawer
    {
        private SerializedProperty keyProperty;
        private SerializedProperty valueProperty;
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            RetrieveProperties(property);
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            RetrieveProperties(property);
            EditorGUI.PropertyField(
                new Rect(
                    position.x,
                    position.y,
                    position.width / 4,
                    EditorGUIUtility.singleLineHeight
                ),
                keyProperty,
                GUIContent.none
            );
            EditorGUI.PropertyField(
                new Rect(
                    position.x + position.width / 4,
                    position.y,
                    3 * position.width / 4,
                    EditorGUIUtility.singleLineHeight
                ),
                valueProperty,
                GUIContent.none
            );
        }

        private void RetrieveProperties(SerializedProperty property)
        {
            keyProperty = property.FindPropertyRelative("key");
            valueProperty = property.FindPropertyRelative("value");
        }
    }
}