namespace EzSeries.Champions
{
    using Base;
    using Helpers;
    using Oasys.Common;
    using Oasys.Common.Enums.GameEnums;
    using Oasys.Common.Extensions;
    using Oasys.Common.GameObject;
    using Oasys.Common.GameObject.Clients;
    using Oasys.Common.Menu.ItemComponents;
    using Oasys.SDK;
    using Oasys.SDK.Events;
    using Oasys.SDK.SpellCasting;

    public class Kindred : Plugin
    {
        private Switch _useQ = null!;
        private Switch _useW = null!;
        private Switch _useE = null!;

        private string _ebuffName = "kindredecharge";
        private string _passiveBuffName = "kindredhittracker";
        private bool _isHunted(AIBaseClient unit) 
            => unit.BuffManager.HasBuff(_ebuffName) || unit.BuffManager.HasBuff(_passiveBuffName);
        
        protected override string PluginName { get; set; } = "Kindred";
        public override void OnLoadPlugin()
        {
            PluginTab.AddItem(_useQ = new Switch { IsOn = true, Title = "Use Q"  });
            PluginTab.AddItem(_useW = new Switch {  IsOn = true, Title = "Use W" });
            PluginTab.AddItem(_useE = new Switch { IsOn = true, Title = "Use E" });

            Orbwalker.OnOrbwalkerAfterBasicAttack += OnAfterAttack;
            CoreEvents.OnCoreMainInputAsync += OnMainInput;
            CoreEvents.OnCoreLaneclearInputAsync += OnLaneClearInput;
        }

        private void OnAfterAttack(float gametime, GameObjectBase unit)
        {
            if (unit is AIBaseClient aiBase && aiBase.IsValidTarget())
                KindredQ(aiBase);
        }
        
        private async Task OnMainInput()
        {
            var pref = UnitManager.EnemyChampions.FirstOrDefault(_isHunted);
            var t = pref ?? UnitManager.EnemyChampions.MinBy(TargetSelector.AttacksLeftToKill);
            if (t != null)
            {
                KindredQGap(t);
                KindredW(t);
                KindredE(t);
            }
        }
        
        private async Task OnLaneClearInput()
        {
            foreach (var u in ObjectManagerExport.JungleObjectCollection)
            {
                var minion = u.Value;
                if (minion.Name.Contains("Mini")) continue;
                if (minion.Name.Contains("Plant")) continue;
                
                KindredQ(minion);
                KindredQGap(minion);
                KindredW(minion);
                KindredE(minion);
            }
        }

        private void KindredQGap(AIBaseClient unit)
        {
            if (unit == null || !unit.IsValidTarget()) return;
            var qSpellClass = Me.GetSpellBook().GetSpellClass(SpellSlot.Q);
            if (qSpellClass.IsSpellReady == false)
                return;
            
            if (!_useQ.IsOn) return;

            if (unit.Distance(Me) <= Me.AttackRange + 300)
                if (unit.Distance(Me) > Me.AttackRange)
                    SpellCastProvider.CastSpell(CastSlot.Q, GameEngine.WorldMousePosition);
        }

        private void KindredQ(AIBaseClient unit)
        {
            if (unit == null || !unit.IsValidTarget()) return;
            var qSpellClass = Me.GetSpellBook().GetSpellClass(SpellSlot.Q);
            if (qSpellClass.IsSpellReady == false)
                return;
            
            if (!_useQ.IsOn) return;
            
            if (unit.Distance(Me) <= Me.AttackRange)
                SpellCastProvider.CastSpell(CastSlot.Q, GameEngine.WorldMousePosition);
        }


        private void KindredW(AIBaseClient unit)
        {
            if (unit == null || !unit.IsValidTarget()) return;
            var wSpellClass = Me.GetSpellBook().GetSpellClass(SpellSlot.W);
            if (wSpellClass.IsSpellReady == false)
                return;
            
            if (!_useW.IsOn) return;

            if (unit.Distance(Me) <= Me.AttackRange + 500)
                SpellCastProvider.CastSpell(CastSlot.W, unit.Position, 1);
        }

        private void KindredE(AIBaseClient unit)
        {
            if (unit == null || !unit.IsValidTarget()) return;
            var eSpellClass = Me.GetSpellBook().GetSpellClass(SpellSlot.E);
            if (eSpellClass.IsSpellReady == false)
                return;
            
            if (!_useE.IsOn) return;
            
            if (unit.Distance(Me) <= Me.AttackRange)
                SpellCastProvider.CastSpell(CastSlot.E, unit.Position);
        }
    }
}