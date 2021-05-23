using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitObstcle : MonoBehaviour {
    [Range(0,10)]
    public float speed = 3;

    [Range(0,5)]
    public float orbitRadius = 1;

    [Range(-5, 5)]
    public float rotateHeightOffset = 0;

    public Transform actor1;
    public Transform actor2;
    private Vector3 rotationCenter;
    private float orbitRadius_old = 1;

    // Use this for initialization
    void Start () 
    {
        rotationCenter = (actor1.position + actor2.position) / 2;
        transform.position = (actor1.position + actor2.position) / 2 + new Vector3(0, 0, orbitRadius);
        transform.position += new Vector3(0, rotateHeightOffset, 0);
    }

    // Update is called once per frame
    void Update () 
    {
        // Parameter changed
        if (Mathf.Abs(orbitRadius_old-orbitRadius)>0.001f)
        {
            transform.position = (actor1.position + actor2.position) / 2 + new Vector3(0, 0, orbitRadius);
            orbitRadius_old = orbitRadius;
            transform.position += new Vector3(0, rotateHeightOffset, 0);
        }
        //transform.position += new Vector3(0, 0, orbitRadius);
        transform.RotateAround(rotationCenter, Vector3.up, 50 * Time.deltaTime * speed);

    }

    public void ChangeObstacleSize(UnityEngine.UI.Dropdown change)
    {
        switch (change.value) {
            case 0:
                transform.localScale = new Vector3(0.2f, 0.13f, 0.2f);
                orbitRadius = 1;
                break;
            case 1:
                transform.localScale = new Vector3(0.4f, 0.3f, 0.4f);
                orbitRadius = 1.5f;
                break;
        }
    }
}
