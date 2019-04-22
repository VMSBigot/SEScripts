using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game;
using VRageMath;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        IMyCargoContainer _cargo1;
        IMyCargoContainer _cargo2;

        public Program()
        {
            _cargo1 = GridTerminalSystem.GetBlockWithName("Test Cargo") as IMyCargoContainer;
            _cargo2 = GridTerminalSystem.GetBlockWithName("Test Cargo 2") as IMyCargoContainer;
        }

        public void Main(string argument, UpdateType updateSource)
        {
            if ((updateSource & UpdateType.Terminal) == UpdateType.Terminal)
            {
                var invFrom = _cargo1.GetInventory();
                var invTo = _cargo2.GetInventory();

                var result = invFrom.TransferItemTo(invTo, 0, amount:100);
                foreach (var item in invTo.GetItems())
                {
                    var defId = item.GetDefinitionId();
                    Echo(defId.SubtypeName);
                }
            }
        }
    }
}
