﻿// Author: Harry Donovan
// Based off of from https://github.com/HDonovan96/Glass-Nomad.

using UnityEngine;

using Photon.Pun;

using UnityEngine.UI;

public class PlayerAttack : MonoBehaviourPunCallbacks
{
    [SerializeField]
    public WeaponObject equipedWeapon;
    
    private Camera charCamera;
    private float timeSinceLastShot;
    private float currentBulletsInMag;
    private float totalAmmo;
    
    // Start is called before the first frame update
    public void Start()
    {
        charCamera = this.GetComponentInChildren<Camera>();

        // Variable initialisation.
        timeSinceLastShot = equipedWeapon.TimeBetweenShots;
        currentBulletsInMag = equipedWeapon.magazineSize;

        totalAmmo = 2 * equipedWeapon.magazineSize;
    }

    // Update is called once per frame
    public void Update()
    {
        // Aborts the script if the GameObject doesn't belong to the client.
        if (!photonView.IsMine)
        {
            return;
        }

        timeSinceLastShot += Time.deltaTime;

        if (Input.GetButton("Fire1"))
        {
            FireWeapon();
        }

        if (Input.GetButton("Reload"))
        {
            ReloadWeapon();
        }

        Text[] allUI = this.gameObject.GetComponentsInChildren<Text>();
        foreach (Text element in allUI)
        {
            if (element.gameObject.name == "TXT_Ammo")
            {
                element.text = "Ammo: " + currentBulletsInMag + " / " + totalAmmo;
            }
        }
    }

    private void ReloadWeapon()
    {
        if (CanReload())
        {
            float bulletsUsed = equipedWeapon.magazineSize - currentBulletsInMag;

            if (totalAmmo > bulletsUsed)
            {
                totalAmmo -= bulletsUsed;
                currentBulletsInMag = equipedWeapon.magazineSize;
            }
            else
            {
                currentBulletsInMag += bulletsUsed;
                totalAmmo = 0;
            }
        }
    }

    private bool CanReload()
    {
        if (currentBulletsInMag != equipedWeapon.magazineSize && totalAmmo != 0)
        {
            return true;
        }
        
        return false;
    }

    private void FireWeapon()
    {
        if (CanFire())
        {
            RaycastHit hit;
            if (Physics.Raycast(charCamera.transform.position, charCamera.transform.forward, out hit, equipedWeapon.range))
            {
                photonView.RPC("Shoot", RpcTarget.All, charCamera.transform.position, charCamera.transform.forward, equipedWeapon.range, equipedWeapon.damagePerShot);
            }
        }
    }

    // Checks if the weapon is ready to fire and reduces ammo.
    // Also resets the time since last shot.
    private bool CanFire()
    {
        if (timeSinceLastShot > equipedWeapon.TimeBetweenShots)
        {
            if (currentBulletsInMag > 0)
            {
                timeSinceLastShot = 0.0f;
                currentBulletsInMag -= 1;
                return true;
            }
        }

        return false;
    }

    [PunRPC]
    public void Shoot(Vector3 cameraPos, Vector3 cameraForward, float weaponRange, int weaponDamage)
    {
        RaycastHit hit;
        if (Physics.Raycast(cameraPos, cameraForward, out hit, weaponRange))
        {
            Debug.Log(hit.transform.gameObject.name + " has been hit");
            if (hit.transform.gameObject.tag == "Player")
            {
                hit.transform.GetComponent<PlayerController>().playerResource.ChangePlayerResource(PlayerResource.Resource.Health, weaponDamage);
            }
            
        }
    }
}
