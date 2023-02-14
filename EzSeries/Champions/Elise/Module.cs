namespace EzSeries.Champions.Elise
{
    using Models;
    using Helpers;
    using Oasys.Common.Extensions;
    using Oasys.Common.GameObject.Clients;
    using Oasys.SDK;
    using Oasys.SDK.SpellCasting;
    using MH = ModuleHelper;

    public class Module : Champion
    {
        private static AIHeroClient MyHero => UnitManager.MyChampion;
        private static Spell Cocoon;

        protected override void OnLoad()
        {
            MH.Initialize();
            Menu.Initialize(Config);

            Cocoon = new Spell(CastSlot.E, 1100);
            Cocoon.SetSkillShot(1100, Prediction.MenuSelected.PredictionType.Line, 0.25f, 1600, 110, true);
        }

        public override async Task OnGameUpdate() { }
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
        
        private static bool BurstCheck(AIBaseClient? unit)
        {
            if (unit == null || !unit.IsValidTarget()) return false;
            
            if (Cocoon.GetPrediction(unit).CollisionObjects.Count > 0) return false;
            if (MH.CocoonTimer() < Cocoon.GetSpellClass().Cooldown - 2) return false;
            
            return MH.CocoonStunned(unit) || true;
        }
        
        private static void NeurotoxinQ(AIBaseClient? unit)
        {
            if (unit == null || !unit.IsValidTarget()) return;
            if (!BurstCheck(unit)) return;

            if (!MH.IsSpiderForm() && MH.NeurotoxinReady())
            {
                if (unit.Distance(MyHero) <= 575)
                {
                    SpellCastProvider.CastSpell(CastSlot.Q, unit.Position);
                }
            }
        }
        
        private static void VolatileW(AIBaseClient? unit)
        {
            if (unit == null || !unit.IsValidTarget()) return;
            if (!BurstCheck(unit)) return;

            if (!MH.IsSpiderForm() && MH.VolatileReady())
            {
                if (unit.Distance(MyHero) <= 950)
                {
                    SpellCastProvider.CastSpell(CastSlot.W, unit.Position);
                }
            }
        }
        
        private static void CocoonE(AIBaseClient? unit)
        {
            if (unit == null || MH.IsSpiderForm()) return;

            if (unit.IsValidTarget() && MH.CocoonReady())
            {
                if (unit.Distance(MyHero) <= 1100)
                    Cocoon.Cast(unit, Menu.GetHitChance());
            }
        }
        
        private static void VenomousBiteQ(AIBaseClient? unit)
        {
            if (unit == null || !unit.IsValidTarget()) return;

            if (MH.IsSpiderForm() && MH.VenomousBiteReady())
            {
                if (unit.Distance(MyHero) <= 475)
                    SpellCastProvider.CastSpell(CastSlot.Q, unit.Position);
            }
        }

        private static void SkitteringFrenzyW(AIBaseClient? unit)
        {
            if (unit == null || !unit.IsValidTarget()) return;

            if (MH.IsSpiderForm() && MH.SkitteringFrenzyReady())
            {
                if (MH.VenomousBiteReady() && unit.Distance(MyHero) <= 575)
                    SpellCastProvider.CastSpell(CastSlot.W, unit.Position);
                
                if (unit.Distance(MyHero) <= 425)
                    SpellCastProvider.CastSpell(CastSlot.W, unit.Position);
            }
        }

        private static void RappelE(AIBaseClient? unit)
        {
            if (unit == null || !unit.IsValidTarget()) return;
            if (!MH.IsSpiderForm() || !MH.RappelReady()) return;

            if (BurstCheck(unit))
            {
                if (unit.Distance(MyHero) <= 825 && MH.VenomousBiteReady())
                {
                    return;
                }
            }

            if (unit.Distance(MyHero) <= 750 && unit is AIHeroClient)
            {
                if (MH.VenomousBiteReady() && unit.Distance(MyHero) > 530)
                    SpellCastProvider.CastSpell(CastSlot.E, unit.Position);

                if (unit.Distance(MyHero) >= 425)
                    SpellCastProvider.CastSpell(CastSlot.E, unit.Position);
            }
        }

        private void SwitchHumanToSpider(AIBaseClient? unit)
        {
            if (unit == null || !unit.IsValidTarget() || !Menu.ChangeForms()) return;
            if (MH.IsSpiderForm() || !MH.TransformReady()) return;

            if (unit is not AIHeroClient)
            {
                if (!MH.NeurotoxinReady() && !MH.VolatileReady() && !MH.CocoonReady())
                    SpellCastProvider.CastSpell(CastSlot.R, MyHero.Position);
            }
            
            if (!MH.NeurotoxinReady())
            {
                if (MH.VenomousBiteReady() && unit.Distance(MyHero) <= 475)
                    SpellCastProvider.CastSpell(CastSlot.R, MyHero.Position);
            }
            
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
            
            if (unit is not AIHeroClient && HandleSpiderlings(unit)) return;
            if (unit.Distance(MyHero) > 425)
                SpellCastProvider.CastSpell(CastSlot.R, MyHero.Position);
            
            if (MH.CocoonReady() && (MH.NeurotoxinReady() || MH.VolatileReady()))
            {
                var pOutput = Cocoon.GetPrediction(unit);
                if (pOutput.HitChance >= Menu.GetHitChance())
                    SpellCastProvider.CastSpell(CastSlot.R, MyHero.Position); ;
            }
        }

        private static bool HandleSpiderlings(AIBaseClient unit)
        {
            return unit.Distance(MyHero) < 425 && (MyHero.BuffManager.HasActiveBuff("elisespiderw") || MH.Spiderlings() > 0);
        }
    }
}