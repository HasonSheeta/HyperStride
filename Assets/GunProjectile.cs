using UnityEngine;
using TMPro;
using Unity.Netcode;

public class GunProjectile : NetworkBehaviour
{
    [SerializeField] public GameObject bullet;
    private GameObject currentBullet;

    public float shootForce, upwardForce;

    //Gun Stats
    public float timeBetweenShooting, spread, reloadTime, timeBetweenShots;
    public int magazineSize, bulletsPerTap;
    public bool allowButtonHold;
    int bulletsLeft, bulletsShot;

    bool shooting, readyToShoot, reloading;

    public Camera fpsCam;
    public Transform attackPoint;

    public GameObject muzzleFlash;
    public TextMeshProUGUI ammunitionDisplay;

    [SerializeField] private AudioClip gunshotSound;
    public AudioSource audioSource;

    public bool allowInvoke = true;
    
    private void Awake() {
        bulletsLeft = magazineSize;
        readyToShoot = true;
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (IsOwner) {
            audioSource = GetComponent<AudioSource>();
        }
        
        //Debug.Log(GetComponent<NetworkObject>().IsSpawned);
    }

    // Update is called once per frame
    void Update()
    {
        // if (!IsOwner) {
        //     return;
        // }
        
        MyInput();

        if (ammunitionDisplay != null) {
            ammunitionDisplay.SetText(bulletsLeft / bulletsPerTap + " / " + magazineSize / bulletsPerTap);
        }
    }

    private void MyInput() {
        if (allowButtonHold) {
            shooting = Input.GetKey(KeyCode.Mouse0);
        }
        else {
            shooting = Input.GetKeyDown(KeyCode.Mouse0);
        }

        //reloading
        if (Input.GetKeyDown(KeyCode.R) && bulletsLeft < magazineSize && !reloading) {
            Reload();
        }
        if (readyToShoot && shooting && !reloading && bulletsLeft <= 0) {
            Reload();
        }

        //shooting
        if (readyToShoot && shooting && !reloading && bulletsLeft > 0) {
            bulletsShot = 0;

            Shoot();
        }
    }

    private void Shoot() {
        readyToShoot = false;

        Ray ray = fpsCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        //check if ray hit
        Vector3 targetPoint;
        if (Physics.Raycast(ray, out hit)) {
            targetPoint = hit.point;
        }
        else {
            targetPoint = ray.GetPoint(75);
        }

        Vector3 directionWithoutSpread = targetPoint - attackPoint.position;

        float x = Random.Range(-spread, spread);
        float y = Random.Range(-spread, spread);

        Vector3 directionWithSpread = directionWithoutSpread + new Vector3(x, y, 0);

        //instantiate projectile
        //GameObject currentBullet = Instantiate(bullet, attackPoint.position, Quaternion.identity);
        FireBulletServerRpc(directionWithSpread.normalized);

        PlayShootSound();

        //rotate projectile to face direction
        //currentBullet.transform.forward = directionWithSpread.normalized;

        //add forces
        //currentBullet.GetComponent<Rigidbody>().AddForce(directionWithSpread.normalized * shootForce, ForceMode.Impulse);
        //currentBullet.GetComponent<Rigidbody>().AddForce(fpsCam.transform.up * upwardForce, ForceMode.Impulse);

        if (muzzleFlash != null) {
            Instantiate(muzzleFlash, attackPoint.position, Quaternion.identity);
        }

        bulletsLeft--;
        bulletsShot++;

        if (allowInvoke) {
            Invoke("ResetShot", timeBetweenShooting);
            allowInvoke = false;
        }

        if (bulletsShot < bulletsPerTap && bulletsLeft > 0) {
            Invoke("Shoot", timeBetweenShots);
        }
    }

    private void ResetShot() {
        readyToShoot = true;
        allowInvoke = true;
    }

    private void Reload() {
        reloading = true;
        Invoke("ReloadFinished", reloadTime);
    }

    private void ReloadFinished() {
        bulletsLeft = magazineSize;
        reloading = false;
    }

    public void PlayShootSound()
    {
        if (IsOwner) // Ensure that only the owner of the object triggers the sound locally
        {
            audioSource.PlayOneShot(gunshotSound);
        }
        else
        {
            PlayShootSoundOnClientRpc(); // If it's not the local player, call ClientRpc to play on all clients
        }
    }

    [ServerRpc]
    public void FireBulletServerRpc(Vector3 dir)
    {
        GameObject b = Instantiate(bullet, attackPoint.position, Quaternion.LookRotation(dir));
        
        var bulletScript = b.GetComponent<Bombs>();
        if (bulletScript != null)
        {
            Collider shooterCol = GetComponentInParent<Collider>();
            bulletScript.Init(dir, shooterCol);
        }
        
        b.GetComponent<NetworkObject>().Spawn(true);
    }

    [ClientRpc]
    public void PlayShootSoundOnClientRpc() {
        if (audioSource != null)
        {
            audioSource.PlayOneShot(gunshotSound);
        }
    }
}
