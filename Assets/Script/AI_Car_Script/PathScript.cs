using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathScript: MonoBehaviour
{
    public Transform[] Path;
    public Color color;
    private void OnDrawGizmos()
    {
        Gizmos.color = color;
        if (Path == null || transform.childCount!=Path.Length)
        {
            Path = new Transform[transform.childCount];
            for (int i = 0; i<transform.childCount; i++)
            {
                Path[i] = transform.GetChild(i);
            }
        }
        for(int i=0; i<Path.Length; i++)
        {
            Gizmos.DrawWireSphere(Path[i].position, 2f);
            if (i > 0) Gizmos.DrawLine(Path[i].position, Path[i - 1].position);
            if (i<Path.Length-1) Gizmos.DrawLine(Path[i].position,Path[i + 1].position);
        }
    }
}
