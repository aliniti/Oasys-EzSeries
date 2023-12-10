namespace EzSeries.Models
{
    using Helpers;
    using Oasys.Common.Enums.GameEnums;
    using Oasys.Common.GameObject.Clients;
    using Oasys.Common.GameObject.Clients.ExtendedInstances.Spells;
    using Oasys.SDK;
    using Oasys.SDK.SpellCasting;
    using SharpDX;

    /// <summary>
    /// Represents a spell in the game.
    /// </summary>
    public class Spell
    {
        /// <summary>
        /// The time when the spell was last casted.
        /// </summary>
        private int _casted;

        /// <summary>
        /// Indicates whether the spell has collision.
        /// </summary>
        private bool _collision;

        /// <summary>
        /// The delay of the spell.
        /// </summary>
        private float _delay;

        /// <summary>
        /// The radius of the spell.
        /// </summary>
        private float _radius;

        /// <summary>
        /// The range of the spell.
        /// </summary>
        private float _range;

        /// <summary>
        /// The slot of the spell.
        /// </summary>
        private readonly CastSlot _slot;

        /// <summary>
        /// The speed of the spell.
        /// </summary>
        private float _speed;

        /// <summary>
        /// The prediction type of the spell.
        /// </summary>
        private Prediction.MenuSelected.PredictionType _type;

        /// <summary>
        /// Initializes a new instance of the <see cref="Spell"/> class with a specified slot.
        /// </summary>
        /// <param name="slot">The slot of the spell.</param>
        public Spell(CastSlot slot)
        {
            _slot = slot;
            _range = float.MaxValue;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Spell"/> class with a specified slot and range.
        /// </summary>
        /// <param name="slot">The slot of the spell.</param>
        /// <param name="range">The range of the spell.</param>
        public Spell(CastSlot slot, float range)
        {
            _slot = slot;
            _range = range;
        }

        /// <summary>
        /// Sets the skill shot parameters for the spell.
        /// </summary>
        /// <param name="delay">The delay of the spell.</param>
        /// <param name="speed">The speed of the spell.</param>
        /// <param name="radius">The radius of the spell.</param>
        /// <param name="collision">Whether the spell has collision.</param>
        /// <param name="type">The prediction type of the spell.</param>
        public void SetSkillShot(float delay, float speed, float radius, bool collision, Prediction.MenuSelected.PredictionType type)
        {
            _type = type;
            _delay = delay;
            _speed = speed;
            _radius = radius;
            _collision = collision;
        }

        /// <summary>
        /// Casts the spell.
        /// </summary>
        /// <returns>True if the spell was casted, false otherwise.</returns>
        public bool Cast()
        {
            if (Environment.TickCount - _casted <= 100)
            {
                return false;
            }

            _casted = Environment.TickCount;
            return SpellCastProvider.CastSpell(_slot);
        }

        /// <summary>
        /// Casts the spell at a unit with a minimum hit chance.
        /// </summary>
        /// <param name="unit">The unit to cast the spell at.</param>
        /// <param name="minimum">The minimum hit chance.</param>
        /// <returns>True if the spell was casted, false otherwise.</returns>
        public bool Cast(AIBaseClient? unit, Prediction.MenuSelected.HitChance minimum)
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

        /// <summary>
        /// Casts the spell at a position.
        /// </summary>
        /// <param name="position">The position to cast the spell at.</param>
        /// <returns>True if the spell was casted, false otherwise.</returns>
        public bool Cast(Vector3 position)
        {
            if (Environment.TickCount - _casted <= 100)
            {
                return false;
            }

            _casted = Environment.TickCount;
            return SpellCastProvider.CastSpell(_slot, position);
        }

        /// <summary>
        /// Casts the spell at a unit's position.
        /// </summary>
        /// <param name="unit">The unit to cast the spell at.</param>
        /// <returns>True if the spell was casted, false otherwise.</returns>
        public bool Cast(AIBaseClient unit)
        {
            if (Environment.TickCount - _casted <= 100)
            {
                return false;
            }

            _casted = Environment.TickCount;
            return SpellCastProvider.CastSpell(_slot, unit.Position);
        }

        /// <summary>
        /// Checks if the spell is ready.
        /// </summary>
        /// <returns>True if the spell is ready, false otherwise.</returns>
        public bool IsReady()
        {
            return this.GetSpellClass().IsSpellReady;
        }

        /// <summary>
        /// Gets the spell class of the spell.
        /// </summary>
        /// <returns>The spell class of the spell.</returns>
        public SpellClass GetSpellClass()
        {
            return UnitManager.MyChampion.GetSpellBook().GetSpellClass((SpellSlot) this._slot - 16);
        }

        /// <summary>
        /// Gets the prediction output for a unit.
        /// </summary>
        /// <param name="unit">The unit to get the prediction for.</param>
        /// <returns>The prediction output for the unit.</returns>
        public Prediction.MenuSelected.PredictionOutput GetPrediction(AIBaseClient? unit)
        {
            return Prediction.MenuSelected.GetPrediction(
                _type, unit, _range, _radius, _delay, _speed, UnitManager.MyChampion.Position, _collision);
        }
    }
}