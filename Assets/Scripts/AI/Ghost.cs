using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class Ghost : MonoBehaviour
{
    //Serialized variables
    [SerializeField] private Material fleeMaterial;
    [SerializeField] private Material respawnMaterial;

    //Auto-properties
    public Pacman Target { get; private set; }
    public NavMeshAgent Agent { get; private set; }
    public MeshRenderer GhostRenderer { get; private set; }
    public Material DefaultMaterial { get; private set; }
    public GhostState DefaultState { get; protected set; }
    public GhostState_Flee FleeState { get; private set; }
    public GhostState_Respawn RespawnState { get; private set; }
    public GhostState CurrentState { get; private set; }

    /// <summary>
    /// Creates necessary references.
    /// </summary>
    protected virtual void Awake()
    {
        //Get mesh renderer reference
        TryGetComponent(out MeshRenderer renderer);
        if (renderer != null)
        {
            GhostRenderer = renderer;
            DefaultMaterial = GhostRenderer.material;
        }
        else
        {
            Debug.LogError($"Ghost: {gameObject.name} must have a Mesh Renderer!");
        }
        //Get nav mesh agent reference
        TryGetComponent(out NavMeshAgent agent);
        if(agent != null)
        {
            Agent = agent;
        }
        else
        {
            Debug.LogError($"Ghost: {gameObject.name} must have a NavMesh Agent!");
        }
        //Initialize states
        FleeState = new GhostState_Flee(this, fleeMaterial);
        RespawnState = new GhostState_Respawn(this, respawnMaterial);
        //Find target reference
        GameObject.FindGameObjectWithTag("Player").TryGetComponent(out Pacman pacman);
        if (pacman != null)
        {
            Target = pacman;
        }
        else
        {
            Debug.LogError($"Ghost: Pacman must be tagged as 'Player'.");
        }
    }

    /// <summary>
    /// Assign delegate/events and set initial state.
    /// </summary>
    protected virtual void Start()
    {
        GameManager.Instance.Event_PickUpPowerPellet += TriggerFleeState;
        GameManager.Instance.Event_EndPowerUp += TriggerDefaultState;
        GameManager.Instance.Event_GameVictory += delegate { SetState(new GhostState_Idle(this)); };
        GameManager.Instance.Delegate_GameOver += delegate { SetState(new GhostState_Idle(this)); };
        SetState(DefaultState);
    }

    /// <summary>
    /// Frame by frame functionality.
    /// </summary>
    protected virtual void Update()
    {
        //Update currently running state
        if (CurrentState != null)
        {
            CurrentState.OnUpdate();
        }
    }

    /// <summary>
    /// Exits current state to enter new state.
    /// </summary>
    /// <param name="state"></param>
    public void SetState(GhostState state)
    {
        if (state != CurrentState)
        {
            //Leave current state
            if (CurrentState != null)
            {
                CurrentState.OnExit();
            }
            //Enter new state
            CurrentState = state;
            CurrentState.OnEnter();
        }
    }

    /// <summary>
    /// Assigned to Power Pellet pickup event
    /// </summary>
    private void TriggerFleeState()
    {
        if (CurrentState.GetType() != typeof(GhostState_Respawn))
        {
            SetState(FleeState);
        }
    }

    /// <summary>
    /// Assigned to Power Pellet end event
    /// </summary>
    protected void TriggerDefaultState()
    {
        if (CurrentState != RespawnState)
        {
            SetState(DefaultState);
        }
    }
}

/// <summary>
/// Interface for creating FSM state classes.
/// </summary>
public interface IGhostState
{
    void OnEnter(); //Called when state is first entered
    void OnUpdate(); //Called every frame state is active
    void OnExit(); //Called when the state is exited
}

/// <summary>
/// Abstract class for defining Ghost states.
/// </summary>
public abstract class GhostState : IGhostState
{
    protected Ghost Instance { get; private set; }

    public GhostState(Ghost instance)
    {
        Instance = instance;
    }

    public virtual void OnEnter()
    {
    }

    public virtual void OnUpdate()
    {
    }

    public virtual void OnExit()
    {
    }

}

public class GhostState_Idle : GhostState
{
    public GhostState_Idle(Ghost instance) : base(instance)
    {
    }

    public override void OnEnter()
    {
        if (Instance.GhostRenderer.material != Instance.DefaultMaterial)
        {
            Instance.GhostRenderer.material = Instance.DefaultMaterial;
        }
        Instance.Agent.isStopped = true;
    }

    public override void OnExit()
    {
        Instance.Agent.isStopped = false;
    }
}

public class GhostState_Chase : GhostState
{
    private Transform target;

    public GhostState_Chase(Ghost instance) : base(instance)
    {
    }

    public override void OnEnter()
    {
        if (Instance.GhostRenderer.material != Instance.DefaultMaterial)
        {
            Instance.GhostRenderer.material = Instance.DefaultMaterial;
        }
        if (Instance.Target != null)
        {
            target = Instance.Target.transform;
        }
        else
        {
            Instance.SetState(new GhostState_Idle(Instance));
        }
    }

    public override void OnUpdate()
    {
        if(Vector3.Distance(Instance.transform.position, target.position) > Instance.Agent.stoppingDistance)
        {
            Instance.Agent.SetDestination(target.position);
        }
        else
        {
            if(Instance.Target.Die() == true)
            {
                Instance.SetState(new GhostState_Idle(Instance));
            }
        }
    }
}

public class GhostState_Flank : GhostState
{
    private Vector3 offset;
    private Transform target;

    public GhostState_Flank(Ghost instance, Vector3 targetOffset) : base(instance)
    {
        offset = targetOffset;
    }

    public override void OnEnter()
    {
        if (Instance.GhostRenderer.material != Instance.DefaultMaterial)
        {
            Instance.GhostRenderer.material = Instance.DefaultMaterial;
        }
        if (Instance.Target != null)
        {
            target = Instance.Target.transform;
        }
        else
        {
            Instance.SetState(new GhostState_Idle(Instance));
        }
    }

    public override void OnUpdate()
    {
        if (Vector3.Distance(Instance.transform.position, target.position) > Instance.Agent.stoppingDistance)
        {
            Instance.Agent.SetDestination(target.position + offset);
            Debug.DrawLine(Instance.transform.position, target.position + offset, Color.magenta);
        }
        else
        {
            if (Instance.Target.Die() == true)
            {
                Instance.SetState(new GhostState_Idle(Instance));
            }
        }
    }
}

public class GhostState_Flee : GhostState
{
    private Material fleeMaterial;
    private Transform target;

    public GhostState_Flee(Ghost instance, Material fleeMat) : base(instance)
    {
        fleeMaterial = fleeMat;
    }

    public override void OnEnter()
    {
        if (fleeMaterial != null)
        {
            Instance.GhostRenderer.material = fleeMaterial;
        }
        else
        {
            Debug.LogError($"Ghost: {Instance.gameObject.name} has no Power Up Material assigned!");
        }

        if (Instance.Target != null)
        {
            target = Instance.Target.transform;
        }
        else
        {
            Instance.SetState(new GhostState_Idle(Instance));
        }
    }

    public override void OnUpdate()
    {
        if(GameManager.Instance.PowerUpTimer == -1)
        {
            if (Vector3.Distance(Instance.transform.position, target.position) < Instance.Agent.stoppingDistance)
            {
                if (Instance.Target.Die() == true)
                {
                    Instance.SetState(new GhostState_Idle(Instance));
                }
            }
        }
        Vector3 dir = (Instance.transform.position - target.position).normalized * (Instance.Agent.stoppingDistance * 2);

        //Ray ray = new Ray(Instance.transform.position, dir);
        //if (Physics.Raycast(ray, out RaycastHit rayHit) == true)
        //{
            //Debug.DrawRay(Instance.transform.position, dir, Color.red);
        //}
        //else
        //{        
        NavMesh.SamplePosition(Instance.transform.position + dir, out NavMeshHit navMeshHit, (Instance.Agent.stoppingDistance * 2), NavMesh.AllAreas);
        Instance.Agent.SetDestination(navMeshHit.position);
        Debug.DrawRay(Instance.transform.position, dir, Color.magenta);
        //}
    }
}

public class GhostState_Respawn : GhostState
{
    private Material respawnMaterial;
    private Vector3 target;

    public GhostState_Respawn(Ghost instance, Material respawnMat) : base(instance)
    {
        respawnMaterial = respawnMat;
    }

    public override void OnEnter()
    {
        if (respawnMaterial != null)
        {
            Instance.GhostRenderer.material = respawnMaterial;
        }
        else
        {
            Debug.LogError($"Ghost: {Instance.gameObject.name} has no Respawn Material assigned!");
        }
        target = GameManager.Instance.GhostSpawnBounds.center;
        Instance.Agent.speed = Instance.Agent.speed * 2;
        Instance.Agent.SetDestination(target);
    }

    public override void OnUpdate()
    {
        if (Vector3.Distance(Instance.transform.position, target) < Instance.Agent.stoppingDistance)
        {
            if (GameManager.Instance.PowerUpTimer != -1)
            {
                Instance.SetState(Instance.FleeState);
            }
            else
            {
                Instance.SetState(Instance.DefaultState);
            }
        }
    }

    public override void OnExit()
    {
        Instance.Agent.speed = Instance.Agent.speed / 2;
    }
}