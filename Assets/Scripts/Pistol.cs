using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRTK;

public class Pistol : Weapon
{
    

    // protected override void StartShooting()
    // {
    //     base.StartShooting();
    // }
    protected override void Shoot()
    {
        Projectile projectile =
            Instantiate(bulletPrefab,
            bulletSpawn.position,
            bulletSpawn.rotation);
        projectile.Init(this);
        projectile.Launch();
        base.Shoot();
    }

    public override void StartUsing(VRTK_InteractUse currentUsingObject)
    {
        base.StartUsing(currentUsingObject);

        StartShooting();
    }

    
  
}
