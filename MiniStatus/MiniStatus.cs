// ReSharper disable StyleCop.SA1200
// ReSharper disable RedundantUsingDirective
// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Sean Lawrence" file="MiniStatus.cs">
//   2018 Sean M Lawrence
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;

using SpaceEngineers.Game.ModAPI.Ingame;

using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Utils;

using VRageMath;

namespace IngameScript
{
    /// <summary>
    /// The program.
    /// </summary>
    public partial class Program : MyGridProgram
    {
        // ===================================================================================
        // CHANGE THESE AS NEEDED
        // ===================================================================================
        
        /// <summary> The name of the cargo display </summary>
        private const string CargoName = "Cargo";

        /// <summary> The name of the ice display </summary>
        private const string IceName = "Ice";

        /// <summary> The name of the fuel display </summary>
        private const string FuelName = "Fuel";
        
        /// <summary> The font size. </summary>
        private const float FontSize = 2f;

        /// <summary> The font name. </summary>
        //private const string FontName = "DEBUG";

        /// <summary>  The foreground color. </summary>
        private readonly Color foregroundColor = Color.White;

        readonly Color GOOD_COLOR = Color.Black;
        readonly Color WARN_COLOR = Color.Blue;
        readonly Color BAD_COLOR = Color.Red;

        // H2 Thresholds
        const float H2_HI = 0.50f;
        const float H2_LOW = 0.25f;

        // Ice Thresholds        
        const float ICE_HI = 90000f;
        const float ICE_LOW = 50000f;

        // Inventory Thresholds
        const float INV_HI = 3000f;
        const float INV_LO = 1000f;
                
        // ===================================================================================
        // DO NOT TOUCH BELOW
        // ===================================================================================
        private IMyTextPanel cargoLCD;

        private IMyTextPanel iceLCD;

        private IMyTextPanel fuelLCD;

        private List<IMyTerminalBlock> allItems;

        public Program()
        {
            // Find LCD Displays
            this.InitDisplays();

            if (this.cargoLCD == null)
            {
                this.Echo("No Left LCD!\n");
            }

            if (this.iceLCD == null)
            {
                this.Echo("No Right LCD!\n");
            }

            if (this.fuelLCD == null)
            {
                this.Echo("No Middle LCD!\n");
            }

            this.allItems = new List<IMyTerminalBlock>();
            this.GridTerminalSystem.GetBlocksOfType(this.allItems, cargo => cargo.HasInventory & cargo.CubeGrid.EntityId == this.Me.CubeGrid.EntityId);
            this.Runtime.UpdateFrequency = UpdateFrequency.Update100;
        }

        public void Main(string argument, UpdateType updateSource)
        {
            switch (updateSource)
            {
                case UpdateType.Trigger:
                    break;

                case UpdateType.Terminal:
                    this.Echo($"Found Blocks: {this.allItems.Count}");
                    break;

                case UpdateType.Update100:
                    this.HandleUpdate();
                    break;
            }
        }

        private void HandleUpdate()
        {
            var iceKg = 0.0f;
            var invKg = 0.0f;

            var gasTotal = 0.0f;
            var gasCurrent = 0.0f;

            foreach (var block in this.allItems)
            {
                var gasTank = block as IMyGasTank;
                if (gasTank != null)
                {
                    gasCurrent += (float)(gasTank.Capacity * gasTank.FilledRatio);
                    gasTotal += gasTank.Capacity;
                }

                if (block.ShowInInventory)
                {
                    var inv = block.GetInventory();
                    foreach (var item in inv.GetItems())
                    {
                        var defId = item.GetDefinitionId();
                        if (defId.SubtypeName == "Ice")
                        {
                            iceKg += (float)item.Amount.RawValue / 1000000;
                        }
                        else
                        {
                            invKg += (float)item.Amount.RawValue / 1000000;
                        }
                    }
                }
            }

            var gas = gasCurrent / gasTotal;

            this.fuelLCD.BackgroundColor = this.GetDisplayColorBelow(gas, H2_HI, H2_LOW);
            var x = this.GetPercentBar(gas);
            this.EchoFuel($"H2\n {x}\n {x}", false);

            this.iceLCD.BackgroundColor = this.GetDisplayColorBelow(iceKg, ICE_HI, ICE_LOW);
            this.EchoIce($"Ice: {iceKg:#,#}kg", false);

            this.cargoLCD.BackgroundColor = this.GetDisplayColorAbove(invKg, INV_HI, INV_LO);
            this.EchoCargo($"Inv: {invKg:#,#}kg", false);
        }

        private string GetPercentBar(float percent)
        {
            var steps = 20;
            var result = "[";
            var step = 1 / steps;
            for (int i = 0; i < steps; i++)
            {
                result += "-";
            }

            result += "]";

            /*if (percent == 1.0f)

            {
                return "[||||||||||||||||||||||||||||||||||||||||]";
            }
            else
            {
                return "[----------------------------------------]";
            }*/

            return result;
        }

        Color GetDisplayColorBelow(float value, float hi, float low)
        {
            if (value > hi)
            {
                return this.GOOD_COLOR;
            }

            if (value > low)
            {
                return this.WARN_COLOR;
            }

            return this.BAD_COLOR;
        }

        Color GetDisplayColorAbove(float value, float hi, float low)
        {
            if (value > hi)
            {
                return this.BAD_COLOR;
            }

            if (value > low)
            {
                return this.WARN_COLOR;
            }

            return this.GOOD_COLOR;
        }

        void InitDisplay(IMyTextPanel panel)
        {
            panel.ShowPublicTextOnScreen();            
            panel.FontColor = this.foregroundColor;
            //panel.Font = FontName;
            panel.FontSize = FontSize;
            panel.WritePublicText(string.Empty);
        }

        void InitDisplays()
        {
            this.cargoLCD = this.GridTerminalSystem.GetBlockWithName(CargoName) as IMyTextPanel;
            this.iceLCD = this.GridTerminalSystem.GetBlockWithName(IceName) as IMyTextPanel;
            this.fuelLCD = this.GridTerminalSystem.GetBlockWithName(FuelName) as IMyTextPanel;

            this.InitDisplay(this.cargoLCD);
            this.InitDisplay(this.iceLCD);
            this.InitDisplay(this.fuelLCD);
        }

        void EchoCargo(string text, bool append = true)
        {
            this.cargoLCD?.WritePublicText($"{text}\n", append);
        }

        void EchoIce(string text, bool append = true)
        {
            this.iceLCD?.WritePublicText($"{text}\n", append);
        }

        void EchoFuel(string text, bool append = true)
        {
            this.fuelLCD?.WritePublicText($"{text}\n", append);
        }
    }
}