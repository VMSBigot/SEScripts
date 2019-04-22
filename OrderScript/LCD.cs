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
        IMyTextPanel _leftLCD;
        IMyTextPanel _rightLCD;
        IMyTextPanel _middleLCD;
        IMyTextPanel _debugLCD;

        void SetupLCD(IMyTextPanel panel)
        {
            panel.ShowPublicTextOnScreen();
            panel.BackgroundColor = BACKGROUND_COLOR;
            panel.FontColor = FOREGROUND_COLOR;
            panel.Font = FONT_NAME;
            panel.FontSize = FONT_SIZE;
            panel.WritePublicText(string.Empty, false);
        }

        private bool InitDisplays()
        {
            _leftLCD = GridTerminalSystem.GetBlockWithName(LEFT_LCD) as IMyTextPanel;
            _rightLCD = GridTerminalSystem.GetBlockWithName(RIGHT_LCD) as IMyTextPanel;
            _middleLCD = GridTerminalSystem.GetBlockWithName(MIDDLE_LCD) as IMyTextPanel;
            _debugLCD = GridTerminalSystem.GetBlockWithName(DEBUG_LCD) as IMyTextPanel;

            if (_leftLCD == null)
            {
                Echo("No Left LCD!\n");
                return false;
            }

            if (_rightLCD == null)
            {
                Echo("No Right LCD!\n");
                return false;
            }

            if (_middleLCD == null)
            {
                Echo("No Middle LCD!\n");
                return false;
            }

            if (_debugLCD == null)
            {
                Echo("No Debug LCD!\n");
            }

            SetupLCD(_leftLCD);
            SetupLCD(_rightLCD);
            SetupLCD(_middleLCD);
            SetupLCD(_debugLCD);

            return true;
        }

        string FormatString(string input)
        {
            //string[] lineArray = new string[lines];

            string result = string.Empty;

            foreach (var str in input.Split('\n'))
            {
                var length = str.Length;
                var lines = (length / SCREEN_WIDTH) + 1;
                int width = SCREEN_WIDTH;
                //EchoD($"Lines: {lines}");


                for (int i = 0; i < lines; i++)

                {
                    var start = i * SCREEN_WIDTH;

                    if (start > length)
                        break;

                    if (start + SCREEN_WIDTH > length)
                        width = (length - start);

                    result += $"{str.Substring(start, width)}\n";
                }
            }
            return result;

        }

        void EchoL(string text, bool append = true)
        {
            _leftLCD?.WritePublicText($"{text}\n", append);
        }

        void EchoR(string text, bool append = true)
        {
            _rightLCD?.WritePublicText($"{text}\n", append);
        }

        void EchoM(string text, bool append = true)
        {
            _middleLCD?.WritePublicText($"{text}\n", append);
        }
        void EchoD(string text, bool append = true)
        {
            _debugLCD?.WritePublicText($"{text}\n", append);
        }

    }
}
