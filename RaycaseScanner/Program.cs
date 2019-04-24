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
        // This file contains your actual script.
        //
        // You can either keep all your code here, or you can create separate
        // code files to make your program easier to navigate while coding.
        //
        // In order to add a new utility class, right-click on your project, 
        // select 'New' then 'Add Item...'. Now find the 'Space Engineers'
        // category under 'Visual C# Items' on the left hand side, and select
        // 'Utility Class' in the main area. Name it in the box below, and
        // press OK. This utility class will be merged in with your code when
        // deploying your final script.
        //
        // You can also simply create a new utility class manually, you don't
        // have to use the template if you don't want to. Just do so the first
        // time to see what a utility class looks like.

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

        public void Save()
        {
            // Called when the program needs to save its state. Use
            // this method to save your state to the Storage field
            // or some other means. 
            // 
            // This method is optional and can be removed if not
            // needed.
        }
        double SCAN_DISTANCE = 1;
        float PITCH = 0;
        float YAW = 0;
        private List<IMyCameraBlock> cameras = new List<IMyCameraBlock>();
        private IMyTextPanel lcd;
        private bool firstrun = true;
        private StringBuilder sb = new StringBuilder();
        private StringBuilder enemies = new StringBuilder();
        private IMyTextPanel enemyDisp;
        private MyDetectedEntityInfo info;
        //int count = 0;
        private List<MyDetectedEntityInfo> detectedEnemies = new List<MyDetectedEntityInfo>();
        public void Main(string argument, UpdateType updateSource)
        {
            // The main entry point of the script, invoked every time
            // one of the programmable block's Run actions are invoked,
            // or the script updates itself. The updateSource argument
            // describes where the update came from. Be aware that the
            // updateSource is a  bitfield  and might contain more than
            // one update type.
            //
            // The method itself is required, but the arguments above
            // can be removed if not needed.
            if (firstrun)
            {
                firstrun = false;
                List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
                GridTerminalSystem.GetBlocks(blocks);

                foreach (var block in blocks)
                {
                    if (block is IMyCameraBlock)//&& block.CustomName.Contains("Scanner Camera"))
                    {
                        cameras.Add((IMyCameraBlock)block);
                        IMyCameraBlock cam = (IMyCameraBlock)block;
                        cam.EnableRaycast = true;
                    }
                    if (block is IMyTextPanel && block.CustomName == "Norm Display")
                        lcd = (IMyTextPanel)block;
                    if (block is IMyTextPanel && block.CustomName == "Enemy Display")
                        enemyDisp = (IMyTextPanel)block;
                }
                Echo("end setup Cameras count - " + cameras.Count);
            }
            sb.Clear();
            foreach (var camera in cameras)
            {
                Echo("wrf");
                if (camera == null)
                    continue;
                if (camera.CanScan(SCAN_DISTANCE))
                    info = camera.Raycast(SCAN_DISTANCE, PITCH, YAW);
                else
                    info = new MyDetectedEntityInfo();
                //sb.Clear();

                Echo(camera.EnableRaycast.ToString());
                Echo(camera.AvailableScanRange.ToString());
                if (sb == null)
                    continue;
                if (info.IsEmpty())
                    continue;
                sb.AppendLine();
                sb.Append(camera.CustomName);
                sb.AppendLine();
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

                    int index = detectedEnemies.FindIndex(f => f.EntityId == info.EntityId);
                    if (index >= 0)
                        Echo("already have that enemy grid, skiping");
                    else
                        detectedEnemies.Add(info);
                }

                sb.AppendLine();
                sb.Append("Range: " + camera.AvailableScanRange.ToString());
                sb.AppendLine();
                foreach (var enem in detectedEnemies)
                {
                    enemies.AppendLine();
                    enemies.Append(enem.Name);
                    enemies.AppendLine();
                    enemies.Append("Hit: " + info.HitPosition.Value.ToString("0.000"));
                    enemies.AppendLine();
                    enemies.Append("Distance: " + Vector3D.Distance(camera.GetPosition(), info.HitPosition.Value).ToString("0.00"));
                    enemies.AppendLine();
                }
            }
            lcd.WriteText(sb.ToString());
            enemyDisp.WriteText(enemies.ToString());
            sb.Clear();
            enemies.Clear();
        }
    }
}