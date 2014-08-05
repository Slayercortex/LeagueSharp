using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LeagueSharp;
using LeagueSharp.Common;

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

                if (args.NewLevel == 4)
                {
                    ObjectManager.Player.Spellbook.LevelUpSpell(GetOrderedList().Reverse().ToList()[0].Slot);
                }

                foreach (MenuInfo mf in GetOrderedList())
                {
                    Console.WriteLine(mf.Slot.ToString());
                    ObjectManager.Player.Spellbook.LevelUpSpell(mf.Slot);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}