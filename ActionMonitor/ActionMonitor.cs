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

        IMyProgrammableBlock _targetProgramableBlock;
        IMyTerminalBlock _monitoredBlock;
        BlockType _blockType;
        BlockState _oldState;
        int _ticks;

        // This will go away at some point
        const string _targetProgramableBlockName = "Bank Computer";
        const string _monitoredBlockName = "Bank Cockpit";

        public Program()
        {
            _targetProgramableBlock = GridTerminalSystem.GetBlockWithName(_targetProgramableBlockName) as IMyProgrammableBlock;
            _monitoredBlock = GridTerminalSystem.GetBlockWithName(_monitoredBlockName) as IMyTerminalBlock;

            _blockType = GetBlockType(_monitoredBlock);
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
        }

        public void Save()
        {
            // Called when the program needs to save its state. Use
            // this method to save your state to the Storage field
            // or some other means. 
            // 
            // This method is optional and can be removed if not
            // needed.
        }

        public void Main(string argument, UpdateType updateType)
        {
            // Handle API Calls
            if ((updateType & UpdateType.Script) == UpdateType.Script) 
            {
                if (argument != string.Empty)
                {
                    Echo("Handling remote API call\n");
                    ParseAPICall(argument);
                }

                return;
            }

            if ((updateType & UpdateType.Update100) == UpdateType.Update100)
            {
                _ticks++;

                var currentState = CheckState(_monitoredBlock, _blockType);
                if (currentState != _oldState)
                {
                    Echo("State change!\n");
                    SendStateChange(_targetProgramableBlock, _targetProgramableBlockName, currentState);
                    _oldState = currentState;
                    return;
                }
            }

            if ((updateType & UpdateType.Terminal) == UpdateType.Terminal)
            {
                if (argument == string.Empty)
                {
                    DumpState();
                    return;
                }
            }

            if ((updateType & UpdateType.Antenna) == UpdateType.Antenna)
            {

            }
        }

        void SendStateChange(IMyProgrammableBlock destination, string blockName, BlockState state)
        {
            var message = $"{blockName}_{state}";
            destination.TryRun(message);
        }

        void ParseAPICall(string input)
        {

        }

        BlockState CheckState(IMyTerminalBlock terminalBlock, BlockType blockType)
        {
            switch (blockType)
            {
                case BlockType.Cockpit:
                    var cockpit = terminalBlock as IMyCockpit;
                    ////List<ITerminalAction> actions = new List<ITerminalAction>();
                    ////cockpit.GetActions(actions);
                    ////foreach (var action in actions)
                    ////{
                    ////    Echo(action.Name.ToString());
                    ////}
                    if (cockpit.IsUnderControl)
                        return BlockState.On;
                    else
                        return BlockState.Off;

                case BlockType.Connector:
                    var connector = terminalBlock as IMyShipConnector;
                    if (connector.Status == MyShipConnectorStatus.Connected)
                        return BlockState.On;
                    else
                        return BlockState.Off;

                case BlockType.Door:
                    var door = terminalBlock as IMyDoor;
                    if (door.Status == DoorStatus.Open)
                        return BlockState.On;
                    else if (door.Status == DoorStatus.Open)
                        return BlockState.Off;
                    else
                        return BlockState.Between;

                case BlockType.Piston:
                    var piston = terminalBlock as IMyPistonBase;
                    if (piston.Status == PistonStatus.Extended)
                        return BlockState.On;
                    else if (piston.Status == PistonStatus.Retracted)
                        return BlockState.Off;
                    else
                        return BlockState.Between;

                // Basic/default
                case BlockType.Basic:
                default:
                    if (terminalBlock.GetValueBool("OnOff"))
                        return BlockState.On;
                    else
                        return BlockState.Off;
            }
        }

        BlockType GetBlockType(IMyTerminalBlock block)
        {
            if (_monitoredBlock is IMyCockpit)
            {
                return BlockType.Cockpit;
            }
            if (_monitoredBlock is IMyDoor)
            {
                return BlockType.Door;
            }
            if (_monitoredBlock is IMyShipConnector)
            {
                return BlockType.Connector;
            }
            if (_monitoredBlock is IMyPistonBase)
            {
                return BlockType.Piston;
            }

            return BlockType.Basic;
        }

        void DumpState()
        {
            var output = string.Empty;
            if (_targetProgramableBlock == null)
            {
                output += $"No destination block\n";
            }
            else
            {
                output += $"Found destination block\n";
            }

            if (_monitoredBlock == null)
            {
                output += $"No monitored block\n";

            }
            else
            {
                output += $"Found monitored block\n";

                output += $"Name: {_monitoredBlockName}\n";
                output += $"Type: {_blockType}\n";
                output += $"State: {_oldState}\n";
                output += $"Ticks: {_ticks}\n";
            }

            var cockpit = _monitoredBlock as IMyCockpit;
            List<ITerminalAction> actions = new List<ITerminalAction>();
            cockpit.GetActions(actions);
            foreach (var action in actions)
            {
                output += $"Action: {action.Name}\n";
            }
            //output += $"\n";
            Echo(output);
        }

        MonitorInfo AddMonitoredBlock(string blockName, string destinationPB, string baseName)
        {
            var result = new MonitorInfo
            {
                Name = blockName,
                DestinationPB = GridTerminalSystem.GetBlockWithName(destinationPB) as IMyProgrammableBlock,
                Block = GridTerminalSystem.GetBlockWithName(blockName) as IMyTerminalBlock
            };
            result.BlockType = GetBlockType(result.Block);
            result.State = CheckState(result.Block, result.BlockType);
            result.BaseName = baseName;

            return result;
        }


        ////MonitorInfo AddMonitoredBlock(string blockName, string destinationPB, string onMessage, string offMessage)
        ////{
        ////    var result = new MonitorInfo();

        ////    result.Name = blockName;
        ////    result.DestinationPB = GridTerminalSystem.GetBlockWithName(destinationPB) as IMyProgrammableBlock;
        ////    result.Block = GridTerminalSystem.GetBlockWithName(blockName) as IMyTerminalBlock;
        ////    result.BlockType = GetBlockType(result.Block);
        ////    result.State = CheckState(result.Block, result.BlockType);
        ////    result.CustomMessages = true;
        ////    result.OnMessage = onMessage;
        ////    result.OffMessage = offMessage;

        ////    return result;
        ////}

        readonly string[] ActionList = new string[4] { "On", "Closing", "Opening", "Off" };        

        struct MonitorInfo
        {
            public string Name;
            public IMyTerminalBlock Block;
            public BlockType BlockType;
            public BlockState State;
            public IMyProgrammableBlock DestinationPB;            
            public string BaseName;
        }

        enum BlockType
        {
            Basic,
            Cockpit,
            Connector,
            Door,
            Piston,
            Rotor,
            LaserAntenna,
            Assembler,
            Refinery
        }

        enum BlockState
        {
            On, Off, Between
        }
    }
}