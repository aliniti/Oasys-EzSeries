namespace EzSeries.Champions
{
    using Base;
    using Helpers;
    using Oasys.Common;
    using Oasys.Common.Enums.GameEnums;
    using Oasys.Common.Extensions;
    using Oasys.Common.GameObject.Clients;
    using Oasys.Common.GameObject.Clients.ExtendedInstances.Spells;
    using Oasys.Common.Logic;
    using Oasys.Common.Menu.ItemComponents;
    using Oasys.SDK;
    using Oasys.SDK.Events;
    using Oasys.SDK.Rendering;
    using Oasys.SDK.SpellCasting;
    using Oasys.SDK.Tools;
    using SharpDX;

    public class Elise : Plugin
    {

        public float Sq, Sw, Se;
        public float Hq, Hw, He;

        public Switch CheckE;
        public Switch Debug;

        public Dictionary<string, float> Stamps = new();

        public bool IsSpiderForm => Me.AttackRange < 200;
        public bool CocoonStunned(AIBaseClient unit) => unit.BuffManager.HasActiveBuff("elisehumane");

        public int SpiderlingCount()
        {
            var b  = Me.BuffManager.GetActiveBuff("elisespiderlingsready");
            if (b != null && b.IsActive)
            {
                return (int) b.Stacks;
            }

            return 0;
        }

        protected override string PluginName { get; set; } = "Elise";

        public override void OnLoadPlugin()
        {
            CoreEvents.OnCoreRender += OnCoreRender;
            CoreEvents.OnCoreMainTick += OnCoreMainTick;
            GameEvents.OnCreateObject += OnCreateObject;
            GameEvents.OnProcessSpell += OnProcessSpell;
            CoreEvents.OnCoreMainInputAsync += CoreEventsOnOnCoreMainInputAsync;
            CoreEvents.OnCoreLaneclearInputAsync += CoreEventsOnOnCoreLaneclearInputAsync;

            Stamps["EliseHumanQ"] = 0;
            Stamps["EliseHumanW"] = 0;
            Stamps["EliseHumanE"] = 0;
            Stamps["EliseSpiderQ"] = 0;
            Stamps["EliseSpiderW"] = 0;
            Stamps["EliseSpiderE"] = 0;
            Stamps["EliseR"] = 0;

            PluginTab.AddItem(CheckE = new Switch { IsOn = false, Title = "Use VeryHigh HitChance" });
            PluginTab.AddItem(Debug = new Switch { Title = "Debug Timers", IsOn = false });
        }

        private async Task CoreEventsOnOnCoreLaneclearInputAsync()
        {
            foreach (var u in ObjectManagerExport.JungleObjectCollection)
            {
                var minion = u.Value;
                if (minion.Name.Contains("Plant")) continue;

                if (!minion.Name.Contains("Mini"))
                {
                    CocoonE(minion);
                    RappelE(minion);
                }

                NeurotoxinQ(minion);
                VolatileW(minion);
                SwitchHumanToSpider(minion);
                SwitchSpiderToHuman(minion);
                SkitteringFrenzyW(minion);
                VenomousBiteQ(minion);
            }
        }

        private async Task CoreEventsOnOnCoreMainInputAsync()
        {
            var t = UnitManager.EnemyChampions.MinBy(Oasys.SDK.TargetSelector.AttacksLeftToKill);
            if (t != null)
            {
                CocoonE(t);
                RappelE(t);

                if (BurstCheck(t))
                {
                    NeurotoxinQ(t);
                    VolatileW(t);
                }

                SwitchHumanToSpider(t);
                SwitchSpiderToHuman(t);
                SkitteringFrenzyW(t);
                VenomousBiteQ(t);
            }
        }

        private void OnCoreRender()
        {
            if (!Debug.IsOn) return;

            // debug draw timers
            var wts = LeagueNativeRendererManager.WorldToScreen(Me.Position);

            // draw timers for opposite form stance
            var spiderText = "Q: " + Sq + " W: " + Sw + " E: " + Se;
            var humanText = "Q: " + Hq + " W: " + Hw + " E: " + He;
            var text = IsSpiderForm ? humanText : spiderText;
            // var font = new Font(RenderFactory.RenderDevice, new FontDescription { FaceName = "Courier New" });

            RenderFactory.DrawText(text, 4, new Vector2(wts[0], wts[1] + 35), SharpDX.Color.LimeGreen, false);
        }

        private async Task OnProcessSpell(AIBaseClient unit, SpellActiveEntry spell)
        {
            if (!unit.IsMe) return;

            if (spell.SpellData.SpellName == "EliseHumanQ")
            {
                var cd = 6 * (100 / (100 + Me.UnitStats.AbilityHaste));
                Stamps["EliseHumanQ"] = GameEngine.GameTime + cd;
            }

            if (spell.SpellData.SpellName == "EliseHumanW")
            {
                var cd = 12 * (100 / (100 + Me.UnitStats.AbilityHaste));
                Stamps["EliseHumanW"] = GameEngine.GameTime + cd;
            }

            if (spell.SpellData.SpellName == "EliseHumanE")
            {
                var cd = new [] { 12, 11.5, 11, 10.5, 10 } [Math.Max(0, Me.GetSpellBook().GetSpellClass(SpellSlot.E).Level - 1)];
                Stamps["EliseHumanE"] = (float) (GameEngine.GameTime + cd);
            }

            if (spell.SpellData.SpellName == "EliseSpiderQCast")
            {
                var cd = 6 * (100 / (100 + Me.UnitStats.AbilityHaste));
                Stamps["EliseHumanQ"] = GameEngine.GameTime + cd;
            }
        }

        private async Task OnCoreMainTick()
        {
            HandleSpellTimers();
        }

        private async Task OnCreateObject(List<AIBaseClient> unitList, AIBaseClient unit, float time)
        {
            if (unit.Name.Contains("Elise") && unit.Name.Contains("R_cas"))
            {
                var cd = 4 * (100 / (100 + Me.UnitStats.AbilityHaste));
                Stamps["EliseR"] = GameEngine.GameTime + cd;
            }

            if (unit.Name.Contains("Elise") && unit.Name.Contains("W_LegBuff"))
            {
                var cd = 12 * (100 / (100 + Me.UnitStats.AbilityHaste));
                Stamps["EliseSpiderW"] = GameEngine.GameTime + cd;
            }

            if (unit.Name.Contains("Elise") && unit.Name.Contains("spider_e_land"))
            {
                var cd = new [] { 12, 11.5, 11, 10.5, 10 } [Math.Max(0, Me.GetSpellBook().GetSpellClass(SpellSlot.E).Level - 1)];
                Stamps["EliseSpiderE"] = (float) (GameEngine.GameTime + cd);
            }
        }

        private void HandleSpellTimers()
        {
            Sq = Stamps["EliseSpiderQ"] - GameEngine.GameTime > 0 ? Stamps["EliseSpiderQ"] - GameEngine.GameTime : 0;
            Sw = Stamps["EliseSpiderW"] - GameEngine.GameTime > 0 ? Stamps["EliseSpiderW"] - GameEngine.GameTime : 0;
            Se = Stamps["EliseSpiderE"] - GameEngine.GameTime > 0 ? Stamps["EliseSpiderE"] - GameEngine.GameTime : 0;
            Hq = Stamps["EliseHumanQ"] - GameEngine.GameTime > 0 ? Stamps["EliseHumanQ"] - GameEngine.GameTime : 0;
            Hw = Stamps["EliseHumanW"] - GameEngine.GameTime > 0 ? Stamps["EliseHumanW"] - GameEngine.GameTime : 0;
            He = Stamps["EliseHumanE"] - GameEngine.GameTime > 0 ? Stamps["EliseHumanE"] - GameEngine.GameTime : 0;
        }

        private bool BurstCheck(AIBaseClient unit)
        {
            if (unit == null || !unit.IsValidTarget()) return false;

            if (He > Me.GetSpellBook().GetSpellClass(SpellSlot.E).Cooldown - 1) return true;
            if (!Me.CheckLineCollision(unit)) return true;
            if (CocoonStunned(unit)) return true;

            return false;
        }



        private void NeurotoxinQ(AIBaseClient unit)
        {
            if (unit == null || !unit.IsValidTarget()) return;

            if (!IsSpiderForm && CanUse(SpellSlot.Q))
            {
                if (unit.Distance(Me) <= 625)
                {
                    SpellCastProvider.CastSpell(CastSlot.Q, unit.Position);
                }
            }
        }

        private void VolatileW(AIBaseClient unit)
        {
            if (unit == null || !unit.IsValidTarget()) return;

            if (!IsSpiderForm && CanUse(SpellSlot.W))
            {
                if (unit.Distance(Me) <= 950)
                {
                    SpellCastProvider.CastSpell(CastSlot.W, unit.Position);
                }
            }
        }

        private void CocoonE(AIBaseClient unit)
        {
            if (unit == null || !unit.IsValidTarget()) return;

            if (!IsSpiderForm && CanUse(SpellSlot.E))
            {
                if (unit.Distance(Me) <= 1100)
                {
                    if (CheckE.IsOn)
                    {
                        var pOutput = LS.Prediction.GetPrediction(unit, 0.25f, 40, 1300);
                        if (pOutput.Hitchance >= LS.HitChance.VeryHigh)
                            SpellCastProvider.CastSpell(CastSlot.E, pOutput.CastPosition);
                    }
                    else
                    {
                        var pOutput = LS.Prediction.GetPrediction(unit, 0.25f, 40, 1300);
                        if (pOutput.Hitchance >= LS.HitChance.High)
                            SpellCastProvider.CastSpell(CastSlot.E, pOutput.CastPosition);
                    }
                }
            }
        }

        private void VenomousBiteQ(AIBaseClient unit)
        {
            if (unit == null || !unit.IsValidTarget()) return;

            if (IsSpiderForm && CanUse(SpellSlot.Q, true))
            {
                if (unit.Distance(Me) <= 475)
                    SpellCastProvider.CastSpell(CastSlot.Q, unit.Position);
            }
        }

        private void SkitteringFrenzyW(AIBaseClient unit)
        {
            if (unit == null || !unit.IsValidTarget()) return;

            if (IsSpiderForm && CanUse(SpellSlot.W, true))
            {
                if (CanUse(SpellSlot.Q, true) && unit.Distance(Me) <= 575)
                    SpellCastProvider.CastSpell(CastSlot.W, unit.Position);
                
                if (unit.Distance(Me) <= 425)
                    SpellCastProvider.CastSpell(CastSlot.W, unit.Position);
            }
        }

        private void RappelE(AIBaseClient unit)
        {
            if (unit == null || !unit.IsValidTarget()) return;

            if (IsSpiderForm && CanUse(SpellSlot.E, true))
            {
                if (BurstCheck(unit) && !Me.CheckLineCollision(unit))
                {
                    if (unit.Distance(Me) <= 825 && CanUse(SpellSlot.Q, true))
                    {
                        return;
                    }
                }

                if (unit.Distance(Me) <= 750)
                {
                    if (CanUse(SpellSlot.Q, true) && unit.Distance(Me) > 530)
                        SpellCastProvider.CastSpell(CastSlot.E, unit.Position);

                    if (unit.Distance(Me) >= Me.AttackRange + 155)
                        SpellCastProvider.CastSpell(CastSlot.E, unit.Position);
                }
            }
        }

        private bool CanTransform(AIBaseClient unit, bool human)
        {
            if (unit == null || !unit.IsValidTarget()) return false;

            if (BurstCheck(unit)) return true;

            if (human)
            {
                if (!CanUse(SpellSlot.Q) && !CanUse(SpellSlot.W) && !CanUse(SpellSlot.E))
                {
                    return false;
                }
            }
            else
            {
                if (!CanUse(SpellSlot.Q, true) && !CanUse(SpellSlot.W, true) && !CanUse(SpellSlot.E, true))
                {
                    return false;
                }
            }

            return true;
        }

        private void SwitchHumanToSpider(AIBaseClient unit)
        {
            if (unit == null || !unit.IsValidTarget()) return;
            if (IsSpiderForm || !CanUse(SpellSlot.R)) return;

            if (unit is AIHeroClient)
            {
                if (!CanTransform(unit, true))
                {
                    Logger.Log("CANT");
                    return;
                }

                if (!CanUse(SpellSlot.Q))
                {
                    if (CanUse(SpellSlot.Q, true) && unit.Distance(Me) <= 475)
                    {
                        SpellCastProvider.CastSpell(CastSlot.R, Me.Position);
                    }
                }

                if (CanUse(SpellSlot.E, true) && unit.Distance(Me) <= 750)
                {
                    if (!CanUse(SpellSlot.Q, true))
                    {
                        SpellCastProvider.CastSpell(CastSlot.R, Me.Position);
                    }

                    else if (unit.Distance(Me) > 475 + Me.AttackRange + 35)
                    {
                        SpellCastProvider.CastSpell(CastSlot.R, Me.Position);
                    }

                    else if (Me.CheckLineCollision(unit))
                    {
                        SpellCastProvider.CastSpell(CastSlot.R, Me.Position);
                    }
                }
            }
            else
            {
                if (!CanUse(SpellSlot.Q) && !CanUse(SpellSlot.W) && !CanUse(SpellSlot.E))
                    SpellCastProvider.CastSpell(CastSlot.R, Me.Position);
            }
        }

        private void SwitchSpiderToHuman(AIBaseClient unit)
        {
            if (unit == null || !unit.IsValidTarget()) return;
            if (!IsSpiderForm || !CanUse(SpellSlot.R)) return;

            if (unit is AIHeroClient)
            {
                if (!CanTransform(unit, false))
                {
                    return;
                }

                if (unit.Distance(Me) > Me.AttackRange)
                {
                    SpellCastProvider.CastSpell(CastSlot.R, Me.Position);
                }

                if (CanTransform(unit, true) && !Me.CheckLineCollision(unit))
                {
                    SpellCastProvider.CastSpell(CastSlot.R, Me.Position);
                }
            }
            else
            {
                if (CanUse(SpellSlot.Q) && CanUse(SpellSlot.W) && CanUse(SpellSlot.E))
                {
                    if (!Me.BuffManager.HasActiveBuff("elisespiderw") && SpiderlingCount() < 1 || Me.Distance(unit) > 525)
                    {
                        if (unit.Distance(Me) > 525)
                        {
                            SpellCastProvider.CastSpell(CastSlot.R, Me.Position);
                        }
                    }
                }

                if (!CanUse(SpellSlot.Q, true) && !CanUse(SpellSlot.W, true))
                {
                    if (!Me.BuffManager.HasActiveBuff("elisespiderw") && SpiderlingCount() < 1 || unit.Distance(Me) > 525 )
                    {
                        if (unit.Distance(Me) > 525)
                        {
                            SpellCastProvider.CastSpell(CastSlot.R, Me.Position);
                        }
                    }
                }
            }
        }


        private bool CanUse(SpellSlot slot, bool spider = false)
        {
            if (Me.GetSpellBook().GetSpellClass(slot).Level < 1)
                return false;

            if (spider && slot == SpellSlot.Q && Sq < 1)
                return true;

            if (spider && slot == SpellSlot.W && Sw < 1)
                return true;

            if (spider && slot == SpellSlot.E && Se < 1)
                return true;

            if (!spider && slot == SpellSlot.Q && Hq < 1)
                return true;

            if (!spider && slot == SpellSlot.W && Hw < 1)
                return true;

            if (!spider && slot == SpellSlot.E && He < 1)
                return true;

            if (slot == SpellSlot.R && Me.GetSpellBook().GetSpellClass(SpellSlot.R).IsSpellReady)
                return true;

            return false;
        }
    }
}