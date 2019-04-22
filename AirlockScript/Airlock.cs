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
        List<IMyTerminalBlock> innerDoors;
        List<IMyTerminalBlock> outerDoors;

        List<IMyTerminalBlock> innerDoorLights;
        List<IMyTerminalBlock> outerDoorLights;

        //IMyAirVent mainVent;
        IMyAirVent purgeVent;

        IMyTextPanel _logOutput;
        //IMyTextPanel _logDebug;

        AirlockState airlockState;
        ButtonPressed buttonPressed;

        public Program()

        {

            // It's recommended to set RuntimeInfo.UpdateFrequency 
            // Replace the Echo
            Echo = this.EchoToLCD;

            // Fetch a log text panel
            this._logOutput = GridTerminalSystem.GetBlockWithName("Airlock LCD") as IMyTextPanel;

            this.innerDoors = new List<IMyTerminalBlock>();
            this.outerDoors = new List<IMyTerminalBlock>();

            this.innerDoorLights = new List<IMyTerminalBlock>();
            this.outerDoorLights = new List<IMyTerminalBlock>();

            //this.mainVent = GridTerminalSystem.GetBlockWithName("Airlock Air Vent Main") as IMyAirVent;
            this.purgeVent = GridTerminalSystem.GetBlockWithName("Airlock Air Vent Purge") as IMyAirVent;

            GridTerminalSystem.SearchBlocksOfName("Airlock Inner Door", this.innerDoors, door => door is IMyDoor);
            GridTerminalSystem.SearchBlocksOfName("Airlock Outer Door", this.outerDoors, door => door is IMyAirtightHangarDoor);

            GridTerminalSystem.SearchBlocksOfName("Airlock Inner Door Light", this.innerDoorLights, door => door is IMyInteriorLight);
            GridTerminalSystem.SearchBlocksOfName("Airlock Outer Door Light", this.outerDoorLights, door => door is IMyInteriorLight);

            this.airlockState = AirlockState.Unknown;
            this.buttonPressed = ButtonPressed.Unknown;
            //Echo(innerDoors.Count.ToString());

            var x = this.purgeVent.CanPressurize;
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



        public void Main(string argument, UpdateType updateSource)

        {

            // The main entry point of the script, invoked every time
            // one of the programmable block's Run actions are invoked,
            // or the script updates itself. The updateSource argument
            // describes where the update came from.
            // 
            // The method itself is required, but the arguments above
            // can be removed if not needed.

            if (argument == string.Empty)
            {
                return;
            }

            switch (argument)
            {
                case "InnerIn":
                    {
                        this.buttonPressed = ButtonPressed.InnerIn;
                        if (this.airlockState == AirlockState.Full)
                        {
                            this.ControlInnerDoors(true);
                        }
                        else
                        {
                            this.PresurizeAirlock();
                        }

                        break;
                    }

                case "InnerOut":
                    {
                        this.buttonPressed = ButtonPressed.InnerOut;
                        if (this.airlockState == AirlockState.Full)
                        {
                            this.ControlInnerDoors(true);
                        }
                        else
                        {
                            this.PresurizeAirlock();
                        }

                        break;
                    }

                case "OuterIn":
                    {
                        this.buttonPressed = ButtonPressed.OuterIn;
                        Echo($"OuterIn: {this.airlockState}");

                        if (this.airlockState == AirlockState.Empty)
                        {
                            this.ControlOuterDoors(true);
                        }
                        else
                        {
                            this.DepresurizeAirlock();
                        }

                        break;
                    }

                case "OuterOut":
                    {
                        Echo("OuterOut");

                        this.buttonPressed = ButtonPressed.OuterOut;
                        if (this.airlockState == AirlockState.Empty)
                        {
                            this.ControlOuterDoors(true);
                        }
                        else
                        {
                            this.DepresurizeAirlock();
                        }

                        break;
                    }

                case "AirPurgeFull":
                    {
                        this.airlockState = AirlockState.Full;
                        if (this.buttonPressed == ButtonPressed.InnerOut || this.buttonPressed == ButtonPressed.InnerIn)
                        {
                            this.ControlInnerDoors(true);
                        }

                        break;
                    }

                case "AirPurgeEmpty":
                    {
                        this.airlockState = AirlockState.Empty;
                        if (this.buttonPressed == ButtonPressed.OuterOut || this.buttonPressed == ButtonPressed.OuterIn)
                        {
                            Echo("Open-1");
                            this.ControlOuterDoors(true);
                        }

                        break;
                    }

                default:
                    {
                        Echo($"Unknown command: {argument}");
                        break;
                    }
            }
        }


        void ControlOuterDoors(bool open)
        {
            foreach (var door in outerDoors)
            {
                if (open)
                {
                    door.ApplyAction("Open_On");
                    this.SetLightColor(this.outerDoorLights, Color.Green);
                }
                else
                {
                    door.ApplyAction("Open_Off");
                    this.SetLightColor(this.outerDoorLights, Color.Red);
                }
            }
        }

        void ControlInnerDoors(bool open)
        {
            foreach (var door in innerDoors)
            {
                if (open)
                {
                    door.ApplyAction("Open_On");
                    this.SetLightColor(this.innerDoorLights, Color.Green);
                }
                else
                {
                    door.ApplyAction("Open_Off");
                    this.SetLightColor(this.innerDoorLights, Color.Red);
                }
            }
        }

        void SetLightColor(List<IMyTerminalBlock> lights, Color color)
        {
            foreach (var light in lights)
            {
                light.SetValue<Color>("Color", color);
            }
        }

        void PresurizeAirlock()
        {
            this.airlockState = AirlockState.Filling;

            this.ControlInnerDoors(false);
            this.ControlOuterDoors(false);


            //this.mainVent.ApplyAction("OnOff_Off");
            this.purgeVent.ApplyAction("Depressurize_Off");
            this.purgeVent.ApplyAction("OnOff_On");
        }

        void DepresurizeAirlock()
        {
            this.airlockState = AirlockState.Emptying;

            this.ControlInnerDoors(false);
            this.ControlOuterDoors(false);

            //this.mainVent.ApplyAction("OnOff_Off");
            this.purgeVent.ApplyAction("Depressurize_On");
            this.purgeVent.ApplyAction("OnOff_On");
        }

        public void EchoToLCD(string text)
        {
            _logOutput?.WritePublicText($"{text}\n", true);
        }

        enum AirlockState
        {
            Unknown,
            Empty,
            Emptying,
            Filling,
            Full
        }

        enum ButtonPressed
        {
            Unknown,
            InnerIn,
            InnerOut,
            OuterIn,
            OuterOut
        }
    }
}