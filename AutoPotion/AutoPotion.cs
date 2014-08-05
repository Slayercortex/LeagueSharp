using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LeagueSharp;
using LeagueSharp.Common;

/*
    Copyright (C) 2014 Nikita Bernthaler

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

namespace AutoPotion
{
    internal class AutoPotion
    {
        private Menu _menu;

        private List<Potion> _potions = new List<Potion>
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
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }

        private void OnGameLoad(EventArgs args)
        {
            try
            {
                _potions = _potions.OrderBy(x => x.Priority).ToList();
                _menu = new Menu(Assembly.GetExecutingAssembly().GetName().Name,
                    Assembly.GetExecutingAssembly().GetName().Name, true);
                _menu.AddSubMenu(new Menu("Health", "Health"));
                _menu.AddSubMenu(new Menu("Mana", "Mana"));
                _menu.SubMenu("Health").AddItem(new MenuItem("HealthPotion", "Use Health Potion").SetValue(true));
                _menu.SubMenu("Health")
                    .AddItem(new MenuItem("HealthPercent", "HP Trigger Percent").SetValue(new Slider(60)));
                _menu.SubMenu("Mana").AddItem(new MenuItem("ManaPotion", "Use Mana Potion").SetValue(true));
                _menu.SubMenu("Mana")
                    .AddItem(new MenuItem("ManaPercent", "MP Trigger Percent").SetValue(new Slider(60)));
                _menu.AddToMainMenu();

                Game.PrintChat(
                    string.Format(
                        "<font color='#F7A100'>{0} v{1} loaded.</font>",
                        Assembly.GetExecutingAssembly().GetName().Name,
                        Assembly.GetExecutingAssembly().GetName().Version
                        )
                    );

                Game.OnGameUpdate += OnGameUpdate;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void OnGameUpdate(EventArgs args)
        {
            try
            {
                if (_menu.Item("HealthPotion").GetValue<Boolean>())
                {
                    if (GetPlayerHealthPercentage() <= _menu.Item("HealthPercent").GetValue<Slider>().Value)
                    {
                        InventorySlot healthSlot = GetPotionSlot(PotionType.Health);
                        if (!IsBuffActive(PotionType.Health))
                            healthSlot.UseItem();
                    }
                }

                if (_menu.Item("ManaPotion").GetValue<Boolean>())
                {
                    if (GetPlayerManaPercentage() <= _menu.Item("ManaPercent").GetValue<Slider>().Value)
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