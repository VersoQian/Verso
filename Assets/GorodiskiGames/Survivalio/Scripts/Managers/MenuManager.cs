using System;
using System.Collections.Generic;
using Game.Cloth;
using Game.Config;
using Game.Domain;
using Game.Equipment;
using Game.Inventory;
using Game.Player;
using Game.Resource;
using Game.Weapon;

namespace Game.Managers
{
    public abstract class BaseManager
    {
        public PlayerController Player;

        public readonly GameModel Model;

        public BaseManager(GameConfig config)
        {
            Model = GameModel.Load(config);
        }

        public InventoryModel CreateInventoryModel(InventoryCategory category, InventoryConfig config, int index = -1,
            int serial = -1, int level = -1, int amount = -1)
        {
            InventoryModel result = null;

            if (category == InventoryCategory.Cloth)
            {
                var clothConfig = config as ClothConfig;
                var clothModel = new ClothModel(clothConfig, serial, level)
                {
                    IsEquipped = Player.Model.EquippedCloth.Contains(serial)
                };
                result = clothModel;
            }
            else if (category == InventoryCategory.Weapon)
            {
                var weaponConfig = config as WeaponConfig;
                var weaponModel = new WeaponModel(weaponConfig, serial, level)
                {
                    IsEquipped = Player.Model.EquippedWeapon == serial
                };
                result = weaponModel;
            }
            else if (category == InventoryCategory.Resource)
            {
                var resourceConfig = config as ResourceConfig;
                result = new ResourceModel(resourceConfig, amount);
            }

            return result;
        }

        public void SaveInventory(List<InventoryModel> inventories)
        {
            foreach (var inventory in inventories)
            {
                var category = inventory.Category;
                if(category == InventoryCategory.Cloth)
                {
                    var clothModel = inventory as ClothModel;
                    var serial = clothModel.Serial;
                    Player.Model.StoredCloth[serial] = clothModel.Index;
                    Player.Model.ClothLevels[serial] = clothModel.Level;
                    Player.Model.Save();
                }
                else if(category == InventoryCategory.Weapon)
                {
                    var weaponModel = inventory as WeaponModel;
                    var serial = weaponModel.Serial;
                    Player.Model.StoredWeapons[serial] = weaponModel.Index;
                    Player.Model.WeaponLevels[serial] = weaponModel.Level;
                    Player.Model.Save();
                }
                else if(category == InventoryCategory.Resource)
                {
                    var resourceModel = inventory as ResourceModel;
                    var type = resourceModel.ResourceType;
                    var amount = resourceModel.Amount;
                    Model.SaveResource(type, amount);
                }
            }
        }
    }

    public sealed class MenuManager : BaseManager, IDisposable
    {
        public event Action<MenuHudType> ON_MENU_BUTTON_CLICK;
        public event Action<EquipmentModel> ON_EQUIP;

        public MenuManager(GameConfig config) : base(config)
        {

        }

        public void Dispose()
        {
            Player.Dispose();
        }

        public void FireMenuButtonClick(MenuHudType hudType)
        {
            ON_MENU_BUTTON_CLICK?.Invoke(hudType);
        }

        public void FireEquip(EquipmentModel model)
        {
            ON_EQUIP?.Invoke(model);
        }
    }
}

