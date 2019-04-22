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
    partial class Program
    {
        // Stupid consts
        const float CobaltRate = 0.24f;
        const float GoldRate = 0.008f;
        const float IronRate = 0.56f;
        const float MagnesiumRate = 0.0056f;
        const float NickelRate = 0.32f;
        const float PlatinumRate = 0.004f;
        const float SiliconRate = 0.56f;
        const float SilverRate = 0.08f;
        const float UraniumRate = 0.0056f;

        // Ore/Ignot status from last check
        MaterialStatus _oreStatus;
        MaterialStatus _ignotStatus;

        float CalcRate(ItemType itemType, float amount)
        {
            switch (itemType)
            {
                case ItemType.Iron:
                    return amount * IronRate;                    
                case ItemType.Nickel:
                    return amount * IronRate;
                case ItemType.Cobalt:
                    return amount * IronRate;
                case ItemType.Magnesium:
                    return amount * IronRate;
                case ItemType.Silicon:
                    return amount * IronRate;
                case ItemType.Silver:
                    return amount * IronRate;
                case ItemType.Gold:
                    return amount * IronRate;
                case ItemType.Platinum:
                    return amount * IronRate;
                case ItemType.Uranium:
                    return amount * IronRate;
                default:
                    return 0f;
            }
        }

        void CheckOre(long gridId)
        {
            _oreStatus = new MaterialStatus();
            var allItems = new List<IMyTerminalBlock>();

            GridTerminalSystem.GetBlocksOfType(allItems, cargo => cargo.HasInventory & cargo.CubeGrid.EntityId == gridId);
            
            foreach (var block in allItems)
            {
                if (!(block is IMyReactor))
                {
                    CheckInventoryOfBlock(block, true);
                }
            }
        }

        // Check inventory/gather count. checkOre = check for ore, if false, check for ignots
        void CheckInventoryOfBlock(IMyTerminalBlock block, bool checkOre)
        {
            MaterialStatus currentStatus = _ignotStatus;
            string typeId = "MyObjectBuilder_Ignot";

            if (checkOre)
            {
                currentStatus = _oreStatus;
                typeId = "MyObjectBuilder_Ore";
            }

            var inventory = block.GetInventory();
            foreach (var item in inventory.GetItems())
            {
                if (item.Content.TypeId.ToString() == typeId)
                {
                    switch (item.Content.SubtypeId.ToString())
                    {
                        case "Iron":
                            currentStatus.SetMaterial(ItemType.Iron);
                            currentStatus.AddAmount(ItemType.Iron, (float) item.Amount.RawValue / 1000000);
                            break;
                        case "Nickel":
                            currentStatus.SetMaterial(ItemType.Nickel);
                            currentStatus.AddAmount(ItemType.Nickel, (float)item.Amount.RawValue / 1000000);
                            break;
                        case "Cobalt":
                            currentStatus.SetMaterial(ItemType.Cobalt);
                            currentStatus.AddAmount(ItemType.Cobalt, (float)item.Amount.RawValue / 1000000);
                            break;
                        case "Magnesium":
                            currentStatus.SetMaterial(ItemType.Magnesium);
                            currentStatus.AddAmount(ItemType.Magnesium, (float)item.Amount.RawValue / 1000000);
                            break;
                        case "Silicon":
                            currentStatus.SetMaterial(ItemType.Silicon);
                            currentStatus.AddAmount(ItemType.Silicon, (float)item.Amount.RawValue / 1000000);
                            break;
                        case "Silver":
                            currentStatus.SetMaterial(ItemType.Silver);
                            currentStatus.AddAmount(ItemType.Silver, (float)item.Amount.RawValue / 1000000);
                            break;
                        case "Gold":
                            currentStatus.SetMaterial(ItemType.Gold);
                            currentStatus.AddAmount(ItemType.Gold, (float)item.Amount.RawValue / 1000000);
                            break;
                        case "Platinum":
                            currentStatus.SetMaterial(ItemType.Platinum);
                            currentStatus.AddAmount(ItemType.Platinum, (float)item.Amount.RawValue / 1000000);
                            break;
                        case "Uranium":
                            currentStatus.SetMaterial(ItemType.Uranium);
                            currentStatus.AddAmount(ItemType.Uranium, (float)item.Amount.RawValue / 1000000);
                            break;
                        default:
                            break;
                    }
                }
            }
        }


        bool TransferItem(IMyTerminalBlock blockFrom, IMyTerminalBlock blockTo, string itemType, ref float amount)
        {
            return true;
        }


        ////string DumpMaterials(MaterialStatus materials)
        ////{
        ////    var result = string.Empty;
        ////    if (materials.HasIron)
        ////    {
        ////        result += $"Iron: {FormatNumber(materials.AmountIron)}\n";
        ////    }
        ////    if (materials.HasNickel)
        ////    {
        ////        result += $"Nickel: {FormatNumber(materials.AmountNickel)}\n";
        ////    }
        ////    if (materials.HasCobalt)
        ////    {
        ////        result += $"Cobalt: {FormatNumber(materials.AmountCobalt)}\n";
        ////    }
        ////    if (materials.HasMagnesium)
        ////    {
        ////        result += $"Magnesium: {FormatNumber(materials.AmountMagnesium)}\n";
        ////    }
        ////    if (materials.HasSilicon)
        ////    {
        ////        result += $"Silicon: {FormatNumber(materials.AmountSilicon)}\n";
        ////    }
        ////    if (materials.HasSilver)
        ////    {
        ////        result += $"Silver: {FormatNumber(materials.AmountSilver)}\n";
        ////    }
        ////    if (materials.HasGold)
        ////    {
        ////        result += $"Gold: {FormatNumber(materials.AmountGold)}\n";
        ////    }
        ////    if (materials.HasPlatinum)
        ////    {
        ////        result += $"Platinum: {FormatNumber(materials.AmountPlatinum)}\n";
        ////    }
        ////    if (materials.HasUranium)
        ////    {
        ////        result += $"Uranium: {FormatNumber(materials.AmountUranium)}\n";
        ////    }

        ////    return result;
        ////}

        string FormatNumber(float number)
        {                   
            if (number < 1000)
                return number.ToString("#,#");
            if (number < 1000000000)
                return $"{number:#,#,}K";

            return $"{number:#,#,,,}M";
        }

        void CheckIgnot(long gridId)
        {
            _ignotStatus = new MaterialStatus();
            var allItems = new List<IMyTerminalBlock>();

            GridTerminalSystem.GetBlocksOfType(allItems, cargo => cargo.HasInventory & cargo.CubeGrid.EntityId == gridId);

            foreach (var block in allItems)
            {
                if (!(block is IMyReactor))
                {
                    CheckInventoryOfBlock(block, false);
                }
            }
        } 
        private class MaterialStatus
        {
            private float[] _amounts;
            private bool[] _hasItem;

            public MaterialStatus()
            {
                _amounts = new float[(int)ItemType.Max];
                _hasItem = new bool[(int)ItemType.Max];
            }

            public void SetMaterial(ItemType itemType)
            {
                _hasItem[(int)itemType] = true;
            }

            public bool HasMaterial(ItemType itemType)
            {
                return _hasItem[(int)itemType];
            }

            public float GetAmount(ItemType itemType)
            {
                return _amounts[(int)itemType];
            }

            public void AddAmount(ItemType itemType, float amount)
            {
                _amounts[(int)itemType] += amount;
            }
        }
    }
}
