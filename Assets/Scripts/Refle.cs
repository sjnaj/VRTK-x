using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Refle : Weapon
{
    [SerializeField]
    private float fireRate;


    private WaitForSeconds wait;

    public ParticleSystem muzzleflashParticles;

    public Light muzzleflashLight;



    private bool isShooting = false;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        wait = new WaitForSeconds(1 / fireRate);
    }

    protected override void Shoot()
    {
        //Play muzzleflash particles
        muzzleflashParticles.Emit(1);
        //Play light flash
        StartCoroutine(MuzzleflashLight());
        Projectile projectile =
            Instantiate(bulletPrefab,
            bulletSpawn.position,
            bulletSpawn.rotation);
        projectile.Init(this);
        projectile.Launch();
        base.Shoot();
    }
    IEnumerator MuzzleflashLight()
    {
        muzzleflashLight.enabled = true;
        yield return new WaitForSeconds(0.02f);
        muzzleflashLight.enabled = false;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if (IsUsing() && !isShooting)
        {
            StartCoroutine(ShootingCo());
            isShooting = true;
        }
    }

    protected override void StopShooting()
    {
        base.StopShooting();
        isShooting = false;
        StopAllCoroutines();
    }

    private IEnumerator ShootingCo()
    {
        while (true)
        {
            StartShooting();
            yield return wait;
        }
    }
}
