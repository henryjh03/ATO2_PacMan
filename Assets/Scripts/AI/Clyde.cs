using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clyde : Ghost
{
    [SerializeField] private Vector2 behaviourSwitchTime = new Vector2(5, 30);

    private float time = 0;
    private float timer = -1;

    protected override void Awake()
    {
        base.Awake();
        DefaultState = new GhostState_Chase(this);
    }

    protected override void Start()
    {
        base.Start();
        time = Random.Range(behaviourSwitchTime.x, behaviourSwitchTime.y);  //Set random interval
        timer = 0;  //Start timer
    }

    protected override void Update()
    {
        base.Update();
        if (GameManager.Instance.PowerUpTimer == -1 && CurrentState != RespawnState)
        {
            timer += Time.deltaTime;    //Increment timer
            if(timer >= time)
            {
                //Change states
                if (CurrentState == DefaultState)
                {
                    SetState(FleeState);
                }
                else if (CurrentState == FleeState)
                {
                    SetState(DefaultState);
                }
                time = Random.Range(behaviourSwitchTime.x, behaviourSwitchTime.y);  //Set random interval
                timer = 0;  //Reset timer
            }
        }
    }
}
