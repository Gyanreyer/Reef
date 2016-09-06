using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Class to manage general stuff going on in the world, put on GameManagerGO
public class GameManager : MonoBehaviour
{
    public GameObject fishBlood;//Prefab for fish blood particle system
    public GameObject fishPrefab;//Prefab for fish

    private Path path;//Path for leader to follow
    private int currentPathPoint;//Index of current point on path leader should seek

    private int numFishToSpawn = 15;//Number of fish to spawn in addition to leader

    private Vector3 followPoint;//Point behind leader for followers to seek/arrive at
    private float followDistance = 1.5f;//Distance behind leader that follow point will be

    private GameObject leader;//Current leader for fish
    private GameObject shark;//Shark that swims around attacking fish

    private List<GameObject> fishList;//List of currently active fish

    //Average position and direction of fish, this object's position and direction will be set to these each frame so that fish cam mode can follow it
    private Vector3 fishCentroid;
    private Vector3 fishDirection;

    //Properties
    public float FollowDistance
    {
        get { return followDistance; }
    }
    public Vector3 FollowPoint
    {
        get { return followPoint; }
    }
    public Vector3 NextPathPoint
    {
        get { return path.Points[currentPathPoint].transform.position; }
    }
    public List<GameObject> FishList
    {
        get { return fishList; }
    }
    public GameObject Leader
    {
        get { return leader; }
    }
    public GameObject Shark
    {
        get { return shark; }
    }

    public Vector3 Centroid
    {
        get { return fishCentroid; }
    }

    public Vector3 FishDirection
    {
        get { return fishDirection; }
    }

    // Use this for initialization
    void Start ()
    {
        path = GameObject.Find("Path").GetComponent<Path>();//Get the leader's path off Path object
        currentPathPoint = 0;//Start path at first point

        shark = GameObject.Find("Shark");//Get shark's game object

        fishList = new List<GameObject>();//Initialize fish list

        //Spawn in leader fish
        leader = (GameObject)Instantiate(fishPrefab, path.Points[0].Position, Quaternion.identity);//Spawn at first point on path
        leader.transform.LookAt(path.Points[1].Position);//Make leader face next point on path
        leader.GetComponent<Fish>().isLeader = true;//Make this fish a leader
        leader.GetComponent<Fish>().maxSpeed--;//Make max speed ever so slightly slower than the rest so followers can keep up

        followPoint = leader.transform.position - leader.transform.forward * followDistance;//Set follow point behind leader

        fishList.Add(leader);//Add leader to fish list, first fish in list will always be leader

        float spawnRad = 10f;//Radius for fish to spawn in around spawn point
        Vector3 spawnPoint = leader.transform.position - leader.transform.forward * spawnRad;//Set follow point as point 10 units behind leader to spawn followers around

        //Spawn follower fish around spawn point
        for (int i = 1; i <= numFishToSpawn; i++)
        {
            fishList.Add((GameObject)Instantiate(fishPrefab, new Vector3(Random.Range(spawnPoint.x - spawnRad, spawnPoint.x + spawnRad), Random.Range(spawnPoint.y - spawnRad, spawnPoint.y + spawnRad), Random.Range(spawnPoint.z - spawnRad, spawnPoint.z + spawnRad)), Quaternion.identity));
            FishList[i].transform.LookAt(FollowPoint);//Make fish orient toward follow point
        }

    }
	
	// Update is called once per frame
	void Update ()
    {
        if(Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }


        //If leader isn't null, update follow point and check for its arrival at current path point
        if (leader)
        {
            followPoint = leader.transform.position - leader.transform.forward * followDistance;
            CheckForArrival();
        }

        //Calculate average fish position and direction
        CalcFishCentroid();
        CalcFishDirection();

        //Set this object's position and direction to fish centroid and average direction so that fish cam can follow this object
        transform.position = fishCentroid;
        transform.forward = fishDirection;
    }

    //Calculate average position of fish
    void CalcFishCentroid()
    {
        if (fishList.Count > 0)
        {
            fishCentroid = Vector3.zero;

            //Sum up positions
            for (int i = 0; i < fishList.Count; i++)
            {
                fishCentroid += fishList[i].transform.position;
            }

            //Divide by number of fish to get average
            fishCentroid /= fishList.Count;
        }
    }

    //Calculate average direction of fish
    void CalcFishDirection()
    {
        if (fishList.Count > 0)
        {
            fishDirection = Vector3.zero;

            //Sum up forward vectors
            for (int i = 0; i < fishList.Count; i++)
            {
                fishDirection += fishList[i].transform.forward;
            }

            //Divide by number of fish to get average and then normalize
            fishDirection /= fishList.Count;
            fishDirection.Normalize();
        }
    }

    //Check if leader has arrived at point on path and needs to seek next one
    void CheckForArrival()
    {
        //If the distance from leader to current target point is less than path's arrival radius, set next point to be followed
        if ((leader.transform.position - path.Points[currentPathPoint].Position).sqrMagnitude < Mathf.Pow(path.radius,2))
        {
            currentPathPoint = (currentPathPoint + 1) % path.Points.Length;//Increment index of current path point and use modulo so it loops back to start
        }
    }

    //Kill a fish on fish list
    public void KillFish(GameObject fish)
    {
       
        Instantiate(fishBlood, fish.transform.position, Quaternion.identity);//Spawn particle system for fish blood at fish's position

        fishList.Remove(fish);//Remove fish from list

        //If fish being killed was the leader and there are still living fish, set new fish for leader
        if (fish.GetComponent<Fish>().isLeader && fishList.Count > 0)
        {
            //Set first fish in list as new leader
            FishList[0].GetComponent<Fish>().isLeader = true;
            leader = fishList[0];          
        }

        Destroy(fish);//Destroy the fish's game object

    }



}
