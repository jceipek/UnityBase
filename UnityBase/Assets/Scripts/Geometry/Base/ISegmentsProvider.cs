using UnityEngine;

public interface ISegmentsProvider {
    Vector3 Segment (int i);
    Vector3 Tangent (int i);
    int Count { get; }
}