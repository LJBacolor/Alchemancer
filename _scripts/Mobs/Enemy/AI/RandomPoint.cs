using UnityEngine;
using UnityEngine.AI;

public class RandomPoint : MonoBehaviour
{
    public static Vector3 randomPoint(Vector3 startPoint, float radius)
    {
        Vector3 Dir = Random.insideUnitSphere * radius;
        Dir += startPoint;
        NavMeshHit hit;
        Vector3 finalPos = Vector3.zero;
        if(NavMesh.SamplePosition(Dir, out hit, radius, 1))
        {
            finalPos = hit.position;
        }
        return finalPos;
    }
}
