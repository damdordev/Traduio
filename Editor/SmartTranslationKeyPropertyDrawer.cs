using Traduio;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SmartTranslationKey))]
public class SmartTranslationKeyPropertyDrawer : PropertyDrawer
{
    private SerializedProperty modeProperty;
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
            modeProperty,
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
        modeProperty = property.FindPropertyRelative("mode");
        valueProperty = property.FindPropertyRelative("value");
    }

}
