namespace EzSeries.Models
{
    using Helpers;
    using Oasys.Common.Enums.GameEnums;
    using Oasys.Common.GameObject.Clients;
    using Oasys.Common.GameObject.Clients.ExtendedInstances.Spells;
    using Oasys.SDK;
    using Oasys.SDK.SpellCasting;
    using SharpDX;

    public class Spell
    {
        private int _casted;
        private bool _collision;
        private float _delay;
        private float _radius;
        private float _range;
        private readonly CastSlot _slot;
        private float _speed;
        private Prediction.MenuSelected.PredictionType _type;
        
        public Spell(CastSlot slot)
        {
            _slot = slot;
            _range = float.MaxValue;
        }

        public Spell(CastSlot slot, float range)
        {
            _slot = slot;
            _range = range;
        }

        public void SetSkillShot(float range, Prediction.MenuSelected.PredictionType type, float delay, float speed, float radius, bool collision)
        {
            _range = range;
            _type = type;
            _delay = delay;
            _speed = speed;
            _radius = radius;
            _collision = collision;
        }

        public bool Cast()
        {
            if (Environment.TickCount - _casted <= 100)
            {
                return false;
            }

            _casted = Environment.TickCount;
            return SpellCastProvider.CastSpell(_slot);
        }

        public bool Cast(AIBaseClient unit, Prediction.MenuSelected.HitChance minimum)
        {
            if (unit == null || !unit.IsValidTarget()) return false;

            var pred = this.GetPrediction(unit);
            if (pred.CollisionObjects.Count >= 1 && pred.Collision)
            {
                return false;
            }

            if (pred.HitChance < minimum) return false;
            if (Environment.TickCount - _casted <= 100)
            {
                return false;
            }
                    
            _casted = Environment.TickCount;
            return SpellCastProvider.CastSpell(_slot, pred.CastPosition);

        }

        public bool Cast(Vector3 position)
        {
            if (Environment.TickCount - _casted <= 100)
            {
                return false;
            }

            _casted = Environment.TickCount;
            return SpellCastProvider.CastSpell(_slot, position);
        }
        
        public bool Cast(AIBaseClient unit)
        {
            if (Environment.TickCount - _casted <= 100)
            {
                return false;
            }

            _casted = Environment.TickCount;
            return SpellCastProvider.CastSpell(_slot, unit.Position);
        }

        public SpellClass GetSpellClass()
        {
            return UnitManager.MyChampion.GetSpellBook().GetSpellClass((SpellSlot) this._slot - 16);
        }
        
        public Prediction.MenuSelected.PredictionOutput GetPrediction(AIBaseClient unit)
        {
            return Prediction.MenuSelected.GetPrediction(
                _type, unit, _range, _radius, _delay, _speed, UnitManager.MyChampion.Position, _collision);
        }
    }
}