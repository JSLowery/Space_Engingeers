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
        /*
 *   R e a d m e
 *   -----------
 * 
 *   In this file you can include any instructions or other comments you want to have injected onto the 
 *   top of your final script. You can safely delete this file if you do not want any such comments.
 * 
 */

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
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
        }



        public void Save()

        {
        }

        List<MyInventoryItem> items = new List<MyInventoryItem>();
        List<IMyTerminalBlock> Blocks = new List<IMyTerminalBlock>();
        List<IMyTerminalBlock> assemblers = new List<IMyTerminalBlock>();
        List<IMyTerminalBlock> refineriess = new List<IMyTerminalBlock>();
        List<IMyBlockGroup> BlockGroups = new List<IMyBlockGroup>();
        List<IMyTerminalBlock> list = new List<IMyTerminalBlock>();
        Dictionary<string, float> Ores = new Dictionary<string, float>();
        Dictionary<string, float> Ingots = new Dictionary<string, float>();
        Dictionary<string, float> Components = new Dictionary<string, float>();
        List<IMyTextPanel> textPanels = new List<IMyTextPanel>();
        public void Main(string argument, UpdateType updateSource)

        {
            items = new List<MyInventoryItem>();
            GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(textPanels);
            if (textPanels.Count == 0)
            {
                return;
            }
            var panel = GridTerminalSystem.GetBlockWithName("Ore Display") as IMyTextPanel;
            ////Echo(panel.CustomName);
            var panel2 = GridTerminalSystem.GetBlockWithName("Ingot Display") as IMyTextPanel;
            var panel3 = GridTerminalSystem.GetBlockWithName("Component Display") as IMyTextPanel;
            GridTerminalSystem.GetBlocksOfType<IMyCargoContainer>(Blocks);
            //Echo(Blocks.Count.ToString());
            GridTerminalSystem.GetBlocksOfType<IMyAssembler>(assemblers);
            //Echo(assemblers.Count.ToString());
            GridTerminalSystem.GetBlocksOfType<IMyRefinery>(refineriess);
            //Echo(refineriess.Count.ToString());
            //Blocks = Blocks.Concat(assemblers).ToList();
            //Blocks = Blocks.Concat(refineriess).ToList();
            //Echo(Blocks.Count.ToString());
            //Echo("block count"+Blocks.Count.ToString());


            get_counts(Blocks);
            get_counts(assemblers);
            get_counts(refineriess);


            //String display = "";
            //String display2 = "";
            //String display3 = "";
            for (int i = 0; i < textPanels.Count; i++)
            {
                var display = textPanels[i];
                String output = "";
                if (display.CustomName.Contains("Ore Display"))
                {
                    output = "Ores\n" + string.Join("\n", Ores.Select(kv => String.Format("{0,-30} - {1, 10}", kv.Key.Split(new Char[] { '/' }, StringSplitOptions.RemoveEmptyEntries)[1], kv.Value)).ToArray());
                    display.WriteText(output);
                }
                else if (display.CustomName.Contains("Ingot Display"))
                {
                    output = "Ingots\n" + string.Join("\n", Ingots.Select(kv => String.Format("{0,-30} - {1, 10}", kv.Key.Split(new Char[] { '/' }, StringSplitOptions.RemoveEmptyEntries)[1], kv.Value)).ToArray());
                    display.WriteText(output);
                }
                else if (display.CustomName.Contains("Components Display"))
                {
                    output = "Components\n" + string.Join("\n", Components.Select(kv => String.Format("{0,-30} - {1, 10}", kv.Key.Split(new Char[] { '/' }, StringSplitOptions.RemoveEmptyEntries)[1], kv.Value)).ToArray());
                    display.WriteText(output);
                }
                
            }
            //display = "Ores\n" + string.Join("\n", Ores.Select(kv => String.Format("{0,-30} - {1, 10}", kv.Key.Split(new Char[] { '/' }, StringSplitOptions.RemoveEmptyEntries)[1], kv.Value)).ToArray());
            //panel.WritePublicText(display);
            //display2 = "Ingots\n" + string.Join("\n", Ingots.Select(kv => String.Format("{0,-30} - {1, 10}", kv.Key.Split(new Char[] { '/' }, StringSplitOptions.RemoveEmptyEntries)[1], kv.Value)).ToArray());
            //panel2.WritePublicText(display2);
            //display3 = "Components\n" + string.Join("\n", Components.Select(kv => String.Format("{0,-30} - {1, 10}", kv.Key.Split(new Char[] { '/' }, StringSplitOptions.RemoveEmptyEntries)[1], kv.Value)).ToArray());

            //panel3.WritePublicText(display3);






            Ores.Clear();
            Ingots.Clear();
            Components.Clear();
            Blocks.Clear();
            assemblers.Clear();
            refineriess.Clear();
            items.Clear();
            BlockGroups.Clear();
            AssemblerDeClutter();
            //Fucking globals, need the ingots list for the refineries section. I need to refactor this piece of steaming shit
            //BalanceRefineries();
            Ingots.Clear();

        }
        void BalanceRefineries()
        {
            var RefineryList = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyRefinery>(RefineryList);
            if (RefineryList == null) return;
            IMyTerminalBlock RefineryInv = null;
            var FirstItem = new List<String>();
            for (int i = 0; i < RefineryList.Count; i++)
            {
                VRage.MyFixedPoint Stone = (VRage.MyFixedPoint)0;
                VRage.MyFixedPoint Iron = 0;
                VRage.MyFixedPoint Nickel = 0;
                VRage.MyFixedPoint Cobalt = 0;
                VRage.MyFixedPoint Silicon = 0;
                VRage.MyFixedPoint Magnesium = 0;
                VRage.MyFixedPoint Silver = 0;
                VRage.MyFixedPoint Gold = 0;
                VRage.MyFixedPoint Platinum = 0;
                VRage.MyFixedPoint Uranium = 0;
                if (RefineryList[i] == null) continue;
                RefineryInv = RefineryList[i];
                var InventoryRef = (IMyInventory)RefineryInv.GetInventory(0);
                var Items = new List<MyInventoryItem>();
                InventoryRef.GetItems(Items);
                if (InventoryRef.IsItemAt(0) && InventoryRef.IsItemAt(1)) FirstItem.Add(Items[0].Type.SubtypeId);
                else FirstItem.Add("X");
                var CargoHolder = GridTerminalSystem.GetBlockWithName("Ore Holder [Ingot:split,ore:p2:split]");
                if (CargoHolder == null)
                {
                    return;
                }
                var CargoInv = (IMyTerminalBlock)CargoHolder;
                IMyInventory InventoryCargo = CargoInv.GetInventory(0);
                if (isFull(InventoryCargo)) return;
                int Count = -1;
                while (InventoryRef.IsItemAt(++Count))
                {
                    if (Items[Count].Type.SubtypeId == "Stone" &&
                     Items[Count].Type.ToString().Contains("Ore"))
                    {
                        Stone += (VRage.MyFixedPoint)Items[Count].Amount;
                        if (Stone > 20000) InventoryRef.TransferItemTo(CargoInv.GetInventory(0),
                            Count, null, true, Items[Count].Amount - 20000);
                    }
                    if (Items[Count].Type.SubtypeId == "Iron" &&
                     Items[Count].Type.ToString().Contains("Ore"))
                    {
                        Iron = Iron + Items[Count].Amount;
                        if (Iron > 20000) InventoryRef.TransferItemTo(CargoInv.GetInventory(0),
                             Count, null, true, Items[Count].Amount - 20000);
                    }
                    if (Items[Count].Type.SubtypeId == "Nickel" &&
                     Items[Count].Type.ToString().Contains("Ore"))
                    {
                        Nickel = Nickel + Items[Count].Amount;
                        if (Nickel > 20000) InventoryRef.TransferItemTo(CargoInv.GetInventory(0),
                             Count, null, true, Items[Count].Amount - 20000);
                    }
                    if (Items[Count].Type.SubtypeId == "Cobalt" &&
                     Items[Count].Type.ToString().Contains("Ore"))
                    {
                        Cobalt = Cobalt + Items[Count].Amount;
                        if (Cobalt > 20000) InventoryRef.TransferItemTo(CargoInv.GetInventory(0),
                             Count, null, true, Items[Count].Amount - 20000);
                    }
                    if (Items[Count].Type.SubtypeId == "Silicon" &&
                     Items[Count].Type.ToString().Contains("Ore"))
                    {
                        Silicon = Silicon + Items[Count].Amount;
                        if (Silicon > 20000) InventoryRef.TransferItemTo(CargoInv.GetInventory(0),
                             Count, null, true, Items[Count].Amount - 20000);
                    }
                    if (Items[Count].Type.SubtypeId == "Magnesium" &&
                     Items[Count].Type.ToString().Contains("Ore"))
                    {
                        Magnesium = Magnesium + Items[Count].Amount;
                        if (Magnesium > 20000) InventoryRef.TransferItemTo(CargoInv.GetInventory(0),
                             Count, null, true, Items[Count].Amount - 20000);
                    }
                    if (Items[Count].Type.SubtypeId == "Silver" &&
                     Items[Count].Type.ToString().Contains("Ore"))
                    {
                        Silver = Silver + Items[Count].Amount;
                        if (Silver > 20000) InventoryRef.TransferItemTo(CargoInv.GetInventory(0),
                             Count, null, true, Items[Count].Amount - 20000);
                    }
                    if (Items[Count].Type.SubtypeId == "Gold" &&
                     Items[Count].Type.ToString().Contains("Ore"))
                    {
                        Gold = Gold + Items[Count].Amount;
                        if (Gold > 20000) InventoryRef.TransferItemTo(CargoInv.GetInventory(0),
                             Count, null, true, Items[Count].Amount - 20000);
                    }
                    if (Items[Count].Type.SubtypeId == "Platinum" &&
                     Items[Count].Type.ToString().Contains("Ore"))
                    {
                        Platinum = Platinum + Items[Count].Amount;
                        if (Platinum > 20000) InventoryRef.TransferItemTo(CargoInv.GetInventory(0),
                             Count, null, true, Items[Count].Amount - 20000);
                    }
                    if (Items[Count].Type.SubtypeId == "Uranium" &&
                     Items[Count].Type.ToString().Contains("Ore"))
                    {
                        Uranium = Uranium + Items[Count].Amount;
                        if (Uranium > 20000) InventoryRef.TransferItemTo(CargoInv.GetInventory(0),
                             Count, null, true, Items[Count].Amount - 20000);
                    }
                }
                Stone = (VRage.MyFixedPoint)0;
                Iron = 0;
                Nickel = 0;
                Cobalt = 0;
                Silicon = 0;
                Magnesium = 0;
                Silver = 0;
                Gold = 0;
                Platinum = 0;
                Uranium = 0;
            }
            int FirstItemCountToMax = 0;
            for (int i = 0; i < FirstItem.Count; i++)
            {
                Echo(FirstItem[i]);
            }
            for (int s = 0; s < FirstItem.Count; s++)
            {
                string holder = FirstItem[s];
                for (int j = 1; j < FirstItem.Count; j++)
                {
                    if (FirstItem[j].Equals("X"))
                        FirstItemCountToMax += 1;
                    if (holder.Equals(FirstItem[j]) && !(holder.Equals("X") && FirstItem[j].Equals("X")))
                    {

                        RefineryInv = (IMyTerminalBlock)RefineryList[s];
                        IMyInventory Inventory = RefineryInv.GetInventory(0);
                        var Items = new List<MyInventoryItem>();
                        Inventory.GetItems(Items);
                        var CargoHolder = GridTerminalSystem.GetBlockWithName("Ore Holder [Ingot:split,ore:p2:split]");
                        if (CargoHolder == null) return;
                        var CargoInv = (IMyTerminalBlock)CargoHolder;

                        if (Items.Count < 1)
                        {
                            return;
                        }
                        Echo("Moving " + Items[0].Type.SubtypeId);
                        Inventory.TransferItemTo(CargoInv.GetInventory(0),
                                                    0, null, true, Items[0].Amount);
                    }

                    if (FirstItemCountToMax == FirstItem.Count)
                    {
                        for (int q = 0; q < RefineryList.Count - 1; q++)
                        {
                            RefineryInv = (IMyTerminalBlock)RefineryList[q];
                            IMyInventory Inventory = RefineryInv.GetInventory(0);
                            var Items = new List<MyInventoryItem>();
                            Inventory.GetItems(Items);
                            var CargoHolder = GridTerminalSystem.GetBlockWithName("Ore Holder [Ingot:split]");
                            if (CargoHolder == null) return;
                            var CargoInv = (IMyTerminalBlock)CargoHolder;
                            IMyInventory InventoryCargo = CargoInv.GetInventory(0);
                            if (Inventory.IsItemAt(0))
                            {
                                double calc = (int)Items[0].Amount * .05;
                                double zero = 1000;

                                if ((double)Inventory.CurrentMass - calc > zero)
                                    calc = 0;
                                for (int p = RefineryList.Count; p > 1; p--)
                                {
                                    //Echo("Moving into 0" + Items[0].Type.SubtypeId);
                                    var RefineryInv1 = (IMyTerminalBlock)RefineryList[p];
                                    Inventory.TransferItemTo(RefineryInv1.GetInventory(0),
                                                                0, null, true, (int)calc);
                                }
                            }
                        }
                    }
                }
            }
        }
        bool isFull(IMyInventory container)
        {
            return ((float)container.CurrentVolume / (float)container.MaxVolume > 0.99F);
        }
        void AssemblerDeClutter()
        {
            var Assemblers = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyAssembler>(Assemblers);
            if (Assemblers == null) return;

            var Cargo = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyCargoContainer>(Cargo);
            if (Cargo == null) return;
            IMyTerminalBlock CargoOwner = null;
            IMyCubeBlock CargoCube = null;

            // Find the first working functional non-empty cargo container
            var CargoIndex = 0;
            //Echo(Cargo.Count.ToString());
            for (CargoIndex = 0; CargoIndex < Cargo.Count; CargoIndex++)
            {
                //Echo(CargoIndex.ToString());
                CargoOwner = Cargo[CargoIndex];
                CargoCube = (IMyCubeBlock)Cargo[CargoIndex];
                //Echo("wtf " + CargoOwner.CustomName + " " + CargoCube.IsWorking.ToString() + " " + CargoCube.IsFunctional.ToString() + " " + CargoOwner.GetInventory(0).IsFull.ToString());
                if (Cargo[CargoIndex].CustomName.Contains("miner")) continue;
                if (CargoCube.IsWorking && CargoCube.IsFunctional && !CargoOwner.GetInventory(0).IsFull) continue;
                //Echo(Cargo[CargoIndex].CustomName);

            }
            if (CargoIndex >= Cargo.Count) return; // no empty cargo containers

            // check assemblers for clogging
            for (var Index = 0; Index < Assemblers.Count; Index++)
            {
                if (Assemblers[Index] == null) continue;
                Echo(Assemblers[Index].CustomName);
                var AssyOwner = (IMyTerminalBlock)Assemblers[Index];
                var Inventory = (IMyInventory)AssyOwner.GetInventory(0);
                var Inventory2 = (IMyInventory)AssyOwner.GetInventory(1);
                var Items = new List<MyInventoryItem>();
                var Items2 = new List<MyInventoryItem>();
                Inventory.GetItems(Items);
                Inventory2.GetItems(Items2);
                VRage.MyFixedPoint MaxAmount = 0;

                int i = -1;
                while (Inventory.IsItemAt(++i))
                { // set MaxAmount based on what it is.
                    if (Items[i].Type.SubtypeId == "Stone") MaxAmount = (VRage.MyFixedPoint)10.00;
                    if (Items[i].Type.SubtypeId == "Iron") MaxAmount = (VRage.MyFixedPoint)600.00;
                    if (Items[i].Type.SubtypeId == "Nickel") MaxAmount = (VRage.MyFixedPoint)70.00;
                    if (Items[i].Type.SubtypeId == "Cobalt") MaxAmount = (VRage.MyFixedPoint)220.00;
                    if (Items[i].Type.SubtypeId == "Silicon") MaxAmount = (VRage.MyFixedPoint)15.00;
                    if (Items[i].Type.SubtypeId == "Magnesium") MaxAmount = (VRage.MyFixedPoint)5.20;
                    if (Items[i].Type.SubtypeId == "Silver") MaxAmount = (VRage.MyFixedPoint)10.00;
                    if (Items[i].Type.SubtypeId == "Gold") MaxAmount = (VRage.MyFixedPoint)5.00;
                    if (Items[i].Type.SubtypeId == "Platinum") MaxAmount = (VRage.MyFixedPoint)0.40;
                    if (Items[i].Type.SubtypeId == "Uranium") MaxAmount = (VRage.MyFixedPoint)0.50;

                    MaxAmount = MaxAmount * 2; // allow this times as much as needed

                    if (Items[i].Amount > MaxAmount) Inventory.TransferItemTo(
                          CargoOwner.GetInventory(0), i, null, true, Items[i].Amount - MaxAmount);
                }
                i = -1;
                while (Inventory2.IsItemAt(++i))//This is to decluter the bottom inventory in Assemlbers
                {
                    //if (Items2[i].Amount>0)
                    Inventory2.TransferItemTo(CargoOwner.GetInventory(0),
                        i, null, true, Items2[i].Amount);
                }
            }
        }
        public void get_counts(List<IMyTerminalBlock> Blocks)
        {

            for (int i = 0; i < Blocks.Count; i++)
            {
                var block = Blocks[i];
                //Echo("i = " + i.ToString() + " Name: " + block.CustomName);
                //       Echo(block.CustomName);
                if (block.HasInventory)
                {
                    for (int j = 0; j < block.InventoryCount; j++)
                    {
                        //Echo("j = " + j.ToString() + " Name: " + block.CustomName);
                        var inv = block.GetInventory(j);
                        //Echo("inv item count " + inv.ItemCount.ToString());
                        items.Clear();
                        inv.GetItems(items);
                        //Echo(block.CustomName);
                        //Echo(items.Count.ToString());
                        //Echo(items.Count.ToString());
                        for (int k = 0; k < items.Count; k++)
                        {
                            //Echo("k = " +k.ToString() + " Name: " + block.CustomName);
                            var item = items[k];

                            //Echo(item.Type.ToString());
                            if (item.Amount > 0)
                            {
                                //Echo(item.Type.ToString() + " Name: " + block.CustomName);
                                if (item.Type.ToString().Contains("MyObjectBuilder_Ore") || item.Type.ToString().Contains("Ice"))
                                {

                                    if (Ores.ContainsKey(item.Type.ToString()))
                                    {
                                        Ores[item.Type.ToString()] += (float)item.Amount;
                                        //Echo("echo ore "+item.Type.ToString() + " " + item.Amount.ToString());
                                        continue;
                                    }

                                    Ores.Add(item.Type.ToString(), (float)item.Amount);
                                }
                                else if (item.Type.ToString().Contains("MyObjectBuilder_Ingot"))
                                {

                                    if (Ingots.ContainsKey(item.Type.ToString()))
                                    {
                                        Ingots[item.Type.ToString()] += (float)item.Amount;
                                        //Echo("echo ingot "+item.Type.ToString() + " " + item.Amount.ToString());
                                        continue;
                                    }

                                    Ingots.Add(item.Type.ToString(), (float)item.Amount);
                                }
                                else if (item.Type.ToString().Contains("Component"))
                                {

                                    if (Components.ContainsKey(item.Type.ToString()))
                                    {
                                        Components[item.Type.ToString()] += (float)item.Amount;

                                        //Echo("echo " + " inv:" + block.CustomName + " type: " + item.Type.ToString() + " amt: " + item.Amount.ToString());
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