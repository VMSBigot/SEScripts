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
        // State of the wizard
        bool _doAssemble;
        bool _doRefine;
        int _baySelected;
        long _remoteGridId;

        ItemType _selectedOre;
        float _selectedOreAmount;

        float _oreToRefine;

        string _numberEntered;      

        void StartWizard()
        {
            ResetButtons();
            SetButton(1, true, "Start", PressedButton.Start);
            SetButton(9, true, "Restart", PressedButton.Restart);
            DisplayButtons();

            _remoteGridId = 0;
            _baySelected = 0;
            _doRefine = false;
            _doAssemble = false;

            var output = "Hello\nWelcome to Space Port 343's self-service system.\nThis will walk you through the process of either refining ore or generating components to build with. " +
                "This display will guide you through that process. The left display shows a list of keys and their current actions. The right display shows the current selections. ";

            EchoM(FormatString(output), false);
            DisplayStatus();
        }

        // End up here from pressing 1 in StartWizard()
        // From here, the logic flows onto either AssemblerWizard
        // or RefineWizard
        void WizardPage1()
        {
            ResetButtons();
            SetButton(1, _bay1Connector.Status == MyShipConnectorStatus.Connected, "Bay 1", PressedButton.Bay1Selected);
            SetButton(2, _bay2Connector.Status == MyShipConnectorStatus.Connected, "Bay 2", PressedButton.Bay2Selected);
            SetButton(3, _bay3Connector.Status == MyShipConnectorStatus.Connected, "Bay 3", PressedButton.Bay3Selected);
            SetButton(4, _bay4Connector.Status == MyShipConnectorStatus.Connected, "Bay 4", PressedButton.Bay4Selected);
            SetButton(9, true, "Restart", PressedButton.Restart);
            DisplayButtons();

            var output = "Please select which bay your ship is docked at:";
            EchoM(FormatString(output), false);
            DisplayStatus();
        }

        void WizardPage2()
        {
            ResetButtons();
            SetButton(1, true, "Refine", PressedButton.SelectRefine);
            SetButton(2, true, "Assemble", PressedButton.SelectAssemble);
            SetButton(9, true, "Restart", PressedButton.Restart);
            DisplayButtons();

            _doRefine = false;
            _doAssemble = false;

            var output = "Would you like to refine or assemble?";

            EchoM(FormatString(output), false);
            DisplayStatus();
        }

        // Assembler Workflow
        void AssemblerWizard1()
        {
            ResetButtons();
            SetButton(1, true, "Start Assembler", PressedButton.StartAssemble);
            SetButton(2, true, "Go Back", PressedButton.GoBack);
            SetButton(9, true, "Restart", PressedButton.Restart);
            DisplayButtons();
                        
            _doAssemble = true;

            var output = "Welcome to the assembler wizard";

            EchoM(FormatString(output), false);
            DisplayStatus();
        }
        
        // Refinery Workflow
        void RefineWizard1()
        {
            ResetButtons();
            SetButton(1, true, "Start Refinery", PressedButton.StartRefine);            
            SetButton(2, true, "Go Back", PressedButton.GoBack);
            SetButton(9, true, "Restart", PressedButton.Restart);
            DisplayButtons();

            _doRefine = true;
            
            var output = "Welcome to the refinery wizard";

            EchoM(FormatString(output), false);
            DisplayStatus();
        }

        void RefineWizard2Page1()
        {
            ResetButtons();
            CheckOre(_remoteGridId);
            SetButton(1, _oreStatus.HasMaterial(ItemType.Iron), $"Iron Ore - {FormatNumber(_oreStatus.GetAmount(ItemType.Iron))}", PressedButton.RefineIron);
            SetButton(2, _oreStatus.HasMaterial(ItemType.Nickel), $"Nickel Ore - {FormatNumber(_oreStatus.GetAmount(ItemType.Nickel))}", PressedButton.RefineNickel);
            SetButton(3, _oreStatus.HasMaterial(ItemType.Cobalt), $"Cobalt Ore - {FormatNumber(_oreStatus.GetAmount(ItemType.Cobalt))}", PressedButton.RefineCobalt);
            SetButton(4, _oreStatus.HasMaterial(ItemType.Magnesium), $"Magnesium Ore - {FormatNumber(_oreStatus.GetAmount(ItemType.Magnesium))}", PressedButton.RefineMagnesium);
            SetButton(5, _oreStatus.HasMaterial(ItemType.Silicon), $"Silicon Ore - {FormatNumber(_oreStatus.GetAmount(ItemType.Silicon))}", PressedButton.RefineSilicon);
            SetButton(6, _oreStatus.HasMaterial(ItemType.Silver), $"Silver Ore - {FormatNumber(_oreStatus.GetAmount(ItemType.Silver))}", PressedButton.RefineSilver);
            SetButton(7, _oreStatus.HasMaterial(ItemType.Gold), $"Gold Ore - {FormatNumber(_oreStatus.GetAmount(ItemType.Gold))}", PressedButton.RefineGold);

            SetButton(8, true, "Next Page", PressedButton.RefineNext);
            SetButton(9, true, "Restart Refinery Process", PressedButton.RefineRestart);
            DisplayButtons();

            var output = "Welcome to the refinery wizard\n";
            output += "Please select which ore you would like to refine.\n";

            EchoM(FormatString(output), false);
            DisplayStatus();
        }

        void RefineWizard2Page2()
        {
            ResetButtons();
            CheckOre(_remoteGridId);

            SetButton(1, _oreStatus.HasMaterial(ItemType.Platinum), $"Platinum Ore - {FormatNumber(_oreStatus.GetAmount(ItemType.Platinum))}", PressedButton.RefinePlatinum);
            SetButton(2, _oreStatus.HasMaterial(ItemType.Uranium), $"Uranium Ore - {FormatNumber(_oreStatus.GetAmount(ItemType.Uranium))}", PressedButton.RefineUranium);
            
            SetButton(8, true, "Prev Page", PressedButton.RefinePrev);
            SetButton(9, true, "Restart Refinery Process", PressedButton.RefineRestart);
            DisplayButtons();

            var output = "Welcome to the refinery wizard\n";
            output += "Please select which ore you would like to refine.\n";

            EchoM(FormatString(output), false);
            DisplayStatus();            
        }

        void RefineWizard3()
        {
            ResetButtons();
            SetButton(1, true, "Select All", PressedButton.RefineSelectAll);
            SetButton(2, true, "Select Amount", PressedButton.RefineSelectSome);
            SetButton(9, true, "Restart Refinery Process", PressedButton.RefineRestart);
            DisplayButtons();            

            var output = $"You have selected {_selectedOre} to refine. How much would you like to refine?";

            EchoM(FormatString(output), false);
            DisplayStatus();
        }

        // Can take a pass through WizardGetNumber()
        void RefineWizard4()
        {
            ResetButtons();
            SetButton(1, true, "Yes", PressedButton.RefineYes);
            SetButton(2, true, "Change Amount", PressedButton.RefineChangeAmount);
            SetButton(9, true, "Restart Refinery Process", PressedButton.RefineRestart);
            DisplayButtons();
            
            var output = $"You have selected to refine {_oreToRefine} of {_selectedOre} ore.\nThis will end up with {CalcRate(_selectedOre, _oreToRefine )}\nIs that correct?";

            EchoM(FormatString(output), false);
            DisplayStatus();
        }


        // Shared Wizard Pages
        void WizardGetNumber()
        {
            ResetButtons();
            SetButton(1, true, "1", PressedButton.Number1);
            SetButton(2, true, "2", PressedButton.Number2);
            SetButton(3, true, "3", PressedButton.Number3);
            SetButton(4, true, "4", PressedButton.Number4);
            SetButton(5, true, "5", PressedButton.Number5);
            SetButton(6, true, "6", PressedButton.Number6);
            SetButton(7, true, "7", PressedButton.Number7);
            SetButton(8, true, "8", PressedButton.Number8);
            SetButton(9, true, "9", PressedButton.Number9);
            DisplayButtons();

            var output = $"Please Enter amount. Zero and Enter/Backspace are on Ctrl-2";
           
            //this.GridTerminalSystem
            EchoM(FormatString(output), false);
            DisplayStatus();
        }

        // Misc
        void DisplayStatus()
        {
            var output = string.Empty;
            output += "      Status:\n";
            output += "  ================\n";

            if (_baySelected == 0)
            {
                output += "  NO BAY SELECTED\n";
            }
            else
            {
                output += $"  Bay {_baySelected}  Selected\n";
            }

            if (_doAssemble)
            {
                output += "  REFINERY ACTIVE\n";
            }

            if (_doRefine)
            {
                output += "  ASSEMBLER ACTIVE\n";
            }

            output += "  ================\n";
            EchoR(FormatString(output), false);
        }
    }
}
