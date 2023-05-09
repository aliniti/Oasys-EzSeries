namespace EzSeries.Champions.Nidalee
{
    using Oasys.Common.Enums.GameEnums;
    using Oasys.Common.EventsProvider;
    using Oasys.Common.GameObject.Clients;
    using Oasys.Common.GameObject.Clients.ExtendedInstances.Spells;
    using Oasys.SDK;

    public abstract class ModuleHelper
    {
        private static AIHeroClient Me => UnitManager.MyChampion;
        private static readonly Dictionary<string, float> Ticks = new();

        // internal spell timers
        private static int _aa;
        private static float _aoc;
        private static float _cq, _cw, _ce;
        private static float _hq, _hw, _he;
        
        private static SpellClass QSpellClass => Me.GetSpellBook().GetSpellClass(SpellSlot.Q);

        public static void Initialize()
        {
            Ticks["Takedown"] = 0;
            Ticks["Pounce"] = 0;
            Ticks["Swipe"] = 0;
            Ticks["AspectOfCougar"] = 0;
            Ticks["JavelinToss"] = 0;
            Ticks["PrimalSurge"] = 0;
            Ticks["Bushwhack"] = 0;

            CoreEvents.OnCoreMainTick += OnUpdate;
            GameEvents.OnCreateObject += OnCreateObject;
            GameEvents.OnGameProcessSpell += OnGameProcessSpell;
            Orbwalker.OnOrbwalkerAfterBasicAttack += (time, target) => _aa++;
        }

        public static bool JavelinReady(int time = 1) => _hq < time;
        public static bool BushwhackReady(int time = 1) => _hw < time;
        public static bool HealReady(int time = 1) => _he < time;
        public static bool TakedownReady(int time = 1) => _cq < time;
        public static bool PounceReady(int time = 1) => _cw < time;
        public static bool SwipeReady(int time = 1) => _ce < time;
        public static bool AspectOfCougarReady(int time = 1) => _aoc < time;
        public static bool AutoAttackCount(int count = 1) => _aa >= count;
        public static bool HasPrimalSurge() => Me.BuffManager.HasActiveBuff("Primalsurge");
        public static bool IsCatForm() => QSpellClass.SpellData.SpellName == "Takedown";
        public static bool IsHunted(AIBaseClient unit) => unit.BuffManager.HasActiveBuff("NidaleePassiveHunted");
        
        private static async Task OnUpdate()
        {
            // if human q == 0 then ready
            _hq = Me.GetSpellBook().GetSpellClass(SpellSlot.Q).Level >= 1
                ? Ticks["JavelinToss"] - GameEngine.GameTime > 0 ? Ticks["JavelinToss"] - GameEngine.GameTime : 0
                : 99;

            // if cougar q == 0 then ready
            _cq = Me.GetSpellBook().GetSpellClass(SpellSlot.Q).Level >= 1 
                ? Ticks["Takedown"] - GameEngine.GameTime > 0 ? Ticks["Takedown"] - GameEngine.GameTime : 0 
                : 99;

            // if cougar w == 0 then ready
            _cw = Me.GetSpellBook().GetSpellClass(SpellSlot.W).Level >= 1 
                ? Ticks["Pounce"] - GameEngine.GameTime > 0 ? Ticks["Pounce"] - GameEngine.GameTime : 0 
                : 99;

            // if human w == 0 then ready
            _hw = Me.GetSpellBook().GetSpellClass(SpellSlot.W).Level >= 1 
                ? Ticks["Bushwhack"] - GameEngine.GameTime > 0 ? Ticks["Bushwhack"] - GameEngine.GameTime : 0 
                : 99;

            // if cougar e == 0 then ready
            _ce = Me.GetSpellBook().GetSpellClass(SpellSlot.E).Level >= 1 
                ? Ticks["Swipe"] - GameEngine.GameTime > 0 ? Ticks["Swipe"] - GameEngine.GameTime : 0 
                : 99;

            // if human e == 0 then ready
            _he = Me.GetSpellBook().GetSpellClass(SpellSlot.E).Level >= 1
                ? Ticks["PrimalSurge"] - GameEngine.GameTime > 0 ? Ticks["PrimalSurge"] - GameEngine.GameTime : 0
                : 99;

            // if aspect of cougar == 0 then ready
            _aoc = Me.GetSpellBook().GetSpellClass(SpellSlot.R).Level >= 1
                ? Ticks["AspectofCougar"] - GameEngine.GameTime > 0 ? Ticks["AspectOfCougar"] - GameEngine.GameTime : 0
                : 99;
        }

        private static async Task OnCreateObject(List<AIBaseClient> unitList, AIBaseClient unit, float time)
        {
            if (!unit.IsMe) return;
            
            if (unit.Name.Contains("Nidalee") && unit.Name.Contains("R_Cas"))
            {
                _aa = 0;
                var cd = 3 * (100 / (100 + Me.UnitStats.AbilityHaste));
                Ticks["AspectOfCougar"] = GameEngine.GameTime + cd;
            }

            if (unit.Name.Contains("Nidalee") && unit.Name.Contains("Cougar_W_Cas"))
            {
                var cd = 6 * (100 / (100 + Me.UnitStats.AbilityHaste));
                Ticks["Pounce"] = GameEngine.GameTime + cd;
            }

            if (unit.Name.Contains("Nidalee") && unit.Name.Contains("Cougar_W_Marked_Cas"))
            {
                var cd = new [] { 3, 2.5, 2, 1.5, 1.5 } [Math.Max(0, Me.GetSpellBook().GetSpellClass(SpellSlot.W).Level - 1)];
                Ticks["Pounce"] = (float) (GameEngine.GameTime + cd);
            }
        }

        private static async Task OnGameProcessSpell(AIBaseClient unit, SpellActiveEntry spell)
        {
            if (!unit.IsMe) return;

            if (spell.SpellData.SpellName.ToLower().Contains("attack"))
            {
                if (Me.BuffManager.HasBuff("Takedown"))
                {
                    var cd = 6 * (100 / (100 + Me.UnitStats.AbilityHaste));
                    Ticks["Takedown"] = GameEngine.GameTime + cd;
                }
            }

            switch (spell.SpellData.SpellName)
            {
                case "Swipe":
                {
                    var cd = 6 * (100 / (100 + Me.UnitStats.AbilityHaste));
                    Ticks["Swipe"] = GameEngine.GameTime + cd;
                    break;
                }
                case "JavelinToss":
                {
                    _aa = 0;
                    var cd = 6 * (100 / (100 + Me.UnitStats.AbilityHaste));
                    Ticks["JavelinToss"] = GameEngine.GameTime + cd;
                    break;
                }
                case "Bushwhack":
                {
                    var cd = new [] { 13, 12, 11, 10, 9 } [Math.Max(0, Me.GetSpellBook().GetSpellClass(SpellSlot.W).Level - 1)]
                             * (100 / (100 + Me.UnitStats.AbilityHaste));

                    Ticks["Bushwhack"] = GameEngine.GameTime + cd;
                    break;
                }
                case "PrimalSurge":
                {
                    var cd = 12 * (100 / (100 + Me.UnitStats.AbilityHaste));
                    Ticks["PrimalSurge"] = GameEngine.GameTime + cd;
                    break;
                }
            }
        }
    }
}