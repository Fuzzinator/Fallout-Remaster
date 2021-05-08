using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class Item
{
    [SerializeField]
    private ItemInfo _info;

    [SerializeField]
    private AmmoInfo _ammo;
    
    [SerializeField]
    private int _charges;

    public ItemInfo Info => _info;
    public int Charges => _charges;
    public AmmoInfo Ammo => _ammo;
    
    public bool CanUseItem()
    {
        var canUse = _ammo == null || _charges > 0;
        if (_info is WeaponInfo weapon)
        {
            canUse = (weapon.UsesAmmo && _charges > 0) || !weapon.UsesAmmo;
        }
        return canUse;
    }

    public bool TryUseItem(int cost)
    {
        //shouldDestroy = false;
        if (_info is WeaponInfo weapon)
        {
            if (weapon.UsesAmmo)
            {
                if (_ammo == null)
                {
                    return false;
                }
                if (_charges >= cost)
                {
                    _charges -=  cost;
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
        else
        {
            if (_charges>0)
            {
                return true;
            }

            //shouldDestroy = _info.DestroyWhenEmpty;
            return false;
        }
    }

    public int ReloadCharges(int newCharges)
    {
        var neededCharges = _info.MaxCharges - _charges;
        if (neededCharges < 0)
        {
            Debug.LogWarning("Object has too many charges! I'm fixing it.");
            _charges = _info.MaxCharges;
        }
        else if (newCharges == 0)
        {
            Debug.LogWarning("Trying to reload with 0 charges? Why tho...");
        } 
        else if (neededCharges >= newCharges)
        {
            _charges += newCharges;
            newCharges = 0;
        }
        else
        {
            _charges = _info.MaxCharges;
            newCharges -= neededCharges;
        }
        return newCharges;
    }
    
}
