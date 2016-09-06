using UnityEngine;
using System.Collections;

public class ParticleSystem : MonoBehaviour {

    public float aliveTime;

	// Use this for initialization
	void Start () {
        Destroy(gameObject, aliveTime);
	}
	
}
