using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GizmoHelper : MonoBehaviour
{
    public static void DrawWireCapsule(Vector3 p1, Vector3 p2, float radius)
    {
        // draw end spheres;
        Gizmos.DrawWireSphere(p1, radius);
        Gizmos.DrawWireSphere(p2, radius);

        // draw plane connecting spheres
        Vector3 from = p1;
        Vector3 to = p2;

        from.x += radius;
        to.x += radius;
        Gizmos.DrawLine(from, to);
        from.x -= radius*2;
        to.x -= radius*2;
        Gizmos.DrawLine(from, to);
        from.z -= radius;
        from.x += radius;
        to.z -= radius;
        to.x += radius;
        Gizmos.DrawLine(from, to);
        from.z += radius*2;
        to.z += radius*2;
        Gizmos.DrawLine(from, to);
    }
}
