using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(VectorMagnitudeAttribute))]
public class VectorMagnitudeDrawer : PropertyDrawer {

	public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
		var vectorMagnitude = attribute as VectorMagnitudeAttribute;
		EditorGUI.BeginProperty (position, label, property);
		switch (property.propertyType) {
			case SerializedPropertyType.Vector2:
				property.vector2Value = EditorGUI.Vector2Field(position,
															   property.displayName,
															   property.vector2Value).normalized * vectorMagnitude.Magnitude;
				break;
			case SerializedPropertyType.Vector3:
				property.vector3Value = EditorGUI.Vector3Field(position,
															   property.displayName,
															   property.vector3Value).normalized * vectorMagnitude.Magnitude;
				break;
			case SerializedPropertyType.Vector4:
				property.vector4Value = EditorGUI.Vector4Field(position,
															   property.displayName,
															   property.vector4Value).normalized * vectorMagnitude.Magnitude;
				break;
			default:
				EditorGUI.HelpBox(position, string.Format("{0} is not an Vector but has [VectorMagnitude].", property.name), MessageType.Error);
				break;
		}
		EditorGUI.EndProperty();
	}
}