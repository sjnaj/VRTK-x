using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour, ITakeDamage
{
    const string RUN_TRIGGER = "Run";

    const string CROUCH_TRIGGER = "Crouch";

    const string SHOOT_TRIGGER = "Shoot";

    const string RELOAD_TRIGGER = "Reload";

    private float _health;


    [SerializeField]
    private float startingHealth;

    [SerializeField]
    private float minTimeUnderCover;

    [SerializeField]
    private float maxTimeUnderCover;

    [SerializeField]
    private int minBulletsToTake;

    [SerializeField]
    private int maxBulletsToTake;

    [SerializeField]
    private int maxShotsOntTime;

    [SerializeField]
    private float rotateSpeed;

    [SerializeField]
    private float damage;

    [Range(0, 100)]
    [SerializeField]
    private int shootingAccuracy;

    [SerializeField]
    private float reactTime;//开枪反应时间

    [SerializeField]
    private float healthThreshold;//逃跑血量阈值


    private bool isShooting;

    private int currentBulletsTaken;

    private int currentMaxShotsTaken;

    private NavMeshAgent agent;

    private Player player;

    private Transform occupiedCoverSpot;

    private Animator animator;

    public float health
    {
        get
        {
            return health;
        }
        set
        {
            _health = Mathf.Clamp(value, 0, startingHealth);
        }
    }

    // public void TakeDamage(
    //     Weapon weapon,
    //     Projectile projectile,
    //     Vector3 contactPoint
    // )
    // {
    //     throw new System.NotImplementedException();
    // }


    void Awake()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        animator.SetTrigger(RUN_TRIGGER);
        _health = startingHealth;
        isShooting = false;
    }

    public void Init(Player player, Transform coverSpot)
    {
        occupiedCoverSpot = coverSpot;
        this.player = player;
        GetToCover();
    }

    private void GetToCover()
    {
        agent.isStopped = false;
        agent.SetDestination(occupiedCoverSpot.position);
    }
    // Start is called before the first frame update


    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

        if (isShooting == false)
            if (agent.isStopped == true)
            {
                StartCoroutine(InitializeShootingCO());
            }
            else if (agent.isStopped == false && (transform.position - occupiedCoverSpot.position).sqrMagnitude < 0.1f)
            {
                agent.isStopped = true;
                StartCoroutine(InitializeShootingCO());
            }
            else if (agent.isStopped == false && IsPlayerVisible())//行走过程中
            {
                if (health >= healthThreshold && currentBulletsTaken > 0)
                    StartCoroutine(walkShoot());
            }

    }


    private void RotateTowardPlayer()
    {
        Vector3 direction = player.GetHeadPosition() - transform.position;
        direction.y = 0;
        Quaternion rotation = Quaternion.LookRotation(direction);
        rotation = Quaternion.RotateTowards(transform.rotation, rotation, rotateSpeed * Time.deltaTime);
        transform.rotation = rotation;
    }

    private bool IsPlayerVisible()
    {
        RaycastHit hit;
        Vector3 direction = player.GetHeadPosition() - transform.position;
        return Physics.Raycast(transform.position, direction, out hit) && hit.collider.GetComponentInParent<Player>();
    }

    private IEnumerator InitializeShootingCO()
    {
        isShooting = true;
        HideBehindCover();
        yield return new WaitForSeconds(Random.Range(minTimeUnderCover, maxTimeUnderCover));
        Shoot();
    }

    private void HideBehindCover()
    {
        animator.SetTrigger(CROUCH_TRIGGER);
    }

    private void Shoot()
    {
        RotateTowardPlayer();
        int shotsOneTime = Random.Range(1, maxShotsOntTime);
        if (currentBulletsTaken > 0)
        {
            if (shotsOneTime > currentBulletsTaken)
                shotsOneTime = currentBulletsTaken;
            currentBulletsTaken -= shotsOneTime;
            while (shotsOneTime-- > 0)
            {
                animator.SetTrigger(SHOOT_TRIGGER);
                bool hitPlay = Random.Range(0, 100) < shootingAccuracy;
                if (hitPlay)
                {
                    if (IsPlayerVisible())
                    {
                        player.TakeDamage(damage);
                    }
                }
            }


        }
        if (currentBulletsTaken <= 0)
        {
            Reload();
        }
        isShooting = false;

    }

    private void Reload()
    {
        if (currentBulletsTaken <= 0)
        {
            currentBulletsTaken = Random.Range(maxBulletsToTake, maxBulletsToTake);
            animator.SetTrigger(RELOAD_TRIGGER);
        }
    }
    private IEnumerator walkShoot()
    {
        isShooting = true;
        yield return new WaitForSeconds(reactTime);
        RotateTowardPlayer();
        if (currentBulletsTaken > 0)
        {
            currentBulletsTaken--;
            bool hitPlay = Random.Range(0, 100) < shootingAccuracy;
            if (hitPlay)
            {
                player.TakeDamage(damage);
            }
        }
    }

    public void TakeDamage(float damage, Vector3 contactPoint)
    {
        health -= damage;
        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }


}

