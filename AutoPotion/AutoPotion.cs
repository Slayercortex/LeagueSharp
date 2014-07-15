using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LeagueSharp;

namespace AutoPotion
{
    internal class AutoPotion
    {
        private const bool UseManaPotion = true;
        private const bool UseHealthPotion = true;

        private const int MinManaPercentage = 60;
        private const int MinHealthPercentage = 60;

        private readonly Action _onLoadAction;

        private readonly List<Potion> _potions = new List<Potion>
        {
            new Potion
            {
                Name = "ItemCrystalFlask",
                MinCharges = 1,
                ItemId = (ItemId) 2041,
                Priority = 1,
                TypeList = new List<PotionType> {PotionType.Health, PotionType.Mana}
            },
            new Potion
            {
                Name = "RegenerationPotion",
                MinCharges = 0,
                ItemId = (ItemId) 2003,
                Priority = 2,
                TypeList = new List<PotionType> {PotionType.Health}
            },
            new Potion
            {
                Name = "FlaskOfCrystalWater",
                MinCharges = 0,
                ItemId = (ItemId) 2004,
                Priority = 3,
                TypeList = new List<PotionType> {PotionType.Mana}
            },
            new Potion
            {
                Name = "ItemMiniRegenPotion",
                MinCharges = 0,
                ItemId = (ItemId) 2010,
                Priority = 4,
                TypeList = new List<PotionType> {PotionType.Health, PotionType.Mana}
            }
        };

        public AutoPotion()
        {
            _onLoadAction = new CallOnce().A(OnLoad);
            _potions = _potions.OrderBy(x => x.Priority).ToList();
            Game.OnGameUpdate += OnGameUpdate;
        }

        private void OnLoad()
        {
            Game.PrintChat(
                string.Format(
                    "<font color='#F7A100'>{0} v{1} loaded.</font>",
                    Assembly.GetExecutingAssembly().GetName().Name,
                    Assembly.GetExecutingAssembly().GetName().Version
                    )
                );
        }

        private void OnGameUpdate(EventArgs args)
        {
            try
            {
                _onLoadAction();
                if (UseHealthPotion)
                {
                    if (GetPlayerHealthPercentage() <= MinHealthPercentage)
                    {
                        InventorySlot healthSlot = GetPotionSlot(PotionType.Health);
                        if (!IsBuffActive(PotionType.Health))
                            healthSlot.UseItem();
                    }
                }

                if (UseManaPotion)
                {
                    if (GetPlayerManaPercentage() <= MinManaPercentage)
                    {
                        InventorySlot manaSlot = GetPotionSlot(PotionType.Mana);
                        if (!IsBuffActive(PotionType.Mana))
                            manaSlot.UseItem();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private InventorySlot GetPotionSlot(PotionType type)
        {
            return (from potion in _potions
                where potion.TypeList.Contains(type)
                from item in ObjectManager.Player.InventoryItems
                where item.Id == potion.ItemId && item.Charges >= potion.MinCharges
                select item).FirstOrDefault();
        }

        private bool IsBuffActive(PotionType type)
        {
            return (from potion in _potions
                where potion.TypeList.Contains(type)
                from buff in ObjectManager.Player.Buffs
                where buff.Name == potion.Name && buff.IsActive
                select potion).Any();
        }

        private float GetPlayerHealthPercentage()
        {
            return ObjectManager.Player.Health*100/ObjectManager.Player.MaxHealth;
        }

        private float GetPlayerManaPercentage()
        {
            return ObjectManager.Player.Mana*100/ObjectManager.Player.MaxMana;
        }
    }
}