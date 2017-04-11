using UnityEngine;

public class FireArm : Powerup
{
    public ProjectileController bulletType;
    public int magazineSize;
    public float bulletEscapeForce;
    public float waitTime;
    private float lastShotTime;
    private int bulletsShot;

    public override void Use()
    {
        if (Time.time - lastShotTime >= waitTime)
        {
            Rigidbody2D bullet = Instantiate(bulletType).projectileBody;
            if (bullet)
            {
                bullet.transform.position = owner.powerupFirePos.position;
                bullet.transform.rotation = owner.powerupFirePos.rotation;
                bullet.velocity = owner.velocity;
                bullet.AddForce(bullet.transform.forward * bulletEscapeForce);
            }
            else Debug.Log("Warning: Bullet is Missing a Rigidbody2D Component");
            bulletsShot++;
            if (bulletsShot >= magazineSize) owner.DestroyPowerup(this);
            lastShotTime = Time.time;
        }
    }
}
