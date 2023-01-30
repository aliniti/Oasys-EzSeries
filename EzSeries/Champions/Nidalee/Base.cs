namespace EzSeries.Champions.Nidalee
{
    using Oasys.Common.Enums.GameEnums;
    using Oasys.Common.EventsProvider;
    using Oasys.Common.GameObject.Clients;
    using Oasys.Common.GameObject.Clients.ExtendedInstances.Spells;
    using Oasys.SDK;

    public abstract class Base
    {
        private static AIHeroClient MyHero => UnitManager.MyChampion;
        private static readonly Dictionary<string, float> Ticks = new();

        // internal spell timers
        private static int _aa;
        private static float _aoc;
        private static float _cq, _cw, _ce;
        private static float _hq, _hw, _he;
        
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

        public static bool JavelinIsReady(int time = 1)
        {
            return _hq < time;
        }

        public static bool BushwhackIsReady(int time = 1)
        {
            return _hw < time;
        }

        public static bool PrimalSurgeIsReady(int time = 1)
        {
            return _he < time;
        }

        public static bool TakedownIsReady(int time = 1)
        {
            return _cq < time;
        }

        public static bool PounceIsReady(int time = 1)
        {
            return _cw < time;
        }

        public static bool SwipeIsReady(int time = 1)
        {
            return _ce < time;
        }

        public static bool AspectOfCougarReady(int time = 1)
        {
            return _aoc < time;
        }
        
        public static bool AutoAttackCount(int count = 1)
        {
            return _aa >= count;
        }
        
        public static bool IsCatForm()
        {
            return MyHero.GetSpellBook().GetSpellClass(SpellSlot.Q).SpellData.SpellName == "Takedown";
        }
        
        public static bool IsHunted(AIBaseClient unit)
        {
            return unit.BuffManager.HasActiveBuff("NidaleePassiveHunted");
        }

        public static bool HasPrimalSurge()
        {
            return MyHero.BuffManager.HasActiveBuff("Primalsurge");
        }

        private static async Task OnUpdate()
        {
            if (MyHero.GetSpellBook().GetSpellClass(SpellSlot.Q).Level >= 1)
            {
                // if cougar q == 0 then ready
                _cq = Ticks["Takedown"] - GameEngine.GameTime > 0 ? Ticks["Takedown"] - GameEngine.GameTime : 0;

                // if human q == 0 then ready
                _hq = Ticks["JavelinToss"] - GameEngine.GameTime > 0 ? Ticks["JavelinToss"] - GameEngine.GameTime : 0;
            }
            else
            {
                _cq = 99;
                _hq = 99;
            }

            if (MyHero.GetSpellBook().GetSpellClass(SpellSlot.W).Level >= 1)
            {
                // if cougar w == 0 then ready
                _cw = Ticks["Pounce"] - GameEngine.GameTime > 0 ? Ticks["Pounce"] - GameEngine.GameTime : 0;

                // if human w == 0 then ready
                _hw = Ticks["Bushwhack"] - GameEngine.GameTime > 0 ? Ticks["Bushwhack"] - GameEngine.GameTime : 0;
            }
            else
            {
                _cw = 99;
                _hw = 99;
            }

            if (MyHero.GetSpellBook().GetSpellClass(SpellSlot.E).Level >= 1)
            {
                // if cougar e == 0 then ready
                _ce = Ticks["Swipe"] - GameEngine.GameTime > 0 ? Ticks["Swipe"] - GameEngine.GameTime : 0;

                // if human e == 0 then ready
                _he = Ticks["PrimalSurge"] - GameEngine.GameTime > 0 ? Ticks["PrimalSurge"] - GameEngine.GameTime : 0;
            }
            else
            {
                _ce = 99;
                _he = 99;
            }
            
            if (MyHero.GetSpellBook().GetSpellClass(SpellSlot.R).Level >= 1)
            {
                // if aspect of cougar == 0 then ready
                _aoc = Ticks["AspectofCougar"] - GameEngine.GameTime > 0 ? Ticks["AspectOfCougar"] - GameEngine.GameTime : 0;
            }
            else
            {
                _aoc = 99;
            }
        }

        private static async Task OnCreateObject(List<AIBaseClient> unitList, AIBaseClient unit, float time)
        {
            if (!unit.IsMe) return;
            
            if (unit.Name.Contains("Nidalee") && unit.Name.Contains("R_Cas"))
            {
                _aa = 0;
                var cd = 3 * (100 / (100 + MyHero.UnitStats.AbilityHaste));
                Ticks["AspectOfCougar"] = GameEngine.GameTime + cd;
            }

            if (unit.Name.Contains("Nidalee") && unit.Name.Contains("Cougar_W_Cas"))
            {
                var cd = 6 * (100 / (100 + MyHero.UnitStats.AbilityHaste));
                Ticks["Pounce"] = GameEngine.GameTime + cd;
            }

            if (unit.Name.Contains("Nidalee") && unit.Name.Contains("Cougar_W_Marked_Cas"))
            {
                var cd = new [] { 3, 2.5, 2, 1.5, 1.5 }[Math.Max(0, MyHero.GetSpellBook().GetSpellClass(SpellSlot.W).Level - 1)];
                Ticks["Pounce"] = (float) (GameEngine.GameTime + cd);
            }
        }

        private static async Task OnGameProcessSpell(AIBaseClient unit, SpellActiveEntry spell)
        {
            if (!unit.IsMe) return;

            if (spell.IsBasicAttack)
            {
                if (MyHero.BuffManager.HasBuff("Takedown") && TakedownIsReady())
                {
                    var cd = 6 * (100 / (100 + MyHero.UnitStats.AbilityHaste));
                    Ticks["Takedown"] = GameEngine.GameTime + cd;
                }
            }

            switch (spell.SpellData.SpellName)
            {
                case "Swipe":
                {
                    var cd = 6 * (100 / (100 + MyHero.UnitStats.AbilityHaste));
                    Ticks["Swipe"] = GameEngine.GameTime + cd;
                    break;
                }
                case "JavelinToss":
                {
                    _aa = 0;
                    var cd = 6 * (100 / (100 + MyHero.UnitStats.AbilityHaste));
                    Ticks["JavelinToss"] = GameEngine.GameTime + cd;
                    break;
                }
                case "Bushwhack":
                {
                    var cd = new [] { 13, 12, 11, 10, 9 }[Math.Max(0, MyHero.GetSpellBook().GetSpellClass(SpellSlot.W).Level - 1)]
                             * (100 / (100 + MyHero.UnitStats.AbilityHaste));

                    Ticks["Bushwhack"] = GameEngine.GameTime + cd;
                    break;
                }
                case "PrimalSurge":
                {
                    var cd = 12 * (100 / (100 + MyHero.UnitStats.AbilityHaste));
                    Ticks["PrimalSurge"] = GameEngine.GameTime + cd;
                    break;
                }
            }
        }
    }
}