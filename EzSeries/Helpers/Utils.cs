namespace EzSeries.Helpers
{
    using Oasys.Common.GameObject.Clients;
    using Oasys.Common.Extensions;
    using SharpDX;

    public static class Utils
    {
        /// <summary> 
        /// <c>IsEpicMonster</c> checks if a given AI unit is an "epic monster" based on the 
        /// name and contents of a specified array. If any element in the array is found in 
        /// the unit's name, the function returns `true`. 
        /// </summary> 
        /// <param name="AIBaseClient"> 
        /// AI model client to be checked for being an epic monster. 
        /// </param> 
        /// <returns> 
        /// a boolean value indicating whether the given AIBaseClient unit has any part of its 
        /// name matching an epic monster name. 
        /// </returns> 
        public static bool IsEpicMonster(this AIBaseClient unit)
        {
            var epic = new [] { "SRU_RiftHerald", "SRU_Baron", "SRU_Dragon" };
            return epic.Any(x => unit.Name.Contains(x));
        }
        
        /// <summary> 
        /// <c>IsLargeMonster</c> checks if the given AIBaseClient has a name containing any 
        /// of the predefined large monsters (`SRU_Blue`, `SRU_Red`, or `Sru_Crab`). 
        /// </summary> 
        /// <param name="AIBaseClient"> 
        /// AI agent for which the function determines if it is large. 
        /// </param> 
        /// <returns> 
        /// a boolean value indicating whether the input `unit` has the name of a large monster 
        /// in its Contains. 
        /// </returns> 
        public static bool IsLargeMonster(this AIBaseClient unit)
        {
            var large = new [] { "SRU_Blue", "SRU_Red", "Sru_Crab" };
            return large.Any(x => unit.Name.Contains(x));
        }
        
        /// <summary> 
        /// <c>IsSmallMonster</c> checks if a given AI base client has any name containing any 
        /// of a predefined list of small monsters 
        /// </summary> 
        /// <param name="AIBaseClient"> 
        /// AI unit whose name is checked against a list of small monsters for the purpose of 
        /// returning `true` if the name contains any member of the list and `false` otherwise. 
        /// </param> 
        /// <returns> 
        /// a boolean value indicating whether the given AI base client is a small monster. 
        /// </returns> 
        public static bool IsSmallMonster(this AIBaseClient unit)
        {
            var small = new [] {  "SRU_Murkwolf", "SRU_Razorbeak", "SRU_Krug", "SRU_Gromp" };
            return small.Any(x => unit.Name.Contains(x));
        }
    }

    public static class Unit
    {
        /// <summary> 
        /// <c>IsValidTarget</c> verifies whether a given `AIBaseClient` instance is valid for 
        /// targeting purposes, based on whether it is alive, visible, and targetable. 
        /// </summary> 
        /// <param name="AIBaseClient?"> 
        /// 3D object being checked for targetability. 
        /// </param> 
        /// <returns> 
        /// a boolean value indicating whether the given unit is a valid target based on its 
        /// alive, visible, and targetable properties. 
        /// </returns> 
        public static bool IsValidTarget(this AIBaseClient? unit)
        {
            return unit is { IsAlive: true, IsVisible: true, IsTargetable: true } ;
        }
    }

    public static class Nav
    {
        /// <summary> 
        /// <c>ExPosition</c> calculates the next position of an AI-controlled unit based on 
        /// its current position and velocity, taking into account the unit's movement speed 
        /// and a lookahead distance. 
        /// </summary> 
        /// <param name="unit"> 
        /// 3D position of an AI base client that the function is meant to calculate and return. 
        /// </param> 
        /// <param name="delay"> 
        /// time interval for the AI entity to move towards its target position before returning 
        /// its current position, which is calculated based on the distance between two points 
        /// in the nav mesh. 
        /// </param> 
        /// <returns> 
        /// the updated position of an AI-controlled entity based on its nav points and movement 
        /// velocity. 
        /// </returns> 
        public static Vector3 ExPosition(AIBaseClient unit, float delay = 0)
        {
            var unitPosition = Vector3.Zero;

            var nav = unit.AIManager.GetNavPoints();
            if (nav.Count == 0)
            {
                return unit.Position;
            }

            if (nav.First().Distance(nav.Last()) <= 50)
            {
                return unit.Position;
            }
            
            for (var i = 0; i < nav.Count - 1; i++)
            {
                var previousPath = nav[i];
                var currentPath = nav[i + 1];
                var direction = (currentPath - previousPath).Normalized();
                var velocity = direction * unit.UnitComponentInfo.UnitBaseMoveSpeed;

                unitPosition = unit.Position + velocity * delay;
            }
            
            return unitPosition;
        }
        
        /// <summary> 
        /// <c>GetPositions</c> generates a list of positions around a central unit based on 
        /// the radius of the unit, maximum number of checks, and incremental radius. The 
        /// function iterates through the radius values to calculate the positions and adds 
        /// them to the provided `posList`. 
        /// </summary> 
        /// <param name="unit"> 
        /// 3D position of the object for which the positions are being computed. 
        /// </param> 
        /// <param name="List<Vector2>"> 
        /// 2D positions of objects that will be generated based on the algorithm used by the 
        /// function. 
        /// </param> 
        public static void GetPositions(AIBaseClient unit, out List<Vector2> posList)
        {
            var posChecked = 0;
            var maxPosChecked = 90;
            var posRadius = 40;
            var radiusIndex = 0;
            
            posList = new List<Vector2>();
            posList.Clear();
            
            while (posChecked < maxPosChecked)
            {
                radiusIndex++;
            
                var curRadius = radiusIndex * 0x2 * posRadius;
                var curCircleChecks = (int) Math.Ceiling(0x2 * Math.PI * curRadius / (0x2 * (double) posRadius));
            
                for (var i = 1; i < curCircleChecks; i++)
                {
                    var cRadians = 0x2 * Math.PI / (curCircleChecks - 0x1) * i;
                    var xPos = (float) Math.Floor(unit.Position.X + curRadius * Math.Cos(cRadians));
                    var yPos = (float) Math.Floor(unit.Position.Z + curRadius * Math.Sin(cRadians));
                    
                    var position = new Vector2(xPos, yPos);
                    posList.Add(position);
                    posChecked++;
                }
            }
        }
    }
}
