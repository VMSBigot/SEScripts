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
        private IMyTextPanel[][] lcdPanels;

        private int row;

        private Color[] palette;

        public Program()
        {
            this.lcdPanels = new IMyTextPanel[32][];
            for (int i = 0; i < 32; i++)
            {
                this.lcdPanels[i] = new IMyTextPanel[32];
                for (int j = 0; j < 32; j++)
                {
                    try
                    {
                        this.lcdPanels[i][j] = GridTerminalSystem.GetBlockWithName($"LCD-{i}-{j}") as IMyTextPanel;
                        SetupLCD(this.lcdPanels[i][j], Color.Blue);
                    }
                    catch
                    {
                    }

                }
            }

            this.palette = new Color[16];
            this.LoadDefaultPalette();
            this.row = 0;
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

        public void Main(string argument, UpdateType updateSource)
        {
            if ((updateSource & UpdateType.Terminal) == UpdateType.Terminal)
            {
                if (argument == string.Empty)
                {
                    this.DumpState();
                    return;
                }
                if (argument.ToUpper() == "RENAME")
                {
                    RenameBlocks();
                    return;
                }

                if (argument.ToUpper() == "LOAD")
                {
                    DisplayImage(wizard);
                }

                if (argument.ToUpper() == "DRAW")
                {
                    this.lcdPanels[0][0].BackgroundColor = GetColor('0');
                    this.lcdPanels[1][0].BackgroundColor = GetColor('1');
                    this.lcdPanels[2][0].BackgroundColor = GetColor('2');
                    this.lcdPanels[3][0].BackgroundColor = GetColor('3');
                    this.lcdPanels[4][0].BackgroundColor = GetColor('4');
                    this.lcdPanels[5][0].BackgroundColor = GetColor('5');
                    this.lcdPanels[6][0].BackgroundColor = GetColor('6');
                    this.lcdPanels[7][0].BackgroundColor = GetColor('7');
                    this.lcdPanels[0][1].BackgroundColor = GetColor('8');
                    this.lcdPanels[1][1].BackgroundColor = GetColor('9');
                    this.lcdPanels[2][1].BackgroundColor = GetColor('A');
                    this.lcdPanels[3][1].BackgroundColor = GetColor('B');
                    this.lcdPanels[4][1].BackgroundColor = GetColor('C');
                    this.lcdPanels[5][1].BackgroundColor = GetColor('D');
                    this.lcdPanels[6][1].BackgroundColor = GetColor('E');
                    this.lcdPanels[7][1].BackgroundColor = GetColor('F');
                    return;
                }
            }
        }

        void DisplayImage(string image)
        {
            var lines = image.Split('#');
            for (int i = 0; i < 32; i++)
            {
                var line = lines[i].ToCharArray();
                for (int j = 0; j < 32; j++)
                {
                    this.lcdPanels[i][j].BackgroundColor = GetColor(line[j]);
                }
            }
        }
        void DumpState()
        {

        }

        void LoadDefaultPalette()
        {
            this.palette[0] = new Color(0, 0, 0); // Black
            this.palette[1] = new Color(1, 0, 0); // Maroon
            this.palette[2] = new Color(0, 1, 0); // Green
            this.palette[3] = new Color(1, 1, 0); // Olive
            this.palette[4] = new Color(0, 0, 1); // Navy
            this.palette[5] = new Color(1, 0, 1); // Purple
            this.palette[6] = new Color(0, 1, 1); // Teal
            this.palette[7] = new Color(2, 2, 2); // Silver

            this.palette[8] = new Color(1, 1, 1); // Gray
            this.palette[9] = new Color(32, 0, 0); // Red
            this.palette[10] = new Color(0, 32, 0); // Lime
            this.palette[11] = new Color(32, 32, 0); // Yellow
            this.palette[12] = new Color(0, 0, 32); // Blue
            this.palette[13] = new Color(32, 0, 32); // Fuchsia
            this.palette[14] = new Color(0, 32, 32); // Aqua
            this.palette[15] = new Color(32, 32, 32); // White
        }

        void LoadPalette(string paletteString)
        {

        }

        void RenameBlocks()
        {
            Echo($"Rename: Row{this.row}");
            try
            {
                var topPanel = this.lcdPanels[0][this.row];
                var panel = topPanel;

                for (int j = 1; j < 32; j++)
                {
                    panel = GetPanelDown(panel);

                    if (panel != null)
                    {
                        panel.CustomName = $"LCD-{j}-{row}";
                    }
                }
            }
            catch (Exception e)
            {
                Echo($"Error finding LCD: {e.Message}");
            }

            this.row++;
        }

        IMyTextPanel GetPanelDown(IMyTextPanel panel)
        {
            var CubeGrid = panel.CubeGrid;
            var Displays = new List<IMyTextPanel>();
            var TargetPosition = panel.Position + new Vector3I(
                                     Base6Directions.GetVector(panel.Orientation.TransformDirection(Base6Directions.Direction.Down)));
            GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(Displays,
                x => ((x.CubeGrid == CubeGrid) && (x.Position == TargetPosition)));
            return Displays.Count != 0 ? Displays[0] : null;
        }

        void SetupLCD(IMyTextPanel panel, Color color)
        {
            panel.ShowPublicTextOnScreen();
            panel.BackgroundColor = color;
            panel.FontColor = color;
            panel.Font = "DEBUG";
            panel.FontSize = 1;
            panel.WritePublicText(string.Empty, false);
        }

        Color GetColor(char c)
        {
            switch (c)
            {
                case '0': return this.palette[0];
                case '1': return this.palette[1];
                case '2': return this.palette[2];
                case '3': return this.palette[3];
                case '4': return this.palette[4];
                case '5': return this.palette[5];
                case '6': return this.palette[6];
                case '7': return this.palette[7];
                case '8': return this.palette[8];
                case '9': return this.palette[9];
                case 'A': return this.palette[10];
                case 'B': return this.palette[11];
                case 'C': return this.palette[12];
                case 'D': return this.palette[13];
                case 'E': return this.palette[14];
                case 'F': return this.palette[15];

                default: return Color.Black;
            }
        }
        const string blankImage =
            "00000000000000000000000000000000#" +
            "00000000000000000000000000000000#" +
            "00000000000000000000000000000000#" +
            "00000000000000000000000000000000#" +
            "00000000000000000000000000000000#" +
            "00000000000000000000000000000000#" +
            "00000000000000000000000000000000#" +
            "00000000000000000000000000000000#" +
            "00000000000000000000000000000000#" +
            "00000000000000000000000000000000#" +
            "00000000000000000000000000000000#" +
            "00000000000000000000000000000000#" +
            "00000000000000000000000000000000#" +
            "00000000000000000000000000000000#" +
            "00000000000000000000000000000000#" +
            "00000000000000000000000000000000#" +
            "00000000000000000000000000000000#" +
            "00000000000000000000000000000000#" +
            "00000000000000000000000000000000#" +
            "00000000000000000000000000000000#" +
            "00000000000000000000000000000000#" +
            "00000000000000000000000000000000#" +
            "00000000000000000000000000000000#" +
            "00000000000000000000000000000000#" +
            "00000000000000000000000000000000#" +
            "00000000000000000000000000000000#" +
            "00000000000000000000000000000000#" +
            "00000000000000000000000000000000#" +
            "00000000000000000000000000000000#" +
            "00000000000000000000000000000000#" +
            "00000000000000000000000000000000#" +
            "00000000000000000000000000000000#";

        const string wizard =
            "00000000003333333333330000000000#" +
            "00000000003BBBBBBBBBB30000000000#" +
            "000000008FBBFFBBBBFFBBF800000000#" +
            "000000008FBB8FBBBBF8BBF800000000#" +
            "000000003BBB003BB300BBB300000000#" +
            "000000003BBB003BB300BBB300000000#" +
            "000000003BBB003BB300BBB300000000#" +
            "000000003BBB10BBBB01BBB300000000#" +
            "0000000019FFBBBBBBBBFF9100000000#" +
            "0000000033FFBBBBBBBBFF3300000000#" +
            "00000019FFFFFFFFFFFFFFFF91000000#" +
            "00000039FFFF88888888FFFF93000000#" +
            "00001999FFFF00000000FFFF99910000#" +
            "00011937FFF8777777778FFF73911000#" +
            "0019998FFF39FFFFFFFF93FFF8999100#" +
            "0139998F8899FFFFFFFF9988F8999310#" +
            "3B9999FF3999FFFFFFFF9993FF9999B3#" +
            "3B9999889999FFFFFFFF9999889999B3#" +
            "3B9999999999FFFFFFFF9999999999B3#" +
            "3B999999119988FFFF889911999999B3#" +
            "3B999999003999FFFF999300999999B3#" +
            "13399999003999888899930099999331#" +
            "00199999003999999999930099999100#" +
            "00199999003999999999930099999100#" +
            "00199999003999999999930099999100#" +
            "00113911119999999999991111931100#" +
            "00001900199999999999999100910000#" +
            "00001100199999999999999100110000#" +
            "00000000199999999999999100000000#" +
            "00000011999BB999999BB99911000000#" +
            "0000001999BBBB9999BBBB9991000000#" +
            "00000001111111111111111110000000#";

        /*
        const string blankImage = 
            "00000000000000000000000000000000#" +
            "00000000000000000000000000000000#" +
            "00000000000000000000000000000000#" +
            "00000000000000000000000000000000#" +
            "00000000000000000000000000000000#" +
            "00000000000000000000000000000000#" +
            "00000000000000000000000000000000#" +
            "00000000000000000000000000000000#" +
            "00000000000000000000000000000000#" +
            "00000000000000000000000000000000#" +
            "00000000000000000000000000000000#" +
            "00000000000000000000000000000000#" +
            "00000000000000000000000000000000#" +
            "00000000000000000000000000000000#" +
            "00000000000000000000000000000000#" +
            "00000000000000000000000000000000#" +
            "00000000000000000000000000000000#" +
            "00000000000000000000000000000000#" +
            "00000000000000000000000000000000#" +
            "00000000000000000000000000000000#" +
            "00000000000000000000000000000000#" +
            "00000000000000000000000000000000#" +
            "00000000000000000000000000000000#" +
            "00000000000000000000000000000000#" +
            "00000000000000000000000000000000#" +
            "00000000000000000000000000000000#" +
            "00000000000000000000000000000000#" +
            "00000000000000000000000000000000#" +
            "00000000000000000000000000000000#" +
            "00000000000000000000000000000000#" +
            "00000000000000000000000000000000#" +
            "00000000000000000000000000000000#";
        */
    }
}