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
        void HandleButton(PressedButton button)
        {
            switch (button)
            {
                case PressedButton.None:
                    break;

                case PressedButton.Restart:
                    StartWizard();
                    break;

                case PressedButton.Start:
                    WizardPage1();
                    break;

                case PressedButton.Bay1Selected:
                    _baySelected = 1;
                    _remoteGridId = _bay1Connector.OtherConnector.CubeGrid.EntityId;
                    WizardPage2();
                    break;

                case PressedButton.Bay2Selected:
                    _baySelected = 2;
                    _remoteGridId = _bay2Connector.OtherConnector.CubeGrid.EntityId;
                    WizardPage2();
                    break;

                case PressedButton.Bay3Selected:
                    _baySelected = 3;
                    _remoteGridId = _bay3Connector.OtherConnector.CubeGrid.EntityId;
                    WizardPage2();
                    break;

                case PressedButton.Bay4Selected:
                    _baySelected = 4;
                    _remoteGridId = _bay4Connector.OtherConnector.CubeGrid.EntityId;
                    WizardPage2();
                    break;

                case PressedButton.SelectAssemble:
                    AssemblerWizard1();
                    break;

                case PressedButton.SelectRefine:
                    RefineWizard1();
                    break;

                case PressedButton.StartAssemble:
                    break;

                case PressedButton.StartRefine:
                    RefineWizard2Page1();
                    break;

                case PressedButton.GoBack:
                    WizardPage2();
                    break;

                case PressedButton.RefineNext:
                    RefineWizard2Page2();
                    break;

                case PressedButton.RefinePrev:
                    RefineWizard2Page1();
                    break;

                case PressedButton.RefineRestart:
                    RefineWizard1();
                    break;

                case PressedButton.RefineIron:
                    RefineWizard3();
                    _selectedOre = ItemType.Iron;
                    _selectedOreAmount = _oreStatus.GetAmount(ItemType.Iron);
                    break;

                case PressedButton.RefineNickel:
                    RefineWizard3();
                    _selectedOre = ItemType.Nickel;
                    _selectedOreAmount = _oreStatus.GetAmount(ItemType.Nickel);
                    break;

                case PressedButton.RefineCobalt:
                    RefineWizard3();
                    _selectedOre = ItemType.Cobalt;
                    _selectedOreAmount = _oreStatus.GetAmount(ItemType.Cobalt);
                    break;

                case PressedButton.RefineMagnesium:
                    RefineWizard3();
                    _selectedOre = ItemType.Magnesium;
                    _selectedOreAmount = _oreStatus.GetAmount(ItemType.Magnesium);
                    break;

                case PressedButton.RefineSilicon:
                    RefineWizard3();
                    _selectedOre = ItemType.Silicon;
                    _selectedOreAmount = _oreStatus.GetAmount(ItemType.Silicon);
                    break;

                case PressedButton.RefineSilver:
                    RefineWizard3();
                    _selectedOre = ItemType.Silver;
                    _selectedOreAmount = _oreStatus.GetAmount(ItemType.Silver);
                    break;

                case PressedButton.RefineGold:
                    RefineWizard3();
                    _selectedOre = ItemType.Gold;
                    _selectedOreAmount = _oreStatus.GetAmount(ItemType.Gold);
                    break;

                case PressedButton.RefinePlatinum:
                    RefineWizard3();
                    _selectedOre = ItemType.Platinum;
                    _selectedOreAmount = _oreStatus.GetAmount(ItemType.Platinum);
                    break;

                case PressedButton.RefineUranium:
                    RefineWizard3();
                    _selectedOre = ItemType.Uranium;
                    _selectedOreAmount = _oreStatus.GetAmount(ItemType.Uranium);
                    break;

                case PressedButton.AssembleRestart:
                    break;

                case PressedButton.AssembleNext1:
                    break;
                case PressedButton.AssembleNext2:
                    break;
                case PressedButton.AssembleNext3:
                    break;
                case PressedButton.AssemblePrev1:
                    break;
                case PressedButton.AssemblePrev2:
                    break;
                case PressedButton.AssemblePrev3:
                    break;

                case PressedButton.RefineSelectAll:
                    _oreToRefine = _selectedOreAmount;
                    RefineWizard4();
                    break;

                case PressedButton.RefineSelectSome:
                    WizardGetNumber();
                    break;

                case PressedButton.RefineYes:
                    DoRefine();
                    // Do transfer here
                    EchoD("DoRefine");
                    break;

                case PressedButton.RefineChangeAmount:
                    RefineWizard3();
                    break;

                case PressedButton.Number0:
                    break;
                case PressedButton.Number1:
                    break;
                case PressedButton.Number2:
                    break;
                case PressedButton.Number3:
                    break;
                case PressedButton.Number4:
                    break;
                case PressedButton.Number5:
                    break;
                case PressedButton.Number6:
                    break;
                case PressedButton.Number7:
                    break;
                case PressedButton.Number8:
                    break;
                case PressedButton.Number9:
                    break;
                case PressedButton.Enter:
                    break;
                case PressedButton.Backspace:
                    break;
            }
        }
    }
}
