using UnityEngine;

public abstract class Powerup : MonoBehaviour
{
    public VehicleController owner;
    public int index;
    public Sprite slotImage;

    public abstract void Use();
    /*public void SetVisibility(bool visible)
    {
        Renderer drawn = GetComponentInChildren<Renderer>();
        if (drawn)
        {
            drawn.enabled = visible;
        }
    }*/
}
