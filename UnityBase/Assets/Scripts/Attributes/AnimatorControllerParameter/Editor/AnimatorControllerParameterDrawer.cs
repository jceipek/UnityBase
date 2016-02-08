using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(AnimatorControllerParameterAttribute))]
public class AnimatorControllerParameterDrawer : PropertyDrawer {

	StateMachineBehaviourContext[] _context = null;

	void GetContextForPropIfNecessary (SerializedProperty property) {
		if (_context == null) {
			_context = AnimatorController.FindStateMachineBehaviourContext(property.serializedObject.targetObject as StateMachineBehaviour);
		}
	}

	public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
		AnimatorControllerParameterAttribute animatorControllerParameter = attribute as AnimatorControllerParameterAttribute;

        if (property.propertyType != SerializedPropertyType.String) {
            EditorGUI.HelpBox(position, string.Format("{0} is not an string but has [AnimatorControllerParameter].", property.name), MessageType.Error);
        }

		int currSelectedIndex = 0;
		bool foundSelectedIndex = false;
		GetContextForPropIfNecessary(property);
		List<AnimatorControllerParameter> validParams = new List<AnimatorControllerParameter>();
		var validParamNames = new List<string>();
		var validParamHashes = new List<int>();
		if (_context != null) {
			AnimatorControllerParameter[] parameters = _context[0].animatorController.parameters;
			for (int i = 0; i < parameters.Length; i++) {
				if (parameters[i].type == animatorControllerParameter.ParamType) {
					validParams.Add(parameters[i]);
					validParamNames.Add(parameters[i].name);
					validParamHashes.Add(parameters[i].nameHash);
					if (parameters[i].name == property.stringValue) {
						foundSelectedIndex = true;
					}
					if (!foundSelectedIndex) {
						currSelectedIndex++;
					}
				}
			}
		}
        EditorGUI.BeginProperty(position, label, property);
		int selectedIndex = EditorGUI.Popup(position, label.text, currSelectedIndex, validParamNames.ToArray());
		if (selectedIndex > validParamNames.Count-1 || selectedIndex < 0) {
			Debug.LogError("AnimatorControllerParameter doesn't exist in "+(property.serializedObject.targetObject as StateMachineBehaviour)+" of "+_context[0].animatorObject.name);
			return;
		}
        string newValue = validParamNames[selectedIndex];
        if (newValue != property.stringValue) {
            property.stringValue = newValue;
        }
        EditorGUI.EndProperty();
	}
}