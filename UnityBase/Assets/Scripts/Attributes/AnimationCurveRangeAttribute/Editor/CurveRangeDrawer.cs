using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(CurveRangeAttribute))]
public class CurveRangeDrawer : PropertyDrawer {

	public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
		CurveRangeAttribute curveRange = attribute as CurveRangeAttribute;

		EditorGUI.BeginProperty (position, label, property);
        if (property.propertyType != SerializedPropertyType.AnimationCurve) {
            EditorGUI.HelpBox(position, string.Format("{0} is not an AnimationCurve but has [CurveRange].", property.name), MessageType.Error);
        } else {
        	property.animationCurveValue = EditorGUI.CurveField(position, property.displayName, property.animationCurveValue, curveRange.Color, curveRange.Ranges);
        }
		EditorGUI.EndProperty();
	}
}