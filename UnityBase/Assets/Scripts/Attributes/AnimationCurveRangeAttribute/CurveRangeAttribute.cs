using UnityEngine;
public class CurveRangeAttribute : PropertyAttribute {
	public Rect Ranges { get; set; }
	public Color Color { get; set; }

	public CurveRangeAttribute () : this(0f, 1f, 0f, 1f) {}

	public CurveRangeAttribute (float[] color) : this(0f, 1f, 0f, 1f, color) {}

	public CurveRangeAttribute (float minX, float maxX, float minY, float maxY) : this(minX, maxX, minY, maxY, new float[] {0f,1f,0f}) {}

	public CurveRangeAttribute (float minX, float maxX, float minY, float maxY, float[] color) : this(new Rect(minX, minY, maxX - minX, maxY - minY), color) {}

	public CurveRangeAttribute (Rect rect, float[] color) {
		Ranges = rect;
		Color = new Color(color[0], color[1], color[2]);
	}
}