using System.Collections.Generic;
using UnityEngine;

namespace ProceduralWorlds.Storm.CityBuilder
{
    public class Vector3CompareBehavior : EqualityComparer<Vector3>
    {
    private float tolerance;

    public Vector3CompareBehavior(float tolerance = 0.001f)
    {
        this.tolerance = tolerance;
    }

    public override bool Equals(Vector3 a, Vector3 b)
    {
        // Consider vectors equal if they're within tolerance distance
        return Vector3.Distance(a, b) < tolerance;
    }

    public override int GetHashCode(Vector3 vector)
    {
        // Round to reduce precision for hashing
        return new Vector3(
            Mathf.Round(vector.x / tolerance) * tolerance,
            Mathf.Round(vector.y / tolerance) * tolerance,
            Mathf.Round(vector.z / tolerance) * tolerance
        ).GetHashCode();
    }

    }
}

