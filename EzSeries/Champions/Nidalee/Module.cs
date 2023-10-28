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
        // ░█▀█░▀█▀░█▀▄░█▀█░█░░░█▀▀░█▀▀
        // ░█░█░░█░░█░█░█▀█░█░░░█▀▀░█▀▀
        // ░▀░▀░▀▀▀░▀▀░░▀░▀░▀▀▀░▀▀▀░▀▀▀

        private static Spell Javelin;
        private static AIHeroClient MyHero => UnitManager.MyChampion;
        
        /// <summary>
        ///     Overrides the base class method to initialize the champion module.
        /// </summary>
        protected override void OnLoad()
        {
            MH.Initialize();
            Menu.Initialize(Config);

            Javelin = new Spell(CastSlot.Q, 1500);
            Javelin.SetSkillShot(1500, Prediction.MenuSelected.PredictionType.Line, 0.25f, 1300, 80, true);
        }

        /// <summary>
        ///     Overrides the base class method to call the champion's OnGameUpdate method.
        /// </summary>
        public override async Task OnGameUpdate() { }

        /// <summary>
        ///     Overrides the base class method to execute Nidalee's combo spells on all attackable enemy champions.
        /// </summary>
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
        
        /// <summary>
        ///     Overrides the base class method to execute Nidalee's harass spells on all attackable enemy champions.
        /// </summary>
        public override async Task OnHarassInput()
        {
            CastPrimalSurge(OrbwalkingMode.Mixed);
            
            foreach (var t in UnitManager.EnemyChampions.Where(Oasys.SDK.TargetSelector.IsAttackable).OrderBy(x => x.Health))
            {
                CastSpear(t);
                SwitchHumanToCat(t, OrbwalkingMode.Mixed);
                SwitchCatToHuman(t, OrbwalkingMode.Mixed);
                CastSwipe(t);
                CastTakedown(t);
            }
        }
        
        /// <summary>
        ///     Overrides the base class method to execute Nidalee's lane clear spells on all attackable enemy minions.
        /// </summary>
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
            
            foreach (var t in UnitManager.EnemyMinions.Where(Oasys.SDK.TargetSelector.IsAttackable).OrderBy(x => x.Health))
            {
                CastSpear(t);
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

            if (unit.IsValidTarget() && MH.JavelinReady())
                if (unit.Distance(MyHero) <= 1500)
                    Javelin.Cast(unit, Menu.GetHitChance());
        }

        private static void CastBushwhack(AIBaseClient? unit)
        {
            if (unit == null || MH.IsCatForm()) return;

            if (unit.IsValidTarget() && MH.BushwhackReady())
                if (unit.Distance(MyHero) <= 900 && !MH.IsHunted(unit))
                    SpellCastProvider.CastSpell(CastSlot.W, unit.Position);
        }

        private static void CastPrimalSurge(OrbwalkingMode mode)
        {
            var myManaPct = MyHero.Mana / MyHero.MaxMana * 100;

            foreach (var hero in ObjectManagerExport.HeroCollection.OrderBy(x => x.Value.Health))
            {
                var unit = hero.Value;
                if (!unit.IsAlly || !unit.IsValidTarget() || !Menu.AutoHealAlly(unit))
                    continue;

                var healthPct = unit.Health / unit.MaxHealth * 100;
                var isCatForm = MH.IsCatForm();
                var canChangeForms = Menu.ChangeForms();
                var isAspectOfCougarReady = MH.AspectOfCougarReady();
                var isHealReady = MH.HealReady();

                switch (mode)
                {
                    case OrbwalkingMode.Combo when healthPct <= Menu.AutoHealAllyPct(unit):
                    case OrbwalkingMode.Mixed when myManaPct > 65 && healthPct <= Menu.AutoHealAllyPct(unit):
                    case OrbwalkingMode.LaneClear when myManaPct > 35 && healthPct <= Menu.AutoHealAllyPct(unit):
                        
                        if (!isCatForm && isHealReady)
                            SpellCastProvider.CastSpell(CastSlot.E, MyHero.Position);
                        
                        if (isCatForm && isHealReady && canChangeForms && isAspectOfCougarReady)
                            SpellCastProvider.CastSpell(CastSlot.R, MyHero.Position);
                        
                        break;
                }
            }
        }

        private static void CastTakedown(AIBaseClient? unit)
        {
            if (MyHero.BuffManager.HasActiveBuff("Takedown")) return;
            if (unit == null || !MH.IsCatForm()) return;

            if (unit.IsValidTarget() && MH.TakedownReady())
                if (unit.Distance(MyHero) <= 300)
                    SpellCastProvider.CastSpell(CastSlot.Q, MyHero.Position);
        }

        private static void CastPounce(AIBaseClient? unit)
        {
            if (unit == null || !unit.IsValidTarget()) return;

            if (MH.IsCatForm() && MH.PounceReady())
                if (unit.Distance(MyHero) <= 375 || MH.IsHunted(unit) && unit.Distance(MyHero) <= 725)
                    SpellCastProvider.CastSpell(CastSlot.W, unit.Position);
        }

        private static void CastSwipe(AIBaseClient? unit)
        {
            if (unit == null || !unit.IsValidTarget()) return;

            if (MH.IsCatForm() && MH.SwipeReady())
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
                if (MH.JavelinReady() && (!MH.PounceReady(3) || unit.Distance(MyHero) > 375))
                {
                    var pOutput = Javelin.GetPrediction(unit);
                    if (pOutput.HitChance >= Menu.GetHitChance())
                        SpellCastProvider.CastSpell(CastSlot.R, MyHero.Position);
                }

                if (!MH.TakedownReady() && !MH.PounceReady() && !MH.SwipeReady())
                    SpellCastProvider.CastSpell(CastSlot.R, MyHero.Position);
            }
        }

        private static void SwitchHumanToCat(AIBaseClient? unit, OrbwalkingMode mode)
        {
            if (unit == null || !unit.IsValidTarget() || !Menu.ChangeForms()) return;
            if (MH.IsCatForm() || !MH.AspectOfCougarReady()) return;
            
            if (MH.IsHunted(unit) && MH.PounceReady() && unit.Distance(MyHero) <= 725)
            {
                if (mode != OrbwalkingMode.LaneClear)
                {
                    if (MH.TakedownReady() || MH.SwipeReady())
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
            
            if (MH.PounceReady() && unit.Distance(MyHero) <= 375)
            {
                if (mode != OrbwalkingMode.LaneClear)
                {
                    if (MH.JavelinReady())
                    {
                        var pOutput = Javelin.GetPrediction(unit);
                        if (pOutput.CollisionObjects.Count > 0 || pOutput.HitChance < Menu.GetHitChance())
                            SpellCastProvider.CastSpell(CastSlot.R, MyHero.Position);
                    }
                    else
                    {
                        if (MH.TakedownReady() || MH.SwipeReady())
                            SpellCastProvider.CastSpell(CastSlot.R, MyHero.Position);
                    }
                }
                else
                {
                    if (!MH.JavelinReady() && !MH.BushwhackReady())
                        SpellCastProvider.CastSpell(CastSlot.R, MyHero.Position);
                }
            }
            
            if (MH.TakedownReady() && unit.Distance(MyHero) <= 300)
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
                    if (MH.TakedownReady() || MH.SwipeReady())
                        SpellCastProvider.CastSpell(CastSlot.R, MyHero.Position);
                }
            }
        }
    }
}