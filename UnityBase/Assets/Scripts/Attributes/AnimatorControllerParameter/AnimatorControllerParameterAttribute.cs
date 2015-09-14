using UnityEngine;
public class AnimatorControllerParameterAttribute : PropertyAttribute {
	public AnimatorControllerParameterType ParamType {get; set;}

	public AnimatorControllerParameterAttribute (AnimatorControllerParameterType paramType) {
		ParamType = paramType;
	}
}