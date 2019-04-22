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
        ////double SCAN_DISTANCE = 100;
        float PITCH = 0;
        float YAW = 0;

        private IMyCameraBlock camera;
        private IMyTextPanel lcd;

        private MyDetectedEntityInfo info;
        private StringBuilder sb = new StringBuilder();

        public Program()
        {
            this.camera = this.GridTerminalSystem.GetBlockWithName("Front Camera") as IMyCameraBlock;
            this.lcd = this.GridTerminalSystem.GetBlockWithName("LCD") as IMyTextPanel;

            if (this.camera == null)
            {
                Echo("Error finding camera");
                return;
            }

            if (this.lcd == null)
            {
                Echo("Error finding LCD");
                return;
            }

            Echo("Startup complete.");

            camera.EnableRaycast = true;
            this.Runtime.UpdateFrequency = UpdateFrequency.Update100;
        }

        private void DoRayCast()
        {
            var maxDistance = this.camera.AvailableScanRange;

            if (maxDistance > 0)
            {
                info = camera.Raycast(maxDistance, PITCH, YAW);

                sb.Clear();
                sb.Append("EntityID: " + info.EntityId);
                sb.AppendLine();
                sb.Append("Name: " + info.Name);
                sb.AppendLine();
                sb.Append("Type: " + info.Type);
                sb.AppendLine();
                sb.Append("Velocity: " + info.Velocity.ToString("0.000"));
                sb.AppendLine();
                sb.Append("Relationship: " + info.Relationship);
                sb.AppendLine();
                sb.Append("Size: " + info.BoundingBox.Size.ToString("0.000"));
                sb.AppendLine();
                sb.Append("Position: " + info.Position.ToString("0.000"));

                if (info.HitPosition.HasValue)
                {
                    sb.AppendLine();
                    sb.Append("Hit: " + info.HitPosition.Value.ToString("0.000"));
                    sb.AppendLine();
                    sb.Append(
                        "Distance: " + Vector3D.Distance(camera.GetPosition(), info.HitPosition.Value)
                            .ToString("0.00"));
                }

                sb.AppendLine();
                sb.Append("Range: " + maxDistance);
                ////Echo(this.sb.ToString());
                lcd.WritePublicText(sb.ToString());
                lcd.ShowPublicTextOnScreen();
            }
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
                Echo("Running raycast");
                this.DoRayCast();
            }

            if ((updateSource & UpdateType.Trigger) == UpdateType.Trigger)
            {
                Echo("Running raycast");
                this.DoRayCast();
            }
        }
    }
}