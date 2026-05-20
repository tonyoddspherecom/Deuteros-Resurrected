using Deuteros.Code.Objects;
using Deuteros.Code.Objects.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deuteros.Code.Platform
{
    public class EnemyDroneBuilder
    {
        //build frequencies based on star system (in reverse for some reason)
        static uint[] BuildFrequencies = { 0x2bc, 0x3e8, 0x3b6, 0x384, 0x384, 0x320, 0x2bc, 0x2bc, 0x320 };
        static uint BuildDay { get; set; }

        public static void BuildDrones(uint previousDay, uint currentDay)
        {
            if (GameCore.SingletonInstance.GameData.ActiveSaveFile.AtWar)
            {
                if (currentDay == BuildDay || BuildDay == 0)
                {
                    BuildDay = currentDay + BuildFrequencies[BuildFrequencies.Length - 1 - GameCore.SingletonInstance.GameData.ActiveSaveFile.StarSystemsCaptured] / 100;

                    foreach (var star in GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Stars.Keys)
                    {
                        var productionDroneCount =
                                   GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets.Values.Count(p => p.ActiveMethanoid && p.ParentStar == star) * 2;

                        foreach (var p in GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets.Values.Where(p => p.ActiveMethanoid && p.ParentStar == star))
                        {
                            if (p.Station.Resources.Stores[Enums.ItemTypes.ios_drone] < 200)
                            {
                                p.Station.Resources.Stores[Enums.ItemTypes.ios_drone] += 2;
                                productionDroneCount -= 2;
                            }
                        }

                        if (productionDroneCount == 0)
                        {
                            productionDroneCount = 1;
                        }
                        else
                        {
                            productionDroneCount = 2;
                        }

                        InterStellarShip enemyFleet = (InterStellarShip)GameCore.SingletonInstance.GameData.ActiveSaveFile.Ships.FirstOrDefault(s => s.ShipType != Enums.Ship_Types.Shuttle && ((InterStellarShip)s).MethanoidOwned && ((InterStellarShip)s).StarLocation == star);

                        if (enemyFleet != null)
                        {
                            enemyFleet.DroneCount += productionDroneCount;
                            if (enemyFleet.DroneCount > 200) enemyFleet.DroneCount = 200;
                        }
                    }
                }
            }

        }

    }
}
