using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ItemContainer : MonoBehaviour
{
    [SerializeField]
    private List<InventorySlot> _items = new List<InventorySlot>();

    private List<InventorySlot> _pooledSlots = new List<InventorySlot>();

    #region Editor Stuff

    private void OnValidate()
    {
        foreach (var item in _items)
        {
            item.SetName();
        }
    }

    #endregion

    public void AddItem(Item item, int count)
    {
        while (count > 0)
        {
            var sameItem = _items.Find(i => i.Item == item && i.Count < i.Item.Info.MaxStackSize);
            if (sameItem != null)
            {
                var remainingSpace = sameItem.Item.Info.MaxStackSize - sameItem.Count;
                if (remainingSpace < count)
                {
                    sameItem.Add(remainingSpace);
                    count -= remainingSpace;
                }
                else
                {
                    sameItem.Add(count);
                    count = 0;
                }
            }
            else
            {
                if (count < item.Info.MaxStackSize)
                {
                    count -= item.Info.MaxStackSize;
                    _items.Add(GetNewObject(item, item.Info.MaxStackSize));
                }
                else
                {
                    _items.Add(GetNewObject(item, count));
                    count = 0;
                }
            }
        }
    }

    private InventorySlot GetNewObject(Item item, int count)
    {
        InventorySlot newSlot = null;
        if (_pooledSlots.Count > 0)
        {
            newSlot = _pooledSlots[0];
            _pooledSlots.Remove(newSlot);
            newSlot.Reset(item, count);
        }

        return
            newSlot ??= new InventorySlot(item,
                count); //shorthand for if newSlow == null newSlot == new InventorySlot else return newSlot;
    }

    private void ReturnObjToPool(InventorySlot slot)
    {
        _pooledSlots.Add(slot);
    }

    public void RemoveItem(Item item, int count = 1)
    {
        while (count > 0)
        {
            var sameItem = _items.Find(i => i.Item == item);
            if (sameItem == null)
            {
                Debug.LogWarning("Trying to remove object from inventory that doesnt exist. This shouldnt happen");
                break;
            }

            if (sameItem.Count > count)
            {
                sameItem.Subtract(count);
                count = 0;
            }
            else
            {
                count -= sameItem.Count;
                var remains = sameItem.Subtract(sameItem.Count);
                if (!remains)
                {
                    ReturnObjToPool(sameItem);
                }
            }
        }
    }

    public void RemoveSlot(InventorySlot slot)
    {
        _items.Remove(slot);
        _pooledSlots.Add(slot);
    }

    public bool TryGetAmmo(ItemInfo info, out InventorySlot slot)
    {
        slot = null;
        if (info != null && _items.Exists(i => i.Item.Info == info))
        {
            slot = _items.Find(i => i.Item.Info == info);
            return true;
        }

        return false;
    }

    public bool TryGetConsumable(ConsumableInfo.Type type, out InventorySlot slot)
    {
        slot = _items.Find(i => i.Item.Info is ConsumableInfo consumable && consumable.ConsumableType == type);
        return slot != null;
    }

    [System.Serializable]
    public class InventorySlot
    {
        [SerializeField]
        private string _name;

        [SerializeField]
        private Item _item;

        [SerializeField]
        private int _count;

        public Item Item => _item;
        public int Count => _count;

        public InventorySlot(Item item, int count)
        {
            _item = item;
            _count = count;
        }

        public void Add(int count)
        {
            _count += count;
        }

        public bool Subtract(int count)
        {
            if (_count <= count)
            {
                return false;
            }

            _count -= count;
            return true;
        }

        public void Reset(Item item, int count)
        {
            _item = item;
            _count = count;
        }

        public void SetCount(int newCount)
        {
            _count = newCount;
        }

        public void SetName()
        {
            _name = $"({_count})-{_item?.Info?.Name}";
        }
    }
}