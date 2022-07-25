using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    //Serialized variables
    [SerializeField] private float speed;
    [SerializeField] private Vector3 offset;

    //Private variables
    private Transform target;
    private Vector3 offsetVector;

    /// <summary>
    /// Find necessary references
    /// </summary>
    private void Awake()
    {
        //Find target
        target = GameObject.FindGameObjectWithTag("Player").transform;
    }

    /// <summary>
    /// Late frame by frame functionality.
    /// </summary>
    void LateUpdate()
    {
        if(target != null)
        {
            //Calculate desired position
            offsetVector = target.position + offset;
            if (Vector3.Distance(transform.position, offsetVector) > 0.1f)
            {
                //Calculate movement
                Vector3 motion = (offsetVector - transform.position);
                if (motion.magnitude < 2 || motion.magnitude > 4)
                {
                    motion = motion.normalized * motion.magnitude * speed * Time.deltaTime;
                }
                else
                {
                    motion = motion.normalized  * speed * Time.deltaTime;
                }
                //Apply movement to object
                transform.position += motion;
            }
            //Rotate toward target
            transform.LookAt(target);
        }
        else
        {
            Debug.LogError("Camera Controller: Camera target must be tagged as 'Player'!");
        }
    }
}