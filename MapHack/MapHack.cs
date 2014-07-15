using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using LeagueSharp;

namespace MapHack
{
    internal class MapHack
    {
        private readonly List<Hero> _heroes = new List<Hero>();

        private readonly Action _onLoadAction;

        public MapHack()
        {
            _onLoadAction = new CallOnce().A(OnLoad);
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

        private void OnGameUpdate(EventArgs args)
        {
            try
            {
                _onLoadAction();
                foreach (
                    Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValid && hero.IsEnemy))
                {
                    if (_heroes.All(t => t.Name != hero.BaseSkinName))
                    {
                        _heroes.Add(new Hero
                        {
                            Name = hero.BaseSkinName,
                            Visible = true,
                            Dead = hero.IsDead,
                            LastPosition = hero.Position
                        });
                    }
                    Hero h = _heroes.FirstOrDefault(heroes => heroes.Name == hero.BaseSkinName);
                    if (h != null)
                    {
                        h.Visible = hero.IsVisible;
                        h.Dead = hero.IsDead;
                        h.LastPosition = hero.IsVisible ? hero.Position : h.LastPosition;
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
                foreach (Hero hero in _heroes)
                {
                    if (!hero.Dead && !hero.Visible)
                    {
                        float[] pos = Drawing.WorldToMinimap(hero.LastPosition);
                        Drawing.DrawText(pos[0] - Convert.ToInt32(hero.Name.Substring(0, 3).Length*5), pos[1] - 5,
                            Color.Red,
                            hero.Name.Substring(0, 3));
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