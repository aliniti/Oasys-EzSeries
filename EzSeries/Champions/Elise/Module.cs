#pragma warning disable CS8618
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
        
        private static Spell _cocoon;
        private static AIHeroClient MyHero => UnitManager.MyChampion;

        /// <summary>
        ///     Overrides the base class method to initialize the champion module.
        /// </summary>
        protected override void OnLoad()
        {
            MH.Initialize();
            Menu.Initialize(Config);

            _cocoon = new Spell(CastSlot.E, 1100);
            _cocoon.SetSkillShot(0.25f, 1600, 110, true, Prediction.MenuSelected.PredictionType.Line);
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
            var noCollision = _cocoon.GetPrediction(unit).CollisionObjects.Count < 1;

            return hasCocoon || usedCocoon && noCollision;
        }

        /// <summary> 
        /// <c>BabySpiderCheck</c> evaluates if an AI unit is near enough to a specified hero 
        /// and has active buffs or spiderling count exceeds 0, returning a boolean value 
        /// indicating whether the condition is met. 
        /// </summary> 
        /// <param name="unit"> 
        /// AIBaseClient object being checked for the Baby Spider effect. 
        /// </param> 
        /// <returns> 
        /// a boolean value indicating whether the specified AI unit is eligible for the Baby 
        /// Spider ability. 
        /// </returns> 
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
        
        /// <summary> 
        /// <c>VolatileW</c> checks if a target is valid and within range, then casts the "W" 
        /// spell if conditions are met. 
        /// </summary> 
        /// <param name="unit"> 
        /// 3D position of an object or entity that is being evaluated for Volatile ability activation. 
        /// </param> 
        private static void VolatileW(AIBaseClient? unit)
        {
            if (unit == null || !unit.IsValidTarget()) return;
            if (!BurstCheck(unit) && MH.CocoonReady(3)) return;

            if (!MH.IsSpiderForm() && MH.VolatileReady())
                if (unit.Distance(MyHero) <= 950)
                    SpellCastProvider.CastSpell(CastSlot.W, unit.Position);
        }
        
        /// <summary> 
        /// <c>CocoonE</c> determines if it should cast the cocoon on a target based on distance 
        /// and validity. 
        /// </summary> 
        /// <param name="unit"> 
        /// AIBaseClient object that CocoonE should operate on. 
        /// </param> 
        private static void CocoonE(AIBaseClient? unit)
        {
            if (unit == null || MH.IsSpiderForm()) return;

            if (unit.IsValidTarget() && MH.CocoonReady())
                if (unit.Distance(MyHero) <= 1100)
                    _cocoon.Cast(unit, Menu.GetHitChance());
        }
        
        /// <summary> 
        /// <c>VenomousBiteQ</c> determines if a target is within range to be bitten by the 
        /// hero's venomous bite ability and casts the spell if so. 
        /// </summary> 
        /// <param name="unit"> 
        /// target entity for the Venomous Bite spell cast, and its validity is checked before 
        /// casting the spell. 
        /// </param> 
        private static void VenomousBiteQ(AIBaseClient? unit)
        {
            if (unit == null || !unit.IsValidTarget()) return;

            if (MH.IsSpiderForm() && MH.VenomousBiteReady())
                if (unit.Distance(MyHero) <= 475)
                    SpellCastProvider.CastSpell(CastSlot.Q, unit.Position);
        }

        /// <summary> 
        /// <c>SkitteringFrenzyW</c> detects if a valid target is within range and checks if 
        /// it is a spider form unit. If so, it casts a spell at the unit based on its distance 
        /// to the player's position. 
        /// </summary> 
        /// <param name="unit"> 
        /// AIBaseClient object that SkitteringFrenzyW will operate on if it is not null and 
        /// has been determined to be a valid target. 
        /// </param> 
        private static void SkitteringFrenzyW(AIBaseClient? unit)
        {
            if (unit == null || !unit.IsValidTarget()) return;

            if (MH.IsSpiderForm() && MH.SkitteringFrenzyReady())
                if (unit.Distance(MyHero) <= 425 || MH.VenomousBiteReady() && unit.Distance(MyHero) <= 575)
                    SpellCastProvider.CastSpell(CastSlot.W, unit.Position);
        }

        /// <summary> 
        /// <c>RappelE</c> evaluates whether to cast `Venomous Bite` on an enemy hero within 
        /// a specific distance range based on their position relative to the calling entity 
        /// and the entity's `Rappel` state. 
        /// </summary> 
        /// <param name="unit"> 
        /// 3D position of the Hero being checked for rappelling. 
        /// </param> 
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

                if (MH.CocoonReady() && _cocoon.GetPrediction(unit).CollisionObjects.Count >= 1)
                    SpellCastProvider.CastSpell(CastSlot.R, MyHero.Position);
            }
        }

        /// <summary> 
        /// <c>SwitchSpiderToHuman</c> determines whether to switch from spider form to human 
        /// form based on various conditions such as distance, valid target, and readiness of 
        /// transformation abilities. If these conditions are met, the function will cast a 
        /// spell or trigger cocoon prediction to attack the specified unit. 
        /// </summary> 
        /// <param name="unit"> 
        /// 3D position of the target entity that the spider will transform into when it reaches 
        /// the specified distance from the hero. 
        /// </param> 
        private static void SwitchSpiderToHuman(AIBaseClient? unit)
        {
            if (unit == null || !unit.IsValidTarget() || !Menu.ChangeForms()) return;
            
            if (!MH.IsSpiderForm() || !MH.TransformReady()) return;
            if (!unit.IsObject(ObjectTypeFlag.AIHeroClient) && BabySpiderCheck(unit)) return;
            
            if (unit.Distance(MyHero) > 425)
                SpellCastProvider.CastSpell(CastSlot.R, MyHero.Position);
            
            if (MH.CocoonReady() && (MH.NeurotoxinReady() || MH.VolatileReady()))
            {
                var pOutput = _cocoon.GetPrediction(unit);
                if (pOutput.HitChance >= Menu.GetHitChance())
                    SpellCastProvider.CastSpell(CastSlot.R, MyHero.Position); ;
            }
        }
    }
}