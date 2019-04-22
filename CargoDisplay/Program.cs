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
using VRage.Utils;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        // ===================================================================================
        //                         CHANGE THESE AS NEEDED
        // ===================================================================================

        // Names of the 3 LCD screens
        readonly string LEFT_LCD = "Text panel Left";
        readonly string RIGHT_LCD = "Text panel Right";
        readonly string MIDDLE_LCD = "Text panel Middle";

        // Set this to true to show indivudual blocks instead of rollup
        readonly bool SHOW_FULL_LIST = false;

        // Display Settings
        readonly float FONT_SIZE = 1.1f;
        readonly string FONT_NAME = "DEBUG";
        readonly Color BACKGROUND_COLOR = Color.Blue;
        readonly Color FOREGROUND_COLOR = Color.White;
        
        // ===================================================================================
        //                         DO NOT TOUCH BELOW
        // ===================================================================================

        IMyTextPanel _leftLCD;
        IMyTextPanel _rightLCD;
        IMyTextPanel _middleLCD;

        SortedDictionary<string, float> _inventoryList;
        SortedDictionary<string, float> _containerList;
        SortedDictionary<string, float> _itemList;

        List<IMyTerminalBlock> _allItems;

        List<IMyTerminalBlock> _containers;
        List<IMyTerminalBlock> _cockpits;
        List<IMyTerminalBlock> _drills;
        List<IMyTerminalBlock> _collectors;
        List<IMyTerminalBlock> _reactors;
        List<IMyTerminalBlock> _sorters;
        List<IMyTerminalBlock> _connectors;
        List<IMyTerminalBlock> _ejectors;
        List<IMyTerminalBlock> _generators;
        List<IMyTerminalBlock> _tanks;
        List<IMyTerminalBlock> _others;

        bool _hasContainers;
        bool _hasCockpits;
        bool _hasDrills;
        bool _hasCollectors;
        bool _hasReactors;
        bool _hasSorters;
        bool _hasConnectors;
        bool _hasEjectors;
        bool _hasGenerators;
        bool _hasTanks;
        bool _hasOthers;

        int currentItem;

        public Program()
        {
            InitLists();
            currentItem = 0;

            DumpStats();
            
            // Find LCD Displays
            InitDisplays();

            if (_leftLCD == null)
            {
                Echo("No Left LCD!\n");
            }

            if (_rightLCD == null)
            {
                Echo("No Right LCD!\n");
            }

            if (_middleLCD == null)
            {
                Echo("No Middle LCD!\n");
            }

            Runtime.UpdateFrequency = UpdateFrequency.Update100;
        }

        private void DumpStats()
        {            
            Echo($"Containers: {_containers.Count}");
            Echo($"Cockpits: {_cockpits.Count}");
            Echo($"Drills: {_drills.Count}");
            Echo($"Collectors: {_collectors.Count}");
            Echo($"Reactors: {_reactors.Count}");
            Echo($"Sorters: {_sorters.Count}");
            Echo($"Connectors: {_connectors.Count}");
            Echo($"Ejectors: {_ejectors.Count}");
            Echo($"Generators: {_generators.Count}");
            Echo($"Tanks: {_tanks.Count}");
            Echo($"Others: {_others.Count}");
        }

        private void InitLists()
        {
            _allItems = new List<IMyTerminalBlock>();
            _containerList = new SortedDictionary<string, float>();
            _itemList = new SortedDictionary<string, float>();
            _inventoryList = new SortedDictionary<string, float>();

            _containers = new List<IMyTerminalBlock>();
            _cockpits = new List<IMyTerminalBlock>();
            _drills = new List<IMyTerminalBlock>();
            _collectors = new List<IMyTerminalBlock>();
            _reactors = new List<IMyTerminalBlock>();
            _sorters = new List<IMyTerminalBlock>();
            _connectors = new List<IMyTerminalBlock>();
            _ejectors = new List<IMyTerminalBlock>();
            _generators = new List<IMyTerminalBlock>();
            _tanks = new List<IMyTerminalBlock>();
            _others = new List<IMyTerminalBlock>();

            _hasContainers = false;
            _hasCockpits = false;
            _hasDrills = false;
            _hasCollectors = false;
            _hasReactors = false;
            _hasSorters = false;
            _hasConnectors = false;
            _hasEjectors = false;
            _hasGenerators = false;
            _hasTanks = false;

            // Get all grid items that can have an inventory and are on the same grid as the P.B.
            GridTerminalSystem.GetBlocksOfType(_allItems, cargo => cargo.HasInventory & cargo.CubeGrid.EntityId == Me.CubeGrid.EntityId);

            foreach (var block in _allItems)
            {
                SortCounters(block);
            }
        }

        private void InitDisplays()
        {
            _leftLCD = GridTerminalSystem.GetBlockWithName(LEFT_LCD) as IMyTextPanel;
            _rightLCD = GridTerminalSystem.GetBlockWithName(RIGHT_LCD) as IMyTextPanel;
            _middleLCD = GridTerminalSystem.GetBlockWithName(MIDDLE_LCD) as IMyTextPanel;

            _middleLCD.ShowPublicTextOnScreen();
            _leftLCD.ShowPublicTextOnScreen();
            _rightLCD.ShowPublicTextOnScreen();

            _leftLCD.BackgroundColor = BACKGROUND_COLOR;
            _rightLCD.BackgroundColor = BACKGROUND_COLOR;
            _middleLCD.BackgroundColor = BACKGROUND_COLOR;

            _leftLCD.FontColor = FOREGROUND_COLOR;
            _rightLCD.FontColor = FOREGROUND_COLOR;
            _middleLCD.FontColor = FOREGROUND_COLOR;

            _leftLCD.Font = FONT_NAME;
            _rightLCD.Font = FONT_NAME;
            _middleLCD.Font = FONT_NAME;

            _leftLCD.FontSize = FONT_SIZE;
            _rightLCD.FontSize = FONT_SIZE;
            _middleLCD.FontSize = FONT_SIZE;
        }

        public void Main(string argument, UpdateType updateSource)
        {
            switch (updateSource)
            {
                case UpdateType.Trigger:
                    {
                        switch (argument)
                        {
                            case "ButtonUp":
                                {
                                    if (currentItem > 0)
                                    {
                                        currentItem--;
                                    }

                                    break;
                                }
                            case "ButtonDown":
                                {
                                    if (currentItem < (_allItems.Count - 1))
                                    {
                                        currentItem++;
                                    }

                                    break;
                                }
                        }

                        break;
                    }

                case UpdateType.Terminal:
                    {
                        Echo($"Found Blocks: {_allItems.Count}");
                        break;
                    }

                case UpdateType.Update100:
                    {                                             
                        _inventoryList.Clear();
                        _itemList.Clear();
                        _containerList.Clear();
                        ResetCounters();
                                                
                        foreach (IMyTerminalBlock block in _allItems)
                        {
                            var inv = block.GetInventory();
                            

                            foreach (var item in inv.GetItems())
                            {
                                var defId = item.GetDefinitionId();

                                var subtype = item.Content.SubtypeId.ToString();
                                if (_inventoryList.ContainsKey(defId.SubtypeName))
                                {
                                    _inventoryList[defId.SubtypeName] += (float)item.Amount.RawValue;
                                }
                                else
                                {
                                    _inventoryList.Add(defId.SubtypeName, (float)item.Amount.RawValue);
                                }
                            }                            

                            if (block.EntityId == _allItems[currentItem].EntityId)
                            {                                
                                foreach (var item in inv.GetItems())
                                {
                                    
                                }
                            }

                            if (block.ShowInInventory)
                            {
                                float per = ((float)inv.CurrentVolume.RawValue / (float)inv.MaxVolume.RawValue) * 100;
                                _containerList.Add(block.CustomName, per);
                            }
                            
                        }

                        EchoL("Containers:", false);
                        foreach (var container in _containerList)
                        {                            
                            EchoL($"    {container.Key}: {container.Value:F1}% full");
                        }

                        EchoR("Full Inventory:", false);
                        foreach (var inventory in _inventoryList)
                        {
                            float value = (float)inventory.Value / 1000000;
                            EchoR($"    {inventory.Key}:  {value:F}");
                        }

                        EchoM(DisplayCounters(), false);
                        break;
                    }
            }
        }

        void EchoL(string text, bool append = true)
        {
            _leftLCD?.WritePublicText($"{text}\n", append);
        }

        void EchoR(string text, bool append = true)
        {
            _rightLCD?.WritePublicText($"{text}\n", append);
        }

        void EchoM(string text, bool append = true)
        {
            _middleLCD?.WritePublicText($"{text}\n", append);
        }

        string FormatCounters(List<IMyTerminalBlock> blocks, string blockName, bool checkGas = false)
        {
            string results = string.Empty;
            float volume = 0f;
            float capacity = 0f;
            int count = 0;

            count = blocks.Count;

            if (checkGas)
            {
                foreach (var block in blocks)
                {
                    var tank = block as IMyGasTank;

                volume += (float)(tank.Capacity * tank.FilledRatio);
                //Echo(tank.FilledRatio.ToString());
                capacity += tank.Capacity;
                }
            }
            else
            {
                foreach (var block in blocks)
                {
                    volume += (float)block.GetInventory().CurrentVolume.RawValue;
                    capacity += (float)block.GetInventory().MaxVolume.RawValue;
                }
            }
            results += $"  {blockName}: ({count}) {volume / capacity:P1} full\n";
            results += $"    {volume:0,0.00}L / {capacity:0,0.00}L\n";

            return results;
        }

        string DisplayCounters()
        {
            string result = string.Empty;

            if (_hasContainers)
                result += FormatCounters(_containers, "Cargo Containers");
            if (_hasCockpits)
                result += FormatCounters(_cockpits, "Cockpits");
            if (_hasDrills)
                result += FormatCounters(_drills, "Drills");
            if (_hasCollectors)
                result += FormatCounters(_collectors, "Collectors");
            if (_hasReactors)
                result += FormatCounters(_reactors, "Reactors");
            if (_hasSorters)
                result += FormatCounters(_sorters, "Sorters");
            if (_hasConnectors)
                result += FormatCounters(_connectors, "Connectors");
            if (_hasEjectors)
                result += FormatCounters(_ejectors, "Ejectors");
            if (_hasGenerators)
                result += FormatCounters(_generators, "Generators");
            if (_hasTanks)
                result += FormatCounters(_tanks, "Tanks", true);
            if (_hasOthers)
                result += FormatCounters(_others, "Others");
            return result;
        }

        void SortCounters(IMyTerminalBlock block)
        {
            if (block is IMyCargoContainer)
            {
                _containers.Add(block as IMyCargoContainer);
                _hasContainers = true;
                return;
            }

            if (block is IMyCockpit)
            {
                _cockpits.Add(block as IMyCockpit);
                _hasCockpits = true;
                return;
            }

            if (block is IMyShipDrill)
            {
                _drills.Add(block as IMyShipDrill);
                _hasDrills = true;
                return;
            }

            if (block is IMyCollector)
            {
                _collectors.Add(block as IMyCollector);
                _hasCollectors = true;
                return;
            }

            if (block is IMyReactor)
            {
                _reactors.Add(block as IMyReactor);
                _hasReactors = true;
                return;
            }

            if (block is IMyConveyorSorter)
            {
                _sorters.Add(block as IMyConveyorSorter);
                _hasSorters = true;
                return;
            }

            if (block is IMyShipConnector)
            {
                Echo(block.BlockDefinition.SubtypeId);
                if (block.BlockDefinition.SubtypeId == "ConnectorSmall")
                {
                    _ejectors.Add(block);
                    _hasEjectors = true;
                }

                if (block.BlockDefinition.SubtypeId == "ConnectorMedium")
                {
                    _connectors.Add(block);
                    _hasConnectors = true;
                }
                return;
            }

            if (block is IMyGasGenerator)
            {
                _generators.Add(block as IMyGasGenerator);
                _hasGenerators = true;
                return;
            }

            if (block is IMyGasTank)
            {
                _tanks.Add(block as IMyGasTank);
                _hasTanks = true;
                return;
            }

            _others.Add(block);
            _hasOthers = true;
        }
        
        void ResetCounters()
        {
        
        }
    }
}