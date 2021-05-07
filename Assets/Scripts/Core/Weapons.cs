using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapons : MonoBehaviour
{
    /*[SerializeField]
    private WeaponInfo _primaryWeapon;
    public WeaponInfo PrimaryWeapon => _primaryWeapon;
    [SerializeField]
    private AmmoContainer _primaryAmmo = new AmmoContainer(null, 0);
    public AmmoContainer PrimaryAmmo => _primaryAmmo;
    [SerializeField]
    private WeaponInfo.AttackMode _primaryWeaponMode;
    public WeaponInfo.AttackMode PrimaryWeaponMode => _primaryWeaponMode;

    [SerializeField]
    private WeaponInfo _secondaryWeapon;
    public WeaponInfo SecondaryWeapon => _primaryWeapon;
    [SerializeField]
    private AmmoContainer _secondaryAmmo = new AmmoContainer(null, 0);
    public AmmoContainer SecondaryAmmo => _secondaryAmmo;
    [SerializeField]
    private WeaponInfo.AttackMode _secondaryWeaponMode;
    public WeaponInfo.AttackMode SecondaryWeaponMode => _primaryWeaponMode;

    [SerializeField]
    private bool _primaryEquipped = true;
    public bool PrimaryEqiupped => _primaryEquipped;

    public WeaponInfo ActiveWeapon => _primaryEquipped ? _primaryWeapon : _secondaryWeapon;
    public AmmoContainer ActiveAmmo => _primaryEquipped ? _primaryAmmo : _secondaryAmmo;
    public WeaponInfo.AttackMode ActiveAttackMode => _primaryEquipped ? _primaryWeaponMode : _secondaryWeaponMode;


    public WeaponInfo OtherWeapon => _primaryEquipped ? _secondaryWeapon : _primaryWeapon;
    public AmmoContainer OtherAmmo => _primaryEquipped ? _secondaryAmmo : _primaryAmmo;
    public WeaponInfo.AttackMode OtherAttackMode => _primaryEquipped ? _secondaryWeaponMode : _primaryWeaponMode;

    public int AmmoInClip => ActiveAmmo?.Count ?? 0;
    
    public bool CanUseWeapon(bool activeWeapon = true)
    {
        var weapon = activeWeapon ? ActiveWeapon : OtherWeapon;
        var ammo = activeWeapon ? ActiveAmmo : OtherAmmo;
        return (weapon.UsesAmmo && (ammo?.Count ?? 0) > 0) || !weapon.UsesAmmo;
    }

    public bool TryUseWeapon(int cost)
    {
        if (ActiveWeapon.UsesAmmo)
        {
            if ((ActiveAmmo?.Count ?? 0) >= cost)
            {
                ActiveAmmo?.Subtract(cost);
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return true;
        }
    }
    
    public void SwapWeapons()
    {
        _primaryEquipped = !_primaryEquipped;
    }*/

    /*[System.Serializable]
    public class AmmoContainer : ItemContainer.InventorySlot
    {
        public new AmmoInfo Item => base.Item as AmmoInfo;
        public AmmoContainer(AmmoInfo ammo, int count) : base(ammo, count) { }
    }*/
}