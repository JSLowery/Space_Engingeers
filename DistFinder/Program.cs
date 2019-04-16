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
        public Program()
        {
            // The constructor, called only once every session and
            // always before any other method is called. Use it to
            // initialize your script. 
            //     
            // The constructor is optional and can be removed if not
            // needed.
            // 
            // It's recommended to set RuntimeInfo.UpdateFrequency 
            // here, which will allow your script to run itself without a 
            // timer block.
            Runtime.UpdateFrequency = UpdateFrequency.Update100;

        }
        double SCAN_DISTANCE = 1;
        float PITCH = 0;
        float YAW = 0;
        private IMyCameraBlock camera;
        private IMyTextPanel lcd;
        private bool firstrun = true;
        private MyDetectedEntityInfo info;
        private StringBuilder sb = new StringBuilder();
        private StringBuilder range = new StringBuilder();
        private IMyTextPanel rangeDisp;
        int count = 0;
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

            if (firstrun)
            {
                Echo("first run");
                firstrun = false;
                List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
                GridTerminalSystem.GetBlocks(blocks);

                foreach (var block in blocks)
                {
                    if (block is IMyCameraBlock && block.CustomName == "Forward Camera")
                        camera = (IMyCameraBlock)block;

                    if (block is IMyTextPanel && block.CustomName == "Distance Display")
                        lcd = (IMyTextPanel)block;
                    if (block is IMyTextPanel && block.CustomName == "Range Display")
                        rangeDisp = (IMyTextPanel)block;
                }
                camera.EnableRaycast = true;
            }
            if (argument == "Update" || argument == "")
            {
                range.Clear();
                range.Append("Range: " + (camera.AvailableScanRange/1000).ToString());
                rangeDisp.WriteText(range.ToString());
                lcd.WriteText(sb.ToString());
                return;
            }
            count++;
            int value = 0;
            if (int.TryParse(argument, out value))
                if (value != 0)
                    SCAN_DISTANCE = value;
            if (camera.CanScan(SCAN_DISTANCE))
                info = camera.Raycast(SCAN_DISTANCE, PITCH, YAW);
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
                sb.Append("Distance: " + Vector3D.Distance(camera.GetPosition(), info.HitPosition.Value).ToString("0.00"));
            }

            sb.AppendLine();
            sb.Append("Range: " + camera.AvailableScanRange.ToString());
            lcd.WriteText(sb.ToString());
        }

    }
}