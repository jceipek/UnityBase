using UnityEngine;

public static class MathHelpers {
    public static float ScaleInputBySymmetricCurve (float input, AnimationCurve curve) {
        return curve.Evaluate(Mathf.Abs(input)) * Mathf.Sign(input);
    }

    public static float ScaleInputByAsymmetricCurve (float input, AnimationCurve curve) {
        return curve.Evaluate(input);
    }

	public static float LinMap (float inputStart, float inputEnd, float outputStart, float outputEnd, float inputValue) {
		float domain = inputEnd - inputStart;
		float range = outputEnd - outputStart;
		return (inputValue-inputStart)/domain * range + outputStart;
	}

    public static float LinMapFrom01 (float outputStart, float outputEnd, float inputValue01) {
        float range = outputEnd - outputStart;
        return inputValue01 * range + outputStart;
    }

    public static float LinMapTo01 (float inputStart, float inputEnd, float inputValue) {
        float domain = inputEnd - inputStart;
        return (inputValue-inputStart)/domain;
    }

    public static int Mod (int n, int m) {
        return ((n % m) + m) % m;
    }

    public static Vector2 RandomOnUnitCircle {
        get {
            float sample = Mathf.PI * 2f * Random.value;
            return new Vector2(Mathf.Cos(sample), Mathf.Sin(sample));
        }
    }
}