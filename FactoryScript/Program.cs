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
        IMyConveyorSorter _bay1SorterIn;
        IMyConveyorSorter _bay2SorterIn;
        IMyConveyorSorter _bay3SorterIn;
        IMyConveyorSorter _bay4SorterIn;

        IMyConveyorSorter _bay1SorterOut;
        IMyConveyorSorter _bay2SorterOut;
        IMyConveyorSorter _bay3SorterOut;
        IMyConveyorSorter _bay4SorterOut;

        IMySensorBlock _bay1Sensor;
        IMySensorBlock _bay2Sensor;
        IMySensorBlock _bay3Sensor;
        IMySensorBlock _bay4Sensor;

        IMyTextPanel _bay1LCD;
        IMyTextPanel _bay2LCD;
        IMyTextPanel _bay3LCD;
        IMyTextPanel _bay4LCD;

        IMyShipConnector _bay1Connector;
        IMyShipConnector _bay2Connector;
        IMyShipConnector _bay3Connector;
        IMyShipConnector _bay4Connector;

        List<IMyTerminalBlock> _outSorters;
        List<IMyTerminalBlock> _inSorters;

        bool _bay1HasShip;
        bool _bay2HasShip;
        bool _bay3HasShip;
        bool _bay4HasShip;

        IMyTextPanel _logOutput;
        int _currentBay;

        public Program()
        {
            // It's recommended to set RuntimeInfo.UpdateFrequency 
            _logOutput = GridTerminalSystem.GetBlockWithName("Bay Debug LCD") as IMyTextPanel;
            Echo = EchoToLCD;            
            ClearEcho();
            
            Echo($"Booting up at {DateTime.Now}");
            _outSorters = new List<IMyTerminalBlock>();
            _inSorters = new List<IMyTerminalBlock>();

            _bay1SorterIn = GridTerminalSystem.GetBlockWithName("Bay 1 Sorter In") as IMyConveyorSorter;
            _bay2SorterIn = GridTerminalSystem.GetBlockWithName("Bay 2 Sorter In") as IMyConveyorSorter;
            _bay3SorterIn = GridTerminalSystem.GetBlockWithName("Bay 3 Sorter In") as IMyConveyorSorter;
            _bay4SorterIn = GridTerminalSystem.GetBlockWithName("Bay 4 Sorter In") as IMyConveyorSorter;

            _bay1SorterOut = GridTerminalSystem.GetBlockWithName("Bay 1 Sorter Out") as IMyConveyorSorter;
            _bay2SorterOut = GridTerminalSystem.GetBlockWithName("Bay 2 Sorter Out") as IMyConveyorSorter;
            _bay3SorterOut = GridTerminalSystem.GetBlockWithName("Bay 3 Sorter Out") as IMyConveyorSorter;
            _bay4SorterOut = GridTerminalSystem.GetBlockWithName("Bay 4 Sorter Out") as IMyConveyorSorter;

            _bay1Sensor = GridTerminalSystem.GetBlockWithName("Bay 1 Sensor") as IMySensorBlock;
            _bay2Sensor = GridTerminalSystem.GetBlockWithName("Bay 2 Sensor") as IMySensorBlock;
            _bay3Sensor = GridTerminalSystem.GetBlockWithName("Bay 3 Sensor") as IMySensorBlock;
            _bay4Sensor = GridTerminalSystem.GetBlockWithName("Bay 4 Sensor") as IMySensorBlock;

            _bay1LCD = GridTerminalSystem.GetBlockWithName("Bay 1 LCD") as IMyTextPanel;
            _bay2LCD = GridTerminalSystem.GetBlockWithName("Bay 2 LCD") as IMyTextPanel;
            _bay3LCD = GridTerminalSystem.GetBlockWithName("Bay 3 LCD") as IMyTextPanel;
            _bay4LCD = GridTerminalSystem.GetBlockWithName("Bay 4 LCD") as IMyTextPanel;

            _bay1Connector = GridTerminalSystem.GetBlockWithName("Bay 1 Connector") as IMyShipConnector;
            _bay2Connector = GridTerminalSystem.GetBlockWithName("Bay 2 Connector") as IMyShipConnector;
            _bay3Connector = GridTerminalSystem.GetBlockWithName("Bay 3 Connector") as IMyShipConnector;
            _bay4Connector = GridTerminalSystem.GetBlockWithName("Bay 4 Connector") as IMyShipConnector;

            GridTerminalSystem.SearchBlocksOfName("Bay Out Sorter", _outSorters, door => door is IMyConveyorSorter);
            GridTerminalSystem.SearchBlocksOfName("Bay In Sorter", _inSorters, door => door is IMyConveyorSorter);

            _bay1HasShip = false;
            _bay2HasShip = false;
            _bay3HasShip = false;
            _bay4HasShip = false;
            Echo($"Found {_inSorters.Count} Inbound sorters and {_outSorters.Count} Outbound Sorters");            
        }       

        public void Main(string argument, UpdateType updateSource)
        {
            if (argument == string.Empty)
            {
                return;
            }

            switch (argument)
            {
                case "Bay1Button":
                    {
                        //var x = _bay
                        EnableBay(1);
                        break;
                    }

                case "Bay2Button":
                    {
                        EnableBay(2);
                        break;
                    }

                case "Bay3Button":
                    {
                        EnableBay(3);
                        break;
                    }

                case "Bay4Button":
                    {
                        EnableBay(4);
                        break;
                    }

                case "Sensor1Activate":
                    {
                        _bay1HasShip = true;
                        break;
                    }

                case "Sensor1Deactivate":
                    {
                        _bay1HasShip = false;
                        break;
                    }

                default:
                    {
                        Echo($"Unknown command: {argument}");
                        break;
                    }
            }
        }

        void DisableSorters()
        {
            _bay1SorterIn.ApplyAction("OnOff_Off");
            _bay2SorterIn.ApplyAction("OnOff_Off");
            _bay3SorterIn.ApplyAction("OnOff_Off");
            _bay4SorterIn.ApplyAction("OnOff_Off");
            _bay1SorterOut.ApplyAction("OnOff_Off");
            _bay2SorterOut.ApplyAction("OnOff_Off");
            _bay3SorterOut.ApplyAction("OnOff_Off");
            _bay4SorterOut.ApplyAction("OnOff_Off");




        }

        void EnableBay(int bay)
        {
            if (bay == 1)
            {
                _bay1SorterIn.ApplyAction("OnOff_On");
                _bay1SorterOut.ApplyAction("OnOff_On");
                _currentBay = 1;
            }
            else
            {
                _bay1SorterIn.ApplyAction("OnOff_Off");
                _bay1SorterOut.ApplyAction("OnOff_Off");
            }
            if (bay == 2)
            {
                _bay2SorterIn.ApplyAction("OnOff_On");
                _bay2SorterOut.ApplyAction("OnOff_On");
                _currentBay = 2;
            }
            else
            {
                _bay2SorterIn.ApplyAction("OnOff_Off");
                _bay2SorterOut.ApplyAction("OnOff_Off");
            }
            if (bay == 3)
            {
                _bay3SorterIn.ApplyAction("OnOff_On");
                _bay3SorterOut.ApplyAction("OnOff_On");
                _currentBay = 3;
            }
            else
            {
                _bay3SorterIn.ApplyAction("OnOff_Off");
                _bay3SorterOut.ApplyAction("OnOff_Off");
            }
            if (bay == 4)
            {
                _bay1SorterIn.ApplyAction("OnOff_On");
                _bay1SorterOut.ApplyAction("OnOff_On");
                _currentBay = 4;
            }
            else
            {
                _bay4SorterIn.ApplyAction("OnOff_Off");
                _bay4SorterOut.ApplyAction("OnOff_Off");
            }
        }

        void SetLightColor(List<IMyTerminalBlock> lights, Color color)
        {
            foreach (var light in lights)
            {
                light.SetValue<Color>("Color", color);
            }
            //door.ApplyAction("Open_On");
        }

        public void EchoToLCD(string text)
        {
            _logOutput?.WritePublicText($"{text}\n", true);
        }
        public void ClearEcho()
        {
            _logOutput?.WritePublicText(string.Empty, false);
        }
    }
}