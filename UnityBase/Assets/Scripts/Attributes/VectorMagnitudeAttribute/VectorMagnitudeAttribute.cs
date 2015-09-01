using UnityEngine;
public class VectorMagnitudeAttribute : PropertyAttribute {
	public float Magnitude { get; protected set; }

	public VectorMagnitudeAttribute () : this(1f) {}

	public VectorMagnitudeAttribute (float magnitude) {
		Magnitude = magnitude;
	}
}