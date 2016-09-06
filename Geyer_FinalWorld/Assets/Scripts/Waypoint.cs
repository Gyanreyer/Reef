using UnityEngine;
using System.Collections;

public class Waypoint : MonoBehaviour
{ 

    //Property returns waypoint's position
    public Vector3 Position
    {
        get { return transform.position;}
    }

    //Draw sphere gizmos for each waypoint
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position,1);
    }
}
