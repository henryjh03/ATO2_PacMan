using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Henry Johns-Hall Last Updated on 9/11/2022
public class MinimapScript : MonoBehaviour
{
    public Transform player;

    void LateUpdate()
    {
        Vector3 newPosition = player.position;
        newPosition.y = transform.position.y;
        transform.position = newPosition;
    }
}

