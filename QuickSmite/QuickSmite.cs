using System;
using System.Linq;
using System.Reflection;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

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

namespace QuickSmite
{
    internal class QuickSmite
    {
        private const string SmiteName = "SummonerSmite";

        private bool _hasSmite;
        private Menu _menu;
        private float _smiteRange;
        private SpellSlot _smiteSlot;

        public QuickSmite()
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }

        private void OnGameLoad(EventArgs args)
        {
            try
            {
                _menu = new Menu(Assembly.GetExecutingAssembly().GetName().Name,
                    Assembly.GetExecutingAssembly().GetName().Name, true);
                _menu.AddItem(new MenuItem("Enable", "Enable").SetValue(new KeyBind("N".ToCharArray()[0], KeyBindType.Toggle)));
                _menu.AddToMainMenu();

                Game.PrintChat(
                    string.Format(
                        "<font color='#F7A100'>{0} v{1} loaded.</font>",
                        Assembly.GetExecutingAssembly().GetName().Name,
                        Assembly.GetExecutingAssembly().GetName().Version
                        )
                    );

                LoadSmiteData();

                Game.OnGameUpdate += OnGameUpdate;
                Drawing.OnDraw += OnDraw;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void LoadSmiteData()
        {
            SpellDataInst[] spells = ObjectManager.Player.SummonerSpellbook.Spells;
            foreach (SpellDataInst spell in spells.Where(spell => spell.Name == SmiteName))
            {
                _hasSmite = true;
                _smiteSlot = spell.Slot;
                _smiteRange = spell.SData.CastRange[0];
            }
        }

        private void OnGameUpdate(EventArgs args)
        {
            try
            {
                if (!_hasSmite || !_menu.Item("Enable").GetValue<KeyBind>().Active)
                    return;
                if (ObjectManager.Player.IsDead || !ObjectManager.Player.IsMe)
                    return;
                Obj_AI_Minion minion = BigMinions.GetNearest(ObjectManager.Player.Position);
                if (minion != null)
                {
                    if (IsMinionSmiteable(minion))
                    {
                        if (minion.Health <= GetSmiteDamage())
                        {
                            SpellState smiteState = ObjectManager.Player.SummonerSpellbook.CanUseSpell(_smiteSlot);
                            if (smiteState == SpellState.Ready)
                            {
                                ObjectManager.Player.SummonerSpellbook.CastSpell(_smiteSlot, minion);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void OnDraw(EventArgs args)
        {
            try
            {
                if (!_hasSmite || !_menu.Item("Enable").GetValue<KeyBind>().Active)
                    return;
                SpellState smiteState = ObjectManager.Player.SummonerSpellbook.CanUseSpell(_smiteSlot);
                Drawing.DrawCircle(ObjectManager.Player.Position, _smiteRange,
                    smiteState == SpellState.Ready ? Color.Blue : Color.Gray);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private int GetSmiteDamage()
        {
            int level = ObjectManager.Player.Level;
            int[] stages =
            {
                20*level + 370,
                30*level + 330,
                40*level + 240,
                50*level + 100
            };
            return stages.Max();
        }

        private bool IsMinionSmiteable(Obj_AI_Minion minion)
        {
            if (minion.IsDead || minion.IsInvulnerable || minion.IsAlly)
                return false;
            if (Vector3.Distance(ObjectManager.Player.Position, minion.Position) <= _smiteRange)
            {
                return true;
            }
            return false;
        }
    }
}