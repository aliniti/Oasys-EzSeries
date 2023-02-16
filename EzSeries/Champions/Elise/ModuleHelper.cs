namespace EzSeries.Champions.Elise
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
        private static float _r;
        private static float _sq, _sw, _se;
        private static float _hq, _hw, _he;
        
        private static SpellClass QSpellClass => Me.GetSpellBook().GetSpellClass(SpellSlot.Q);

        public static void Initialize()
        {
            Ticks["Neurotoxin"] = 0;
            Ticks["Volatile"] = 0;
            Ticks["Cocoon"] = 0;
            Ticks["VenomousBite"] = 0;
            Ticks["SkitteringFrenzy"] = 0;
            Ticks["Rappel"] = 0;
            Ticks["Transform"] = 0;
            
            CoreEvents.OnCoreMainTick += OnCoreMainTick;
            GameEvents.OnCreateObject += OnCreateObject;
            GameEvents.OnGameProcessSpell += OnProcessSpell;
        }
        
        public static float CocoonTimer(int time = 1) => _he;
        public static bool NeurotoxinReady(int time = 1) => _hq < time;
        public static bool VolatileReady(int time = 1) => _hw < time;
        public static bool CocoonReady(int time = 1) => _he < time;
        public static bool VenomousBiteReady(int time = 1) => _sq < time;
        public static bool SkitteringFrenzyReady(int time = 1) => _sw < time;
        public static bool RappelReady(int time = 1) => _se < time;
        public static bool TransformReady(int time = 1) => _r < time;
        public static bool CocoonStunned(AIBaseClient unit) => unit.BuffManager.HasActiveBuff("Cocoon");
        public static bool IsSpiderForm() => QSpellClass.SpellData.SpellName == "EliseSpiderQCast";
        
        public static int Spiderlings()
        {
            var b  = Me.BuffManager.GetActiveBuff("elisespiderlingsready");
            return b != null && b.IsActive ? (int) b.Stacks : 0;
        }

        private static async Task OnCoreMainTick()
        {
            // if spider q == 0 then ready
            _sq = Me.GetSpellBook().GetSpellClass(SpellSlot.Q).Level >= 1 
                ? Ticks["VenomousBite"] - GameEngine.GameTime > 0 ? Ticks["VenomousBite"] - GameEngine.GameTime : 0 
                : 99;

            // if human q == 0 then ready
            _hq = Me.GetSpellBook().GetSpellClass(SpellSlot.Q).Level >= 1 
                ? Ticks["Neurotoxin"] - GameEngine.GameTime > 0 ? Ticks["Neurotoxin"] - GameEngine.GameTime : 0 
                : 99;

            // if spider w == 0 then ready
            _sw = Me.GetSpellBook().GetSpellClass(SpellSlot.W).Level >= 1
                ? Ticks["SkitteringFrenzy"] - GameEngine.GameTime > 0 ? Ticks["SkitteringFrenzy"] - GameEngine.GameTime : 0
                : 99;

            // if human w == 0 then ready
            _hw = Me.GetSpellBook().GetSpellClass(SpellSlot.W).Level >= 1 
                ? Ticks["Volatile"] - GameEngine.GameTime > 0 ? Ticks["Volatile"] - GameEngine.GameTime : 0 
                : 99;

            // if human e == 0 then ready
            _he = Me.GetSpellBook().GetSpellClass(SpellSlot.E).Level >= 1 
                ? Ticks["Cocoon"] - GameEngine.GameTime > 0 ? Ticks["Cocoon"] - GameEngine.GameTime : 0 
                : 99;

            // if spider e == 0 then ready
            _se = Me.GetSpellBook().GetSpellClass(SpellSlot.E).Level >= 1 
                ? Ticks["Rappel"] - GameEngine.GameTime > 0 ? Ticks["Rappel"] - GameEngine.GameTime : 0 
                : 99;

            // if elise r == 0 then ready
            _r = Me.GetSpellBook().GetSpellClass(SpellSlot.R).Level >= 1 
                ? Ticks["Transform"] - GameEngine.GameTime > 0 ? Ticks["Transform"] - GameEngine.GameTime : 0 
                : 99;
        }
        
        private static async Task OnCreateObject(List<AIBaseClient> unitList, AIBaseClient unit, float time)
        {
            if (!unit.IsMe) return;
            
            if (unit.Name.Contains("Elise") && unit.Name.Contains("R_cas"))
            {
                var cd = 4 * (100 / (100 + Me.UnitStats.AbilityHaste));
                Ticks["Transform"] = GameEngine.GameTime + cd;
            }

            if (unit.Name.Contains("Elise") && unit.Name.Contains("W_LegBuff"))
            {
                var cd = 12 * (100 / (100 + Me.UnitStats.AbilityHaste));
                Ticks["SkitteringFrenzy"] = GameEngine.GameTime + cd;
            }

            if (unit.Name.Contains("Elise") && unit.Name.Contains("spider_e_land"))
            {
                var cd = new [] { 12, 11.5, 11, 10.5, 10 } [Math.Max(0, Me.GetSpellBook().GetSpellClass(SpellSlot.E).Level - 1)];
                Ticks["Rappel"] = (float) (GameEngine.GameTime + cd);
            }
        }

        private static async Task OnProcessSpell(AIBaseClient unit, SpellActiveEntry spell)
        {
            if (!unit.IsMe) return;

            switch (spell.SpellData.SpellName)
            {
                case "EliseHumanQ":
                {
                    var cd = 6 * (100 / (100 + Me.UnitStats.AbilityHaste));
                    Ticks["Neurotoxin"] = GameEngine.GameTime + cd;
                    break;
                }
                case "EliseHumanW":
                {
                    var cd = 12 * (100 / (100 + Me.UnitStats.AbilityHaste));
                    Ticks["Volatile"] = GameEngine.GameTime + cd;
                    break;
                }
                case "EliseHumanE":
                {
                    var cd = new [] { 12, 11.5, 11, 10.5, 10 } [Math.Max(0, Me.GetSpellBook().GetSpellClass(SpellSlot.E).Level - 1)];
                    Ticks["Cocoon"] = (float) (GameEngine.GameTime + cd);
                    break;
                }
                case "EliseSpiderQCast":
                {
                    var cd = 6 * (100 / (100 + Me.UnitStats.AbilityHaste));
                    Ticks["VenomousBite"] = GameEngine.GameTime + cd;
                    break;
                }
            }
        }
    }
}