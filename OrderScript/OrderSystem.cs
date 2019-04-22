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
        readonly string LEFT_LCD = "Order Left LCD";
        readonly string RIGHT_LCD = "Order Right LCD";
        readonly string MIDDLE_LCD = "Order Middle LCD";
        readonly string DEBUG_LCD = "Order Debug LCD";

        // Display Settings
        readonly float FONT_SIZE = 1.1f;
        readonly string FONT_NAME = "DEBUG";
        readonly Color BACKGROUND_COLOR = Color.Black;
        readonly Color FOREGROUND_COLOR = Color.White;
        readonly int SCREEN_WIDTH = 36;
        // ===================================================================================
        //                         DO NOT TOUCH BELOW
        // ===================================================================================

        // Blocks we care about
        IMyCockpit _cockpit;
        IMyAssembler _assembler;
        IMyShipConnector _bay1Connector;
        IMyShipConnector _bay2Connector;
        IMyShipConnector _bay3Connector;
        IMyShipConnector _bay4Connector;

        IMyCargoContainer _oreCargo;
        IMyCargoContainer _ignotCargo;
        IMyCargoContainer _materialCargo;

        bool init = false;

        // Stuff that is per-page
        bool[][] _buttonEnabled;
        string[][] _buttonText;
        PressedButton[][] _buttonData;
        
        public Program()
        {
            _buttonEnabled = new bool[2][];
            _buttonEnabled[0] = new bool[9];
            _buttonEnabled[1] = new bool[9];

            _buttonText = new string[2][];
            _buttonText[0] = new string[9];
            _buttonText[1] = new string[9];

            _buttonData = new PressedButton[2][];
            _buttonData[0] = new PressedButton[9];
            _buttonData[1] = new PressedButton[9];

            _oreStatus = new MaterialStatus();
            _ignotStatus = new MaterialStatus();

            // Find LCD Displays
            if (!InitDisplays())
            {
                Echo("ERROR ON STARTUP: LCD ERROR");
                return;
            }

            // Get Blocks we care about
            _cockpit = GridTerminalSystem.GetBlockWithName("ACockpit") as IMyCockpit;
            _assembler = GridTerminalSystem.GetBlockWithName("Assembler") as IMyAssembler;

            _oreCargo = GridTerminalSystem.GetBlockWithName("Ore Cargo Bin") as IMyCargoContainer;
            _ignotCargo = GridTerminalSystem.GetBlockWithName("Ignot Cargo Bin") as IMyCargoContainer;
            _materialCargo = GridTerminalSystem.GetBlockWithName("Material Cargo Bin") as IMyCargoContainer;

            _bay1Connector = GridTerminalSystem.GetBlockWithName("Bay 1 Connector") as IMyShipConnector;
            _bay2Connector = GridTerminalSystem.GetBlockWithName("Bay 2 Connector") as IMyShipConnector;
            _bay3Connector = GridTerminalSystem.GetBlockWithName("Bay 3 Connector") as IMyShipConnector;
            _bay4Connector = GridTerminalSystem.GetBlockWithName("Bay 4 Connector") as IMyShipConnector;

            ////Runtime.UpdateFrequency = UpdateFrequency.Update100;
            init = true;

            Echo("Startup complete");

            StartWizard();
        }


        // Main Entry point
        public void Main(string argument, UpdateType updateSource)
        {
            if (!init)
                return;

            switch (updateSource)
            {
                case UpdateType.Trigger:
                    {
                        HandleTrigger(argument);

                        break;
                    }

                case UpdateType.Terminal:
                    {
                        Echo("Running...");
                        break;
                    }

                case UpdateType.Update100:
                    {
                        break;
                    }
            }
        }

        void SetButton(int button, bool enabled, string text, PressedButton pressedButton, int row = 0)
        {
            _buttonEnabled[row][button - 1] = enabled;
            _buttonText[row][button - 1] = text;
            _buttonData[row][button - 1] = pressedButton;
        }

        void ResetButtons()
        {           
            _buttonEnabled[0] = new bool[9];
            _buttonEnabled[1] = new bool[9];           
            _buttonText[0] = new string[9];
            _buttonText[1] = new string[9];          
            _buttonData[0] = new PressedButton[9];
            _buttonData[1] = new PressedButton[9];
        }

        PressedButton DecodeButton(int button, int row = 0)
        {
            if (_buttonEnabled[row][button -1])
            {
                return _buttonData[row][button - 1];
            }

            return PressedButton.None;
        }

        private void HandleTrigger(string argument)
        {
            switch (argument)
            {
                case "Button1":
                    {
                        //EchoD(argument);
                        HandleButton(DecodeButton(1));
                        break;
                    }
                case "Button2":
                    {
                        //EchoD(argument);
                        HandleButton(DecodeButton(2));
                        break;
                    }

                case "Button3":
                    {
                        //EchoD(argument);
                        HandleButton(DecodeButton(3));
                        break;
                    }

                case "Button4":
                    {
                        //EchoD(argument);
                        HandleButton(DecodeButton(4));
                        break;
                    }

                case "Button5":
                    {
                        //EchoD(argument);
                        HandleButton(DecodeButton(5));
                        break;
                    }

                case "Button6":
                    {
                        //EchoD(argument);
                        HandleButton(DecodeButton(6));
                        break;
                    }

                case "Button7":
                    {
                        //EchoD(argument);
                        HandleButton(DecodeButton(7));
                        break;
                    }

                case "Button8":
                    {
                        //EchoD(argument);
                        HandleButton(DecodeButton(8));
                        break;
                    }

                case "Button9":
                    {
                        //EchoD(argument);
                        HandleButton(DecodeButton(9));
                        break;
                    }

                case "Button1Row2":
                    {
                        //EchoD(argument);
                        HandleButton(DecodeButton(1, 1));
                        break;
                    }
                case "Button2Row2":
                    {
                        //EchoD(argument);
                        HandleButton(DecodeButton(2, 1));
                        break;
                    }

                case "Button3Row2":
                    {
                        //EchoD(argument);
                        HandleButton(DecodeButton(3, 1));
                        break;
                    }

                case "Button4Row2":
                    {
                        //EchoD(argument);
                        HandleButton(DecodeButton(4, 1));
                        break;
                    }

                case "Button5Row2":
                    {
                        //EchoD(argument);
                        HandleButton(DecodeButton(5, 1));
                        break;
                    }

                case "Button6Row2":
                    {
                        //EchoD(argument);
                        HandleButton(DecodeButton(6, 1));
                        break;
                    }

                case "Button7Row2":
                    {
                        //EchoD(argument);
                        HandleButton(DecodeButton(7, 1));
                        break;
                    }

                case "Button8Row2":
                    {
                        //EchoD(argument);
                        HandleButton(DecodeButton(8, 1));
                        break;
                    }

                case "Button9Row2":
                    {
                        //EchoD(argument);
                        HandleButton(DecodeButton(9,1 ));
                        break;
                    }

                case "SensorOn":
                    {
                        EchoD(argument);
                        break;
                    }

                case "SensorOff":
                    {
                        EchoD(argument);
                        break;
                    }
                case "ClearScreen":
                    {
                        EchoL(string.Empty, false);
                        EchoM(string.Empty, false);
                        EchoR(string.Empty, false);
                        EchoD(string.Empty, false);
                        break;
                    }
            }
        }

        void DisplayButtons()
        {
            var result = "  Button Setup:\n";
            result += "  ======================\n";

            for (int i=0; i < 9; i++)
            {
                if (_buttonEnabled[0][i])
                {
                    result += $" {i + 1})  {_buttonText[0][i]}\n";
                }
                else
                {
                    result += $" {i + 1}) \n";
                }
            }

            result += "  ======================\n";

            EchoL(result, false);
        }

        void DoRefine()
        {
            var destBoxInventory = _materialCargo.GetInventory();
            var remainAmount = _oreToRefine;

            //            destBoxInventory.TransferItemFrom
        }
    }
}
