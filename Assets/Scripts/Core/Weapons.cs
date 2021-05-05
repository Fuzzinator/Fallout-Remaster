using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapons : MonoBehaviour
{
    [SerializeField]
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

    public void SwapWeapons()
    {
        _primaryEquipped = !_primaryEquipped;
    }
    
    [System.Serializable]
    public class AmmoContainer : ItemContainer.InventorySlot
    {
        public AmmoContainer(AmmoInfo ammo, int count) : base(ammo, count)
        {
            
        }
    }
}
