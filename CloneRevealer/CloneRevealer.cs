using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using LeagueSharp;

namespace CloneRevealer
{
    internal class CloneRevealer
    {
        private readonly string[] _champions =
        {
            "Shaco", "LeBlanc", "MonkeyKing", "Yorick"
        };

        private readonly Action _onLoadAction;

        public CloneRevealer()
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
                foreach (
                    Obj_AI_Hero hero in
                        ObjectManager.Get<Obj_AI_Hero>()
                            .Where(hero => hero.IsValid && hero.IsEnemy && !hero.IsDead)
                            .Where(hero => _champions.Contains(hero.Name)))
                {
                    Drawing.DrawCircle(hero.Position, hero.BoundingRadius + 25, Color.Yellow);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}