using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof (NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    // const string RUN_TRIGGER = "Run";
    // const string CROUCH_TRIGGER = "Crouch";
    // const string SHOOT_TRIGGER = "Shoot";
    // const string RELOAD_TRIGGER = "Reload";
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
    private float reactTime; //开枪反应时间

    [SerializeField]
    private float healthThreshold; //逃跑血量阈值

    private bool isShooting;

    private bool isCover;

    private int currentBulletsTaken;

    private int currentMaxShotsTaken;

    private NavMeshAgent agent;

    private Player player;

    private Transform occupiedCoverSpot;

    private Animator animator;

    private AudioSource mainAudioSource;

    public AudioSource walkAudioSource;

    [System.Serializable]
    public class SoundClips
    {
        public AudioClip walkSound;

        public AudioClip shootSound;

        public AudioClip reloadSoundOutOfAmmo;
    }

    public SoundClips soundClips;

    public Transform bulletPrefab;

    public Transform bulletSpawnpoint;

    public float bulletForce;

    private float health;

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
        mainAudioSource = GetComponent<AudioSource>();
        currentBulletsTaken = Random.Range(minBulletsToTake, maxBulletsToTake);
        walkAudioSource.clip = soundClips.walkSound;
        walkAudioSource.loop = true;

        health = startingHealth;

        isShooting = false;
        isCover = false;
    }

    public void Init(Player player, Transform coverSpot)
    {
        occupiedCoverSpot = coverSpot;
        this.player = player;
        GetToCover();
    }

    public bool getState()
    {
        return agent.isStopped;
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
        // Debug.Log((transform.position - occupiedCoverSpot.position).sqrMagnitude);
        if (isShooting == false)
        {
            if (agent.velocity.magnitude > 1f)
            {
                animator.SetFloat("Vertical", 1.0f, 0, Time.deltaTime);
                animator.SetFloat("Horizontal", 0.0f, 0, Time.deltaTime);
                if (!walkAudioSource.isPlaying)
                {
                    walkAudioSource.Play();
                }
            }
            else
            {
                animator.SetFloat("Vertical", 0.0f, 0, Time.deltaTime);
                animator.SetFloat("Horizontal", 0.0f, 0, Time.deltaTime);
                if (walkAudioSource.isPlaying)
                {
                    walkAudioSource.Pause();
                }
            }
            if (
                agent.isStopped == true && !isCover // 导航到据点且不处于隐藏状态
            )
            {
                StartCoroutine(InitializeShootingCO());
            }
            else if (
                agent.isStopped == false &&
                new Vector2(transform.position.x - occupiedCoverSpot.position.x,
                    transform.position.z - occupiedCoverSpot.position.z)
                    .magnitude <
                1f
            )
            //导航到位(不考虑高度差异)
            {
                agent.isStopped = true;
            }
            else if (
                agent.isStopped == false && IsPlayerVisible() //行走过程中看见玩家
            )
            {
                if (health >= healthThreshold && currentBulletsTaken > 0)
                {
                    StartCoroutine(Shoot());
                    occupiedCoverSpot.position = player.GetHeadPosition();
                    agent.SetDestination(occupiedCoverSpot.position); //追击
                }
                else
                    run(); //
            }
        }
    }

    private void run()
    {
        if ((transform.position - player.GetHeadPosition()).magnitude < 20f)
        {
            occupiedCoverSpot.position =
                (transform.position - player.GetHeadPosition()) * 20;
            agent.SetDestination(occupiedCoverSpot.position);
        }
    }

    private IEnumerator RotateTowardPlayer()
    {
        Vector3 direction = player.GetHeadPosition() - transform.position;
        direction.y = 0;
        Quaternion rotation = Quaternion.LookRotation(direction);
        Quaternion deltaRotation = Quaternion.identity;
        while (transform.rotation != rotation)
        {
            deltaRotation =
                Quaternion
                    .RotateTowards(transform.rotation,
                    rotation,
                    rotateSpeed * Time.deltaTime);
            transform.rotation = deltaRotation;
            yield return 1;
        }
    }

    private bool IsPlayerVisible()
    {
        RaycastHit hit;
        Vector3 direction = player.GetHeadPosition() - transform.position;

        return Physics
            .Raycast(transform.position, direction, out hit, 50, ~(1 << 2)) &&
        hit.collider.transform.parent.tag == "Player";
    }

    private IEnumerator InitializeShootingCO()
    {
        isCover = true;
        HideBehindCover();
        yield return new WaitForSeconds(Random
                    .Range(minTimeUnderCover, maxTimeUnderCover));
        StartCoroutine(Shoot());
        isCover = false;
    }

    private void HideBehindCover()
    {
        // animator.play("cover");
    }

    private IEnumerator Shoot()
    {
        isShooting = true;
        yield return StartCoroutine(RotateTowardPlayer());
        if (IsPlayerVisible())
        {
            if (currentBulletsTaken <= 0)
            {
                yield return StartCoroutine(Reload());
            }
            bool flag = agent.isStopped;
            if (!flag) agent.isStopped = true; //射击时暂停导航
            isShooting = true;
            int shotsOneTime = Random.Range(1, maxShotsOntTime);
            if (currentBulletsTaken > 0)
            {
                if (shotsOneTime > currentBulletsTaken)
                    shotsOneTime = currentBulletsTaken;
                currentBulletsTaken -= shotsOneTime;
                while (shotsOneTime-- > 0)
                {
                    yield return StartCoroutine(ShootOnce());
                }
            }

            if (!flag) agent.isStopped = false;
        }
        isShooting = false;
    }

    private IEnumerator ShootOnce()
    {
        //Play shoot sound
        mainAudioSource.clip = soundClips.shootSound;
        mainAudioSource.Play();

        animator.Play("Fire", 1, 0.0f);

        //Spawn bullet from bullet spawnpoint
        var bullet =
            (Transform)
            Instantiate(bulletPrefab,
            bulletSpawnpoint.transform.position,
            bulletSpawnpoint.transform.rotation);

        //Add velocity to the bullet
        bullet.GetComponent<Rigidbody>().velocity =
            bullet.transform.forward * bulletForce;
        bool hitPlay = Random.Range(0, 100) < shootingAccuracy;

        if (hitPlay && IsPlayerVisible())
        {
            player.TakeDamage (damage);
        }
        yield return new WaitForSeconds(0.15f);
    }

    private IEnumerator Reload()
    {
        currentBulletsTaken = Random.Range(minBulletsToTake, maxBulletsToTake);
        animator.Play("Reload", 1, 0.0f);
        mainAudioSource.clip = soundClips.reloadSoundOutOfAmmo;
        yield return new WaitForSeconds(mainAudioSource.clip.length);
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Destroy (gameObject);
        }
    }
}
