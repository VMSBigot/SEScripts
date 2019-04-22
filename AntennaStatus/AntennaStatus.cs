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
        // This code monitors a block for a change in a status we care about then
        // send a message along to another P.B. to act on. 

        List<IMyTerminalBlock> _laser1LCDs;
        List<IMyTerminalBlock> _laser2LCDs;
        List<IMyTerminalBlock> _laser3LCDs;
        List<IMyTerminalBlock> _laser4LCDs;
        List<IMyTerminalBlock> _laser5LCDs;
        List<IMyTerminalBlock> _laser6LCDs;
        List<IMyTerminalBlock> _laser7LCDs;

        IMyLaserAntenna _laserAntenna1;
        IMyLaserAntenna _laserAntenna2;
        IMyLaserAntenna _laserAntenna3;
        IMyLaserAntenna _laserAntenna4;
        IMyLaserAntenna _laserAntenna5;
        IMyLaserAntenna _laserAntenna6;
        IMyLaserAntenna _laserAntenna7;

        bool _oldStatus1;
        bool _oldStatus2;
        bool _oldStatus3;
        bool _oldStatus4;
        bool _oldStatus5;
        bool _oldStatus6;
        bool _oldStatus7;

        public Program()
        {
            _laser1LCDs = new List<IMyTerminalBlock>();
            _laser2LCDs = new List<IMyTerminalBlock>();
            _laser3LCDs = new List<IMyTerminalBlock>();
            _laser4LCDs = new List<IMyTerminalBlock>();
            _laser5LCDs = new List<IMyTerminalBlock>();
            _laser6LCDs = new List<IMyTerminalBlock>();
            _laser7LCDs = new List<IMyTerminalBlock>();

            GridTerminalSystem.SearchBlocksOfName("LCD Panel 1-", _laser1LCDs, lcd => lcd is IMyTextPanel);
            GridTerminalSystem.SearchBlocksOfName("LCD Panel 2-", _laser2LCDs, lcd => lcd is IMyTextPanel);
            GridTerminalSystem.SearchBlocksOfName("LCD Panel 3-", _laser3LCDs, lcd => lcd is IMyTextPanel);
            GridTerminalSystem.SearchBlocksOfName("LCD Panel 4-", _laser4LCDs, lcd => lcd is IMyTextPanel);
            GridTerminalSystem.SearchBlocksOfName("LCD Panel 5-", _laser5LCDs, lcd => lcd is IMyTextPanel);
            GridTerminalSystem.SearchBlocksOfName("LCD Panel 6-", _laser6LCDs, lcd => lcd is IMyTextPanel);
            GridTerminalSystem.SearchBlocksOfName("LCD Panel 7-", _laser7LCDs, lcd => lcd is IMyTextPanel);

            _laserAntenna1 = GridTerminalSystem.GetBlockWithName("Moon to Space Port Relay") as IMyLaserAntenna;
            _laserAntenna2 = GridTerminalSystem.GetBlockWithName("Moon to Earth Relay") as IMyLaserAntenna;
            _laserAntenna3 = GridTerminalSystem.GetBlockWithName("Moon to Chrono Relay") as IMyLaserAntenna;
            _laserAntenna4 = GridTerminalSystem.GetBlockWithName("Moon to Mars Relay") as IMyLaserAntenna;
            _laserAntenna5 = GridTerminalSystem.GetBlockWithName("Moon to Titan Relay") as IMyLaserAntenna;
            _laserAntenna6 = GridTerminalSystem.GetBlockWithName("Moon to Alien Relay") as IMyLaserAntenna;
            _laserAntenna7 = GridTerminalSystem.GetBlockWithName("Moon to Alien Moon Relay") as IMyLaserAntenna;

            Runtime.UpdateFrequency = UpdateFrequency.Update100;

            _oldStatus1 = _laserAntenna1.Status == MyLaserAntennaStatus.Connected;
            _oldStatus2 = _laserAntenna2.Status == MyLaserAntennaStatus.Connected;
            _oldStatus3 = _laserAntenna3.Status == MyLaserAntennaStatus.Connected;
            _oldStatus4 = _laserAntenna4.Status == MyLaserAntennaStatus.Connected;
            _oldStatus5 = _laserAntenna5.Status == MyLaserAntennaStatus.Connected;
            _oldStatus6 = _laserAntenna6.Status == MyLaserAntennaStatus.Connected;
            _oldStatus7 = _laserAntenna7.Status == MyLaserAntennaStatus.Connected;
        }
        public void Main(string argument, UpdateType updateType)
        {
            if ((updateType & UpdateType.Update100) == UpdateType.Update100)
            {
                var status1 = _laserAntenna1.Status == MyLaserAntennaStatus.Connected;
                var status2 = _laserAntenna2.Status == MyLaserAntennaStatus.Connected;
                var status3 = _laserAntenna3.Status == MyLaserAntennaStatus.Connected;
                var status4 = _laserAntenna4.Status == MyLaserAntennaStatus.Connected;
                var status5 = _laserAntenna5.Status == MyLaserAntennaStatus.Connected;
                var status6 = _laserAntenna6.Status == MyLaserAntennaStatus.Connected;
                var status7 = _laserAntenna7.Status == MyLaserAntennaStatus.Connected;

                if (_oldStatus1 == status1)
                {
                    SetColor(_laser1LCDs, status1);
                }
                if (_oldStatus2 == status2)
                {
                    SetColor(_laser2LCDs, status2);
                }
                if (_oldStatus3 == status3)
                {
                    SetColor(_laser3LCDs, status3);
                }
                if (_oldStatus4 == status4)
                {
                    SetColor(_laser4LCDs, status4);
                }
                if (_oldStatus5 == status5)
                {
                    SetColor(_laser5LCDs, status5);
                }
                if (_oldStatus6 == status6)
                {
                    SetColor(_laser6LCDs, status6);
                }
                if (_oldStatus7 == status7)
                {
                    SetColor(_laser7LCDs, status7);
                }

                _oldStatus1 = status1;
                _oldStatus2 = status2;
                _oldStatus3 = status3;
                _oldStatus4 = status4;
                _oldStatus5 = status5;
                _oldStatus6 = status6;
                _oldStatus7 = status7;
            }

            if ((updateType & UpdateType.Terminal) == UpdateType.Terminal)
            {
                if (argument == string.Empty)
                {
                    DumpState();
                    return;
                }
            }
        }
        
        void SetColor(List<IMyTerminalBlock> blocks, bool status)
        {
            foreach (IMyTextPanel block in blocks)
            {
                if (status)
                    block.BackgroundColor = Color.Green;
                else
                    block.BackgroundColor = Color.Red;
            }
        }

        void DumpState()
        {
            var output = string.Empty;
            output += "Found:\n";
            output += $"LCD1: {_laser1LCDs.Count}\n";
            output += $"LCD2: {_laser2LCDs.Count}\n";
            output += $"LCD3: {_laser3LCDs.Count}\n";
            output += $"LCD4: {_laser4LCDs.Count}\n";
            output += $"LCD5: {_laser5LCDs.Count}\n";
            output += $"LCD6: {_laser6LCDs.Count}\n";
            output += $"LCD7: {_laser7LCDs.Count}\n";

            output += $"Antenna1: {!(_laserAntenna1 == null)}\n";
            output += $"Antenna2: {!(_laserAntenna2 == null)}\n";
            output += $"Antenna3: {!(_laserAntenna3 == null)}\n";
            output += $"Antenna4: {!(_laserAntenna4 == null)}\n";
            output += $"Antenna5: {!(_laserAntenna5 == null)}\n";
            output += $"Antenna6: {!(_laserAntenna6 == null)}\n";
            output += $"Antenna7: {!(_laserAntenna7 == null)}\n";
            Echo(output);
        }
    }
}