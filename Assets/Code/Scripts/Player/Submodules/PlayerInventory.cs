using FR8Runtime.CodeUtility;
using FR8Runtime.Pickups;
using FR8Runtime.UI;
using UnityEngine.UIElements;

namespace FR8Runtime.Player.Submodules
{
    [System.Serializable]
    public class PlayerInventory
    {
        public int capacity = 10;
        public PickupObject[] items;

        private bool dirty;
        private int index;
        private PlayerAvatar avatar;
        private InventoryCell[] hotbarCells;
        
        public const int HotbarSize = 10;

        public void Init(PlayerAvatar avatar)
        {
            this.avatar = avatar;
            items = new PickupObject[capacity];

            avatar.UpdateEvent += Update;
            avatar.interactionManager.DropEvent += OnDrop;
            avatar.interactionManager.PickupEvent += OnPickup;

            var hotbarParent = FindUtility.Find(avatar.transform, "UI/Hotbar");
            hotbarCells = new InventoryCell[HotbarSize];

            for (var i = 0; i < HotbarSize && i < hotbarParent.childCount; i++)
            {
                hotbarCells[i] = hotbarParent.GetChild(i).GetComponent<InventoryCell>();
            }

            dirty = true;
        }

        public void Update()
        {
            var input = avatar.input.SwitchHotbar;
            if (input != -1)
            {
                ChangeItem(input);
            }
            var move = avatar.input.MoveHotbar;
            if (move != 0)
            {
                ChangeItem(index + move);
            }

            UpdateUI();
        }

        private void ChangeItem(int index)
        {
            StowItem();
            this.index = index;
            GetItem();
            dirty = true;
        }

        private void StowItem()
        {
            var pickup = avatar.interactionManager.HeldObject;
            if (!pickup) return;

            avatar.interactionManager.Drop();
            items[index] = pickup;
            pickup.transform.SetParent(avatar.transform);
            pickup.gameObject.SetActive(false);
        }

        private void GetItem()
        {
            var pickup = items[index];
            if (!pickup) return;

            avatar.interactionManager.Pickup(pickup);
            pickup.transform.SetParent(null);
            pickup.gameObject.SetActive(true);
        }

        private void OnDrop(PickupObject target)
        {
            if (items[index] != target) return;
            items[index] = null;

            dirty = true;
        }

        private void OnPickup(PickupObject target)
        {
            if (items[index]) return;
            items[index] = target;

            dirty = true;
        }

        public void UpdateUI()
        {
            if (!dirty) return;

            for (var i = 0; i < hotbarCells.Length; i++)
            {
                var cell = hotbarCells[i];
                var item = items[i];

                cell.SetSelected(i == index);
                cell.SetItem(item);
            }
        }
    }
}