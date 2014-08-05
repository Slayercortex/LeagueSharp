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

namespace UniversalLeveler
{
    internal class UniversalLeveler
    {
        private Menu _menu;

        public UniversalLeveler()
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }

        private void OnGameLoad(EventArgs args)
        {
            try
            {
                _menu = new Menu(Assembly.GetExecutingAssembly().GetName().Name,
                    Assembly.GetExecutingAssembly().GetName().Name, true);
                _menu.AddItem(new MenuItem("Pattern", "Early Level Pattern").SetValue(new StringList(new[]
                {
                    "x 2 3 1",
                    "x 2 1",
                    "x 1 3",
                    "x 1 2"
                })));
                _menu.AddItem(new MenuItem("Q", "Q").SetValue(new Slider(3, 3, 1)));
                _menu.AddItem(new MenuItem("W", "W").SetValue(new Slider(1, 3, 1)));
                _menu.AddItem(new MenuItem("E", "E").SetValue(new Slider(2, 3, 1)));
                _menu.AddToMainMenu();

                Game.PrintChat(
                    string.Format(
                        "<font color='#F7A100'>{0} v{1} loaded.</font>",
                        Assembly.GetExecutingAssembly().GetName().Name,
                        Assembly.GetExecutingAssembly().GetName().Version
                        )
                    );

                CustomEvents.Unit.OnLevelUp += OnLevelUp;
                Console.WriteLine("Level:" + ObjectManager.Player.Level);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private IEnumerable<MenuInfo> GetOrderedList()
        {
            return new List<MenuInfo>
            {
                new MenuInfo
                {
                    Slot = SpellSlot.Q,
                    Value = _menu.Item("Q").GetValue<Slider>().Value
                },
                new MenuInfo
                {
                    Slot = SpellSlot.W,
                    Value = _menu.Item("W").GetValue<Slider>().Value
                },
                new MenuInfo
                {
                    Slot = SpellSlot.E,
                    Value = _menu.Item("E").GetValue<Slider>().Value
                }
            }.OrderBy(x => x.Value).Reverse().ToList();
        }

        private MenuInfo GetMenuInfoByPriority(int priority)
        {
            return new List<MenuInfo>
            {
                new MenuInfo
                {
                    Slot = SpellSlot.Q,
                    Value = _menu.Item("Q").GetValue<Slider>().Value
                },
                new MenuInfo
                {
                    Slot = SpellSlot.W,
                    Value = _menu.Item("W").GetValue<Slider>().Value
                },
                new MenuInfo
                {
                    Slot = SpellSlot.E,
                    Value = _menu.Item("E").GetValue<Slider>().Value
                }
            }.OrderBy(x => x.Value).Reverse().First(s => s.Value == priority);
        }

        private void OnLevelUp(Obj_AI_Base sender, CustomEvents.Unit.OnLevelUpEventArgs args)
        {
            try
            {
                if (!sender.IsValid || !sender.IsMe)
                    return;
                Utility.Map.MapType map = Utility.Map.GetMap();

                if ((map == Utility.Map.MapType.SummonersRift || map == Utility.Map.MapType.TwistedTreeline) &&
                    args.NewLevel == 1)
                    return;

                if ((map == Utility.Map.MapType.CrystalScar || map == Utility.Map.MapType.HowlingAbyss) &&
                    args.NewLevel == 3)
                    return;

                ObjectManager.Player.Spellbook.LevelUpSpell(SpellSlot.R);

                var mf = new MenuInfo();
                switch (args.NewLevel)
                {
                    case 2:
                        switch (_menu.Item("Pattern").GetValue<StringList>().SelectedIndex)
                        {
                            case 0:
                            case 1:
                                mf = GetMenuInfoByPriority(2);
                                break;
                            case 2:
                            case 3:
                                mf = GetMenuInfoByPriority(1);
                                break;
                        }
                        break;
                    case 3:
                        switch (_menu.Item("Pattern").GetValue<StringList>().SelectedIndex)
                        {
                            case 0:
                            case 2:
                                mf = GetMenuInfoByPriority(3);
                                break;
                            case 1:
                                mf = GetMenuInfoByPriority(1);
                                break;
                            case 3:
                                mf = GetMenuInfoByPriority(2);
                                break;
                        }
                        break;
                    case 4:
                        switch (_menu.Item("Pattern").GetValue<StringList>().SelectedIndex)
                        {
                            case 0:
                                mf = GetMenuInfoByPriority(1);
                                break;
                        }
                        break;
                }
                if (mf != null)
                {
                    ObjectManager.Player.Spellbook.LevelUpSpell(mf.Slot);
                }

                foreach (MenuInfo mi in GetOrderedList())
                {
                    ObjectManager.Player.Spellbook.LevelUpSpell(mi.Slot);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}