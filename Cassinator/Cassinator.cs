using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Igniter;
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

namespace Cassinator
{
    internal class Cassinator
    {
        private const string ChampionName = "Cassiopeia";

        private readonly Ignite _ignite = new Ignite();

        private readonly HeroSpell _spellE = new HeroSpell
        {
            Slot = SpellSlot.E,
            Range = 700,
            Delay = 0f,
        };

        private readonly HeroSpell _spellQ = new HeroSpell
        {
            Slot = SpellSlot.Q,
            Range = 850,
            Delay = 0.6f,
            Width = 140f,
            Speed = float.MaxValue,
            Duration = 3f
        };

        private readonly HeroSpell _spellR = new HeroSpell
        {
            Slot = SpellSlot.R,
            Delay = 0.6f,
            Width = 80f,
            Speed = float.MaxValue,
            Range = 850
        };

        private readonly HeroSpell _spellW = new HeroSpell
        {
            Slot = SpellSlot.W,
            Delay = 0.6f,
            Width = 200f,
            Speed = 2500f,
            Range = 850
        };

        private Menu _menu;

        private Orbwalking.Orbwalker _orbwalker;

        public Cassinator()
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }

        private void OnGameLoad(EventArgs args)
        {
            try
            {
                if (ObjectManager.Player.BaseSkinName != ChampionName)
                {
                    return;
                }

                _menu = new Menu(Assembly.GetExecutingAssembly().GetName().Name,
                    Assembly.GetExecutingAssembly().GetName().Name, true);

                _menu.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
                _orbwalker = new Orbwalking.Orbwalker(_menu.SubMenu("Orbwalking"));

                var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
                SimpleTs.AddToMenu(targetSelectorMenu);

                var comboMenu = new Menu("Combo", "combo");
                comboMenu.AddItem(new MenuItem("comboUltimate", "Use Ultimate").SetValue(false));
                comboMenu.AddItem(new MenuItem("comboUltimateHealth", "Ultimate below Health").SetValue(new Slider(50)));
                comboMenu.AddItem(new MenuItem("comboIgnite", "Use Ignite").SetValue(true));

                var mixedMenu = new Menu("Mixed", "mixed");
                mixedMenu.AddItem(new MenuItem("mixedQ", "Harass Q").SetValue(true));
                mixedMenu.AddItem(new MenuItem("mixedW", "Harass W").SetValue(false));
                mixedMenu.AddItem(new MenuItem("mixedE", "Harass E").SetValue(true));
                mixedMenu.AddItem(
                    new MenuItem("mixedToggle", "Harass Toggle").SetValue(new KeyBind("Y".ToCharArray()[0],
                        KeyBindType.Toggle)));

                var clearMenu = new Menu("Lane/Jungle Clear", "clear");
                clearMenu.AddItem(new MenuItem("clearQ", "Use Q").SetValue(true));
                clearMenu.AddItem(new MenuItem("clearW", "Use W").SetValue(true));
                clearMenu.AddItem(new MenuItem("clearE", "Use E").SetValue(true));

                var ultimateMenu = new Menu("Ultimate", "ultimate");
                ultimateMenu.AddItem(
                    new MenuItem("ultimateKey", "Hold Key").SetValue(new KeyBind("R".ToCharArray()[0], KeyBindType.Press)));
                ultimateMenu.AddItem(new MenuItem("ultimateMin", "Min. Enemies in Range").SetValue(new Slider(1, 5, 1)));
                ultimateMenu.AddItem(
                    new MenuItem("ultimateRange", "Range").SetValue(new Slider(_spellR.Range - 50, _spellR.Range)));

                var killstealMenu = new Menu("Killsteal", "killsteal");
                killstealMenu.AddItem(new MenuItem("killstealEnabled", "Enabled").SetValue(true));
                killstealMenu.AddItem(new MenuItem("killstealE", "Use E").SetValue(true));
                killstealMenu.AddItem(new MenuItem("killstealIgnite", "Use Ignite").SetValue(true));

                var drawingMenu = new Menu("Drawing", "drawing");
                drawingMenu.AddItem(new MenuItem("drawingQ", "Q Range").SetValue(false));
                drawingMenu.AddItem(new MenuItem("drawingW", "W Range").SetValue(false));
                drawingMenu.AddItem(new MenuItem("drawingE", "E Range").SetValue(false));
                drawingMenu.AddItem(new MenuItem("drawingR", "R Range").SetValue(false));


                var exploitMenu = new Menu("Exploit", "exploit");
                exploitMenu.AddItem(new MenuItem("exploitE", "No Face Exploit E").SetValue(true));

                var miscMenu = new Menu("Misc", "misc");
                miscMenu.AddItem(new MenuItem("CircleLag", "Lag Free Circles").SetValue(true));
                miscMenu.AddItem(new MenuItem("CircleQuality", "Circles Quality").SetValue(new Slider(30, 100, 10)));
                miscMenu.AddItem(new MenuItem("CircleThickness", "Circles Thickness").SetValue(new Slider(2, 10, 1)));

                _menu.AddSubMenu(targetSelectorMenu);
                _menu.AddSubMenu(comboMenu);
                _menu.AddSubMenu(mixedMenu);
                _menu.AddSubMenu(clearMenu);
                _menu.AddSubMenu(ultimateMenu);
                _menu.AddSubMenu(killstealMenu);
                _menu.AddSubMenu(drawingMenu);
                _menu.AddSubMenu(exploitMenu);
                _menu.AddSubMenu(miscMenu);
                _menu.AddToMainMenu();

                Game.PrintChat(
                    string.Format(
                        "<font color='#F7A100'>{0} v{1} loaded.</font>",
                        Assembly.GetExecutingAssembly().GetName().Name,
                        Assembly.GetExecutingAssembly().GetName().Version
                        )
                    );

                Game.OnGameUpdate += OnGameUpdate;
                Drawing.OnDraw += OnDraw;
                _ignite.CanKillstealEnemies += IgniteOnCanKillstealEnemies;
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
                if (!ObjectManager.Player.IsDead)
                {
                    _orbwalker.SetAttacks(true);
                    Killsteal();
                    Ultimate();
                    Mixed();
                    Combo();
                    LaneClear();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void Ultimate()
        {
            Obj_AI_Base target = _orbwalker.GetTarget();
            if (!_menu.Item("ultimateKey").GetValue<KeyBind>().Active || !target.IsValidTarget() ||
                target.GetType() != typeof (Obj_AI_Hero))
            {
                return;
            }
            CastR(target);
        }

        private void Mixed()
        {
            Obj_AI_Base target = _orbwalker.GetTarget();
            if ((_orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Mixed &&
                 _menu.Item("mixedToggle").GetValue<KeyBind>().Active == false) || !target.IsValidTarget() ||
                target.GetType() != typeof (Obj_AI_Hero))
            {
                return;
            }
            _orbwalker.SetAttacks(false);
            if (_menu.Item("mixedQ").GetValue<bool>())
            {
                if (!HasPoisonBuff(target))
                {
                    CastQ(target);
                }
                else
                {
                    BuffInstance buff = GetPoisonBuff(target);
                    if (buff.EndTime - Game.Time <= (_spellQ.Duration/2))
                    {
                        CastQ(target);
                    }
                }
            }
            if (_menu.Item("mixedW").GetValue<bool>())
            {
                CastW(target);
            }
            if (_menu.Item("mixedE").GetValue<bool>())
            {
                if (HasPoisonBuff(target))
                {
                    BuffInstance buff = GetPoisonBuff(target);
                    if (buff.EndTime - Game.Time > (_spellQ.Duration/6))
                    {
                        CastE(target);
                    }
                }
            }
        }

        private void Combo()
        {
            Obj_AI_Base target = _orbwalker.GetTarget();
            if (_orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Combo || !target.IsValidTarget() ||
                target.GetType() != typeof (Obj_AI_Hero))
            {
                return;
            }
            _orbwalker.SetAttacks(false);
            if (_ignite.CanKill(target as Obj_AI_Hero))
            {
                _ignite.Cast(target as Obj_AI_Hero);
            }
            if (_menu.Item("comboUltimate").GetValue<bool>() &&
                (target.Health*100/target.MaxHealth) <= _menu.Item("comboUltimateHealth").GetValue<Slider>().Value)
            {
                CastR(target);
            }
            if (!HasPoisonBuff(target))
            {
                CastQ(target);
            }
            else
            {
                BuffInstance buff = GetPoisonBuff(target);
                if (buff.EndTime - Game.Time <= (_spellQ.Duration/2))
                {
                    CastQ(target);
                }
            }
            CastW(target);
            if (HasPoisonBuff(target))
            {
                BuffInstance buff = GetPoisonBuff(target);
                if (buff.EndTime - Game.Time > (_spellQ.Duration/6))
                {
                    CastE(target);
                }
            }
        }

        private void LaneClear()
        {
            Obj_AI_Base target = _orbwalker.GetTarget();
            if (_orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.LaneClear || !target.IsValidTarget() ||
                target.GetType() != typeof (Obj_AI_Minion))
            {
                return;
            }
            _orbwalker.SetAttacks(
                !(ObjectManager.Player.Spellbook.CanUseSpell(_spellQ.Slot) == SpellState.Ready ||
                  ObjectManager.Player.Spellbook.CanUseSpell(_spellW.Slot) == SpellState.Ready ||
                  (HasPoisonBuff(target) && ObjectManager.Player.Spellbook.CanUseSpell(_spellE.Slot) == SpellState.Ready))
                );
            if (_menu.Item("clearQ").GetValue<bool>())
            {
                CastQ(target, true);
            }
            if (_menu.Item("clearW").GetValue<bool>())
            {
                CastW(target, true);
            }
            if (_menu.Item("clearE").GetValue<bool>())
            {
                if (HasPoisonBuff(target))
                {
                    BuffInstance buff = GetPoisonBuff(target);
                    if (buff.EndTime - Game.Time > (_spellQ.Duration/6))
                    {
                        CastE(target);
                    }
                }
            }
        }

        private bool HasPoisonBuff(Obj_AI_Base target)
        {
            return target.Buffs.Any(buff => buff.Type == BuffType.Poison);
        }

        private BuffInstance GetPoisonBuff(Obj_AI_Base target)
        {
            return target.Buffs.FirstOrDefault(buff => buff.Type == BuffType.Poison);
        }

        private void Killsteal()
        {
            if (!_menu.Item("killstealEnabled").GetValue<bool>())
            {
                return;
            }
            if (_menu.Item("killstealE").GetValue<bool>())
            {
                foreach (
                    Obj_AI_Hero hero in
                        ObjectManager.Get<Obj_AI_Hero>()
                            .Where(
                                hero =>
                                    hero.IsValidTarget() && hero.IsEnemy &&
                                    Vector3.Distance(ObjectManager.Player.Position, hero.ServerPosition) > _spellE.Range)
                            .Where(hero => DamageLib.getDmg(hero, DamageLib.SpellType.E) >= hero.Health))
                {
                    CastE(hero);
                }
            }
        }

        private void CastQ(Obj_AI_Base target, bool aoe = false)
        {
            if (ObjectManager.Player.Spellbook.CanUseSpell(_spellQ.Slot) != SpellState.Ready)
            {
                return;
            }
            if (Vector3.Distance(ObjectManager.Player.Position, target.ServerPosition) > _spellQ.Range)
            {
                return;
            }
            Prediction.PredictionOutput bestPosition = aoe
                ? Prediction.GetBestAOEPosition(target, _spellQ.Delay, _spellQ.Width, _spellQ.Speed,
                    ObjectManager.Player.Position, _spellQ.Range, false, Prediction.SkillshotType.SkillshotCircle)
                : Prediction.GetBestPosition(target, 0.4f, 75f, 99999f, ObjectManager.Player.Position, _spellQ.Range,
                    false, Prediction.SkillshotType.SkillshotCircle);
            if (bestPosition.HitChance == Prediction.HitChance.HighHitchance)
            {
                ObjectManager.Player.Spellbook.CastSpell(_spellQ.Slot, bestPosition.CastPosition);
            }
        }

        private void CastW(Obj_AI_Base target, bool aoe = false)
        {
            if (ObjectManager.Player.Spellbook.CanUseSpell(_spellW.Slot) != SpellState.Ready)
            {
                return;
            }
            if (Vector3.Distance(ObjectManager.Player.Position, target.ServerPosition) > _spellW.Range)
            {
                return;
            }
            Prediction.PredictionOutput bestPosition = aoe
                ? Prediction.GetBestAOEPosition(target, _spellW.Delay, _spellW.Width, _spellW.Speed,
                    ObjectManager.Player.Position, _spellW.Range, false, Prediction.SkillshotType.SkillshotCircle)
                : Prediction.GetBestPosition(target, 0.4f, 106f, 2500f, ObjectManager.Player.Position, _spellW.Range,
                    false, Prediction.SkillshotType.SkillshotCircle);
            if (bestPosition.HitChance == Prediction.HitChance.HighHitchance)
            {
                ObjectManager.Player.Spellbook.CastSpell(_spellW.Slot, bestPosition.CastPosition);
            }
        }

        private void CastE(Obj_AI_Base target)
        {
            if (ObjectManager.Player.Spellbook.CanUseSpell(_spellE.Slot) != SpellState.Ready)
            {
                return;
            }
            if (Vector3.Distance(ObjectManager.Player.Position, target.ServerPosition) > _spellE.Range)
            {
                return;
            }
            if (_menu.Item("exploitE").GetValue<bool>())
            {
                using (var memoryStream = new MemoryStream())
                {
                    using (var binaryWriter = new BinaryWriter(memoryStream))
                    {
                        binaryWriter.Write((byte) 154);
                        binaryWriter.Write(ObjectManager.Player.NetworkId);
                        binaryWriter.Write((byte) ((byte) _spellE.Slot & 63));
                        binaryWriter.Write(Game.CursorPos.X);
                        binaryWriter.Write(Game.CursorPos.Y);
                        binaryWriter.Write(Game.CursorPos.X);
                        binaryWriter.Write(Game.CursorPos.Y);
                        binaryWriter.Write(target.NetworkId);
                    }
                    Game.SendPacket(memoryStream.ToArray(), PacketChannel.C2S, PacketProtocolFlags.Reliable);
                }
            }
            else
            {
                ObjectManager.Player.Spellbook.CastSpell(_spellE.Slot, target);
            }
        }

        private void CastR(Obj_AI_Base target)
        {
            if (ObjectManager.Player.Spellbook.CanUseSpell(_spellR.Slot) != SpellState.Ready)
            {
                return;
            }
            if (Vector3.Distance(ObjectManager.Player.Position, target.ServerPosition) >
                _menu.Item("ultimateRange").GetValue<Slider>().Value)
            {
                return;
            }
            Prediction.PredictionOutput bestPosition = Prediction.GetBestAOEPosition(target, _spellR.Delay,
                _spellR.Width, _spellR.Speed, ObjectManager.Player.Position,
                _menu.Item("ultimateRange").GetValue<Slider>().Value, false, Prediction.SkillshotType.SkillshotCone);
            if (bestPosition.TargetsHit >= _menu.Item("ultimateMin").GetValue<Slider>().Value)
            {
                ObjectManager.Player.Spellbook.CastSpell(_spellR.Slot, bestPosition.CastPosition);
            }
        }

        private void IgniteOnCanKillstealEnemies(object sender, IgniteEventArgs args)
        {
            if (((_orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed ||
                  _menu.Item("mixedToggle").GetValue<KeyBind>().Active)) &&
                _orbwalker.GetTarget() == args.Enemies.FirstOrDefault())
                return;

            if (_orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo &&
                _orbwalker.GetTarget() == args.Enemies.FirstOrDefault())
                return;

            if (!_menu.Item("killstealEnabled").GetValue<bool>())
            {
                return;
            }
            if (_menu.Item("killstealIgnite").GetValue<bool>())
            {
                _ignite.Cast(args.Enemies.FirstOrDefault());
            }
        }

        private void OnDraw(EventArgs args)
        {
            try
            {
                if (_menu == null)
                {
                    return;
                }
                if (_menu.Item("CircleLag").GetValue<bool>())
                {
                    if (_menu.Item("drawingQ").GetValue<bool>())
                    {
                        Utility.DrawCircle(ObjectManager.Player.Position, _spellQ.Range, Color.Gray,
                            _menu.Item("CircleThickness").GetValue<Slider>().Value,
                            _menu.Item("CircleQuality").GetValue<Slider>().Value);
                    }
                    if (_menu.Item("drawingW").GetValue<bool>())
                    {
                        Utility.DrawCircle(ObjectManager.Player.Position, _spellW.Range, Color.Gray,
                            _menu.Item("CircleThickness").GetValue<Slider>().Value,
                            _menu.Item("CircleQuality").GetValue<Slider>().Value);
                    }
                    if (_menu.Item("drawingE").GetValue<bool>())
                    {
                        Utility.DrawCircle(ObjectManager.Player.Position, _spellE.Range, Color.Gray,
                            _menu.Item("CircleThickness").GetValue<Slider>().Value,
                            _menu.Item("CircleQuality").GetValue<Slider>().Value);
                    }
                    if (_menu.Item("drawingR").GetValue<bool>())
                    {
                        Utility.DrawCircle(ObjectManager.Player.Position,
                            _menu.Item("ultimateRange").GetValue<Slider>().Value, Color.Gray,
                            _menu.Item("CircleThickness").GetValue<Slider>().Value,
                            _menu.Item("CircleQuality").GetValue<Slider>().Value);
                    }
                }
                else
                {
                    if (_menu.Item("drawingQ").GetValue<bool>())
                    {
                        Drawing.DrawCircle(ObjectManager.Player.Position, _spellQ.Range, Color.Gray);
                    }
                    if (_menu.Item("drawingW").GetValue<bool>())
                    {
                        Drawing.DrawCircle(ObjectManager.Player.Position, _spellW.Range, Color.Gray);
                    }
                    if (_menu.Item("drawingE").GetValue<bool>())
                    {
                        Drawing.DrawCircle(ObjectManager.Player.Position, _spellE.Range, Color.Gray);
                    }
                    if (_menu.Item("drawingR").GetValue<bool>())
                    {
                        Drawing.DrawCircle(ObjectManager.Player.Position,
                            _menu.Item("ultimateRange").GetValue<Slider>().Value, Color.Gray);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}