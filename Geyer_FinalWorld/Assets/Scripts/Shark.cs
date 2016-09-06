using UnityEngine;
using System.Collections;

//Enum for shark mode
enum SharkMode
{
    Wander,//Wander around the world
    Chase,//Chase a selected fish
    Eat//Wander around ignoring fish for a bit after eating one
}

//Vehicle class for shark
public class Shark : Vehicle
{
    private SharkMode sharkMode;//Current shark mode

    private float decisionTimer;//Time elapsed since last wander decision was made
    public float decisionInterval = 0.2f;//Amount of time to wait between each wander decision

    private float chaseTimer;//Counts up while shark is chasing a fish, if it takes too long shark will give up
    private float eatTimer;//Counts up after shark eats fish, used for waiting until chasing fish again

    public float chaseRadius = 75f;//If fish is within this radius, shark will attack them

    private Vector3 steerForce;//Force calculated in CalcSteeringForces and applied each frame

    private Vector3 wanderPoint;//Current point to seek for wandering, change every time decision timer hits interval

    private GameObject targetFish;//Current fish that shark is targeting (if no fish in range is null and will be set to nearest fish when one enters attack radius)


    //Force weights
    public float wanderWeight = 50f;
    public float chaseWeight = 50f;
    public float inBoundsWeight = 250f;

    //Initialization
    override public void Start()
    {
        base.Start();//Do base vehicle initialization

        wanderPoint = GetWanderPoint(20, 15);//Start wander point 20 units ahead of shark as temp until timer allows new one to be picked

        sharkMode = SharkMode.Wander;//Start mode in wander

        //Initialize timers to 0
        decisionTimer = 0;
        chaseTimer = 0;
        eatTimer = 0;
    }

    //Calculate and apply steering forces
    protected override void CalcSteeringForces()
    {
        //Reset steering force
        steerForce = Vector3.zero;

        //Increment decision timer
        decisionTimer += Time.deltaTime;

        //WANDER STATE
        if (sharkMode == SharkMode.Wander)
        {
            //If time for new decision, reset decision timer and get new wander point
            if (decisionTimer > decisionInterval)
            {
                decisionTimer = 0;
                wanderPoint = GetWanderPoint(20, 15);
            }

            //Add weighted steering force for seeking wander point
            steerForce += Seek(wanderPoint) * wanderWeight;

            targetFish = CheckForFish(chaseRadius);//Check for other fish, if any are within radius then set them as target

            //If the fish check didn't return null we found a fish to chase, set mode accordingly for next frame
            if (targetFish)
            {
                sharkMode = SharkMode.Chase;
            }
        }

        //CHASE STATE
        else if (sharkMode == SharkMode.Chase)
        {
            chaseTimer += Time.deltaTime;//Increment chase tiemr

            //If it's been chasing the fish for more than 30 seconds, the shark will give up
            //Additionally, if the target fish is already null for some reason, we should exit this mode
            if (chaseTimer > 30 || !targetFish)
            {
                targetFish = null;//Remove target

                sharkMode = SharkMode.Eat;//Set mode to eat so that shark will wander/ignore fish for a bit             
            }
            else
            { 
                //Add weighted steering force for pursuing target fish
                steerForce += Pursue(targetFish, 5) * chaseWeight;
            }

        }

        //EAT STATE
        else
        {        
            //Wander exactly like in wander state by picking new wander point and seeking it
            if (decisionTimer > decisionInterval)
            {
                decisionTimer = 0;
                
                wanderPoint = GetWanderPoint(20, 15f);           
            }

            steerForce += Seek(wanderPoint) * wanderWeight;

            //Increment eat timer
            eatTimer += Time.deltaTime;

            //Wait 10 seconds before returning to normal wander mode and checking for fish again
            if (eatTimer > 10)
            {
                chaseTimer = 0;
                eatTimer = 0;
                sharkMode = SharkMode.Wander;
            }    
        }

        //Add weighted steering force for staying in bounds of the world/avoiding ground collisions
        steerForce += StayInBounds() * inBoundsWeight;

        //Limit force to max force
        steerForce = Vector3.ClampMagnitude(steerForce, maxForce);

        //Apply force
        ApplyForce(steerForce);
    }


    // Check for other fish in radius around shark, if one is found then return it
    public GameObject CheckForFish(float attackRadius)
    {
        float attackRadiusSq = Mathf.Pow(attackRadius,2);//Square the attack radius to compare with distSq

        //Square distance to fish being checked
        float distSq;

        GameObject closestFish = null;//GameObject holds closest fish within attack radius to be returned (will be null if none within attack radius)
        float closestDistSq = attackRadiusSq;//Distance to current closest fish

        //Run through list of fish
        for (int i = 0; i < gm.FishList.Count; i++)
        {
            //Get dist squared to fish
            distSq = (transform.position - gm.FishList[i].transform.position).sqrMagnitude;

            //If this distance is within our attack radius, and closer than currently stored closest dist, we have a new closest fish
            if(distSq < attackRadiusSq && distSq < closestDistSq)
            {
                closestFish = gm.FishList[i];//Set closest fish to current fish

                closestDistSq = distSq;//Store distance sq to fish as closest distance sq
            }
        }
       
        return closestFish;//Return closest fish, will be null if none within attack radius
    }

    //Runs every time another collider enters this shark's trigger collider 
    public void OnTriggerEnter(Collider other)
    {
        //If the object being collided with is a fish and the shark isn't supposed to be ignoring fish...
        if (other.tag == "Fish" && sharkMode != SharkMode.Eat)
        {
            gm.KillFish(other.gameObject);//Kill the fish in game manager

            sharkMode = SharkMode.Eat;//Set shark mode to eat
        }
    }


}
