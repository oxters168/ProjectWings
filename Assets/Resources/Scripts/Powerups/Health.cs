public class Health : Powerup
{
    public float percent;

	public override void Use()
    {
        owner.ModifyHealth(percent * owner.totalHealth);
        owner.DestroyPowerup(this);
    }
}
