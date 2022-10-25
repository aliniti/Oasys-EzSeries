namespace EzSeries.Champions
{
    #region

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
    using SharpDX;
    
    using Orbwalker = Oasys.SDK.Orbwalker;
    using TargetSelector = Oasys.Common.Logic.TargetSelector;

    #endregion

    public class Nidalee : Plugin
    {
        #region Fields
        
        private Switch _checkq;
        private Switch _debug;
        private Switch _waveclear;
        
        private int _aa;
        private float _cq;
        private float _cw, _ce;
        private float _hq, _hw, _he;
        private readonly Dictionary<string, float> _stamps = new();

        #endregion

        #region Properties and Encapsulation

        // currently the only efficient way
        private bool IsCatForm => Me.AttackRange < 200;
        private bool IsHunted(AIBaseClient unit) => unit.BuffManager.HasActiveBuff("NidaleePassiveHunted");
        private bool HasPrimalSurge() => Me.BuffManager.HasActiveBuff("Primalsurge");
        protected override string PluginName { get; set; } = "Nidalee";

        #endregion

        #region Override Methods

        public override void OnLoadPlugin()
        {
            CoreEvents.OnCoreRender += OnRender;
            CoreEvents.OnCoreMainTick += OnCoreMainTick;
            GameEvents.OnCreateObject += OnCreateObject;
            GameEvents.OnProcessSpell += OnProcessSpell;

            CoreEvents.OnCoreMainInputAsync += OnMainInput;
            CoreEvents.OnCoreHarassInputAsync += OnHarassInput;
            CoreEvents.OnCoreLaneclearInputAsync += OnLaneClearInput;
            Orbwalker.OnOrbwalkerAfterBasicAttack += ( time,  target) => _aa++;

            _stamps["Takedown"] = 0;
            _stamps["Pounce"] = 0;
            _stamps["Swipe"] = 0;
            _stamps["AspectOfCougar"] = 0;
            _stamps["JavelinToss"] = 0;
            _stamps["PrimalSurge"] = 0;
            _stamps["Bushwhack"] = 0;
            
            PluginTab.AddItem(_checkq = new Switch { IsOn = false, Title = "Use VeryHigh HitChance" });
            PluginTab.AddItem(new InfoDisplay { Title = "You can try it ^ but" });
            PluginTab.AddItem(new InfoDisplay { Title = "turn off VeryHigh HitChance if its not Casting Q!" });
            
            // PluginTab.AddItem(_waveclear = new Switch { IsOn = false, Title = "Experimental Wave Clear" });
            // PluginTab.AddItem(new InfoDisplay { Title = "Turn On Experimental Wave Clear to clear minions in lane!" });
            
            PluginTab.AddItem(_debug = new Switch { Title = "Debug Cooldowns", IsOn = false });
            PluginTab.AddItem(new InfoDisplay { Title = "Debugging cooldown timers per spell!" });

            MainTab.AddItem(new InfoDisplay { Title = "Thanks for using EzSeries by Kurisu!" });
            MainTab.AddItem(new InfoDisplay { Title = "Target selection priority is hard coded to Mouse Position" });
            MainTab.AddItem(new InfoDisplay { Title = "Keep your mouse near the target you want! :^)" });
            MainTab.AddItem(new InfoDisplay { Title = "No settings, hold space to win! :^)" });
        }
        
        #endregion

        #region Private Methods and Operators
        
        private async Task OnMainInput()
        {
            var pref = UnitManager.EnemyChampions.FirstOrDefault(IsHunted);
            var t = pref ?? 
                    UnitManager.EnemyChampions
                        .OrderBy(x => x.Distance(GameEngine.ScreenMousePosition))
                        .FirstOrDefault(TargetSelector.IsAttackable);
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
        private async Task OnHarassInput()
        {
            var pref = UnitManager.EnemyChampions.FirstOrDefault(IsHunted);
            var t = pref ?? 
                    UnitManager.EnemyChampions
                        .OrderBy(x => x.Distance(GameEngine.ScreenMousePosition))
                        .FirstOrDefault(TargetSelector.IsAttackable);
            
            if (t != null)
            {
                CastSpear(t);
                SwitchCatToHuman(t, OrbwalkingMode.Mixed);
            }
        }
        
        private async Task OnLaneClearInput()
        {
            // Todo:
        }


        private void OnRender()
        {
            if (!_debug.IsOn) return;

            // debug draw timers
            var wts = LeagueNativeRendererManager.WorldToScreen(Me.Position);

            // draw timers for opposite form stance
            var cougarText = "Q: " + _cq + " W: " + _cw + " E: " + _ce;
            var humanText = "Q: " + _hq + " W: " + _hw + " E: " + _he;
            var text = IsCatForm ? cougarText : humanText;
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
                _aa = 0;
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
            if (Me.GetSpellBook().GetSpellClass(SpellSlot.Q).Level >= 1)
            {
                // if cougar q == 0 then ready
                _cq = _stamps["Takedown"] - GameEngine.GameTime > 0 ? _stamps["Takedown"] - GameEngine.GameTime : 0;

                // if human q == 0 then ready
                _hq = _stamps["JavelinToss"] - GameEngine.GameTime > 0 ? _stamps["JavelinToss"] - GameEngine.GameTime : 0;
            }
            else
            {
                _cq = 99;
                _hq = 99;
            }

            if (Me.GetSpellBook().GetSpellClass(SpellSlot.W).Level >= 1)
            {
                // if cougar w == 0 then ready
                _cw = _stamps["Pounce"] - GameEngine.GameTime > 0 ? _stamps["Pounce"] - GameEngine.GameTime : 0;
            
                // if human w == 0 then ready
                _hw = _stamps["Bushwhack"] - GameEngine.GameTime > 0 ? _stamps["Bushwhack"] - GameEngine.GameTime : 0;
            }
            else
            {
                _cw = 99;
                _hw = 99;
            }

            if (Me.GetSpellBook().GetSpellClass(SpellSlot.E).Level >= 1)
            {
                // if cougar e == 0 then ready
                _ce = _stamps["Swipe"] - GameEngine.GameTime > 0 ? _stamps["Swipe"] - GameEngine.GameTime : 0;

                // if human e == 0 then ready
                _he = _stamps["PrimalSurge"] - GameEngine.GameTime > 0 ? _stamps["PrimalSurge"] - GameEngine.GameTime : 0;
            }
            else
            {
                _ce = 99;
                _he = 99;
            }
        }

        private async Task OnCreateObject(List<AIBaseClient> unitList, AIBaseClient unit, float time)
        {
            if (unit.Name.Contains("Nidalee") && unit.Name.Contains("R_Cas"))
            {
                _aa = 0;
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

        private void CastSpear(AIBaseClient unit)
        {
            if (unit == null || !unit.IsValidTarget()) return;
            if (IsCatForm || !(_hq < 1)) return;

            if (!(unit.Distance(Me) <= 1500)) return;
            
            if (_checkq.IsOn)
            {
                var pInput = new Prediction.MenuSelected.PredictionInput(
                    Prediction.MenuSelected.PredictionType.Line, unit, 1500, 80, 0.25f, 1300,
                    Me.Position, true);

                var predictionOutput = Prediction.MenuSelected.GetPrediction(pInput);
                if (predictionOutput.HitChance >= Prediction.MenuSelected.HitChance.VeryHigh)
                    SpellCastProvider.CastSpell(CastSlot.Q, predictionOutput.CastPosition);
            }
            else
            {
                var pInput = new Prediction.MenuSelected.PredictionInput(
                    Prediction.MenuSelected.PredictionType.Line, unit, 1500, 80, 0.25f, 1300,
                    Me.Position, true);

                var predictionOutput = Prediction.MenuSelected.GetPrediction(pInput);
                if (predictionOutput.HitChance >= Prediction.MenuSelected.HitChance.High)
                    SpellCastProvider.CastSpell(CastSlot.Q, predictionOutput.CastPosition);
            }
        }

        private void CastBushwhack(AIBaseClient unit)
        {
            if (unit == null || !unit.IsValidTarget()) return;
            if (IsCatForm || !(_hw < 1)) return;

            if (unit.Distance(Me) <= 900 && !IsHunted(unit))
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
                    if (_checkq.IsOn)
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
                if (_cw < 1 && unit.Distance(Me) <= 750)
                {
                    if (mode != OrbwalkingMode.LaneClear)
                    {
                        if (_cq < 1 || _ce < 1)
                            SpellCastProvider.CastSpell(CastSlot.R, Me.Position);
                    }
                    else
                    {
                        if (HasPrimalSurge() && _aa >= 3 || _aa >= 2)
                            SpellCastProvider.CastSpell(CastSlot.R, Me.Position);
                    }
                }

                if (_cq < 1 && unit.Distance(Me) <= 300)
                {
                    if (mode != OrbwalkingMode.LaneClear)
                    {
                        if (_cq < 1 || _ce < 1)
                            SpellCastProvider.CastSpell(CastSlot.R, Me.Position);
                    }
                    else
                    {
                        if (HasPrimalSurge() && _aa >= 3 || _aa >= 2)
                            SpellCastProvider.CastSpell(CastSlot.R, Me.Position);
                    }
                }
            }
            else if (unit.Distance(Me) <= 375)
            {
                if (_checkq.IsOn)
                {
                    var pOutput = LS.Prediction.GetPrediction(unit, 0.25f, 80, 1300);
                    if (pOutput.Hitchance < LS.HitChance.VeryHigh)
                        SpellCastProvider.CastSpell(CastSlot.R, Me.Position);
                }
                else
                {
                    var pOutput = LS.Prediction.GetPrediction(unit, 0.25f, 80, 1300);
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