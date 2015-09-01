using UnityEditor;
using UnityEngine;
using UnityEditorInternal;

[CustomEditor(typeof(HermiteSpline))]
public class HermiteSplineEditor : Editor {

	SerializedObject _serializedHermiteSpline;
	ReorderableList _hermiteNodesList;

	void OnEnable () {
		_serializedHermiteSpline = new SerializedObject(target);
		var hermiteNodes = _serializedHermiteSpline.FindProperty("_hermiteNodes");
		_hermiteNodesList = new ReorderableList(serializedObject,
                				 hermiteNodes, true, true, true, true);
		_hermiteNodesList.elementHeight = EditorGUIUtility.singleLineHeight * 3f;
		_hermiteNodesList.drawHeaderCallback += rect => GUI.Label(rect, hermiteNodes.displayName);
		_hermiteNodesList.drawElementCallback += (rect, index, active, focused) => {
		    rect.height = EditorGUIUtility.singleLineHeight;

		    var prop = _hermiteNodesList.serializedProperty.GetArrayElementAtIndex(index);
		    var pointProp = prop.FindPropertyRelative("Point");
		    var controlPointProp = prop.FindPropertyRelative("ControlPoint");
			pointProp.vector3Value = EditorGUI.Vector3Field(rect, "P "+index, pointProp.vector3Value);
		    rect.y += rect.height;
			controlPointProp.vector3Value = EditorGUI.Vector3Field(rect, "CP "+index, controlPointProp.vector3Value);
		};
	}

	private bool _autoCompute = false;
	private bool _showControlPoints = true;
	public override void OnInspectorGUI () {
		_serializedHermiteSpline.Update();
		_showControlPoints = GUILayout.Toggle(_showControlPoints, "Display Control Points");
		EditorGUILayout.PropertyField(_serializedHermiteSpline.FindProperty("_displayResolution"));
		EditorGUILayout.PropertyField(_serializedHermiteSpline.FindProperty("_intervals"));
		EditorGUILayout.PropertyField(_serializedHermiteSpline.FindProperty("_guessCount"));
		_hermiteNodesList.DoLayoutList();
		var hermiteSpline = target as HermiteSpline;
		_autoCompute = GUILayout.Toggle(_autoCompute, "Auto-Compute Control Points");
        if (_autoCompute) {
            hermiteSpline.AutoComputeCatmullRomControlPoints();
            SceneView.RepaintAll();
        }
        var closedCurveProp = _serializedHermiteSpline.FindProperty("_closedCurve");
        closedCurveProp.boolValue = GUILayout.Toggle(closedCurveProp.boolValue, "Close Curve");
        if (closedCurveProp.boolValue) {
			hermiteSpline.EnsureClosedCurve();
            SceneView.RepaintAll();
        }
		_serializedHermiteSpline.ApplyModifiedProperties();
	}

	void OnSceneGUI () {
		_serializedHermiteSpline.Update();

		var hermiteNodes = _serializedHermiteSpline.FindProperty("_hermiteNodes");
		int nodeCount = hermiteNodes.arraySize;

		var closedCurve = _serializedHermiteSpline.FindProperty("_closedCurve").boolValue;
		if (closedCurve) {
			nodeCount--;
		}

		for (int i = 0; i < nodeCount; i++) {
			var node = hermiteNodes.GetArrayElementAtIndex(i);
			var pointProp = node.FindPropertyRelative("Point");
			var controlPointProp = node.FindPropertyRelative("ControlPoint");
			var newPointPos = Handles.FreeMoveHandle(pointProp.vector3Value,
													 Quaternion.identity,
													 HandleUtility.GetHandleSize(pointProp.vector3Value) * 0.1f,
													 Vector3.one, Handles.SphereCap);
			controlPointProp.vector3Value += newPointPos - pointProp.vector3Value;
			pointProp.vector3Value = newPointPos;
		}

		for (int i = 0; i < nodeCount && _showControlPoints; i++) {
			var node = hermiteNodes.GetArrayElementAtIndex(i);
			var pointProp = node.FindPropertyRelative("Point");
			var controlPointProp = node.FindPropertyRelative("ControlPoint");
			controlPointProp.vector3Value = Handles.FreeMoveHandle(controlPointProp.vector3Value,
																   Quaternion.identity,
																   HandleUtility.GetHandleSize(controlPointProp.vector3Value) * 0.1f,
																   Vector3.one, Handles.RectangleCap);
			Handles.DrawLine(pointProp.vector3Value, controlPointProp.vector3Value);
		}

		_serializedHermiteSpline.ApplyModifiedProperties();
	}
}