using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pacman : MonoBehaviour
{
    //Serialized variables
    [SerializeField] private int lives = 3;
    [SerializeField] private float invincibleTime = 3;
    [SerializeField] private float speed = 3;
    [SerializeField] private Transform pacmanSpawn;
    [SerializeField] private GameObject[] lifeIcons;
    [SerializeField] private AudioClip deathClip;

    //Private variables
    private int currentLives = 0;
    private float respawnTimer = -1;
    private Vector2 input;
    private CharacterController controller;
    private AudioSource aSrc;

    /// <summary>
    /// Creates necessary references.
    /// </summary>
    private void Awake()
    {
        //Find controller reference
        TryGetComponent(out CharacterController charController);
        if (charController != null)
        {
            controller = charController;
        }
        else
        {
            Debug.LogError("Pacman: Character controller required.");
        }
        //Find audio source
        TryGetComponent(out AudioSource audioSource);
        if(audioSource != null)
        {
            aSrc = audioSource;
        }
        else
        {
            Debug.LogError("Pacman: Audio source required.");
        }
        //Check for trigger collider
        bool colliderFound = false;
        foreach (Collider col in GetComponents<Collider>())
        {
            if(col.isTrigger == true)
            {
                colliderFound = true;
                break;
            }
        }
        if(colliderFound == false)
        {
            Debug.LogError("Pacman: Collider with 'isTrigger' set to true is required.");
        }
    }

    /// <summary>
    /// Set initial game state.
    /// </summary>
    private void Start()
    {
        //Set lives
        currentLives = lives;
        for (int i = 0; i < lifeIcons.Length; i++)
        {
            if (lifeIcons[i] != null)
            {
                if (i < currentLives)
                {
                    lifeIcons[i].SetActive(true);
                }
                else
                {
                    lifeIcons[i].SetActive(false);
                }
            }
        }
        //Victory method assigned to event
        GameManager.Instance.Event_GameVictory += delegate { enabled = false; };
    }

    /// <summary>
    /// Frame by frame functionality.
    /// </summary>
    void Update()
    {
        //Respawn timer
        if(respawnTimer > -1)
        {
            respawnTimer += Time.deltaTime; 
            if(respawnTimer > invincibleTime)
            {
                respawnTimer = -1;
            }
        }

        //Read inputs
        input.x = Input.GetAxisRaw("Horizontal");
        input.y = Input.GetAxisRaw("Vertical");
        Vector3 motion = Vector3.zero;
        //Left/right movement
        if (input.x > 0)
        {
            transform.forward = Vector3.right;
            motion = transform.forward.normalized;
        }
        if (input.x < 0)
        {
            transform.forward = -Vector3.right;
            motion = transform.forward.normalized;
        }
        //Forward/backward movement
        if (input.y > 0)
        {
            transform.forward = Vector3.forward;
            motion = transform.forward.normalized;
        }
        if (input.y < 0)
        {
            transform.forward = -Vector3.forward;
            motion = transform.forward.normalized;
        }
        //Apply movement to controller
        controller.Move(motion.normalized * speed * Time.deltaTime);
    }

    /// <summary>
    /// World interactions.
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        //Detect collisions
        switch (other.tag)
        {
            case "Pellet":
                GameManager.Instance.PickUpPellet(1);
                other.gameObject.SetActive(false);
                break;
            case "Power Pellet":
                GameManager.Instance.PickUpPellet(1, 1);
                other.gameObject.SetActive(false);
                break;
            case "Bonus Item":
                GameManager.Instance.PickUpPellet(50, 2);
                other.gameObject.SetActive(false);
                break;
            case "Ghost":
                if (GameManager.Instance.PowerUpTimer > -1)
                {
                    other.TryGetComponent(out Ghost ghost);
                    if (ghost.CurrentState != ghost.RespawnState)
                    {
                        GameManager.Instance.EatGhost(ghost);
                    }
                }
                break;
        }
    }

    /// <summary>
    /// Respawns the player.
    /// </summary>
    private void RespawnPlayer()
    {
        //Move player to spawn
        controller.enabled = false;
        transform.position = pacmanSpawn.position;
        transform.forward = pacmanSpawn.forward;
        controller.enabled = true;
        respawnTimer = 0;
    }

    /// <summary>
    /// Subtracts player lives.
    /// </summary>
    /// <returns></returns>
    public bool Die()
    {
        if (respawnTimer == -1)
        {
            //Subtract life
            aSrc.PlayOneShot(deathClip);
            if (currentLives > 0)
            {
                currentLives--;
                if(currentLives < lifeIcons.Length)
                {
                    lifeIcons[currentLives].SetActive(false);
                }
                else
                {
                    Debug.LogError("There are less life icons then the player's current lives.");
                }
                RespawnPlayer();
                return false;
            }
            else    //Game over
            {
                enabled = false;
                GameManager.Instance.Delegate_GameOver.Invoke();
                return true;
            }
        }
        return false;
    }
}
