namespace EzSeries.Champions.Nidalee
{
    using Models;
    using Helpers;
    using Oasys.Common;
    using Oasys.Common.Extensions;
    using Oasys.Common.GameObject.Clients;
    using Oasys.Common.Logic;

    using Oasys.SDK;
    using Oasys.SDK.SpellCasting;
    using TargetSelector = Oasys.SDK.TargetSelector;

    public class Module : Champion
    {
        private static AIHeroClient MyHero => UnitManager.MyChampion;
        private static Spell _javelin;
        
        protected override void OnLoad()
        {
            Base.Initialize();
            Menu.Initialize(Config);

            _javelin = new Spell(CastSlot.Q, 1500);
            _javelin.SetSkillShot(1500, Prediction.MenuSelected.PredictionType.Line, 0.25f, 1300, 80, true);
        }

        public override async Task OnGameUpdate() { }

        public override async Task OnMainInput()
        {
            CastPrimalSurge(OrbwalkingMode.Combo);
            
            foreach (var t in UnitManager.EnemyChampions.Where(TargetSelector.IsAttackable).OrderBy(x => x.Health))
            {
                CastSpear(t);
                CastBushwhack(t);
                SwitchHumanToCat(t, OrbwalkingMode.Combo);
                SwitchCatToHuman(t, OrbwalkingMode.Combo);
                CastSwipe(t);
                CastTakedown(t);
                CastPounce(t);
            }
        }
        
        public override async  Task OnHarassInput()
        {
            CastPrimalSurge(OrbwalkingMode.Mixed);
            
            foreach (var t in UnitManager.EnemyChampions.Where(TargetSelector.IsAttackable).OrderBy(x => x.Health))
            {
                CastSpear(t);
                SwitchHumanToCat(t, OrbwalkingMode.Combo);
                SwitchCatToHuman(t, OrbwalkingMode.Combo);
                CastSwipe(t);
                CastTakedown(t);
            }
        }
        
        public override async  Task OnLaneClearInput()
        {            
            CastPrimalSurge(OrbwalkingMode.LaneClear);
            
            foreach (var t in UnitManager.EnemyMinions.Where(TargetSelector.IsAttackable).OrderBy(x => x.Health))
            {
                CastSpear(t);
                CastBushwhack(t);
                SwitchHumanToCat(t, OrbwalkingMode.LaneClear);
                SwitchCatToHuman(t, OrbwalkingMode.LaneClear);
                CastSwipe(t);
                CastTakedown(t);
                CastPounce(t);
            }
        }

        private static void CastSpear(AIBaseClient? unit)
        {
            if (unit == null || Base.IsCatForm()) return;

            if (unit.IsValidTarget() && Base.JavelinIsReady())
            {
                if (unit.Distance(MyHero) <= 1500)
                    _javelin.Cast(unit, Menu.GetHitChance());
            }
        }

        private static void CastBushwhack(AIBaseClient? unit)
        {
            if (unit == null || Base.IsCatForm()) return;

            if (unit.IsValidTarget() && Base.BushwhackIsReady())
            {
                if (unit.Distance(MyHero) <= 900 && !Base.IsHunted(unit))
                    SpellCastProvider.CastSpell(CastSlot.W, unit.Position);
            }
        }

        private static void CastPrimalSurge(OrbwalkingMode mode)
        {
            foreach (var hero in ObjectManagerExport.HeroCollection.OrderBy(x => x.Value.Health))
            {
                var unit = hero.Value;
                switch (unit.IsAlly)
                {
                    case true when (unit.IsValidTarget() && Menu.AutoHealAlly(unit)):
                    {
                        var health = unit.Health / unit.MaxHealth * 100;
                        var myMana = MyHero.Mana / MyHero.MaxMana * 100;

                        switch (mode)
                        {
                            case OrbwalkingMode.LaneClear:
                                if (myMana > 35 && health <= Menu.AutoHealAllyPct(unit))
                                    if (!Base.IsCatForm() && Base.PrimalSurgeIsReady())
                                        SpellCastProvider.CastSpell(CastSlot.E, MyHero.Position);
                                if (Base.IsCatForm() && Menu.ChangeForms() && Base.AspectOfCougarReady() && Base.PrimalSurgeIsReady())
                                    SpellCastProvider.CastSpell(CastSlot.R, MyHero.Position);
                                break;
                            case OrbwalkingMode.Combo:
                                if (health <= Menu.AutoHealAllyPct(unit))
                                    if (!Base.IsCatForm() && Base.PrimalSurgeIsReady())
                                        SpellCastProvider.CastSpell(CastSlot.E, MyHero.Position);
                                if (Base.IsCatForm() && Menu.ChangeForms() && Base.AspectOfCougarReady() && Base.PrimalSurgeIsReady())
                                    SpellCastProvider.CastSpell(CastSlot.R, MyHero.Position);
                                break;
                            case OrbwalkingMode.Mixed:
                                if (myMana > 65 && health <= Menu.AutoHealAllyPct(unit))
                                    if (!Base.IsCatForm() && Base.PrimalSurgeIsReady())
                                        SpellCastProvider.CastSpell(CastSlot.E, MyHero.Position);
                                if (Base.IsCatForm() && Menu.ChangeForms() && Base.AspectOfCougarReady() && Base.PrimalSurgeIsReady())
                                    SpellCastProvider.CastSpell(CastSlot.R, MyHero.Position);
                                break;
                        }

                        break;
                    }
                }
            }
        }

        private static void CastTakedown(AIBaseClient? unit)
        {
            if (MyHero.BuffManager.HasActiveBuff("Takedown")) return;
            if (unit == null || !Base.IsCatForm()) return;

            if (unit.IsValidTarget() && Base.TakedownIsReady())
            {
                if (unit.Distance(MyHero) <= 300)
                    SpellCastProvider.CastSpell(CastSlot.Q, MyHero.Position);
            }
        }

        private static void CastPounce(AIBaseClient? unit)
        {
            if (unit == null || !unit.IsValidTarget()) return;

            if (Base.IsCatForm() && Base.PounceIsReady())
            {
                if (Base.IsHunted(unit) && unit.Distance(MyHero) <= 750)
                    SpellCastProvider.CastSpell(CastSlot.W, unit.Position);

                if (unit.Distance(MyHero) <= 375)
                    SpellCastProvider.CastSpell(CastSlot.W, unit.Position);
            }
        }

        private static void CastSwipe(AIBaseClient? unit)
        {
            if (unit == null || !unit.IsValidTarget()) return;

            if (Base.IsCatForm() && Base.SwipeIsReady())
            {
                if (unit.Distance(MyHero) <= 300)
                    SpellCastProvider.CastSpell(CastSlot.E, unit.Position);
            }
        }

        private static void SwitchCatToHuman(AIBaseClient? unit, OrbwalkingMode mode)
        {
            if (unit == null || !unit.IsValidTarget() || !Menu.ChangeForms()) return;

            if (Base.IsCatForm() && Base.AspectOfCougarReady())
            {
                if (Base.JavelinIsReady() && (!Base.PounceIsReady(3) || unit.Distance(MyHero) > 375))
                {
                    var pOutput = _javelin.GetPrediction(unit);
                    if (pOutput.HitChance >= Menu.GetHitChance())
                        SpellCastProvider.CastSpell(CastSlot.R, MyHero.Position);
                }

                if (!Base.TakedownIsReady() && !Base.PounceIsReady() && !Base.SwipeIsReady())
                    SpellCastProvider.CastSpell(CastSlot.R, MyHero.Position);
            }
        }

        private static void SwitchHumanToCat(AIBaseClient? unit, OrbwalkingMode mode)
        {
            if (unit == null || !unit.IsValidTarget() || !Menu.ChangeForms()) return;
            
            if (Base.IsCatForm() || !Base.AspectOfCougarReady()) return;
            if (Base.IsHunted(unit))
            {
                if (Base.PounceIsReady() && unit.Distance(MyHero) <= 750)
                {
                    if (mode != OrbwalkingMode.LaneClear)
                    {
                        if (Base.TakedownIsReady()|| Base.SwipeIsReady())
                            SpellCastProvider.CastSpell(CastSlot.R, MyHero.Position);
                    }
                    else
                    {
                        if (Menu.SafeToFastClear() || !Menu.RequireMinAutoAttacks())
                            SpellCastProvider.CastSpell(CastSlot.R, MyHero.Position);
                        
                        if (Base.HasPrimalSurge() && Base.AutoAttackCount(Menu.ClearMinAttacksWithHeal())
                            || Base.AutoAttackCount(Menu.ClearMinAttacks()))
                                SpellCastProvider.CastSpell(CastSlot.R, MyHero.Position);
                    }
                }

                if (Base.TakedownIsReady() && unit.Distance(MyHero) <= 300)
                {
                    if (mode != OrbwalkingMode.LaneClear)
                    {
                        if (Base.TakedownIsReady() || Base.SwipeIsReady())
                            SpellCastProvider.CastSpell(CastSlot.R, MyHero.Position);
                    }
                    else
                    {
                        if (Menu.SafeToFastClear() || !Menu.RequireMinAutoAttacks())
                            SpellCastProvider.CastSpell(CastSlot.R, MyHero.Position);
                        
                        if (Base.HasPrimalSurge() && Base.AutoAttackCount(Menu.ClearMinAttacksWithHeal())
                            || Base.AutoAttackCount(Menu.ClearMinAttacks()))
                                SpellCastProvider.CastSpell(CastSlot.R, MyHero.Position);
                    }
                }
            }
            else if (unit.Distance(MyHero) <= 375)
            {
                if (mode != OrbwalkingMode.LaneClear)
                {
                    if (!Base.JavelinIsReady())
                    {
                        SpellCastProvider.CastSpell(CastSlot.R, MyHero.Position);
                    }
                    else
                    {
                        var pOutput = _javelin.GetPrediction(unit);
                        if (pOutput.HitChance < Menu.GetHitChance())
                            SpellCastProvider.CastSpell(CastSlot.R, MyHero.Position);
                    }
                }
                else
                {
                    if (!Base.JavelinIsReady() && !Base.BushwhackIsReady())
                        SpellCastProvider.CastSpell(CastSlot.R, MyHero.Position);
                }
            }
        }
    }
}