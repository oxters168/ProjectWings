using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PowerupBox : MonoBehaviour
{
    public Powerup[] powerups;
    public int scrollIndex = -1;
    private float shuffleSpeed;
    public GameObject glassBox;
    public Image powerupSlotImage;
    public float respawnTime = 7;
    private bool isHidden;

    private Transform powerupHolder;
    private Powerup[] copies;

    void Start()
    {
        SetupPowerups();
        StartCoroutine(Shuffle());
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isHidden)
        {
            VehicleController pickerUpper = other.GetComponentInParent<VehicleController>();
            if (pickerUpper)
            {
                if (powerups != null && powerups.Length > 0) pickerUpper.GivePowerup(powerups[scrollIndex]);
                StartCoroutine(Respawn());
            }
        }
    }

    private IEnumerator Respawn()
    {
        powerupSlotImage.gameObject.SetActive(false);
        glassBox.SetActive(false);
        isHidden = true;

        float deathTime = Time.time;
        while(Time.time - deathTime < respawnTime) yield return null;

        glassBox.SetActive(true);
        powerupSlotImage.gameObject.SetActive(true);
        isHidden = false;
    }
    private void SetupPowerups()
    {
        powerupHolder = new GameObject("Powerups").transform;
        powerupHolder.parent = transform;

        if(powerups != null && powerups.Length > 0)
        {
            scrollIndex = Random.Range(0, powerups.Length);
            copies = new Powerup[powerups.Length];
            for (int i = 0; i < powerups.Length; i++)
            {
                copies[i] = Instantiate(powerups[i]);
                copies[i].transform.position = transform.position;
                copies[i].transform.parent = powerupHolder;
                //if (i != scrollIndex) copies[i].gameObject.SetActive(false);
            }
        }
    }
    private IEnumerator Shuffle()
    {
        shuffleSpeed = Random.Range(0.05f, 0.2f);
        while(true)
        {
            if (powerups != null && powerups.Length > 0)
            {
                //copies[scrollIndex].gameObject.SetActive(false);
                scrollIndex++;
                if (scrollIndex >= powerups.Length) scrollIndex = 0;
                if (copies[scrollIndex].slotImage) { powerupSlotImage.overrideSprite = copies[scrollIndex].slotImage; powerupSlotImage.enabled = true; }
                else powerupSlotImage.enabled = false;
                //copies[scrollIndex].gameObject.SetActive(true);
            }
            yield return new WaitForSeconds(shuffleSpeed);
        }
    }
}
