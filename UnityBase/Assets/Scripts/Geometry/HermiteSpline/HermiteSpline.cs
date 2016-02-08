using UnityEngine;

[System.Serializable]
public struct HermiteNode {
    public Vector3 Point;
    public Vector3 ControlPoint;
    public HermiteNode (Vector3 point, Vector3 controlPoint) {
        Point = point;
        ControlPoint = controlPoint;
    }
}

public class HermiteSpline : MonoBehaviour, ISegmentsProvider {
    [SerializeField] HermiteNode[] _hermiteNodes = new HermiteNode[3];
    [SerializeField] int _displayResolution = 20;
    [SerializeField] bool _closedCurve = false;

    [Header("Tune ClosestHermitePointToPoint (Newton-Raphson)")]
    [SerializeField] int _intervals = 8;
    [SerializeField] int _guessCount = 10;

    void OnEnable () {
        if (_closedCurve) {
            EnsureClosedCurve();
        }
    }

    void OnDrawGizmos () {
        // AutoComputeCatmullRomControlPoints();
        var stepSize = 1f/(_displayResolution);
        Vector3 lastPoint = _hermiteNodes[0].Point;
        for (int j = 0; j < _hermiteNodes.Length - 1; j++) {
            if (j == _hermiteNodes.Length - 2) {
                // last point of last segment should reach p1
                stepSize = 1f/(_displayResolution - 1f);
            }
            for (int i = 0; i < _displayResolution; i++) {
                var hA = HermitePointN(j);
                var hB = HermitePointN(j+1);
                var currPoint = PointAtFraction(hA, hB, stepSize * i);
                var currTangent = TangentAtFraction(hA, hB, stepSize * i);

                Vector3 pos = Vector3.zero;//transform.position;
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(lastPoint + pos, currPoint + pos);
                Gizmos.color = Color.green;
                Gizmos.DrawLine(currPoint + pos, currPoint + currTangent + pos);
                lastPoint = currPoint;
            }
        }
    }

    public void EnsureClosedCurve () {
        if (_hermiteNodes.Length > 1) {
            _hermiteNodes[_hermiteNodes.Length - 1] = _hermiteNodes[0];
        }
    }

    public void AutoComputeCatmullRomControlPoints () {
        for (int i = 0; i < _hermiteNodes.Length-1; i++) {
            if (i > 0) {
                _hermiteNodes[i].ControlPoint = (_hermiteNodes[i + 1].Point -
                                                _hermiteNodes[i - 1].Point)/2f;
            } else {
                _hermiteNodes[i].ControlPoint = (_hermiteNodes[i + 1].Point -
                                                _hermiteNodes[i].Point)/2f;
            }
            if (i == _hermiteNodes.Length - 2) {
                _hermiteNodes[i+1].ControlPoint = (_hermiteNodes[i + 1].Point -
                                                   _hermiteNodes[i].Point)/2f;
            }
        }
    }

    private HermiteNode HermitePointN (int n) {
        return _hermiteNodes[n];
    }

    public HermiteNode ClosestHermitePointToPoint (Vector3 point) {
        return ClosestHermitePointToPoint (point, _intervals, _guessCount);
    }

    public HermiteNode ClosestHermitePointToPoint (Vector3 point, int intervals, int guessCount) {
        HermiteNode hA;
        HermiteNode hB;
        // This is iterative Newton-Raphson (finding the roots of a function)
        // guess_i+1 = guess_i - f(x)/f'(x)
        // in this case, f = SqrDistanceDerivativeAtFraction
        // and f' = SecondSqrDistanceDerivativeAtFraction
        // We can't use f = SqrDistanceAtFraction because we want to find the minimums of
        // SqrDistanceAtFraction rather than where SqrDistanceAtFraction is 0; point isn't on the curve.
        // However, the zeroes of the first derivative are the potential min distances to the curve

        // we start guess_0 with a basic subdivision search; Newton-Raphson needs a good initial guess
        float guessFraction = ClosestGuessToPoint(point, intervals, out hA, out hB);
        while (guessCount > 0) {
            guessFraction = NewtonRaphson(point, hA.Point, hA.ControlPoint, hB.Point, hB.ControlPoint, guessFraction);
            guessCount--;
        }

        return new HermiteNode(PointAtFraction(hA, hB, guessFraction),
                               TangentAtFraction(hA, hB, guessFraction));
    }

    private float ClosestGuessToPoint (Vector3 point, int intervals,
                                       out HermiteNode hermiteA, out HermiteNode hermiteB) {
        var bestSqrDist = Mathf.Infinity;
        float bestFraction = 0f;
        hermiteA = _hermiteNodes[0];
        hermiteB = _hermiteNodes[_hermiteNodes.Length - 1];

        var stepSize = 1f/intervals;
        for (int j = 0; j < _hermiteNodes.Length - 1; j++) {
            if (j == _hermiteNodes.Length - 2) {
                // last point of last segment should reach p1
                stepSize = 1f/(intervals - 1f);
            }
            for (int i = 0; i < intervals; i++) {
                var hA = HermitePointN(j);
                var hB = HermitePointN(j+1);
                var fraction = stepSize * i;
                var currPoint = PointAtFraction(hA, hB, fraction);
                var currSqrDist = (currPoint - point).sqrMagnitude;
                if (currSqrDist < bestSqrDist) {
                    bestSqrDist = currSqrDist;
                    hermiteA = hA;
                    hermiteB = hB;
                    bestFraction = fraction;
                }
            }
        }

        return bestFraction;
    }

    private float SqrDistanceAtFraction (Vector3 testPoint,
                                         Vector3 pointA, Vector3 controlPointA,
                                         Vector3 pointB, Vector3 controlPointB, float fraction) {
        return ((2f*Mathf.Pow(fraction, 3) - 3f*Mathf.Pow(fraction, 2) + 1f) * pointA
              + (Mathf.Pow(fraction, 3) - 2f*Mathf.Pow(fraction, 2) + fraction) * controlPointA
              + (-2f*Mathf.Pow(fraction, 3) + 3f*Mathf.Pow(fraction, 2)) * pointB
              + (Mathf.Pow(fraction, 3) - Mathf.Pow(fraction, 2)) * controlPointB
              - testPoint).sqrMagnitude;
    }

    private float SqrDistanceDerivativeAtFraction (Vector3 testPoint,
                                                   Vector3 pointA, Vector3 controlPointA,
                                                   Vector3 pointB, Vector3 controlPointB, float fraction) {
    // Derivative of
    // ((Symbol["Tx"] - ((2*t^3 - 3*t^2 + 1) * Symbol["Ax"] + (t^3 - 2*t^2 + t) * Symbol["cAx"] + (-2*t^3 + 3*t^2) * Symbol["Bx"] + (t^3 - t^2) * Symbol["cBx"]))^2 +
    // (Symbol["Ty"] - ((2*t^3 - 3*t^2 + 1) * Symbol["Ay"] + (t^3 - 2*t^2 + t) * Symbol["cAy"] + (-2*t^3 + 3*t^2) * Symbol["By"] + (t^3 - t^2) * Symbol["cBy"]))^2 +
    // (Symbol["Tz"] - ((2*t^3 - 3*t^2 + 1) * Symbol["Az"] + (t^3 - 2*t^2 + t) * Symbol["cAz"] + (-2*t^3 + 3*t^2) * Symbol["Bz"] + (t^3 - t^2) * Symbol["cBz"]))^2)^(1/2)

        float t = fraction;
        float t2 = t*t;
        float t3 = t2*t;

        Vector3 intermediate = 2*Vector3.Scale(-pointA*(1-3*t2+2*t3) -
                                  pointB*(3*t2-2*t3) +
                                  testPoint -
                                  controlPointA * (t-2*t2 + t3) -
                                  controlPointB * (-t2+t3),
                                 -pointA*(-6*t+6*t2) -
                                  pointB * (6*t-6*t2) -
                                  controlPointA *
                                  (1-4*t+3*t2) -
                                  controlPointB * (-2*t+3*t2));

        return intermediate.x + intermediate.y + intermediate.z;
    }

    private float SecondSqrDistanceDerivativeAtFraction (Vector3 testPoint,
                                                         Vector3 pointA, Vector3 controlPointA,
                                                         Vector3 pointB, Vector3 controlPointB, float fraction) {
    // Second Derivative of
    // ((Symbol["Tx"] - ((2*t^3 - 3*t^2 + 1) * Symbol["Ax"] + (t^3 - 2*t^2 + t) * Symbol["cAx"] + (-2*t^3 + 3*t^2) * Symbol["Bx"] + (t^3 - t^2) * Symbol["cBx"]))^2 +
    // (Symbol["Ty"] - ((2*t^3 - 3*t^2 + 1) * Symbol["Ay"] + (t^3 - 2*t^2 + t) * Symbol["cAy"] + (-2*t^3 + 3*t^2) * Symbol["By"] + (t^3 - t^2) * Symbol["cBy"]))^2 +
    // (Symbol["Tz"] - ((2*t^3 - 3*t^2 + 1) * Symbol["Az"] + (t^3 - 2*t^2 + t) * Symbol["cAz"] + (-2*t^3 + 3*t^2) * Symbol["Bz"] + (t^3 - t^2) * Symbol["cBz"]))^2)^(1/2)

        float t = fraction;
        float t2 = t*t;
        float t3 = t2*t;

        Vector3 part1 = (-pointA*(-6*t + 6*t2) - pointB*(6*t - 6*t2) - controlPointA*(1 - 4*t + 3*t2) - controlPointB*(-2*t + 3*t2));
        part1 = Vector3.Scale(part1, part1);
        Vector3 part2 = (-pointA*(-6 + 12*t) - pointB*(6 - 12*t) - controlPointA*(-4 + 6*t) - controlPointB*(-2 + 6*t));
        Vector3 part3 = (-pointA*(1 - 3*t2 + 2*t3) - pointB*(3*t2 - 2*t3) + testPoint - controlPointA*(t - 2*t2 + t3) - controlPointB*(-t2 + t3));
        Vector3 intermediate = 2*(part1 + Vector3.Scale(part2, part3));

        return intermediate.x + intermediate.y + intermediate.z;
    }

    private float NewtonRaphson (Vector3 testPoint,
                                 Vector3 pointA, Vector3 controlPointA,
                                 Vector3 pointB, Vector3 controlPointB, float guessFraction) {
        float sqrDist = SqrDistanceDerivativeAtFraction(testPoint,
                                              pointA, controlPointA,
                                              pointB, controlPointB, guessFraction);
        float sqrDistDeriv = SecondSqrDistanceDerivativeAtFraction(testPoint,
                                                             pointA, controlPointA,
                                                             pointB, controlPointB, guessFraction);
        return Mathf.Clamp01(guessFraction - sqrDist/sqrDistDeriv); // better guess
    }

    private Vector3 PointAtFraction (HermiteNode hermiteNodeA, HermiteNode hermiteNodeB, float fraction) {
        return PointAtFraction(hermiteNodeA.Point, hermiteNodeA.ControlPoint,
                               hermiteNodeB.Point, hermiteNodeB.ControlPoint,
                               fraction);
    }

    private Vector3 PointAtFraction (Vector3 pointA, Vector3 controlPointA,
                                    Vector3 pointB, Vector3 controlPointB, float fraction) {
        return (2f*Mathf.Pow(fraction, 3) - 3f*Mathf.Pow(fraction, 2) + 1f) * pointA
             + (Mathf.Pow(fraction, 3) - 2f*Mathf.Pow(fraction, 2) + fraction) * controlPointA
             + (-2f*Mathf.Pow(fraction, 3) + 3f*Mathf.Pow(fraction, 2)) * pointB
             + (Mathf.Pow(fraction, 3) - Mathf.Pow(fraction, 2)) * controlPointB;
    }

    private Vector3 TangentAtFraction (HermiteNode hermiteNodeA, HermiteNode hermiteNodeB, float fraction) {
        return TangentAtFraction(hermiteNodeA.Point, hermiteNodeA.ControlPoint,
                               hermiteNodeB.Point, hermiteNodeB.ControlPoint,
                               fraction);
    }

    private Vector3 TangentAtFraction (Vector3 pointA, Vector3 controlPointA,
                                      Vector3 pointB, Vector3 controlPointB, float fraction) {
        return (fraction*(controlPointB*(3*fraction - 2) +
                          6*(pointA - pointB)*(fraction - 1)) +
                controlPointA*(3*Mathf.Pow(fraction, 2) -
                               4*fraction + 1)).normalized;
    }

    public Vector3 Segment (int i) {
        float rawFrac = i/(float)_displayResolution;
        int index = (int)rawFrac;
        float fraction = rawFrac - index;
        var hA = HermitePointN(index);
        var hB = HermitePointN(index+1);
        return PointAtFraction(hA, hB, fraction);
    }

    public Vector3 Tangent (int i) {
        float rawFrac = i/(float)_displayResolution;
        int index = (int)rawFrac;
        float fraction = rawFrac - index;
        var hA = HermitePointN(index);
        var hB = HermitePointN(index+1);
        return TangentAtFraction(hA, hB, fraction);
    }

    public int Count { get { return (_hermiteNodes.Length - 1) * _displayResolution; } }
}