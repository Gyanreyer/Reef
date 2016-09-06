using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Require vehicles have a character controller
[RequireComponent(typeof(CharacterController))]

//Abstract class for vehicles
abstract public class Vehicle : MonoBehaviour
{
    protected GameManager gm;//Gamemanager

    //Acceleration and velocity of vehicle
    protected Vector3 acceleration;
    protected Vector3 velocity;

    //Desired vector to use in force calculations
    protected Vector3 desired;

    public float maxForce;//Max force of vehicle
    public float maxSpeed;//Max speed of vehicle
    public float mass;//Mass of vehicle

    protected CharacterController charControl;//Character controller of this object

    //Property to get velocity
    public Vector3 Velocity
    {
        get { return velocity; }
    }

    //Run at start to set up/initialize stuff
	virtual public void Start()
    {
        //Initialize accel and velocity
        acceleration = Vector3.zero;
        velocity = transform.forward;

        //Get character controller on object
        charControl = GetComponent<CharacterController>();

        //Get game manager
        gm = GameObject.Find("GameManagerGO").GetComponent<GameManager>();

    }

	//Run once every frame
	virtual public void Update ()
    {
        //Calculate all necessary steering forces
        CalcSteeringForces();

        //Add acceleration to velocity
        velocity += acceleration * Time.deltaTime;

        //Limit velocity magnitude to max speed
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);

        //Move position by current velocity
        charControl.Move(velocity * Time.deltaTime);
        transform.forward = velocity.normalized;//Set forward vector so that it faces in direction of velocity

        acceleration = Vector3.zero;//Reset acceleration

	}

    //Require that children have method to calculate steering forces every frame
    abstract protected void CalcSteeringForces();

    //Apply a given force to change vehicle's acceleration
    protected void ApplyForce(Vector3 steeringForce)
    {
        acceleration += (steeringForce / mass);//Add given force to accel divided by mass
    }

    //Get force to seek given position
    protected Vector3 Seek(Vector3 targetPosition)
    {
        //Desired vector from vehicle to position of target
        desired = targetPosition - transform.position;     

        desired -= velocity;//Subtract velocity from desired to get force to apply

        desired = desired.normalized * maxForce;//Set magnitude to max force

        return desired;//Return desired force to seek target position
    }

    //Get force to arrive at a point (seek and slow to a stop when within given arrival radius)
    protected Vector3 Arrive(Vector3 targetPosition, float arrivalRadius)
    {
        float distSq = (targetPosition - transform.position).sqrMagnitude;//Get dist squared from target point
        arrivalRadius *= arrivalRadius;//Square arrival radius for comparison with distSq

        desired = Seek(targetPosition);

        //If within arrival radius, reduce force by how close vehicle is to target
        if(distSq < arrivalRadius)
        {
            desired *= Mathf.Sqrt(distSq/arrivalRadius);//Scale force by distance/arrival radius so that in the center it is 0 and on the edge it is 1
        }

        return desired;//Return arrival force
    }

    //Seek a predicted position a given distance ahead of target
    public Vector3 Pursue(GameObject target, float predictDist)
    {
        //Get predicted position by multiplying target's forward vector by predict distance
        Vector3 predictedPos = target.transform.position + target.transform.forward * predictDist;

        return Seek(predictedPos);//Return force to seek predicted pos
    }


    //Returns point to wander toward, takes param for radius of sphere in which point will be selected
    public Vector3 GetWanderPoint(float pointDistance, float pointRadius)
    {
        Vector3 wanderPoint = transform.position + transform.forward * pointDistance;//Get a point given distance out in front of vehicle

        //Get a random point whose coordinates are centered around this point
        wanderPoint.x += Random.Range(-pointRadius,pointRadius);
        wanderPoint.y += Random.Range(-pointRadius/2, pointRadius/2);//y range halved so that there is less vertical motion than horizontal
        wanderPoint.z += Random.Range(-pointRadius, pointRadius);

        return wanderPoint;//Return new wander point
    }

    //Apply force to stay in bounds and to avoid colliding with the terrain
    public Vector3 StayInBounds()
    {
        desired = Vector3.zero;//Reset desired vector

        Vector3 futurePoint = transform.position + transform.forward * 20;//Get a future point ahead of vehicle to check against boundaries

        float terrainHeight = Terrain.activeTerrain.SampleHeight(futurePoint);//Get y height of terrain at this future point's x and z position

        //If the future point will be out of bounds horizontally on the x or z axes, simply set desired to steer back toward center point where x = 0, z = 0
        if(futurePoint.x < -100 || futurePoint.x > 100 || futurePoint.z < -100 || futurePoint.z > 100)
        {
            desired.x = -transform.position.x;
            desired.z = -transform.position.z;
        }

        //The the future point will be too high, set desired to steer down toward y = 20
        if(futurePoint.y > 40)
        {
            desired.y = 20-transform.position.y;
        }
        //If the future point is underneath the terrain, set desired to steer up toward y = 20
        else if(futurePoint.y < terrainHeight)
        {
            desired.y = 20+transform.position.y;
        }

        desired = desired.normalized * maxForce;//Set desired magnitude to max force

        return desired;//Return force to apply
    }

}
