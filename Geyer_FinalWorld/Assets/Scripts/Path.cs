using UnityEngine;
using System.Collections;

//Class for path for leader to follow, put on GameObject with all waypoints on path as children
public class Path : MonoBehaviour {

    //Array of points for path
    private Waypoint[] points;

    public float radius;//Radius of arrival at points

    //Property for points array
    public Waypoint[] Points
    {
        get { return points; }
    }

	// Use this for initialization, executes before any Start() methods so that GameManager can access points array
	void Awake ()
    {
        radius = 10f;
        
        points = new Waypoint[transform.childCount];//Number of children this object has is how many waypoints are on path

        //Get waypoint children and store them in path array
        for (int i = 0; i < points.Length; i++)
        {
            points[i] = transform.GetChild(i).GetComponent<Waypoint>();
        }        
	}
}
