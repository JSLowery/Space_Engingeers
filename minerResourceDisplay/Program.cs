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
        private StringBuilder sbDrills = new StringBuilder();
        private IMyTextPanel lcd;
        private IMyTextPanel drillLCD;
        private List<IMyCargoContainer> cargos = new List<IMyCargoContainer>();
        private List<IMyShipDrill> drills = new List<IMyShipDrill>();
        Dictionary<string, float> Ores = new Dictionary<string, float>();
        Dictionary<string, float> Ingots = new Dictionary<string, float>();
        Dictionary<string, float> Components = new Dictionary<string, float>();
        private List<IMyTerminalBlock> blocksForDisplay = new List<IMyTerminalBlock>();
        List<MyInventoryItem> items = new List<MyInventoryItem>();
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
                    if (block is IMyCargoContainer && block.CustomName.Contains("miner"))
                    {
                        cargos.Add((IMyCargoContainer)block);
                        blocksForDisplay.Add(block);
                    }
                    if (block is IMyTextPanel && block.CustomName == "Miner Display")
                        lcd = (IMyTextPanel)block;
                    if (block is IMyShipDrill)
                    {
                        drills.Add((IMyShipDrill)block);
                        blocksForDisplay.Add(block);
                    }
                    if (block is IMyTextPanel && block.CustomName == "Miner Display (drills)")
                        drillLCD = (IMyTextPanel)block;
                }
            }
            float inventoriesSum = 0;
            float inventoriesCount = 0;
            foreach (IMyCargoContainer cargo in cargos)
            {
                inventoriesCount++;
                IMyInventory InventoryCargo = cargo.GetInventory(0);
                inventoriesSum += (float)InventoryCargo.MaxVolume;
                float percentFull = isFull(InventoryCargo);
                long max = InventoryCargo.MaxVolume.RawValue;
                sb.Append(String.Format("{0,-40}", cargo.CustomName));
                sb.AppendLine();
                sb.Append(graphSB(percentFull, max));
                sb.AppendLine();
            }

            foreach (IMyShipDrill drill in drills)
            {
                inventoriesCount++;
                IMyInventory drillInventory = drill.GetInventory(0);
                inventoriesSum += (float)drillInventory.MaxVolume;
                float percentFull = isFull(drillInventory);
                long max = drillInventory.MaxVolume.RawValue;
                sbDrills.Append(String.Format("{0,-10}", drill.CustomName));
                sbDrills.Append(graphSB(percentFull, max));
                sbDrills.AppendLine();
            }
            float inventoriesFull = inventoriesSum * inventoriesCount;
            get_counts(blocksForDisplay);
            int itmCount = 0;
            foreach (var item in Ores)
            {
                itmCount++;
                if (itmCount % 2 == 0)
                    sb.AppendLine();
                sb.Append(item.Key.Split(new Char[] { '/' }, StringSplitOptions.RemoveEmptyEntries)[1] + " " + (item.Value / inventoriesFull).ToString("0.")+ "%");
            }
            Ores.Clear();
            Ingots.Clear();
            Components.Clear();
            lcd.WriteText(sb.ToString());
            drillLCD.WriteText(sbDrills.ToString());
            sbDrills.Clear();
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
        public void get_counts(List<IMyTerminalBlock> Blocks)
        {

            for (int i = 0; i < Blocks.Count; i++)
            {
                var block = Blocks[i];
                if (block.HasInventory)
                {
                    for (int j = 0; j < block.InventoryCount; j++)
                    {
                        var inv = block.GetInventory(j);
                        items.Clear();
                        inv.GetItems(items);
                        for (int k = 0; k < items.Count; k++)
                        {
                            var item = items[k];
                            if (item.Amount > 0)
                            {
                                if (item.Type.ToString().Contains("MyObjectBuilder_Ore") || item.Type.ToString().Contains("Ice"))
                                {

                                    if (Ores.ContainsKey(item.Type.ToString()))
                                    {
                                        Ores[item.Type.ToString()] += (float)item.Amount;
                                        continue;
                                    }

                                    Ores.Add(item.Type.ToString(), (float)item.Amount);
                                }
                                else if (item.Type.ToString().Contains("MyObjectBuilder_Ingot"))
                                {

                                    if (Ingots.ContainsKey(item.Type.ToString()))
                                    {
                                        Ingots[item.Type.ToString()] += (float)item.Amount;
                                        continue;
                                    }

                                    Ingots.Add(item.Type.ToString(), (float)item.Amount);
                                }
                                else if (item.Type.ToString().Contains("Component"))
                                {

                                    if (Components.ContainsKey(item.Type.ToString()))
                                    {
                                        Components[item.Type.ToString()] += (float)item.Amount;
                                        continue;
                                    }
                                    Components.Add(item.Type.ToString(), (float)item.Amount);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}