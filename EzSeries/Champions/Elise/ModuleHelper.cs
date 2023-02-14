namespace EzSeries.Champions.Elise
{
    using Oasys.Common.Enums.GameEnums;
    using Oasys.Common.EventsProvider;
    using Oasys.Common.GameObject.Clients;
    using Oasys.Common.GameObject.Clients.ExtendedInstances.Spells;
    using Oasys.SDK;
    using Oasys.SDK.Tools;

    public abstract class ModuleHelper
    {
        private static AIHeroClient Me => UnitManager.MyChampion;
        private static readonly Dictionary<string, float> Ticks = new();
        
        // internal spell timers
        private static float _r;
        private static float _sq, _sw, _se;
        private static float _hq, _hw, _he;

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
        
        public static bool NeurotoxinReady(int time = 1)
        {
            return _hq < time;
        }
        
        public static bool VolatileReady(int time = 1)
        {
            return _hw < time;
        }
        
        public static bool CocoonReady(int time = 1)
        {
            return _he < time;
        }
        
        public static float CocoonTimer()
        {
            return _he;
        }

        public static bool VenomousBiteReady(int time = 1)
        {
            return _sq < time;
        }
        
        public static bool SkitteringFrenzyReady(int time = 1)
        {
            return _sw < time;
        }
        
        public static bool RappelReady(int time = 1)
        {
            return _se < time;
        }
        
        public static bool TransformReady(int time = 1)
        {
            return _r < time;
        }
        
        public static int Spiderlings()
        {
            var b  = Me.BuffManager.GetActiveBuff("elisespiderlingsready");
            if (b != null && b.IsActive)
            {
                return (int) b.Stacks;
            }

            return 0;
        }
        
        public static bool CocoonStunned(AIBaseClient unit)
        {
            return unit.BuffManager.HasActiveBuff("Cocoon");
        }

        public static bool IsSpiderForm()
        {
            return Me.GetSpellBook().GetSpellClass(SpellSlot.Q).SpellData.SpellName == "EliseSpiderQCast";
        }
        
        private static async Task OnCoreMainTick()
        {
            if (Me.GetSpellBook().GetSpellClass(SpellSlot.Q).Level >= 1)
            {
                // if spider q == 0 then ready
                _sq = Ticks["VenomousBite"] - GameEngine.GameTime > 0 ? Ticks["VenomousBite"] - GameEngine.GameTime : 0;

                // if human q == 0 then ready
                _hq = Ticks["Neurotoxin"] - GameEngine.GameTime > 0 ? Ticks["Neurotoxin"] - GameEngine.GameTime : 0;
            }
            else
            {
                _sq = 99;
                _hq = 99;
            }

            if (Me.GetSpellBook().GetSpellClass(SpellSlot.W).Level >= 1)
            {
                // if spider w == 0 then ready
                _sw = Ticks["SkitteringFrenzy"] - GameEngine.GameTime > 0 ? Ticks["SkitteringFrenzy"] - GameEngine.GameTime : 0;

                // if human w == 0 then ready
                _hw = Ticks["Volatile"] - GameEngine.GameTime > 0 ? Ticks["Volatile"] - GameEngine.GameTime : 0;
            }
            else
            {
                _sw = 99;
                _hw = 99;
            }

            if (Me.GetSpellBook().GetSpellClass(SpellSlot.E).Level >= 1)
            {
                // if spider e == 0 then ready
                _se = Ticks["Rappel"] - GameEngine.GameTime > 0 ? Ticks["Rappel"] - GameEngine.GameTime : 0;

                // if human e == 0 then ready
                _he = Ticks["Cocoon"] - GameEngine.GameTime > 0 ? Ticks["Cocoon"] - GameEngine.GameTime : 0;
            }
            else
            {
                _se = 99;
                _he = 99;
            }
            
            if (Me.GetSpellBook().GetSpellClass(SpellSlot.R).Level >= 1)
            {
                // if elise r == 0 then ready
                _r = Ticks["Transform"] - GameEngine.GameTime > 0 ? Ticks["Transform"] - GameEngine.GameTime : 0;
            }
            else
            {
                _r = 99;
            }
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

            if (spell.SpellData.SpellName == "EliseHumanQ")
            {
                var cd = 6 * (100 / (100 + Me.UnitStats.AbilityHaste));
                Ticks["Neurotoxin"] = GameEngine.GameTime + cd;
            }
            
            if (spell.SpellData.SpellName == "EliseHumanW")
            {
                var cd = 12 * (100 / (100 + Me.UnitStats.AbilityHaste));
                Ticks["Volatile"] = GameEngine.GameTime + cd;
            }
            
            if (spell.SpellData.SpellName == "EliseHumanE")
            {
                var cd = new [] { 12, 11.5, 11, 10.5, 10 }[Math.Max(0, Me.GetSpellBook().GetSpellClass(SpellSlot.E).Level - 1)];
                Ticks["Cocoon"] = (float) (GameEngine.GameTime + cd);
            }
            
            if (spell.SpellData.SpellName == "EliseSpiderQCast")
            {
                var cd = 6 * (100 / (100 + Me.UnitStats.AbilityHaste));
                Ticks["VenomousBite"] = GameEngine.GameTime + cd;
            }
        }
    }
}