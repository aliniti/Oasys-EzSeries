namespace EzSeries.Champions
{
    #region

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
    using SharpDX;
    using Orbwalker = Oasys.SDK.Orbwalker;
    using TargetSelector = Oasys.SDK.TargetSelector;

    #endregion

    public class Nidalee : Plugin
    {
        #region Fields

        private int _aaCounter;

        private Switch _checkQ;
        private float _cq;
        private float _cw, _ce;
        private Switch _debug;
        private float _hq, _hw, _he;

        private readonly Dictionary<string, float> _stamps = new();

        #endregion

        #region Properties and Encapsulation

        // currently the only efficient way
        private bool IsCatForm => Me.AttackRange < 200;
        public override string PluginName { get; set; } = "Nidalee";

        #endregion

        #region Override Methods

        public override void OnLoadPlugin()
        {
            CoreEvents.OnCoreRender += OnRender;
            CoreEvents.OnCoreMainTick += OnCoreMainTick;
            GameEvents.OnCreateObject += OnCreateObject;
            GameEvents.OnProcessSpell += OnProcessSpell;

            CoreEvents.OnCoreMainInputAsync += OnMainInput;
            CoreEvents.OnCoreLaneclearInputAsync += OnLaneClearInput;
            Orbwalker.OnOrbwalkerAfterBasicAttack += ( time,  target) => _aaCounter++;

            _stamps["Takedown"] = 0;
            _stamps["Pounce"] = 0;
            _stamps["Swipe"] = 0;
            _stamps["AspectOfCougar"] = 0;
            _stamps["JavelinToss"] = 0;
            _stamps["PrimalSurge"] = 0;
            _stamps["Bushwhack"] = 0;

            PluginTab.AddItem(_checkQ = new Switch { IsOn = false, Title = "Use VeryHigh HitChance" });
            PluginTab.AddItem(_debug = new Switch { Title = "Debug Timers", IsOn = false });
        }

        #endregion

        #region Public Methods and Operators

        public bool IsHunted(AIBaseClient unit)
        {
            return unit.BuffManager.HasActiveBuff("NidaleePassiveHunted");
        }

        public bool HasPrimalSurge()
        {
            return Me.BuffManager.HasActiveBuff("Primalsurge");
        }

        #endregion

        #region Private Methods and Operators
        
        private async Task OnMainInput()
        {
            var t = UnitManager.EnemyChampions.MinBy(TargetSelector.AttacksLeftToKill);
            if (t != null)
            {
                CastSpear(t);
                CastBushwhack(t);
                CastPrimalSurge(t, OrbwalkingMode.Combo);
                SwitchHumanToCat(t, OrbwalkingMode.Combo);
                SwitchCatToHuman(t, OrbwalkingMode.Combo);
                CastSwipe(t);
                CastTakedown(t);
                CastPounce(t);
            }
        }
        
        private async Task OnLaneClearInput()
        {
            foreach (var u in ObjectManagerExport.JungleObjectCollection)
            {
                var minion = u.Value;
                if (minion.Name.Contains("Mini")) continue;
                if (minion.Name.Contains("Plant")) continue;

                CastSpear(minion);
                CastBushwhack(minion);
                CastPrimalSurge(minion, OrbwalkingMode.LaneClear);
                SwitchHumanToCat(minion, OrbwalkingMode.LaneClear);
                SwitchCatToHuman(minion, OrbwalkingMode.LaneClear);
                CastSwipe(minion);
                CastTakedown(minion);
                CastPounce(minion);
            }
        }


        private void OnRender()
        {
            if (!_debug.IsOn) return;

            // debug draw timers
            var wts = LeagueNativeRendererManager.WorldToScreen(Me.Position);

            // draw timers for opposite form stance
            var cougarText = "Q: " + _cq + " W: " + _cw + " E: " + _ce;
            var humanText = "Q: " + _hq + " W: " + _hw + " E: " + _he;
            var text = IsCatForm ? humanText : cougarText;
            // var font = new Font(RenderFactory.RenderDevice, new FontDescription { FaceName = "Courier New" });

            RenderFactory.DrawText(text, 4, new Vector2(wts[0], wts[1] + 35), Color.LimeGreen, false);
        }

        private async Task OnProcessSpell(AIBaseClient unit, SpellActiveEntry spell)
        {
            if (!unit.IsMe) return;

            if (spell.IsBasicAttack)
                if (Me.BuffManager.HasBuff("Takedown") && _cq < 1)
                {
                    var cd = 6 * (100 / (100 + Me.UnitStats.AbilityHaste));
                    _stamps["Takedown"] = GameEngine.GameTime + cd;
                }

            if (spell.SpellData.SpellName == "Swipe")
            {
                var cd = 6 * (100 / (100 + Me.UnitStats.AbilityHaste));
                _stamps["Swipe"] = GameEngine.GameTime + cd;
            }

            if (spell.SpellData.SpellName == "JavelinToss")
            {
                _aaCounter = 0;
                var cd = 6 * (100 / (100 + Me.UnitStats.AbilityHaste));
                _stamps["JavelinToss"] = GameEngine.GameTime + cd;
            }

            if (spell.SpellData.SpellName == "Bushwhack")
            {
                var cd = new [] { 13, 12, 11, 10, 9 } [Math.Max(0 , Me.GetSpellBook().GetSpellClass(SpellSlot.W).Level - 1)]
                         * (100 / (100 + Me.UnitStats.AbilityHaste));

                _stamps["Bushwhack"] = GameEngine.GameTime + cd;
            }

            if (spell.SpellData.SpellName == "PrimalSurge")
            {
                var cd = 12 * (100 / (100 + Me.UnitStats.AbilityHaste));
                _stamps["PrimalSurge"] = GameEngine.GameTime + cd;
            }
        }

        private async Task OnCoreMainTick()
        {
            HandleSpellTimers();
        }

        private async Task OnCreateObject(List<AIBaseClient> unitList, AIBaseClient unit, float time)
        {
            if (unit.Name.Contains("Nidalee") && unit.Name.Contains("R_Cas"))
            {
                _aaCounter = 0;
                var cd = 3 * (100 / (100 + Me.UnitStats.AbilityHaste));
                _stamps["AspectOfCougar"] = GameEngine.GameTime + cd;
            }

            if (unit.Name.Contains("Nidalee") && unit.Name.Contains("Cougar_W_Cas"))
            {
                var cd = 6 * (100 / (100 + Me.UnitStats.AbilityHaste));
                _stamps["Pounce"] = GameEngine.GameTime + cd;
            }

            if (unit.Name.Contains("Nidalee") && unit.Name.Contains("Cougar_W_Marked_Cas"))
            {
                var cd = new [] { 3, 2.5, 2, 1.5, 1.5 } [Math.Max(0, Me.GetSpellBook().GetSpellClass(SpellSlot.W).Level - 1)];
                _stamps["Pounce"] = (float) (GameEngine.GameTime + cd);
            }
        }

        #endregion

        #region Human & Cougar Combat

        private void HandleSpellTimers()
        {
            _cq = _stamps["Takedown"] - GameEngine.GameTime > 0 ? _stamps["Takedown"] - GameEngine.GameTime : 0;
            _cw = _stamps["Pounce"] - GameEngine.GameTime > 0 ? _stamps["Pounce"] - GameEngine.GameTime : 0;
            _ce = _stamps["Swipe"] - GameEngine.GameTime > 0 ? _stamps["Swipe"] - GameEngine.GameTime : 0;
            _hq = _stamps["JavelinToss"] - GameEngine.GameTime > 0 ? _stamps["JavelinToss"] - GameEngine.GameTime : 0;
            _hw = _stamps["Bushwhack"] - GameEngine.GameTime > 0 ? _stamps["Bushwhack"] - GameEngine.GameTime : 0;
            _he = _stamps["PrimalSurge"] - GameEngine.GameTime > 0 ? _stamps["PrimalSurge"] - GameEngine.GameTime : 0;
        }

        private void CastSpear(AIBaseClient unit)
        {
            if (unit == null || !unit.IsValidTarget()) return;
            if (IsCatForm || !(_hq < 1)) return;

            if (unit.Distance(Me) <= 1500)
            {
                if (_checkQ.IsOn)
                {
                    var pOutput = LS.Prediction.GetPrediction(unit, 0.25f, 40, 1300);
                    if (pOutput.Hitchance >= LS.HitChance.VeryHigh)
                        SpellCastProvider.CastSpell(CastSlot.Q, pOutput.CastPosition);
                }
                else
                {
                    var pInput = new Prediction.MenuSelected.PredictionInput(
                        Prediction.MenuSelected.PredictionType.Line, unit, 1500, 40, 0.25f, 1300,
                        Me.Position, true);


                    var predictionOutput = Prediction.MenuSelected.GetPrediction(pInput);
                    if (predictionOutput.HitChance >= Prediction.MenuSelected.HitChance.High)
                        SpellCastProvider.CastSpell(CastSlot.Q, predictionOutput.CastPosition);
                }
            }
        }

        private void CastBushwhack(AIBaseClient unit)
        {
            if (unit == null || !unit.IsValidTarget()) return;
            if (IsCatForm || !(_hw < 1)) return;

            if (unit.Distance(Me) <= 900)
                SpellCastProvider.CastSpell(CastSlot.W, unit.Position);
        }

        private void CastPrimalSurge(AIBaseClient unit, OrbwalkingMode mode)
        {
            if (unit == null || !unit.IsValidTarget()) return;
            if (IsCatForm || !(_he < 1)) return;

            var healthPct = Me.Health / Me.MaxHealth * 100;
            var manaPct = Me.Mana / Me.MaxMana * 100;

            switch (mode)
            {
                case OrbwalkingMode.LaneClear:
                    if (manaPct > 35 && healthPct <= 90)
                        SpellCastProvider.CastSpell(CastSlot.E, Me.Position);
                    break;
                case OrbwalkingMode.Combo:
                    if (healthPct <= 65)
                        SpellCastProvider.CastSpell(CastSlot.E, Me.Position);
                    break;
                case OrbwalkingMode.Mixed:
                    if (manaPct > 65 && healthPct <= 85)
                        SpellCastProvider.CastSpell(CastSlot.E, Me.Position);
                    break;
            }
        }

        private void CastTakedown(AIBaseClient unit)
        {
            if (unit == null || !unit.IsValidTarget()) return;
            if (IsCatForm && _cq < 1)
                if (unit.Distance(Me) <= 300)
                    if (SpellCastProvider.CastSpell(CastSlot.Q, Me.Position))
                        Orbwalker.AttackReset();
        }

        private void CastPounce(AIBaseClient unit)
        {
            if (unit == null || !unit.IsValidTarget()) return;
            if (IsCatForm && _cw < 1)
            {
                if (IsHunted(unit) && unit.Distance(Me) <= 750)
                    SpellCastProvider.CastSpell(CastSlot.W, unit.Position);

                if (!IsHunted(unit) && unit.Distance(Me) <= 375)
                    SpellCastProvider.CastSpell(CastSlot.W, unit.Position);
            }
        }

        private void CastSwipe(AIBaseClient unit)
        {
            if (unit == null || !unit.IsValidTarget()) return;
            if (!IsCatForm || !(_ce < 1)) return;

            if (unit.Distance(Me) <= 300)
                SpellCastProvider.CastSpell(CastSlot.E, unit.Position);
        }

        private void SwitchCatToHuman(AIBaseClient unit, OrbwalkingMode mode)
        {
            if (unit == null || !unit.IsValidTarget()) return;
            var r = Me.GetSpellBook().GetSpellClass(SpellSlot.R);

            if (IsCatForm && r.IsSpellReady)
            {
                if (_hq < 1 && (_cw >= 3 || unit.Distance(Me) > 375))
                {
                    if (_checkQ.IsOn)
                    {
                        var pOutput = LS.Prediction.GetPrediction(unit, 0.25f, 40, 1300);
                        if (pOutput.Hitchance >= LS.HitChance.VeryHigh)
                            SpellCastProvider.CastSpell(CastSlot.R, Me.Position);
                    }
                    else
                    {
                        var pOutput = LS.Prediction.GetPrediction(unit, 0.25f, 40, 1300);
                        if (pOutput.Hitchance >= LS.HitChance.High)
                            SpellCastProvider.CastSpell(CastSlot.R, Me.Position);
                    }
                }
                else
                {
                    if (_cq > 1 && _cw > 1 && _ce > 1)
                        SpellCastProvider.CastSpell(CastSlot.R, Me.Position);

                    if (_hq < 1 && (_cw >= 3 || unit.Distance(Me) > 375))
                        SpellCastProvider.CastSpell(CastSlot.R, Me.Position);
                }
            }
        }

        private void SwitchHumanToCat(AIBaseClient unit, OrbwalkingMode mode)
        {
            if (unit == null || !unit.IsValidTarget()) return;
            var r = Me.GetSpellBook().GetSpellClass(SpellSlot.R);

            if (IsCatForm || !r.IsSpellReady) return;
            
            if (IsHunted(unit))
            {
                if (_cw > 1) return;
                if (!(unit.Distance(Me) <= 750)) return;
                
                if (mode != OrbwalkingMode.LaneClear)
                {
                    if (_cq < 1 || _ce < 1)
                        SpellCastProvider.CastSpell(CastSlot.R, Me.Position);
                }
                else
                {
                    if ((HasPrimalSurge() && _aaCounter >= 3) || _aaCounter >= 2)
                        SpellCastProvider.CastSpell(CastSlot.R, Me.Position);
                }
            }
            else
            {
                if (!(unit.Distance(Me) <= 375)) return;
                
                if (_checkQ.IsOn)
                {
                    var pOutput = LS.Prediction.GetPrediction(unit, 0.25f, 40, 1300);
                    if (pOutput.Hitchance < LS.HitChance.VeryHigh)
                        SpellCastProvider.CastSpell(CastSlot.R, Me.Position);
                }
                else
                {
                    var pOutput = LS.Prediction.GetPrediction(unit, 0.25f, 40, 1300);
                    if (pOutput.Hitchance < LS.HitChance.High)
                        SpellCastProvider.CastSpell(CastSlot.R, Me.Position);
                }

                if (mode != OrbwalkingMode.LaneClear)
                {
                    if (_hq > 1)
                        SpellCastProvider.CastSpell(CastSlot.R, Me.Position);
                }
                else
                {
                    if (_hq > 1 && _hw > 1)
                        SpellCastProvider.CastSpell(CastSlot.R, Me.Position);
                }
            }
        }

        #endregion
    }
}