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
        private bool firstrun = true;
        private StringBuilder sb = new StringBuilder();
        private IMyTextPanel lcd;
        private List<IMyCargoContainer> cargos = new List<IMyCargoContainer>();
        private List<IMyShipDrill> drills = new List<IMyShipDrill>();
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
                    if (block is IMyCargoContainer)//&& block.CustomName.Contains("Scanner Camera"))
                    {
                        cargos.Add((IMyCargoContainer)block);
                    }
                    if (block is IMyTextPanel && block.CustomName == "Graph Display")
                        lcd = (IMyTextPanel)block;
                    if (block is IMyShipDrill)
                    {
                        drills.Add((IMyShipDrill)block);
                    }
                }
            }
            foreach (IMyCargoContainer cargo in cargos)
            {
                IMyInventory InventoryCargo = cargo.GetInventory(0);
                float percentFull = isFull(InventoryCargo);
                long max = InventoryCargo.MaxVolume.RawValue;
                sb.Append(String.Format("{0,-40}", cargo.CustomName));
                sb.Append(graphSB(percentFull, max));
                sb.AppendLine();
            }
            foreach (IMyShipDrill drill in drills)
            {
                IMyInventory drillInventory = drill.GetInventory(0);
                float percentFull = isFull(drillInventory);
                long max = drillInventory.MaxVolume.RawValue;
                sb.Append(String.Format("{0,-40}", drill.CustomName));
                sb.Append(graphSB(percentFull, max));
                sb.AppendLine();
            }
            lcd.WriteText(sb.ToString());
            sb.Clear();
        }
        float isFull(IMyInventory container)
        {
            return (float)container.CurrentVolume / (float)container.MaxVolume;
        }
        String graphSB(float percent, long maxSize)
        {
            StringBuilder sg = new StringBuilder();
            int maxDisp = 20;
            String startst = (percent * 100).ToString("0.") + " ";
            String st = "[";
            int dispPer = (int)(maxDisp * percent) + 1;
            sg.Append(st.PadRight((dispPer), '|'));
            if (dispPer < maxDisp)
                sg.Append("".PadRight(maxDisp - dispPer, '-'));
            sg.Append("]");

            return String.Format("{0, -3}% {1}", startst, sg.ToString());
        }
    }
}