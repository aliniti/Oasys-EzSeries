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
    using MH = ModuleHelper;

    public class Module : Champion
    {
        private static AIHeroClient MyHero => UnitManager.MyChampion;
        private static Spell Javelin;
        
        protected override void OnLoad()
        {
            MH.Initialize();
            Menu.Initialize(Config);

            Javelin = new Spell(CastSlot.Q, 1500);
            Javelin.SetSkillShot(1500, Prediction.MenuSelected.PredictionType.Line, 0.25f, 1300, 80, true);
        }

        public override async Task OnGameUpdate() { }

        public override async Task OnMainInput()
        {
            CastPrimalSurge(OrbwalkingMode.Combo);
            
            foreach (var t in UnitManager.EnemyChampions.Where(Oasys.SDK.TargetSelector.IsAttackable).OrderBy(x => x.Health))
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
            
            foreach (var t in UnitManager.EnemyChampions.Where(Oasys.SDK.TargetSelector.IsAttackable).OrderBy(x => x.Health))
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
            
            foreach (var t in UnitManager.EnemyJungleMobs.Where(Oasys.SDK.TargetSelector.IsAttackable).OrderBy(x => x.Health))
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
            if (unit == null || MH.IsCatForm()) return;

            if (unit.IsValidTarget() && MH.JavelinIsReady())
            {
                if (unit.Distance(MyHero) <= 1500)
                    Javelin.Cast(unit, Menu.GetHitChance());
            }
        }

        private static void CastBushwhack(AIBaseClient? unit)
        {
            if (unit == null || MH.IsCatForm()) return;

            if (unit.IsValidTarget() && MH.BushwhackIsReady())
            {
                if (unit.Distance(MyHero) <= 900 && !MH.IsHunted(unit))
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
                                    if (!MH.IsCatForm() && MH.PrimalSurgeIsReady())
                                        SpellCastProvider.CastSpell(CastSlot.E, MyHero.Position);
                                if (MH.IsCatForm() && Menu.ChangeForms() && MH.AspectOfCougarReady() && MH.PrimalSurgeIsReady())
                                    SpellCastProvider.CastSpell(CastSlot.R, MyHero.Position);
                                break;
                            case OrbwalkingMode.Combo:
                                if (health <= Menu.AutoHealAllyPct(unit))
                                    if (!MH.IsCatForm() && MH.PrimalSurgeIsReady())
                                        SpellCastProvider.CastSpell(CastSlot.E, MyHero.Position);
                                if (MH.IsCatForm() && Menu.ChangeForms() && MH.AspectOfCougarReady() && MH.PrimalSurgeIsReady())
                                    SpellCastProvider.CastSpell(CastSlot.R, MyHero.Position);
                                break;
                            case OrbwalkingMode.Mixed:
                                if (myMana > 65 && health <= Menu.AutoHealAllyPct(unit))
                                    if (!MH.IsCatForm() && MH.PrimalSurgeIsReady())
                                        SpellCastProvider.CastSpell(CastSlot.E, MyHero.Position);
                                if (MH.IsCatForm() && Menu.ChangeForms() && MH.AspectOfCougarReady() && MH.PrimalSurgeIsReady())
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
            if (unit == null || !MH.IsCatForm()) return;

            if (unit.IsValidTarget() && MH.TakedownIsReady())
            {
                if (unit.Distance(MyHero) <= 300)
                    SpellCastProvider.CastSpell(CastSlot.Q, MyHero.Position);
            }
        }

        private static void CastPounce(AIBaseClient? unit)
        {
            if (unit == null || !unit.IsValidTarget()) return;

            if (MH.IsCatForm() && MH.PounceIsReady())
            {
                if (MH.IsHunted(unit) && unit.Distance(MyHero) <= 750)
                    SpellCastProvider.CastSpell(CastSlot.W, unit.Position);

                if (unit.Distance(MyHero) <= 375)
                    SpellCastProvider.CastSpell(CastSlot.W, unit.Position);
            }
        }

        private static void CastSwipe(AIBaseClient? unit)
        {
            if (unit == null || !unit.IsValidTarget()) return;

            if (MH.IsCatForm() && MH.SwipeIsReady())
            {
                if (unit.Distance(MyHero) <= 300)
                    SpellCastProvider.CastSpell(CastSlot.E, unit.Position);
            }
        }

        private static void SwitchCatToHuman(AIBaseClient? unit, OrbwalkingMode mode)
        {
            if (unit == null || !unit.IsValidTarget() || !Menu.ChangeForms()) return;

            if (MH.IsCatForm() && MH.AspectOfCougarReady())
            {
                if (MH.JavelinIsReady() && (!MH.PounceIsReady(3) || unit.Distance(MyHero) > 375))
                {
                    var pOutput = Javelin.GetPrediction(unit);
                    if (pOutput.HitChance >= Menu.GetHitChance())
                        SpellCastProvider.CastSpell(CastSlot.R, MyHero.Position);
                }

                if (!MH.TakedownIsReady() && !MH.PounceIsReady() && !MH.SwipeIsReady())
                    SpellCastProvider.CastSpell(CastSlot.R, MyHero.Position);
            }
        }

        private static void SwitchHumanToCat(AIBaseClient? unit, OrbwalkingMode mode)
        {
            if (unit == null || !unit.IsValidTarget() || !Menu.ChangeForms()) return;
            if (MH.IsCatForm() || !MH.AspectOfCougarReady()) return;
            
            if (MH.IsHunted(unit) && MH.PounceIsReady() && unit.Distance(MyHero) <= 750)
            {
                if (mode != OrbwalkingMode.LaneClear)
                {
                    if (MH.TakedownIsReady() || MH.SwipeIsReady())
                        SpellCastProvider.CastSpell(CastSlot.R, MyHero.Position);
                }
                else
                {
                    if (Menu.SafeToFastClear() || !Menu.RequireMinAutoAttacks())
                        SpellCastProvider.CastSpell(CastSlot.R, MyHero.Position);

                    if (MH.HasPrimalSurge() && MH.AutoAttackCount(Menu.ClearMinAttacksWithHeal())
                        || MH.AutoAttackCount(Menu.ClearMinAttacks()))
                        SpellCastProvider.CastSpell(CastSlot.R, MyHero.Position);
                }
            }
            
            if (MH.PounceIsReady() && unit.Distance(MyHero) <= 375)
            {
                if (mode != OrbwalkingMode.LaneClear)
                {
                    if (MH.JavelinIsReady())
                    {
                        var pOutput = Javelin.GetPrediction(unit);
                        if (pOutput.CollisionObjects.Count > 0 || pOutput.HitChance < Menu.GetHitChance())
                            SpellCastProvider.CastSpell(CastSlot.R, MyHero.Position);
                    }
                    else
                    {
                        if (MH.TakedownIsReady() || MH.SwipeIsReady())
                            SpellCastProvider.CastSpell(CastSlot.R, MyHero.Position);
                    }
                }
                else
                {
                    if (!MH.JavelinIsReady() && !MH.BushwhackIsReady())
                        SpellCastProvider.CastSpell(CastSlot.R, MyHero.Position);
                }
            }
            
            if (MH.TakedownIsReady() && unit.Distance(MyHero) <= 300)
            {
                if (mode == OrbwalkingMode.LaneClear)
                {
                    if (Menu.SafeToFastClear() || !Menu.RequireMinAutoAttacks())
                        SpellCastProvider.CastSpell(CastSlot.R, MyHero.Position);

                    if (MH.HasPrimalSurge() && MH.AutoAttackCount(Menu.ClearMinAttacksWithHeal())
                        || MH.AutoAttackCount(Menu.ClearMinAttacks()))
                        SpellCastProvider.CastSpell(CastSlot.R, MyHero.Position);
                }
                else
                {
                    if (MH.TakedownIsReady() || MH.SwipeIsReady())
                        SpellCastProvider.CastSpell(CastSlot.R, MyHero.Position);
                }
            }
        }
    }
}