namespace EzSeries.Champions.Elise
{
    using Models;
    using Helpers;
    using Oasys.Common.Enums.GameEnums;
    using Oasys.Common.Extensions;
    using Oasys.Common.GameObject.Clients;
    using Oasys.SDK;
    using Oasys.SDK.SpellCasting;
    using MH = ModuleHelper;

    public class Module : Champion
    {
        
        // ░█▀▀░█░░░▀█▀░█▀▀░█▀▀
        // ░█▀▀░█░░░░█░░▀▀█░█▀▀
        // ░▀▀▀░▀▀▀░▀▀▀░▀▀▀░▀▀▀
        
        private static Spell Cocoon;
        private static AIHeroClient MyHero => UnitManager.MyChampion;

        /// <summary>
        ///     Overrides the base class method to initialize the champion module.
        /// </summary>
        protected override void OnLoad()
        {
            MH.Initialize();
            Menu.Initialize(Config);

            Cocoon = new Spell(CastSlot.E, 1100);
            Cocoon.SetSkillShot(1100, Prediction.MenuSelected.PredictionType.Line, 0.25f, 1600, 110, true);
        }

        /// <summary>
        ///     Overrides the base class method to call the champion's OnGameUpdate method.
        /// </summary>
        public override async Task OnGameUpdate() { }

        
        /// <summary>
        ///     Overrides the base class method to execute Elise's combo spells on all attack-able enemy champions.
        /// </summary>
        public override async Task OnMainInput()
        {
            foreach (var t in UnitManager.EnemyChampions.Where(TargetSelector.IsAttackable).OrderBy(x => x.Health))
            {
                CocoonE(t);
                RappelE(t);
                NeurotoxinQ(t);
                VolatileW(t);
                SwitchHumanToSpider(t);
                SwitchSpiderToHuman(t);
                SkitteringFrenzyW(t);
                VenomousBiteQ(t);
            }
        }

        /// <summary>
        ///     Overrides the base class method to execute Elise's harass spells on all attack-able enemy champions.
        /// </summary>
        public override async Task OnHarassInput()
        {
            foreach (var t in UnitManager.EnemyChampions.Where(TargetSelector.IsAttackable).OrderBy(x => x.Health))
            {
                CocoonE(t);
                RappelE(t);
                NeurotoxinQ(t);
                VolatileW(t);
                SwitchHumanToSpider(t);
                SwitchSpiderToHuman(t);
                SkitteringFrenzyW(t);
                VenomousBiteQ(t);
            }
        }

        /// <summary>
        ///    Overrides the base class method to execute Elise's lane clear spells on all attack-able enemy jungle mobs.
        /// </summary>
        public override async Task OnLaneClearInput()
        {
            foreach (var t in UnitManager.EnemyJungleMobs.Where(TargetSelector.IsAttackable).OrderBy(x => x.Health))
            {
                CocoonE(t);
                RappelE(t);
                NeurotoxinQ(t);
                VolatileW(t);
                SwitchHumanToSpider(t);
                SwitchSpiderToHuman(t);
                SkitteringFrenzyW(t);
                VenomousBiteQ(t);
            }
        }

        // ░█▀▀░█▀█░█▀█░█▀▄░▀█▀░▀█▀░▀█▀░█▀█░█▀█░█▀▀
        // ░█░░░█░█░█░█░█░█░░█░░░█░░░█░░█░█░█░█░▀▀█
        // ░▀▀▀░▀▀▀░▀░▀░▀▀░░▀▀▀░░▀░░▀▀▀░▀▀▀░▀░▀░▀▀▀
        
        private static bool BurstCheck(AIBaseClient? unit)
        {
            if (unit == null || !unit.IsValidTarget() ) return false;
            
            var hasCocoon = MH.CocoonStunned(unit);
            var usedCocoon = MH.CocoonTimer() > MH.CocoonTotal() - 2;
            var noCollision = Cocoon.GetPrediction(unit).CollisionObjects.Count < 1;

            return hasCocoon || usedCocoon && noCollision;
        }

        private static bool BabySpiderCheck(AIBaseClient unit)
        {
            return unit.Distance(MyHero) < 425 && (MyHero.BuffManager.HasActiveBuff("elisespiderw") || MH.Spiderlings() > 0);
        }
        
        // ░█▀▀░█▀█░█▄█░█▀▄░█▀█░▀█▀
        // ░█░░░█░█░█░█░█▀▄░█▀█░░█░
        // ░▀▀▀░▀▀▀░▀░▀░▀▀░░▀░▀░░▀░

        private static void NeurotoxinQ(AIBaseClient? unit)
        {
            if (unit == null || !unit.IsValidTarget()) return;
            if (!BurstCheck(unit) && MH.CocoonReady(3)) return;
            
            if (!MH.IsSpiderForm() && MH.NeurotoxinReady())
                if (unit.Distance(MyHero) <= 575)
                    SpellCastProvider.CastSpell(CastSlot.Q, unit.Position);
        }
        
        private static void VolatileW(AIBaseClient? unit)
        {
            if (unit == null || !unit.IsValidTarget()) return;
            if (!BurstCheck(unit) && MH.CocoonReady(3)) return;

            if (!MH.IsSpiderForm() && MH.VolatileReady())
                if (unit.Distance(MyHero) <= 950)
                    SpellCastProvider.CastSpell(CastSlot.W, unit.Position);
        }
        
        private static void CocoonE(AIBaseClient? unit)
        {
            if (unit == null || MH.IsSpiderForm()) return;

            if (unit.IsValidTarget() && MH.CocoonReady())
                if (unit.Distance(MyHero) <= 1100)
                    Cocoon.Cast(unit, Menu.GetHitChance());
        }
        
        private static void VenomousBiteQ(AIBaseClient? unit)
        {
            if (unit == null || !unit.IsValidTarget()) return;

            if (MH.IsSpiderForm() && MH.VenomousBiteReady())
                if (unit.Distance(MyHero) <= 475)
                    SpellCastProvider.CastSpell(CastSlot.Q, unit.Position);
        }

        private static void SkitteringFrenzyW(AIBaseClient? unit)
        {
            if (unit == null || !unit.IsValidTarget()) return;

            if (MH.IsSpiderForm() && MH.SkitteringFrenzyReady())
                if (unit.Distance(MyHero) <= 425 || MH.VenomousBiteReady() && unit.Distance(MyHero) <= 575)
                    SpellCastProvider.CastSpell(CastSlot.W, unit.Position);
        }

        private static void RappelE(AIBaseClient? unit)
        {
            if (unit == null || !unit.IsValidTarget()) return;
            if (!MH.IsSpiderForm() || !MH.RappelReady()) return;

            if (BurstCheck(unit))
                if (unit.Distance(MyHero) <= 825 && MH.VenomousBiteReady())
                    return;

            if (unit.Distance(MyHero) <= 750 && unit is AIHeroClient)
                if (unit.Distance(MyHero) >= 425 || MH.VenomousBiteReady() && unit.Distance(MyHero) > 530)
                    SpellCastProvider.CastSpell(CastSlot.E, unit.Position);
        }
        
        // ░▀█▀░█▀▄░█▀█░█▀█░█▀▀░█▀▀░█▀█░█▀▄░█▄█
        // ░░█░░█▀▄░█▀█░█░█░▀▀█░█▀▀░█░█░█▀▄░█░█
        // ░░▀░░▀░▀░▀░▀░▀░▀░▀▀▀░▀░░░▀▀▀░▀░▀░▀░▀
        
        private void SwitchHumanToSpider(AIBaseClient? unit)
        {
            if (unit == null || !unit.IsValidTarget() || !Menu.ChangeForms()) return;
            if (MH.IsSpiderForm() || !MH.TransformReady()) return;

            if (!unit.IsObject(ObjectTypeFlag.AIHeroClient))
                if (!MH.NeurotoxinReady() && !MH.VolatileReady() && !MH.CocoonReady())
                    SpellCastProvider.CastSpell(CastSlot.R, MyHero.Position);
            
            if (!MH.NeurotoxinReady())
                if (MH.VenomousBiteReady() && unit.Distance(MyHero) <= 475)
                    SpellCastProvider.CastSpell(CastSlot.R, MyHero.Position);
            
            if (MH.RappelReady() && unit.Distance(MyHero) <= 750)
            {
                if (!MH.VenomousBiteReady())
                    SpellCastProvider.CastSpell(CastSlot.R, MyHero.Position);

                if (unit.Distance(MyHero) > 475 + MyHero.AttackRange + 35)
                    SpellCastProvider.CastSpell(CastSlot.R, MyHero.Position);

                if (MH.CocoonReady() && Cocoon.GetPrediction(unit).CollisionObjects.Count >= 1)
                    SpellCastProvider.CastSpell(CastSlot.R, MyHero.Position);
            }
        }

        private static void SwitchSpiderToHuman(AIBaseClient? unit)
        {
            if (unit == null || !unit.IsValidTarget() || !Menu.ChangeForms()) return;
            
            if (!MH.IsSpiderForm() || !MH.TransformReady()) return;
            if (!unit.IsObject(ObjectTypeFlag.AIHeroClient) && BabySpiderCheck(unit)) return;
            
            if (unit.Distance(MyHero) > 425)
                SpellCastProvider.CastSpell(CastSlot.R, MyHero.Position);
            
            if (MH.CocoonReady() && (MH.NeurotoxinReady() || MH.VolatileReady()))
            {
                var pOutput = Cocoon.GetPrediction(unit);
                if (pOutput.HitChance >= Menu.GetHitChance())
                    SpellCastProvider.CastSpell(CastSlot.R, MyHero.Position); ;
            }
        }
    }
}