using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkingObstacle : MonoBehaviour {

    UnityEngine.AI.NavMeshAgent agent;
    Vector3 initialPosition;

	// Use this for initialization
	void Start () {
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        initialPosition = transform.position;
        agent.SetDestination(-transform.position);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void ResetPosition()
    {
        transform.position = initialPosition;
        agent.SetDestination(-transform.position);
    }
}
