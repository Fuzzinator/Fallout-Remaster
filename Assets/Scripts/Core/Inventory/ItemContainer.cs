using Serializable = System.SerializableAttribute;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ItemContainer : MonoBehaviour
{
    [SerializeField]
    private List<InventorySlot> _items = new List<InventorySlot>();
    private List<InventorySlot> _pooledSlots = new List<InventorySlot>();
    public void AddItem(ItemInfo item, int count)
    {
        while (count > 0)
        {
            var sameItem = _items.Find(i => i.Item == item && i.Count < i.Item.MaxStackSize);
            if (sameItem != null)
            {
                var remainingSpace = sameItem.Item.MaxStackSize - sameItem.Count;
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
                if (count < item.MaxStackSize)
                {
                    count -= item.MaxStackSize;
                    _items.Add(GetNewObject(item, item.MaxStackSize));
                }
                else
                {
                    _items.Add(GetNewObject(item, count));
                    count = 0;
                }
            }
        }
    }

    private InventorySlot GetNewObject(ItemInfo item, int count)
    {
        InventorySlot newSlot = null;
        if (_pooledSlots.Count > 0)
        {
            newSlot = _pooledSlots[0];
            _pooledSlots.Remove(newSlot);
            newSlot.Reset(item, count);
        }
        return newSlot ??= new InventorySlot(item, count);//shorthand for if newSlow == null newSlot == new InventorySlot else return newSlot;
    }

    private void ReturnObjToPool(InventorySlot slot)
    {
        _pooledSlots.Add(slot);
    }

    public void RemoveItem(ItemInfo item, int count = 1)
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

    [Serializable]
    public class InventorySlot
    {
        [SerializeField]
        private ItemInfo _item;
        [SerializeField]
        private int _count;
        public ItemInfo Item => _item;
        public int Count => _count;

        public InventorySlot(ItemInfo item, int count)
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
        public void Reset(ItemInfo item, int count)
        {
            _item = item;
            _count = count;
        }
    }
}