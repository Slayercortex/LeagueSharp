using System;
using System.Linq;
using System.Reflection;
using LeagueSharp;
using SharpDX;
using Color = System.Drawing.Color;

namespace QuickSmite
{
    internal class QuickSmite
    {
        private const string SmiteName = "SummonerSmite";
        private readonly Action _onLoadAction;
        private readonly Action _smiteDataAction;

        private bool _hasSmite;
        private float _smiteRange;
        private SpellSlot _smiteSlot;

        public QuickSmite()
        {
            _onLoadAction = new CallOnce().A(OnLoad);
            _smiteDataAction = new CallOnce().A(LoadSmiteData);
            Game.OnGameUpdate += OnGameUpdate;
            Drawing.OnDraw += OnDraw;
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
                _onLoadAction();
                _smiteDataAction();
                if (!_hasSmite)
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
                if (!_hasSmite)
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