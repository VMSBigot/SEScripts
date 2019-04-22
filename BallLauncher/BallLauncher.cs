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

using VRageMath;

namespace IngameScript
{
    /// <summary>
    /// The program.
    /// </summary>
    public partial class Program : MyGridProgram
    {
        private IMySpaceBall spaceBall;

        private IMyRadioAntenna antenna;

        private IMyShipMergeBlock mergeBlockBase;

        private IMyShipMergeBlock mergeBlockProjectile;

        ////private List<IMyRadioAntenna> antennae;

        ////private List<IMyTerminalBlock> allItems;

        ////private List<IMySpaceBall> spaceBalls;
        public Program()
        {
            ////this.allItems = new List<IMyTerminalBlock>();
            ////this.spaceBalls = new List<IMySpaceBall>();

            ////this.GridTerminalSystem.GetBlocksOfType(this.allItems);
            ////this.GridTerminalSystem.GetBlocksOfType(this.spaceBalls);

            ////Echo($"Found {this.allItems.Count} items");

            ////if (this.spaceBalls != null)
            ////{
            ////    Echo($"Found {this.spaceBalls.Count} space balls");
            ////}

            ////this.antennae = new List<IMyRadioAntenna>();
            ////this.GridTerminalSystem.GetBlocksOfType(this.antennae);
            ////if (this.antennae != null)
            ////{
            ////    this.Echo($"Found {this.antennae.Count} antennae");
            ////    foreach (var radioAntenna in this.antennae)
            ////    {
            ////        this.Echo(radioAntenna.CustomName);
            ////    }
            ////}

            var errors = false;

            this.antenna = this.GridTerminalSystem.GetBlockWithName("Antenna") as IMyRadioAntenna;
            if (this.antenna == null)
            {
                this.Echo("Failed to connect to antenna");
                errors = true;
            }

            this.mergeBlockBase = this.GridTerminalSystem.GetBlockWithName("Merge Block Base") as IMyShipMergeBlock;
            if (this.mergeBlockBase == null)
            {
                this.Echo("Failed to connect to merge block base");
                errors = true;
            }

            this.mergeBlockProjectile = this.GridTerminalSystem.GetBlockWithName("Merge Block Projectile") as IMyShipMergeBlock;
            if (this.mergeBlockProjectile == null)
            {
                this.Echo("Failed to connect to merge block projectile");
                errors = true;
            }

            this.spaceBall = this.GridTerminalSystem.GetBlockWithName("Space Ball Test") as IMySpaceBall;
            if (this.spaceBall == null)
            {
                this.Echo("Failed to connect to space ball");
                errors = true;
            }

            if (errors)
            {
                this.Echo("Errors detected. Please fix and recompile");
            }

        }

        public void Save()
        {
            // Called when the program needs to save its state. Use
            // this method to save your state to the Storage field
            // or some other means. 
            // This method is optional and can be removed if not
            // needed.
        }

        public void Main(string argument, UpdateType updateSource)
        {
            if ((updateSource & UpdateType.Script) == UpdateType.Script)
            {
            }

            if ((updateSource & UpdateType.Update100) == UpdateType.Update100)
            {
            }

            if ((updateSource & UpdateType.Terminal) == UpdateType.Terminal)
            {
                if (this.mergeBlockProjectile.IsConnected)
                {
                    this.mergeBlockProjectile.Enabled = false;
                }
                //this.antenna.
                this.spaceBall.VirtualMass = 0f;
            }
        }
    }
}