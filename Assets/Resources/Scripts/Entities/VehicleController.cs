using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class VehicleController : MonoBehaviour, Damageable
{
    public const float ME2KI = 3600f / 1000f, KI2ME = 1000f / 3600f, KI2MI = 0.621371f;

    public VehicleStatsController statsUIPrefab;
    public ParticleSystem smokePrefab, firePrefab;
    public ExplosionController explosionPrefab;

    public GameObject vehicleObject;
    public MapController mapController;
    public CameraController followingCamera;
    public VehicleStatsController popupStatsUI;

    public Driver driver;

    private bool disabled;

    public float totalHealth;
    public float hullStrength;
    public float health;

    public float cappedTopSpeed;
    public float acceleration;
    public float cappedRotSpeed;
    public float rotAcceleration;
    private float defaultLinearDrag, defaultAngularDrag;
    public float rotTimeout;
    private Coroutine rotTimeoutCoroutine;
    //public float revolutionsPerSecond;

    public Transform powerupFirePos;
    private Powerup powerup1, powerup2, powerup3, selectedPowerup;

    public Transform[] thrusters;
    public GameObject[] frontBumpers, sidePanels, rearBumpers;
    public Transform smokePosition, firePosition, explosionPosition;
    private ParticleSystem smoke, fire;
    private ExplosionController explosion;
    private Rigidbody2D _vehicleBody;
    public Rigidbody2D vehicleBody { get { if (!_vehicleBody) PrepareVehicleBody(); return _vehicleBody; } private set { _vehicleBody = value; } }

    public float thrustForce { get { if (vehicleBody) return ((acceleration - Physics.gravity.magnitude) * vehicleBody.mass); else return 0; } }
    public float topSpeed { get { if (vehicleBody) return (((thrustForce / vehicleBody.drag) - Time.fixedDeltaTime * thrustForce) / vehicleBody.mass); else return 0; } }
    public float torqueForce { get { if (vehicleBody) return (rotAcceleration * vehicleBody.mass); else return 0; } }
    public float topRotSpeed { get { if (vehicleBody) return (((torqueForce / vehicleBody.angularDrag) - Time.fixedDeltaTime * torqueForce) / vehicleBody.mass); else return 0; } }

    public Vector2 velocity;

    #region Control Input
    //public bool fullStickControl;
    public float rotationError = 1;

    private Vector2 _inputRightStick;
    public Vector2 inputRightStick { get { return _inputRightStick; } set { if (!disabled) _inputRightStick = value; } }
    private Vector2 _inputLeftStick;
    public Vector2 inputLeftStick { get { return _inputLeftStick; } set { if (!disabled) _inputLeftStick = value; } }
    private float _inputThruster;
    public float inputThruster { get { return _inputThruster; } set { if (!disabled) _inputThruster = value; } }
    private bool _inputFire;
    public bool inputFire { get { return _inputFire; } set { if (!disabled) _inputFire = value; } }
    private bool _selection1;
    public bool selection1 { get { return _selection1; } set { if (!disabled) _selection1 = value; } }
    private bool _selection2;
    public bool selection2 { get { return _selection2; } set { if (!disabled) _selection2 = value; } }
    private bool _selection3;
    public bool selection3 { get { return _selection3; } set { if (!disabled) _selection3 = value; } }
    #endregion

    void Start ()
    {
        PrepareVehicleBody();
        PrepareDamageEffects();
        ModifyHealth(totalHealth);
        SetupThruster();
        SetupPopupStatsUI();
    }
    void Update()
    {
        ApplyInput();
        UpdateCameraInfo();
    }
	void FixedUpdate ()
    {
        ApplyPhysicsInput();
        //DebugPhysics();
        velocity = vehicleBody.velocity;
    }
    void OnCollisionEnter2D(Collision2D col)
    {
        //Debug.Log(col.relativeVelocity + " == " + velocity);
        Vector2 changeInVelocity = velocity - vehicleBody.velocity;

        VehicleController otherShip = col.gameObject.GetComponent<VehicleController>();
        Vector2 otherChangeInVelocity = Vector2.zero;
        float otherMass = 0;
        if (otherShip) { otherChangeInVelocity = otherShip.velocity - otherShip.vehicleBody.velocity; otherMass = otherShip.vehicleBody.mass; }

        float force = changeInVelocity.magnitude * vehicleBody.mass + otherChangeInVelocity.magnitude * otherMass;
        force /= 11;

        float hullDamage = force - (hullStrength * vehicleBody.mass) - vehicleBody.mass;
        if (hullDamage < 0) hullDamage = 0;

        ModifyHealth(-hullDamage);
    }

    private void PrepareVehicleBody()
    {
        if (!_vehicleBody)
        {
            _vehicleBody = GetComponent<Rigidbody2D>();
            defaultLinearDrag = _vehicleBody.drag;
            defaultAngularDrag = _vehicleBody.angularDrag;
        }
    }
    private void PrepareDamageEffects()
    {
        smoke = Instantiate(smokePrefab);
        smoke.transform.position = smokePosition.position;
        smoke.transform.parent = smokePosition;

        fire = Instantiate(firePrefab);
        fire.transform.position = firePosition.position;
        fire.transform.parent = firePosition;

        explosion = Instantiate(explosionPrefab);
        explosion.transform.position = explosionPosition.position;
        explosion.transform.parent = explosionPosition;
    }
    private void SetupThruster()
    {
        if (thrusters != null)
        {
            foreach (Transform thruster in thrusters)
            {
                thruster.gameObject.AddComponent<ThrusterController>().vehicleController = this;
            }
        }
    }
    private void SetupPopupStatsUI()
    {
        popupStatsUI = Instantiate(statsUIPrefab);
        popupStatsUI.transform.SetParent(transform);
        popupStatsUI.target = transform;
    }

    private void DebugPhysics()
    {
        float speed = vehicleBody.velocity.magnitude;
        Debug.Log("Name(" + name + ") Speed(" + speed + "m/s, " + (speed * ME2KI) + "km/h" + ") TopSpeed(" + topSpeed + "m/s, " + (topSpeed * ME2KI) + "km/h" + ") Up(" + transform.up + ")");
    }
    private void UpdateCameraInfo()
    {
        float healthPercent = health / totalHealth;
        int speed = (int)(vehicleBody.velocity.magnitude * ME2KI);

        if (followingCamera)
        {
            if(followingCamera.healthBar)
            {
                followingCamera.healthBar.value = healthPercent;
            }
            if (followingCamera.speedFeed)
            {
                followingCamera.speedFeed.text = speed + " km/h";
            }
        }
        if(popupStatsUI)
        {
            if(popupStatsUI.healthbar)
            {
                popupStatsUI.healthbar.value = healthPercent;
            }
            if(popupStatsUI.speedometer)
            {
                popupStatsUI.speedometer.fillAmount = speed / (cappedTopSpeed + 100);
            }
        }
    }

    private void ApplyInput()
    {
        if (followingCamera)
        {
            followingCamera.inputHorizontal = _inputRightStick.x;
            followingCamera.inputVertical = _inputRightStick.y;
        }

        #region Powerups
        if (_selection1) SelectPowerup(powerup1);
        if (_selection2) SelectPowerup(powerup2);
        if (_selection3) SelectPowerup(powerup3);

        if (_inputFire && selectedPowerup) selectedPowerup.Use();
        #endregion
    }
    private void ApplyPhysicsInput()
    {
        if (_inputThruster > 0) vehicleBody.drag = thrustForce / ((cappedTopSpeed * KI2ME * vehicleBody.mass) + (Time.fixedDeltaTime / thrustForce));
        else vehicleBody.drag = defaultLinearDrag;

        MakeFire();
        vehicleBody.AddForce(transform.forward * thrustForce * _inputThruster);

        #region Move Rotation
        /*float percentRotation = 0;
        if (fullStickControl)
        {
            if (_inputLeftStick != Vector2.zero)
            {
                float desiredAngle = VectorHelpers.AngleSigned(Vector3.up, _inputLeftStick, Vector3.back);
                float currentAngle = VectorHelpers.AngleSigned(Vector3.up, transform.forward, Vector3.back);
                float deltaAngle = desiredAngle - currentAngle;
                if (Mathf.Abs(deltaAngle) > 180) deltaAngle = -deltaAngle % 180;
                float maxRotationAngle = revolutionsPerSecond * 360 * Time.deltaTime;

                if (Mathf.Abs(deltaAngle) > rotationError) percentRotation = Mathf.Clamp(deltaAngle / maxRotationAngle, -1, 1);
            }
        }
        else percentRotation = _inputLeftStick.x;

        if (percentRotation != 0)
        {
            vehicleBody.MoveRotation(vehicleBody.rotation + (-percentRotation * revolutionsPerSecond * 360 * Time.deltaTime));
            vehicleBody.angularVelocity = 0f;
        }*/
        //if (inputRotation != 0) { shipBody.MoveRotation(shipBody.rotation + (-inputRotation * revolutionsPerSecond * 360 * Time.deltaTime)); shipBody.angularVelocity = 0f; }
        #endregion

        #region Torque Rotation
        if (inputLeftStick.x != 0)
        {
            if(rotTimeoutCoroutine != null) { StopCoroutine(rotTimeoutCoroutine); rotTimeoutCoroutine = null; }
            vehicleBody.angularDrag = torqueForce / ((cappedRotSpeed * KI2ME * vehicleBody.mass) + (Time.fixedDeltaTime / torqueForce));
        }
        else if (vehicleBody.angularDrag > defaultAngularDrag && rotTimeoutCoroutine == null)
        {
            rotTimeoutCoroutine = StartCoroutine(ResetRotDrag());
        }
        vehicleBody.AddTorque(-Mathf.Clamp(inputLeftStick.x, -1, 1) * torqueForce);
        #endregion
    }
    private IEnumerator ResetRotDrag()
    {
        yield return new WaitForSeconds(rotTimeout);
        vehicleBody.angularDrag = defaultAngularDrag;
    }
    public void Freeze(bool freeze)
    {
        vehicleBody.isKinematic = freeze;
        disabled = freeze;

        if (freeze)
        {
            _inputThruster = 0;
            _inputLeftStick = Vector2.zero;
            _inputFire = false;
        }
    }
    private void MakeFire()
    {
        if(thrusters != null)
        {
            foreach(Transform thruster in thrusters)
            {
                thruster.gameObject.GetComponent<ThrusterController>().thrusterPercent = _inputThruster;
            }
        }
    }

    public void ModifyHealth(float amount)
    {
        if (health + amount >= 0 && health + amount <= totalHealth) health += amount;
        else if (amount > 0) health = totalHealth;
        else if (amount < 0) health = 0;

        SmokeAndFire();
        CheckDeath();
        //StartCoroutine(CheckDeath());
    }
    private void CheckDeath()
    {
        if (health <= 0)
        {
            DestroyAllPowerups();
            Freeze(true);
            SetSmoke(0);
            SetFire(0);
            if (vehicleObject) vehicleObject.SetActive(false);
            if (explosion)
            {
                explosion.explosionSize = 4;
                explosion.explosionDamage = 50;
                explosion.Explode();
                //yield return new WaitForSeconds(explosion.duration);
            }
            //gameObject.SetActive(false);
            //if (vehicleObject) vehicleObject.SetActive(true);
        }
    }
    private void SmokeAndFire()
    {
        float smokePercent = 0.75f, firePercent = 0.33f;

        float healthPortion = totalHealth * smokePercent;
        #region Smoke
        if (health < healthPortion)
        {
            float percentDead = 1 - (health / healthPortion);
            SetSmoke(percentDead);
        }
        else
        {
            SetSmoke(0);
        }
        #endregion

        healthPortion = totalHealth * firePercent;
        #region Fire
        if (health < healthPortion)
        {
            float percentDead = 1 - (health / healthPortion);
            SetFire(percentDead);
        }
        else
        {
            SetFire(0);
        }
        #endregion
    }
    private void SetSmoke(float percent)
    {
        if (smoke)
        {
            float grayscale = (1 - percent) * 0.4f + 0.6f;
            smoke.startColor = new Color(grayscale, grayscale, grayscale);
            smoke.startSize = percent * 7 + 3;
            ParticleSystem.EmissionModule emission = smoke.emission;
            emission.rate = percent * 50;
        }
    }
    private void SetFire(float percent)
    {
        if (fire)
        {
            fire.startSize = percent * 1 + 1;
            ParticleSystem.EmissionModule emission = fire.emission;
            emission.rate = percent * 100;
        }
    }

    public void GivePowerup(Powerup powPrefab)
    {
        if (!powerup1 || !powerup2 || !powerup3)
        {
            Powerup given = Instantiate(powPrefab);
            given.owner = this;
            Image splitCameraPowerupSlotImage = null, popupPowerupSlotImage = null;
            //Transform powerupParent = transform;

            if (!powerup1)
            {
                powerup1 = given;
                //if (followingCamera) powerupParent = followingCamera.powerupImage1.transform;
                if (followingCamera) splitCameraPowerupSlotImage = followingCamera.powerupImage1;
                if (popupStatsUI) popupPowerupSlotImage = popupStatsUI.powerup1;
            }
            else if (!powerup2)
            {
                powerup2 = given;
                //if (followingCamera) powerupParent = followingCamera.powerupImage2.transform;
                if (followingCamera) splitCameraPowerupSlotImage = followingCamera.powerupImage2;
                if (popupStatsUI) popupPowerupSlotImage = popupStatsUI.powerup2;
            }
            else
            {
                powerup3 = given;
                //if (followingCamera) powerupParent = followingCamera.powerupImage3.transform;
                if (followingCamera) splitCameraPowerupSlotImage = followingCamera.powerupImage3;
                if (popupStatsUI) popupPowerupSlotImage = popupStatsUI.powerup3;
            }

            if (splitCameraPowerupSlotImage && given.slotImage) { splitCameraPowerupSlotImage.overrideSprite = given.slotImage; splitCameraPowerupSlotImage.enabled = true; }
            if (popupPowerupSlotImage && given.slotImage) { popupPowerupSlotImage.overrideSprite = given.slotImage; popupPowerupSlotImage.enabled = true; }
            //given.transform.position = powerupParent.position;
            given.transform.position = transform.position;
            //given.transform.localScale = Vector3.one * 0.1f;
            //given.transform.parent = powerupParent;
            given.transform.parent = transform;
            //if (!followingCamera) given.SetVisibility(false);
        }
    }
    public void SelectPowerup(Powerup powerup)
    {
        Image followBackSlotImage = null;
        Image popupBackSlot = null;
        Image followBackSlot1 = null, followBackSlot2 = null, followBackSlot3 = null;
        Image popupBackSlot1 = null, popupBackSlot2 = null, popupBackSlot3 = null;
        if (followingCamera)
        {
            followBackSlot1 = followingCamera.powerupImage1.transform.parent.GetComponent<Image>();
            popupBackSlot1 = popupStatsUI.powerup1.transform.parent.GetComponent<Image>();
            followBackSlot2 = followingCamera.powerupImage2.transform.parent.GetComponent<Image>();
            popupBackSlot2 = popupStatsUI.powerup2.transform.parent.GetComponent<Image>();
            followBackSlot3 = followingCamera.powerupImage3.transform.parent.GetComponent<Image>();
            popupBackSlot3 = popupStatsUI.powerup3.transform.parent.GetComponent<Image>();

            followBackSlot1.color = Color.white;
            popupBackSlot1.color = Color.white;
            followBackSlot2.color = Color.white;
            popupBackSlot2.color = Color.white;
            followBackSlot3.color = Color.white;
            popupBackSlot3.color = Color.white;
        }

        if (powerup && (powerup == powerup1 || powerup == powerup2 || powerup == powerup3))
        {
            if (powerup == powerup1) { followBackSlotImage = followBackSlot1; popupBackSlot = popupBackSlot1; }
            else if (powerup == powerup2) { followBackSlotImage = followBackSlot2; popupBackSlot = popupBackSlot2; }
            else { followBackSlotImage = followBackSlot3; popupBackSlot = popupBackSlot3; }

            if (selectedPowerup != powerup)
            {
                selectedPowerup = powerup;
                if (followBackSlotImage) followBackSlotImage.color = Color.red;
                if (popupBackSlot) popupBackSlot.color = Color.red;
                return;
            }
        }

        selectedPowerup = null;
    }
    public void DestroyAllPowerups()
    {
        DestroyPowerup(powerup1);
        DestroyPowerup(powerup2);
        DestroyPowerup(powerup3);
        /*if (powerups != null)
        {
            for (int i = 0; i < powerups.Length; i++)
            {
                DestroyPowerup(i);
            }
        }*/
    }
    public void DestroyPowerup(Powerup powerup)
    {
        if (powerup != null)
        {
            if (powerup == selectedPowerup) SelectPowerup(null);
            if (powerup == powerup1 || powerup == powerup2 || powerup == powerup3)
            {
                Image splitPowerupImage = null, popupPowerupImage = null;
                if(powerup == powerup1)
                {
                    if (followingCamera) splitPowerupImage = followingCamera.powerupImage1;
                    if (popupStatsUI) popupPowerupImage = popupStatsUI.powerup1;
                }
                else if (powerup == powerup2)
                {
                    if (followingCamera) splitPowerupImage = followingCamera.powerupImage2;
                    if (popupStatsUI) popupPowerupImage = popupStatsUI.powerup2;
                }
                else if (powerup == powerup3)
                {
                    if (followingCamera) splitPowerupImage = followingCamera.powerupImage3;
                    if (popupStatsUI) popupPowerupImage = popupStatsUI.powerup3;
                }
                if (splitPowerupImage) splitPowerupImage.enabled = false;
                if (popupPowerupImage) popupPowerupImage.enabled = false;
                Destroy(powerup.gameObject);
                powerup = null;
            }
        }
    }
    public void Dispose()
    {
        if (followingCamera) Destroy(followingCamera.gameObject);
        if (popupStatsUI) Destroy(popupStatsUI.gameObject);
        Destroy(gameObject);
    }
}
