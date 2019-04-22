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
        // ----------------------- 
        // -- Power Graphs v1.4.6 -- 
        // By Remaarn 
        // ----------------------- 
        // Updates: 
        // v1.4.6 - Added graph line color indicators next to info text. 
        // v1.4.5 - Fixed battery max charge line. 
        // v1.4.4 - Improved graph scaling when long history is enabled. 
        // v1.4.3 - Added option for missing value filling behavior. 
        // For previous updates see the change notes on the steam workshop page. 

        // -- Setup -- 
        // Add the Lcd_Name_Tag (shown in settings) to any LCDs you wish to display 
        // graphs on. To show only certain graph types on a display, write the graph 
        // types into the CustomData of the LCD, one per line. 
        // Avaliable graph types are: 
        //  usage 
        //  battery 
        //  solar 
        //  reactor 

        // - Sensor LCD Culling - 
        // To save performance, LCDs can be set to only update when a player is in 
        // range of a sensor. To enable this behavior, add the sensor name tag to 
        // any sensor you want to use. Then add the names of any LCD panels that you 
        // want the sensor to activate to the CustomData of the sensor, one name per line. 

        // By default wide LCDs will not use their full width. To enable this 
        // behaviour set Enable_Long_History to true. 

        // Running the program block with the argument 'togglestatus' will manually 
        // cycle the graph on any displays that are not set to a specific display 
        // type. This is usefull if auto cycling is set to false. 

        // -- Settings -- 

        // The text LCD names must contain to be used 
        const string Lcd_Name_Tag = "[PowerGraph]";

        // The text sensor names must contain if they are to be used as LCD triggers 
        const string Sensor_Name_Tag = "[LCDTrigger]";

        // How often current power stats are collected 
        const double Data_Update_Interval_In_Seconds = 60;

        // How often LCDs are redrawn 
        // WARNING: Sub-second times may impact game performance 
        const double LCD_Refresh_Interval_In_Seconds = 10;

        // Should LCDs without a specific display type cycle through all display types 
        const bool Auto_Cycle_Graphs = true;

        // How often LCDs cycle between assigned graph types 
        const double Auto_Cycle_Interval_In_Seconds = 10;

        // When enabled the script will add values of zero after a power outage for 
        // the duration of the outage. 
        // NOTE: Pausing the game will have the same effect as a power outage. If you 
        // aren't likely to experience frequent power outages and you pause the 
        // game often you may want to set this to false. 
        const bool Zero_Fill_Missing_Values = true;

        // Should the script update LCDs on connected grids (through rotors, connectors, etc.) 
        const bool Display_On_LCDs_Of_Connected_Grids = false;

        // Should the power of connected small grids be included 
        const bool Include_Power_Of_Connected_Small_Grids = false;

        // Should the power of connected large grids be included 
        const bool Include_Power_Of_Connected_Large_Grids = false;

        // Set to true to make full use of wide LCDs. Will use more memory and performance 
        const bool Enable_Long_History = false;

        // If set to true the displays will use the smallest font size giving the most resolution. 
        // WARNING: High resolution requries more performance. 
        //          If on a server it will also use more network bandwidth. 
        const bool Enable_High_Resolution = false;

        // -- Graph Colors -- 
        // Colors are defined as RGB. Each r g b value is an int ranging 
        // from 0 to 7. 0 Being the darkest and 7 being the brightest. 

        readonly RGB Background_Color = new RGB(r: 0, g: 0, b: 0);
        readonly RGB Text_Color = new RGB(r: 7, g: 7, b: 7);
        readonly RGB Graph_Axes_Color = new RGB(r: 7, g: 7, b: 7);
        readonly RGB Current_Usage_Color = new RGB(r: 3, g: 7, b: 3);
        readonly RGB Required_Power_Color = new RGB(r: 7, g: 2, b: 2);
        readonly RGB Battery_Charge_Color = new RGB(r: 1, g: 2, b: 7);
        readonly RGB Max_Battery_Charge_Color = new RGB(r: 3, g: 3, b: 3);
        readonly RGB Current_Solar_Color = new RGB(r: 6, g: 5, b: 1);
        readonly RGB Max_Solar_Color = new RGB(r: 6, g: 3, b: 1);
        readonly RGB Reactor_Output_Color = new RGB(r: 2, g: 4, b: 2);

        // -- Advaned Settings -- 

        readonly string[] animFrames = new[] {
            "Power Graphs |---",
            "Power Graphs -|--",
            "Power Graphs --|-",
            "Power Graphs ---|",
            "Power Graphs --|-",
            "Power Graphs -|--"
        };

        const int LCD_Res_X = Enable_High_Resolution ? Max_Res_X : 128;
        const int LCD_Res_Y = Enable_High_Resolution ? Max_Res_Y : 128;

        const int Wide_LCD_Res_X = Enable_High_Resolution ? Max_WIDE_Res_X : 256;
        const int Wide_LCD_Res_Y = LCD_Res_Y;

        const float Pixel_Font_Scale = Enable_High_Resolution ? 0.1f : 0.1390625f;

        const int Max_Res_X = 178;
        const int Max_Res_Y = 178;
        const int Max_WIDE_Res_X = 356;

        // Add the subtypeIds of any modded wide LCDs here 
        readonly string[] wideLCDSubtypes = new[] { "LargeLCDPanelWide" };

        // -- END OF SETTINGS -- 

        struct Bitmap
        {
            public int Width;
            public int Height;
            public int Stride;
            public char[] Pixels;
        }

        const int historyValueCount = (Enable_Long_History ? Wide_LCD_Res_X : LCD_Res_X) - 10;

        float[] powerUsageValues = new float[historyValueCount];
        float[] powerReqValues = new float[historyValueCount];
        float[] batteryChargeValues = new float[historyValueCount];
        float[] batteryMaxChargeValues = new float[historyValueCount];
        float[] solarMaxOutputValues = new float[historyValueCount];
        float[] solarOutputValues = new float[historyValueCount];
        float[] reactorOutputValues = new float[historyValueCount];
        DateTime[] sampleTimes = new DateTime[historyValueCount];

        [Flags]
        enum DisplayTypes
        {
            Auto = 0,
            PowerUsage = 1,
            BatteryCharge = 2,
            SolarOutput = 4,
            ReactorOutput = 8,
            Invalid = 16
        }

        DisplayTypes displayType = DisplayTypes.PowerUsage;

        const UpdateType updateTypes = UpdateType.Terminal | UpdateType.Trigger | UpdateType.Update1 | UpdateType.Update10 | UpdateType.Update100;

        List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
        List<IMyBatteryBlock> batteries = new List<IMyBatteryBlock>();
        List<IMySolarPanel> solarPanels = new List<IMySolarPanel>();
        List<IMyReactor> reactors = new List<IMyReactor>();
        List<IMyTextPanel> panels = new List<IMyTextPanel>();
        List<IMySensorBlock> sensors = new List<IMySensorBlock>();
        List<MyDetectedEntityInfo> detectedEntities = new List<MyDetectedEntityInfo>();
        StringBuilder stringBuilder = new StringBuilder();

        DateTime lastDataUpdate;
        DateTime lastDisplayCycle;

        Closures closures;

        struct LCDStatus
        {
            public IMyTextPanel LCD;
            public DateTime LastUpdate;
            public bool ShowText;
            public bool IsCleared;
            public DisplayTypes DisplayType;
        }

        List<LCDStatus> lcdStatuses = new List<LCDStatus>();

        struct CachedBitmap
        {
            public Bitmap Bitmap;
            public DateTime LastUpdate;
        }

        CachedBitmap[] cachedBitmaps = new CachedBitmap[((int)DisplayTypes.Invalid - 1) * 2];

        bool bitmapDrawnThisFrame;

        int runningFrame;

        class Closures
        {
            public readonly IMyCubeGrid BaseGrid;
            public readonly Func<IMyTerminalBlock, bool> PowerBlockCollectorFunc;
            public readonly Func<IMyTerminalBlock, bool> PowerProducerCollectorFunc;
            public readonly Func<IMyTextPanel, bool> LCDCollectorFunc;
            public readonly Func<LCDStatus, bool> FindLCDStatusByLCDPred;
            public readonly Func<LCDStatus, bool> FindLCDStatusByNamePred;
            public readonly Func<IMySensorBlock, bool> SensorCollectorFunc;

            public Closures(IMyCubeGrid grid)
            {
                BaseGrid = grid;
                PowerBlockCollectorFunc = IsValidPowerBlock;
                PowerProducerCollectorFunc = IsValidPowerProducer;
                LCDCollectorFunc = IsValidLCD;
                FindLCDStatusByLCDPred = FindLCDStatusByLCD;
                FindLCDStatusByNamePred = FindLCDStatusByName;
                SensorCollectorFunc = IsValidSensorBlock;
            }

            public bool IsValidPowerBlock(IMyTerminalBlock block) => block.CubeGrid == BaseGrid
                || (Include_Power_Of_Connected_Small_Grids && block.CubeGrid.GridSizeEnum == MyCubeSize.Small)
                || (Include_Power_Of_Connected_Large_Grids && block.CubeGrid.GridSizeEnum == MyCubeSize.Large);

            public bool IsValidPowerProducer(IMyTerminalBlock block) => block is IMyReactor || block is IMyBatteryBlock || block is IMySolarPanel;
            public bool IsValidLCD(IMyTextPanel lcd) => (Display_On_LCDs_Of_Connected_Grids || lcd.CubeGrid == BaseGrid) && lcd.IsWorking && lcd.CustomName.Contains(Lcd_Name_Tag, StringComparison.Ordinal);
            public bool IsValidSensorBlock(IMySensorBlock block) => (Display_On_LCDs_Of_Connected_Grids || block.CubeGrid == BaseGrid) && block.CustomName.Contains(Sensor_Name_Tag, StringComparison.Ordinal) && !string.IsNullOrWhiteSpace(block.CustomData);

            public IMyTextPanel LCDPanel;
            public bool FindLCDStatusByLCD(LCDStatus status) => status.LCD == LCDPanel;

            public string LCDName;
            public bool FindLCDStatusByName(LCDStatus status) => status.LCD.CustomName == LCDName;
        }

        struct RGB
        {
            public byte R, G, B;

            public RGB(byte r, byte g, byte b)
            {
                R = r;
                G = g;
                B = b;
            }
        }

        struct Colors
        {
            public char Background;
            public char Text;
            public char Graph_Axes;
            public char Current_Usage;
            public char Required_Power;
            public char Battery_Charge;
            public char Max_Battery_Charge;
            public char Current_Solar;
            public char Max_Solar;
            public char Reactor_Output;
        }

        Colors colors;

        readonly char[] clearValues;

        BitmapFont font;
        BitmapFont.CharBitmap squareBmp;

        public Program()
        {
            font = new BitmapFont();
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
            closures = new Closures(Me.CubeGrid);

            colors.Background = RgbToPixel(Background_Color);
            colors.Text = RgbToPixel(Text_Color);
            colors.Graph_Axes = RgbToPixel(Graph_Axes_Color);
            colors.Current_Usage = RgbToPixel(Current_Usage_Color);
            colors.Required_Power = RgbToPixel(Required_Power_Color);
            colors.Battery_Charge = RgbToPixel(Battery_Charge_Color);
            colors.Max_Battery_Charge = RgbToPixel(Max_Battery_Charge_Color);
            colors.Current_Solar = RgbToPixel(Current_Solar_Color);
            colors.Max_Solar = RgbToPixel(Max_Solar_Color);
            colors.Reactor_Output = RgbToPixel(Reactor_Output_Color);

            clearValues = Enumerable.Repeat(colors.Background, Wide_LCD_Res_X * Wide_LCD_Res_Y + Wide_LCD_Res_Y).ToArray();

            squareBmp = new BitmapFont.CharBitmap(3, 3, "#########");

            var now = DateTime.Now;

            for (int i = 0; i < sampleTimes.Length; i++)
                sampleTimes[i] = now;

            if (Storage != "")
                Load(Storage);

            Storage = "";

            lastDataUpdate = now - TimeSpan.FromSeconds(Data_Update_Interval_In_Seconds);
        }

        void Load(string storage)
        {
            var deserializer = DataChunks.Deserializer.LoadFromData(storage, 8, Echo);

            if (!deserializer.IsValid)
            {
                Echo("ERROR: Invalid saved data");
                return;
            }

            if (!deserializer.LoadFloatArray("powerUsageValues", powerUsageValues))
                Echo("ERROR: Failed to load powerUsageValues");

            if (!deserializer.LoadFloatArray("powerReqValues", powerReqValues))
                Echo("ERROR: Failed to load powerReqValues");

            if (!deserializer.LoadFloatArray("batteryChargeValues", batteryChargeValues))
                Echo("ERROR: Failed to load batteryChargeValues");

            deserializer.LoadFloatArray("batteryMaxChargeValues", batteryMaxChargeValues);
            deserializer.LoadFloatArray("solarMaxOutputValues", solarMaxOutputValues);

            if (!deserializer.LoadFloatArray("solarOutputValues", solarOutputValues))
                Echo("ERROR: Failed to load solarOutputValues");

            deserializer.LoadFloatArray("reactorOutputValues", reactorOutputValues);

            var storedSampleTimes = deserializer.LoadLongArray("sampleTimes");

            if (storedSampleTimes != null)
            {
                if (sampleTimes.Length > storedSampleTimes.Length)
                {
                    int offset = sampleTimes.Length - storedSampleTimes.Length;

                    for (int i = 0; i < storedSampleTimes.Length; i++)
                        sampleTimes[offset + i] = DateTime.FromBinary(storedSampleTimes[i]);
                }
                else if (storedSampleTimes.Length > sampleTimes.Length)
                {
                    int offset = storedSampleTimes.Length - sampleTimes.Length;

                    for (int i = 0; i < sampleTimes.Length; i++)
                        sampleTimes[i] = DateTime.FromBinary(storedSampleTimes[offset + i]);
                }
                else
                {
                    for (int i = 0; i < storedSampleTimes.Length; i++)
                        sampleTimes[i] = DateTime.FromBinary(storedSampleTimes[i]);
                }
            }
            else
            {
                Echo("ERROR: Failed to load sampleTimes");
            }
        }

        public void Save()
        {
            var serializer = DataChunks.Serializer.Create();

            serializer.AddArray("powerUsageValues", powerUsageValues);
            serializer.AddArray("powerReqValues", powerReqValues);
            serializer.AddArray("batteryChargeValues", batteryChargeValues);
            serializer.AddArray("batteryMaxChargeValues", batteryMaxChargeValues);
            serializer.AddArray("solarMaxOutputValues", solarMaxOutputValues);
            serializer.AddArray("solarOutputValues", solarOutputValues);
            serializer.AddArray("reactorOutputValues", reactorOutputValues);

            var binarySampleTimes = new long[sampleTimes.Length];

            for (int i = 0; i < binarySampleTimes.Length; i++)
                binarySampleTimes[i] = sampleTimes[i].ToBinary();

            serializer.AddArray("sampleTimes", binarySampleTimes);

            Storage = serializer.Save();
        }

        public void Main(string argument, UpdateType updateType)
        {
            if ((updateType & updateTypes) == 0)
                return;

            Echo(animFrames[runningFrame]);
            Echo($"Tracking {lcdStatuses.Count} LCDs");

            runningFrame = (runningFrame + 1) % animFrames.Length;

            GridTerminalSystem.GetBlocksOfType(blocks, closures.PowerBlockCollectorFunc);

            var now = DateTime.Now;

            double secondsSinceLastUpdate = (now - lastDataUpdate).TotalSeconds;

            secondsSinceLastUpdate = Math.Min(secondsSinceLastUpdate, Data_Update_Interval_In_Seconds * sampleTimes.Length);

            if (Zero_Fill_Missing_Values)
            {
                while (secondsSinceLastUpdate > Data_Update_Interval_In_Seconds * 2)
                {
                    AddValueToArray(sampleTimes, now - TimeSpan.FromSeconds(secondsSinceLastUpdate));
                    AddZeroPowerValues();
                    secondsSinceLastUpdate -= Data_Update_Interval_In_Seconds;
                }
            }

            if (secondsSinceLastUpdate > Data_Update_Interval_In_Seconds)
            {
                RecordPowerValues(blocks);
                AddValueToArray(sampleTimes, now);
                lastDataUpdate = now;
            }

            bool refreshAutoNow = false;

            if (argument == "togglestatus")
            {
                CycleDisplayType(now);
                refreshAutoNow = true;
            }
            else if (Auto_Cycle_Graphs && (now - lastDisplayCycle).TotalSeconds > Auto_Cycle_Interval_In_Seconds)
            {
                CycleDisplayType(now);
            }

            for (int i = 0; i < lcdStatuses.Count; i++)
            {
                var status = lcdStatuses[i];
                status.ShowText = true;
                lcdStatuses[i] = status;
            }

            UpdateSensors();
            UpdateLCDs(now, refreshAutoNow);
        }

        void UpdateSensors()
        {
            GridTerminalSystem.GetBlocksOfType(sensors, closures.SensorCollectorFunc);

            if (sensors.Count <= 0)
                return;

            Echo($"Tracking {sensors.Count} sensors");

            foreach (var sensor in sensors)
            {
                sensor.DetectedEntities(detectedEntities);
                bool sensorActive = detectedEntities.Count > 0;

                // TODO: Custom struct enumerator 
                var lcdNames = sensor.CustomData.SplitOnChar('\n');

                foreach (var item in lcdNames)
                {
                    closures.LCDName = item;

                    int statusIndex = lcdStatuses.FindIndex(closures.FindLCDStatusByNamePred);

                    if (statusIndex != -1)
                    {
                        var status = lcdStatuses[statusIndex];
                        status.ShowText = sensorActive;
                        lcdStatuses[statusIndex] = status;
                    }
                }
            }
        }

        void UpdateLCDs(DateTime now, bool refreshAutoNow)
        {
            bitmapDrawnThisFrame = false;

            GridTerminalSystem.GetBlocksOfType(panels, closures.LCDCollectorFunc);

            foreach (var item in panels)
            {
                closures.LCDPanel = item;

                int statusIndex = lcdStatuses.FindIndex(closures.FindLCDStatusByLCDPred);

                LCDStatus status;

                if (statusIndex == -1)
                {
                    statusIndex = lcdStatuses.Count;
                    lcdStatuses.Add(status = new LCDStatus { LCD = item, LastUpdate = now - TimeSpan.FromSeconds(LCD_Refresh_Interval_In_Seconds) });

                    item.Font = "Monospace";
                    item.FontSize = Pixel_Font_Scale;
                    item.ShowPublicTextOnScreen();
                }
                else
                {
                    status = lcdStatuses[statusIndex];
                }

                if (!status.ShowText && !status.IsCleared)
                {
                    item.WritePublicText("");
                    status.IsCleared = true;
                    continue;
                }

                if (!bitmapDrawnThisFrame && (refreshAutoNow || (now - status.LastUpdate).TotalSeconds > LCD_Refresh_Interval_In_Seconds))
                {
                    bool isWide = Enable_Long_History && wideLCDSubtypes.ContainsString(item.BlockDefinition.SubtypeId);
                    var displayTypes = GetDisplayTypes(item.CustomData);
                    var nextDisplayType = GetNextDisplayType(status.DisplayType, displayTypes);

                    if (status.DisplayType == DisplayTypes.Invalid && nextDisplayType != DisplayTypes.Invalid)
                    {
                        item.Font = "Monospace";
                        item.FontSize = Pixel_Font_Scale;
                    }

                    status.DisplayType = nextDisplayType;

                    if (status.DisplayType == DisplayTypes.Invalid)
                    {
                        item.Font = "Debug";
                        item.FontSize = 1f;
                        item.WritePublicText("ERROR: Invalid graph types in CustomData\nNOTE: LCDs may take a few seconds to\nupdate after changes");
                    }
                    else
                    {
                        var bitmap = GetBitmapForDisplayType(status.DisplayType, isWide, now);

                        BlitBitmap(item, bitmap);
                    }

                    status.LastUpdate = now;
                    status.IsCleared = false;
                }

                lcdStatuses[statusIndex] = status;
            }

            for (int i = 0; i < lcdStatuses.Count; i++)
            {
                if (!panels.Contains(lcdStatuses[i].LCD))
                    lcdStatuses.RemoveAt(i--);
            }
        }

        static DisplayTypes GetNextDisplayType(DisplayTypes current, DisplayTypes choices)
        {
            if (current == DisplayTypes.Invalid || choices == DisplayTypes.Invalid)
                return DisplayTypes.Invalid;

            if (choices == DisplayTypes.Auto)
                return DisplayTypes.Auto;

            if (current == DisplayTypes.Auto)
                current = DisplayTypes.PowerUsage;

            do
            {
                current = (DisplayTypes)((int)current << 1);

                if (current == DisplayTypes.Invalid)
                    current = DisplayTypes.PowerUsage;
            }
            while ((choices & current) == 0);

            return current;
        }

        void CycleDisplayType(DateTime now)
        {
            do
            {
                displayType = (DisplayTypes)((int)displayType << 1);

                if (displayType == DisplayTypes.Invalid)
                    displayType = DisplayTypes.PowerUsage;

            }
            while (!IsDisplayTypeRelevant(displayType));

            lastDisplayCycle = now;
        }

        void RecordPowerValues(List<IMyTerminalBlock> blocks)
        {
            float currentPowerUsage, currentPowerRequired;
            GetPowerState(blocks, out currentPowerUsage, out currentPowerRequired);

            AddValueToArray(powerUsageValues, currentPowerUsage);
            AddValueToArray(powerReqValues, currentPowerRequired);

            float storedPower, maxStoredPower;
            GetBatteryCharge(blocks, out storedPower, out maxStoredPower);
            AddValueToArray(batteryChargeValues, storedPower);
            AddValueToArray(batteryMaxChargeValues, maxStoredPower);

            float solarMaxPower, solarPower;
            GetSolarOutput(blocks, out solarMaxPower, out solarPower);
            AddValueToArray(solarMaxOutputValues, solarMaxPower);
            AddValueToArray(solarOutputValues, solarPower);

            float reactorPower;
            GetReactorOutput(blocks, out reactorPower);
            AddValueToArray(reactorOutputValues, reactorPower);
        }

        void AddZeroPowerValues()
        {
            AddValueToArray(powerUsageValues, 0);
            AddValueToArray(powerReqValues, 0);
            AddValueToArray(batteryChargeValues, 0);
            AddValueToArray(batteryMaxChargeValues, 0);
            AddValueToArray(solarMaxOutputValues, 0);
            AddValueToArray(solarOutputValues, 0);
            AddValueToArray(reactorOutputValues, 0);
        }

        bool IsDisplayTypeRelevant(DisplayTypes displayType)
        {
            switch (displayType)
            {
                case DisplayTypes.BatteryCharge: return batteries.Count > 0;
                case DisplayTypes.SolarOutput: return solarPanels.Count > 0;
                case DisplayTypes.ReactorOutput: return reactors.Count > 0;
            }

            return true;
        }

        static DisplayTypes GetDisplayTypes(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return DisplayTypes.Auto;

            if (text.Contains("\n", StringComparison.Ordinal))
            {
                var lines = text.SplitOnChar('\n');
                var displayTypes = DisplayTypes.Auto;

                for (int i = 0; i < lines.Length; i++)
                    displayTypes |= GetDisplayType(lines[i]);

                if ((displayTypes & DisplayTypes.Invalid) != 0)
                    displayTypes = DisplayTypes.Invalid;

                return displayTypes;
            }

            return GetDisplayType(text);
        }

        static DisplayTypes GetDisplayType(string typeString)
        {
            switch (typeString.ToLowerInvariant().Trim())
            {
                case "usage": return DisplayTypes.PowerUsage;
                case "battery": return DisplayTypes.BatteryCharge;
                case "solar": return DisplayTypes.SolarOutput;
                case "reactor": return DisplayTypes.ReactorOutput;
                default: return DisplayTypes.Invalid;
            }
        }

        Bitmap GetBitmapForDisplayType(DisplayTypes type, bool isWide, DateTime now)
        {
            if (type == DisplayTypes.Auto)
                type = displayType;

            int bmpIndex = (int)type - 1;

            if (isWide)
                bmpIndex += cachedBitmaps.Length / 2;

            var bitmap = cachedBitmaps[bmpIndex];

            if (bitmap.Bitmap.Pixels == null)
            {
                cachedBitmaps[bmpIndex].Bitmap = CreateBitmap(isWide ? Wide_LCD_Res_X : LCD_Res_X, LCD_Res_Y);
                bitmap = cachedBitmaps[bmpIndex];
                DrawPicture(bitmap.Bitmap, type, now);
                cachedBitmaps[bmpIndex].LastUpdate = now;
            }
            else if (!bitmapDrawnThisFrame && (now - bitmap.LastUpdate).TotalSeconds > Math.Max(Data_Update_Interval_In_Seconds, LCD_Refresh_Interval_In_Seconds))
            {
                DrawPicture(bitmap.Bitmap, type, now);
                cachedBitmaps[bmpIndex].LastUpdate = now;
            }

            return bitmap.Bitmap;
        }

        static Bitmap CreateBitmap(int width, int height)
        {
            int stride = width + 1;
            var pixels = new char[stride * height - 1];

            for (int i = 0; i < height - 1; i++)
                pixels[i * stride + width] = '\n';

            return new Bitmap { Width = width, Height = height, Stride = stride, Pixels = pixels };
        }

        static void AddValueToArray<T>(T[] array, T value)
        {
            Array.Copy(array, 1, array, 0, array.Length - 1);

            array[array.Length - 1] = value;
        }

        static float GetMaxValue(float[] array, int maxCount)
        {
            float max = 0;

            for (int i = array.Length - maxCount; i < array.Length; i++)
                max = Math.Max(max, array[i]);

            return max;
        }

        void DrawPicture(Bitmap bitmap, DisplayTypes displayType, DateTime now)
        {
            ClearBitmap(ref bitmap);

            var graphArea = new Rectangle(5, 20, bitmap.Width - 10, bitmap.Height - 30);

            if (displayType == DisplayTypes.PowerUsage || displayType == DisplayTypes.SolarOutput)
            {
                graphArea.Y += 7;
                graphArea.Height -= 7;
            }

            DrawGraphAxes(ref bitmap, ref graphArea, colors.Graph_Axes);

            var textPos = new Point(graphArea.X, graphArea.Bottom + 2);
            DrawString(ref bitmap, ref textPos, GetTimeSpanString(now - sampleTimes[0]), colors.Text);
            DrawString(ref bitmap, textPos, " ago", colors.Text);
            DrawString(ref bitmap, new Point(graphArea.Right - 15, graphArea.Bottom + 2), "now", colors.Text);

            graphArea.X++; // Adjust position of curves 

            switch (displayType)
            {
                case DisplayTypes.PowerUsage:
                    {
                        DrawString(ref bitmap, new Point(bitmap.Width - font.MeasureString("Power Usage") - 1, 1), "Power Usage", colors.Text);

                        float maxMegaWatts = Math.Max(GetMaxValue(powerReqValues, graphArea.Width), GetMaxValue(powerUsageValues, graphArea.Width));
                        float scale = 1f / maxMegaWatts;

                        DrawCurve(ref bitmap, powerReqValues, graphArea, scale, colors.Required_Power);
                        DrawCurve(ref bitmap, powerUsageValues, graphArea, scale, colors.Current_Usage);

                        textPos = new Point(1, 1);
                        DrawCharBitmap(ref bitmap, textPos, squareBmp, colors.Current_Usage);
                        textPos.X += squareBmp.Width + 1;
                        DrawString(ref bitmap, ref textPos, "Current: ", colors.Text);
                        DrawWatts(ref bitmap, ref textPos, colors.Text, powerUsageValues[powerUsageValues.Length - 1] * 1E6f);
                        textPos.Y += 8;
                        textPos.X = 1;
                        DrawCharBitmap(ref bitmap, textPos, squareBmp, colors.Required_Power);
                        textPos.X += squareBmp.Width + 1;
                        DrawString(ref bitmap, ref textPos, "Required: ", colors.Text);
                        DrawWatts(ref bitmap, ref textPos, colors.Text, powerReqValues[powerReqValues.Length - 1] * 1E6f);
                        textPos.Y += 8;
                        textPos.X = 1;
                        DrawString(ref bitmap, ref textPos, "Max: ", colors.Text);
                        DrawWatts(ref bitmap, ref textPos, colors.Text, maxMegaWatts * 1E6f);
                    }
                    break;
                case DisplayTypes.BatteryCharge:
                    {
                        DrawString(ref bitmap, new Point(bitmap.Width - font.MeasureString("Battery Charge") - 1, 1), "Battery Charge", colors.Text);

                        float megaWattHours = Math.Max(GetMaxValue(batteryMaxChargeValues, graphArea.Width), GetMaxValue(batteryChargeValues, graphArea.Width));
                        float scale = 1f / megaWattHours;
                        float maxCharge = batteryMaxChargeValues[batteryMaxChargeValues.Length - 1];

                        if (maxCharge < megaWattHours)
                        {
                            int maxChargeLineY = graphArea.Y + (graphArea.Height - (int)(maxCharge * scale * graphArea.Height));
                            DrawLine(ref bitmap, new Point(graphArea.X, maxChargeLineY), new Point(graphArea.Right, maxChargeLineY), colors.Max_Battery_Charge);
                        }

                        DrawCurve(ref bitmap, batteryChargeValues, graphArea, scale, colors.Battery_Charge);

                        textPos = new Point(1, 1);
                        DrawCharBitmap(ref bitmap, textPos, squareBmp, colors.Battery_Charge);
                        textPos.X += squareBmp.Width + 1;
                        DrawString(ref bitmap, ref textPos, "Current: ", colors.Text);
                        DrawWattHours(ref bitmap, ref textPos, colors.Text, batteryChargeValues[batteryChargeValues.Length - 1] * 1E6f);
                        textPos.Y += 8;
                        textPos.X = 1;
                        DrawString(ref bitmap, ref textPos, "Max: ", colors.Text);
                        DrawWattHours(ref bitmap, ref textPos, colors.Text, megaWattHours * 1E6f);
                    }
                    break;
                case DisplayTypes.SolarOutput:
                    {
                        DrawString(ref bitmap, new Point(bitmap.Width - font.MeasureString("Solar Output") - 1, 1), "Solar Output", colors.Text);

                        float megaWatts = Math.Max(GetMaxValue(solarMaxOutputValues, graphArea.Width), GetMaxValue(solarOutputValues, graphArea.Width));
                        float scale = 1f / megaWatts;

                        DrawCurve(ref bitmap, solarMaxOutputValues, graphArea, scale, colors.Max_Solar);
                        DrawCurve(ref bitmap, solarOutputValues, graphArea, scale, colors.Current_Solar);

                        textPos = new Point(1, 1);
                        DrawCharBitmap(ref bitmap, textPos, squareBmp, colors.Max_Solar);
                        textPos.X += squareBmp.Width + 1;
                        DrawString(ref bitmap, ref textPos, "Generated: ", colors.Text);
                        DrawWatts(ref bitmap, ref textPos, colors.Text, solarMaxOutputValues[solarMaxOutputValues.Length - 1] * 1E6f);
                        textPos.Y += 8;
                        textPos.X = 1;
                        DrawCharBitmap(ref bitmap, textPos, squareBmp, colors.Current_Solar);
                        textPos.X += squareBmp.Width + 1;
                        DrawString(ref bitmap, ref textPos, "Used: ", colors.Text);
                        DrawWatts(ref bitmap, ref textPos, colors.Text, solarOutputValues[solarOutputValues.Length - 1] * 1E6f);
                        textPos.Y += 8;
                        textPos.X = 1;
                        DrawString(ref bitmap, ref textPos, "Max: ", colors.Text);
                        DrawWatts(ref bitmap, ref textPos, colors.Text, megaWatts * 1E6f);
                    }
                    break;
                case DisplayTypes.ReactorOutput:
                    {
                        DrawString(ref bitmap, new Point(bitmap.Width - font.MeasureString("Reactor Output") - 1, 1), "Reactor Output", colors.Text);

                        float megaWatts = GetMaxValue(reactorOutputValues, graphArea.Width);
                        float scale = 1f / megaWatts;

                        DrawCurve(ref bitmap, reactorOutputValues, graphArea, scale, colors.Reactor_Output);

                        textPos = new Point(1, 1);
                        DrawCharBitmap(ref bitmap, textPos, squareBmp, colors.Reactor_Output);
                        textPos.X += squareBmp.Width + 1;
                        DrawString(ref bitmap, ref textPos, "Current: ", colors.Text);
                        DrawWatts(ref bitmap, ref textPos, colors.Text, reactorOutputValues[reactorOutputValues.Length - 1] * 1E6f);
                        textPos.Y += 8;
                        textPos.X = 1;
                        DrawString(ref bitmap, ref textPos, "Max: ", colors.Text);
                        DrawWatts(ref bitmap, ref textPos, colors.Text, megaWatts * 1E6f);
                    }
                    break;
            }

            bitmapDrawnThisFrame = true;
        }

        string GetTimeSpanString(TimeSpan timeSpan)
        {
            var text = stringBuilder.AppendTimeSpan(timeSpan).ToString();
            stringBuilder.Clear();
            return text;
        }

        void DrawGraphAxes(ref Bitmap bitmap, ref Rectangle graphArea, char color)
        {
            DrawLine(ref bitmap, graphArea.BottomLeft(), graphArea.BottomRight(), color);
            DrawLine(ref bitmap, graphArea.BottomLeft(), graphArea.Location, color);

            //DrawChar(ref bitmap, new Point(graphArea.X, graphArea.Bottom + 2), '0', color); 
            //DrawChar(ref bitmap, new Point(graphArea.Right, graphArea.Bottom + 2), '1', color); 

            //DrawChar(ref bitmap, new Point(graphArea.X - 5, graphArea.Bottom - 5), '0', color); 
            //DrawChar(ref bitmap, new Point(graphArea.X - 5, graphArea.Y), '1', color); 
        }

        void BlitBitmap(IMyTextPanel textPanel, Bitmap bitmap)
        {
            stringBuilder.Append(bitmap.Pixels);
            textPanel.WritePublicText(stringBuilder);
            stringBuilder.Clear();
        }

        void ClearBitmap(ref Bitmap bitmap/*, char clearChar*/)
        {
            //for (int i = 0; i < bitmap.Pixels.Length; i++) 
            //    bitmap.Pixels[i] = clearChar; 

            Array.Copy(clearValues, 0, bitmap.Pixels, 0, bitmap.Pixels.Length);

            for (int i = 0; i < bitmap.Height - 1; i++)
                bitmap.Pixels[i * bitmap.Stride + bitmap.Width] = '\n';
        }

        static char RgbToPixel(RGB color) => RgbToPixel(color.R, color.G, color.B);
        static char RgbToPixel(byte r, byte g, byte b) => (char)(0xe100 + (r << 6) + (g << 3) + b);

        static void DrawLine(ref Bitmap bitmap, Point pos1, Point pos2, char color)
        {
            int x = pos1.X;
            int y = pos1.Y;
            int x2 = pos2.X;
            int y2 = pos2.Y;

            x = MathHelper.Clamp(x, 0, bitmap.Width - 1);
            x2 = MathHelper.Clamp(x2, 0, bitmap.Width - 1);
            y = MathHelper.Clamp(y, 0, bitmap.Height - 1);
            y2 = MathHelper.Clamp(y2, 0, bitmap.Height - 1);

            int w = x2 - x;
            int h = y2 - y;
            int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;

            if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
            if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
            if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;

            int longest = Math.Abs(w);
            int shortest = Math.Abs(h);

            if (longest <= shortest)
            {
                longest = Math.Abs(h);
                shortest = Math.Abs(w);

                if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;

                dx2 = 0;
            }

            int numerator = longest >> 1;

            for (int i = 0; i <= longest; i++)
            {
                bitmap.Pixels[y * bitmap.Stride + x] = color;

                numerator += shortest;

                if (numerator >= longest)
                {
                    numerator -= longest;
                    x += dx1;
                    y += dy1;
                }
                else
                {
                    x += dx2;
                    y += dy2;
                }
            }
        }

        static void DrawCurve(ref Bitmap bitmap, float[] values, Rectangle area, float heightScale, char color)
        {
            int valueOffset = 0;

            if (values.Length > area.Width)
                valueOffset = values.Length - area.Width;

            int valueCount = Math.Min(values.Length, area.Width);

            for (int i = 0; i < valueCount; i++)
            {
                float v = values[valueOffset + i] * heightScale;
                int y = Math.Min((int)(v * area.Height), area.Height);

                if (v >= 0 && v <= 1)
                    bitmap.Pixels[(area.Y + (area.Height - y)) * bitmap.Stride + area.X + i] = color;

                if (i == 0)
                    continue;

                float pv = MathHelper.Saturate(values[valueOffset + i - 1] * heightScale);
                int py = Math.Min((int)(pv * area.Height), area.Height);

                if (y > py)
                {
                    for (; py < y; py++)
                        bitmap.Pixels[(area.Y + (area.Height - py)) * bitmap.Stride + area.X + i] = color;
                }
                else if (y < py)
                {
                    for (; y < py; y++)
                        bitmap.Pixels[(area.Y + (area.Height - y)) * bitmap.Stride + area.X + i - 1] = color;
                }
            }
        }

        void DrawChar(ref Bitmap bitmap, Point pos, char character, char color)
            => DrawChar(ref bitmap, ref pos, character, color);

        void DrawChar(ref Bitmap bitmap, ref Point pos, char character, char color)
        {
            if (character == ' ')
            {
                pos.X += 3;
                return;
            }

            var charBmp = font[character];

            DrawCharBitmap(ref bitmap, pos, charBmp, color);

            pos.X += charBmp.Width + 1;
        }

        static void DrawCharBitmap(ref Bitmap bitmap, Point pos, BitmapFont.CharBitmap charBmp, char color)
        {
            var charPos = pos;
            charPos.Y += charBmp.YOffset;

            var charBmpPos = new Point();

            if (charPos.X < 0) charBmpPos.X = -charPos.X;
            if (charPos.Y < 0) charBmpPos.Y = -charPos.Y;

            var charSize = new Point(charBmp.Width, charBmp.Height);
            int charRight = charPos.X + charBmp.Width;
            int charBottom = charPos.Y + charBmp.Height;

            if (charRight > bitmap.Width)
                charSize.X = charBmp.Width - (charRight - bitmap.Width);

            if (charBottom > bitmap.Height)
                charSize.Y = charBmp.Height - (charBottom - bitmap.Height);

            for (int y = charBmpPos.Y; y < charSize.Y; y++)
            {
                for (int x = charBmpPos.X; x < charSize.X; x++)
                {
                    int charPxIndex = y * charBmp.Width + x;

                    if (charBmp.Pixels[charPxIndex] == '#')
                    {
                        int bmpIndex = (charPos.Y + y) * bitmap.Stride + charPos.X + x;
                        bitmap.Pixels[bmpIndex] = color;
                    }
                }
            }
        }

        void DrawString(ref Bitmap bitmap, Point pos, string text, char color)
            => DrawString(ref bitmap, ref pos, text, color);

        void DrawString(ref Bitmap bitmap, ref Point pos, string text, char color)
        {
            //Echo(text); 

            for (int i = 0; i < text.Length; i++)
                DrawChar(ref bitmap, ref pos, text[i], color);
        }

        void DrawWatts(ref Bitmap bitmap, Point pos, char color, float watts)
            => DrawWatts(ref bitmap, ref pos, color, watts);

        void DrawWatts(ref Bitmap bitmap, ref Point pos, char color, float watts)
        {
            //Echo($"{watts}"); 

            if (watts == 0)
            {
                DrawChar(ref bitmap, ref pos, '0', color);
            }
            else if (watts > 1E9f)
            {
                var a = watts / 1E9f;
                DrawString(ref bitmap, ref pos, Math.Round(a, 1).ToString(), color);
                DrawChar(ref bitmap, ref pos, 'G', color);
            }
            else if (watts > 1E6f)
            {
                var a = watts / 1E6f;
                DrawString(ref bitmap, ref pos, Math.Round(a, 1).ToString(), color);
                DrawChar(ref bitmap, ref pos, 'M', color);
            }
            else if (watts > 1000)
            {
                var a = watts / 1000;
                DrawString(ref bitmap, ref pos, Math.Round(a, 1).ToString(), color);
                DrawChar(ref bitmap, ref pos, 'K', color);
            }
            else if (watts < 0.1f)
            {
                DrawString(ref bitmap, ref pos, "<0.1", color);
            }
            else
            {
                DrawString(ref bitmap, ref pos, Math.Round(watts, 1).ToString(), color);
            }

            DrawChar(ref bitmap, ref pos, 'W', color);
        }

        void DrawWattHours(ref Bitmap bitmap, Point pos, char color, float wattHours)
            => DrawWattHours(ref bitmap, ref pos, color, wattHours);

        void DrawWattHours(ref Bitmap bitmap, ref Point pos, char color, float wattHours)
        {
            DrawWatts(ref bitmap, ref pos, color, wattHours);
            DrawChar(ref bitmap, ref pos, 'h', color);
        }

        readonly MyDefinitionId electricityId = new MyDefinitionId(typeof(MyObjectBuilder_GasProperties), "Electricity");
        readonly MyDefinitionId oxygenId = new MyDefinitionId(typeof(MyObjectBuilder_GasProperties), "Oxygen");
        readonly MyDefinitionId hydrogenId = new MyDefinitionId(typeof(MyObjectBuilder_GasProperties), "Hydrogen");

        void GetPowerState(List<IMyTerminalBlock> blocks, out float currentPowerUsage, out float currentPowerRequired)
        {
            currentPowerUsage = 0;
            currentPowerRequired = 0;

            foreach (var item in blocks)
            {
                MyResourceSinkComponent sink;
                if (item.Components.TryGet(out sink) && sink.AcceptsResourceType(electricityId))
                {
                    var currentInput = sink.CurrentInputByType(electricityId);
                    var maxInput = sink.MaxRequiredInputByType(electricityId);
                    var requiredInput = sink.RequiredInputByType(electricityId);

                    var batt = item as IMyBatteryBlock;

                    if (batt != null && batt.CurrentOutput >= currentInput)
                        continue;

                    currentPowerUsage += currentInput;
                    currentPowerRequired += requiredInput;
                }
            }

            //blocks.GetItemsOfType(producerBlocks, closures.PowerProducerCollectorFunc); 

            //foreach (var item in blocks) 
            //{ 
            //    MyResourceSourceComponent source; 
            //    if (item.Components.TryGet(out source)) 
            //    { 
            //        var currentOutput = source.CurrentOutputByType(electricityId); 
            //        var maxOutput = source.MaxOutputByType(electricityId); 
            //    } 
            //} 
        }

        void GetBatteryCharge(List<IMyTerminalBlock> blocks, out float storedPower, out float maxStoredPower)
        {
            storedPower = 0;
            maxStoredPower = 0;

            blocks.GetItemsOfType(batteries, closures.PowerBlockCollectorFunc);

            foreach (var item in batteries)
            {
                storedPower += item.CurrentStoredPower;
                maxStoredPower += item.MaxStoredPower;
            }
        }

        void GetSolarOutput(List<IMyTerminalBlock> blocks, out float solarMaxPower, out float solarPower)
        {
            solarMaxPower = 0;
            solarPower = 0;

            blocks.GetItemsOfType(solarPanels);

            foreach (var item in solarPanels)
            {
                solarMaxPower += item.MaxOutput;
                solarPower += item.CurrentOutput;
            }
        }

        void GetReactorOutput(List<IMyTerminalBlock> blocks, out float reactorPower)
        {
            reactorPower = 0;

            blocks.GetItemsOfType(reactors);

            foreach (var item in reactors)
            {
                reactorPower += item.CurrentOutput;
            }
        }
    }

    public class BitmapFont
    {
        public struct CharBitmap
        {
            public int Width;
            public int Height;
            public string Pixels;
            public int YOffset;

            public CharBitmap(int width, int height, string pixels)
            {
                Width = width;
                Height = height;
                Pixels = pixels;
                YOffset = 0;
            }

            public CharBitmap(int width, int height, string pixels, int yOffset)
            {
                Width = width;
                Height = height;
                Pixels = pixels;
                YOffset = yOffset;
            }
        }

        const char startChar = '\'';
        const char endChar = 'z';
        readonly CharBitmap[] charBitmaps;

        public int GetCharIndex(char character)
        {
            int charIndex = character - startChar;

            if (charIndex < 0 || charIndex >= charBitmaps.Length)
                throw new ArgumentException($"Character out of bounds '{character}'");

            return charIndex;
        }

        public CharBitmap this[char character] => charBitmaps[GetCharIndex(character)];
        public CharBitmap this[int index] => charBitmaps[index];

        public int MeasureString(string text)
        {
            int width = 0;

            for (int i = 0; i < text.Length; i++)
            {
                var character = text[i];

                if (character == ' ')
                {
                    width += 3;
                    continue;
                }

                var charBmp = this[character];
                width += charBmp.Width + 1;
            }

            return width;
        }

        public BitmapFont()
        {
            const char s = startChar;

            charBitmaps = new CharBitmap[endChar - s + 1];

            charBitmaps['\'' - s] = new CharBitmap(1, 2, "##");
            charBitmaps['(' - s] = new CharBitmap(2, 5, " ## # #  #");
            charBitmaps[')' - s] = new CharBitmap(2, 5, "#  # # ## ");
            charBitmaps['*' - s] = new CharBitmap(5, 3, "# # # ### # # #", 1);
            charBitmaps['+' - s] = new CharBitmap(3, 3, " # ### # ", 1);
            charBitmaps[',' - s] = new CharBitmap(2, 2, " ## ", 4);
            charBitmaps['-' - s] = new CharBitmap(3, 1, "###", 2);
            charBitmaps['.' - s] = new CharBitmap(1, 1, "#", 4);
            charBitmaps['/' - s] = new CharBitmap(5, 5, "    #   #   #   #   #    ");

            charBitmaps['0' - s] = new CharBitmap(3, 5, "#### ## ## ####");
            charBitmaps['1' - s] = new CharBitmap(3, 5, " #  #  #  #  # ");
            charBitmaps['2' - s] = new CharBitmap(3, 5, "###  #####  ###");
            charBitmaps['3' - s] = new CharBitmap(3, 5, "###  ####  ####");
            charBitmaps['4' - s] = new CharBitmap(3, 5, "# ## ####  #  #");
            charBitmaps['5' - s] = new CharBitmap(3, 5, "####  ###  ####");
            charBitmaps['6' - s] = new CharBitmap(3, 5, "####  #### ####");
            charBitmaps['7' - s] = new CharBitmap(3, 5, "###  #  # #  # ");
            charBitmaps['8' - s] = new CharBitmap(3, 5, "#### ##### ####");
            charBitmaps['9' - s] = new CharBitmap(3, 5, "#### ####  ### ");

            charBitmaps[':' - s] = new CharBitmap(1, 3, "# #", 2);
            charBitmaps['<' - s] = new CharBitmap(2, 3, " ##  #", 1);
            charBitmaps['=' - s] = new CharBitmap(3, 3, "###   ###", 1);
            charBitmaps['>' - s] = new CharBitmap(2, 3, "#  ## ", 1);
            charBitmaps['?' - s] = new CharBitmap(4, 5, " ## #  #  #       # ");
            charBitmaps['@' - s] = new CharBitmap(5, 5, " ### # #### ####     ### ");

            charBitmaps['A' - s] = new CharBitmap(4, 5, " ## #  ######  ##  #");
            charBitmaps['B' - s] = new CharBitmap(4, 5, "### #  #### #  #### ");
            charBitmaps['C' - s] = new CharBitmap(4, 5, " ####   #   #    ###");
            charBitmaps['D' - s] = new CharBitmap(4, 5, "### #  ##  ##  #### ");
            charBitmaps['E' - s] = new CharBitmap(4, 5, "#####   ### #   ####");
            charBitmaps['F' - s] = new CharBitmap(4, 5, "#####   ### #   #   ");
            charBitmaps['G' - s] = new CharBitmap(4, 5, " ####   # ###  # ###");
            charBitmaps['H' - s] = new CharBitmap(4, 5, "#  ##  ######  ##  #");
            charBitmaps['I' - s] = new CharBitmap(3, 5, "### #  #  # ###");
            charBitmaps['J' - s] = new CharBitmap(4, 5, "####  #   #   # ##  ");
            charBitmaps['K' - s] = new CharBitmap(4, 5, "#  ## # ##  # # #  #");
            charBitmaps['L' - s] = new CharBitmap(4, 5, "#   #   #   #   ####");
            charBitmaps['M' - s] = new CharBitmap(5, 5, "#   ### ### # ##   ##   #");
            charBitmaps['N' - s] = new CharBitmap(4, 5, "#  ### ## ###  ##  #");
            charBitmaps['O' - s] = new CharBitmap(4, 5, " ## #  ##  ##  # ## ");
            charBitmaps['P' - s] = new CharBitmap(4, 5, "### #  #### #   #   ");
            charBitmaps['Q' - s] = new CharBitmap(4, 5, " ## #  ### ## #  # #");
            charBitmaps['R' - s] = new CharBitmap(4, 5, "### #  #### # # #  #");
            charBitmaps['S' - s] = new CharBitmap(4, 5, " ####    ##    #### ");
            charBitmaps['T' - s] = new CharBitmap(3, 5, "### #  #  #  # ");
            charBitmaps['U' - s] = new CharBitmap(4, 5, "#  ##  ##  ##  # ## ");
            charBitmaps['V' - s] = new CharBitmap(5, 5, "#   ##   ##   # ###   #  ");
            charBitmaps['W' - s] = new CharBitmap(5, 5, "#   ##   ## # ### ###   #");
            charBitmaps['X' - s] = new CharBitmap(5, 5, "#   # # #   #   # # #   #");
            charBitmaps['Y' - s] = new CharBitmap(5, 5, "#   # # #   #    #    #  ");
            charBitmaps['Z' - s] = new CharBitmap(4, 5, "####  #  #  #   ####");

            charBitmaps['[' - s] = new CharBitmap(2, 5, "### # # ##");
            charBitmaps['\\' - s] = new CharBitmap(5, 5, "#     #     #     #     #");
            charBitmaps[']' - s] = new CharBitmap(2, 5, "## # # ###");
            charBitmaps['^' - s] = new CharBitmap(3, 2, " # # #");
            charBitmaps['_' - s] = new CharBitmap(5, 1, "#####", 5);
            charBitmaps['`' - s] = new CharBitmap(2, 2, "#  #");

            charBitmaps['a' - s] = new CharBitmap(4, 3, "##### #### #", 2);
            charBitmaps['b' - s] = new CharBitmap(3, 5, "#  #  ## # ####");
            charBitmaps['c' - s] = new CharBitmap(3, 3, "####  ###", 2);
            charBitmaps['d' - s] = new CharBitmap(3, 5, "  #  ##### ####");
            charBitmaps['e' - s] = new CharBitmap(3, 3, "#####  ##", 2);
            charBitmaps['f' - s] = new CharBitmap(3, 5, " ###  ## #  #  ");
            charBitmaps['g' - s] = new CharBitmap(3, 5, "#### ####  # ##", 2);
            charBitmaps['h' - s] = new CharBitmap(3, 5, "#  #  #### ## #");
            charBitmaps['i' - s] = new CharBitmap(1, 4, "# ##", 1);
            charBitmaps['j' - s] = new CharBitmap(2, 5, " #   # ## ", 1);
            charBitmaps['k' - s] = new CharBitmap(3, 5, "#  # ### # ## #");
            charBitmaps['l' - s] = new CharBitmap(1, 5, "#####");
            charBitmaps['m' - s] = new CharBitmap(5, 3, " # # # # ## # #", 2);
            charBitmaps['n' - s] = new CharBitmap(3, 3, "## # ## #", 2);
            charBitmaps['o' - s] = new CharBitmap(3, 3, "#### ####", 2);
            charBitmaps['p' - s] = new CharBitmap(3, 5, "#### #####  #  ", 2);
            charBitmaps['q' - s] = new CharBitmap(3, 5, "#### ####  #  #", 2);
            charBitmaps['r' - s] = new CharBitmap(3, 3, " ###  #  ", 2);
            charBitmaps['s' - s] = new CharBitmap(3, 3, "## ### ##", 2);
            charBitmaps['t' - s] = new CharBitmap(3, 4, " # ### #  ##", 1);
            charBitmaps['u' - s] = new CharBitmap(3, 3, "# ## ####", 2);
            charBitmaps['v' - s] = new CharBitmap(3, 3, "# ## # # ", 2);
            charBitmaps['w' - s] = new CharBitmap(5, 3, "#   ## # # # # ", 2);
            charBitmaps['x' - s] = new CharBitmap(3, 3, "# # # # #", 2);
            charBitmaps['y' - s] = new CharBitmap(3, 5, "# ## # ##  ### ", 2);
            charBitmaps['z' - s] = new CharBitmap(5, 3, "####   #   ####", 2);
        }
    }

    public static class DataChunks
    {
        struct DataChunk
        {
            public string Name;
            public int DataOffset;
            public int DataLength;

            public DataChunk(string name, int dataOffset, int dataLength)
            {
                Name = name;
                DataOffset = dataOffset;
                DataLength = dataLength;
            }
        }

        public struct Serializer
        {
            List<DataChunk> chunks;
            List<byte[]> dataArrays;
            int dataOffset;

            public static Serializer Create()
            {
                return new Serializer
                {
                    chunks = new List<DataChunk>(),
                    dataArrays = new List<byte[]>()
                };
            }

            public void AddArray(string name, long[] array)
            {
                int dataLength = array.Length * sizeof(long);
                chunks.Add(new DataChunk(name, dataOffset, dataLength));

                var data = new byte[dataLength];
                Buffer.BlockCopy(array, 0, data, 0, dataLength);
                dataArrays.Add(data);

                dataOffset += dataLength;
            }

            public void AddArray(string name, float[] array)
            {
                int dataLength = array.Length * sizeof(float);
                chunks.Add(new DataChunk(name, dataOffset, dataLength));

                var data = new byte[dataLength];
                Buffer.BlockCopy(array, 0, data, 0, dataLength);
                dataArrays.Add(data);

                dataOffset += dataLength;
            }

            public string Save()
            {
                var bytes = new List<byte>();
                var intArray = new int[1];
                var intBytes = new byte[sizeof(int)];

                dataOffset = 0;

                bytes.AddArray(Encoding.Unicode.GetBytes("DCH2")); // Version 
                dataOffset += sizeof(char) * 4;

                WriteInt(bytes, intArray, intBytes, chunks.Count);
                dataOffset += sizeof(int);

                for (int i = 0; i < chunks.Count; i++)
                    dataOffset += sizeof(int) + chunks[i].Name.Length * sizeof(char) + sizeof(int) + sizeof(int);

                for (int i = 0; i < chunks.Count; i++)
                {
                    var chunk = chunks[i];
                    WriteInt(bytes, intArray, intBytes, chunk.Name.Length);
                    bytes.AddArray(Encoding.Unicode.GetBytes(chunk.Name));
                    WriteInt(bytes, intArray, intBytes, chunk.DataOffset + dataOffset);
                    WriteInt(bytes, intArray, intBytes, chunk.DataLength);
                }

                for (int i = 0; i < dataArrays.Count; i++)
                    bytes.AddArray(dataArrays[i]);

                return Convert.ToBase64String(bytes.ToArray());
            }

            static void WriteInt(List<byte> bytes, int[] intArray, byte[] intBytes, int value)
            {
                intArray[0] = value;
                Buffer.BlockCopy(intArray, 0, intBytes, 0, sizeof(int));
                bytes.AddArray(intBytes);
            }
        }

        public struct Deserializer
        {
            byte[] data;
            DataChunk[] chunks;

            public bool IsValid => chunks != null;

            public static Deserializer LoadFromData(string data, int maxChunks, Action<string> echo)
            {
                byte[] bytes;

                if (data.StartsWith("DCH1", StringComparison.Ordinal))
                    bytes = CharsToBytes(data.ToCharArray());
                else
                    bytes = Convert.FromBase64String(data);

                var chunks = LoadDataChunks(bytes, maxChunks, echo);

                return new Deserializer
                {
                    data = bytes,
                    chunks = chunks
                };
            }

            static byte[] CharsToBytes(char[] chars)
            {
                var bytes = new byte[chars.Length * sizeof(char)];
                Buffer.BlockCopy(chars, 0, bytes, 0, bytes.Length);

                return bytes;
            }

            static DataChunk[] LoadDataChunks(byte[] data, int maxChunks, Action<string> echo)
            {
                var intArray = new int[1];
                int byteOffset = 0;
                var version = ReadVersion(data, ref byteOffset);

                if (version != "DCH1" && version != "DCH2")
                {
                    echo($"Invalid version '{version}'");
                    return null;
                }

                int chunkCount = ReadInt(data, intArray, ref byteOffset);

                if (chunkCount > maxChunks)
                {
                    echo($"Invalid chunk count '{chunkCount}'");
                    return null;
                }

                var chunks = new DataChunk[chunkCount];

                for (int i = 0; i < chunks.Length; i++)
                    chunks[i] = ReadDataChunk(data, intArray, ref byteOffset);

                return chunks;
            }

            static string ReadVersion(byte[] data, ref int offset)
            {
                var version = Encoding.Unicode.GetString(data, offset, sizeof(char) * 4);
                offset += sizeof(char) * 4;
                return version;
            }

            static DataChunk ReadDataChunk(byte[] data, int[] intArray, ref int offset)
            {
                int nameLength = ReadInt(data, intArray, ref offset);
                var name = Encoding.Unicode.GetString(data, offset, sizeof(char) * nameLength);

                offset += nameLength * sizeof(char);

                int dataOffset = ReadInt(data, intArray, ref offset);
                int dataLength = ReadInt(data, intArray, ref offset);

                return new DataChunk(name, dataOffset, dataLength);
            }

            static int ReadInt(byte[] bytes, int[] intArray, ref int offset)
            {
                Buffer.BlockCopy(bytes, offset, intArray, 0, sizeof(int));

                offset += sizeof(int);

                return intArray[0];
            }

            DataChunk? FindChunk(string name)
            {
                for (int i = 0; i < chunks.Length; i++)
                {
                    if (chunks[i].Name == name)
                        return chunks[i];
                }

                return null;
            }

            public long[] LoadLongArray(string name)
            {
                var chunkOrNull = FindChunk(name);

                if (chunkOrNull == null)
                    return null;

                var chunk = chunkOrNull.Value;
                var array = new long[chunk.DataLength / sizeof(long)];
                CopyChunkData(chunk, array, sizeof(long));

                return array;
            }

            public float[] LoadFloatArray(string name)
            {
                var chunkOrNull = FindChunk(name);

                if (chunkOrNull == null)
                    return null;

                var chunk = chunkOrNull.Value;
                var array = new float[chunk.DataLength / sizeof(float)];

                CopyChunkData(chunk, array, sizeof(float));

                return array;
            }

            public bool LoadFloatArray(string name, float[] array)
            {
                var chunkOrNull = FindChunk(name);

                if (chunkOrNull == null)
                    return false;

                CopyChunkData(chunkOrNull.Value, array, sizeof(float));

                return true;
            }

            void CopyChunkData<T>(DataChunk chunk, T[] array, int elementSize)
            {
                int dataArrayLength = chunk.DataLength / elementSize;
                int arrayOffset = 0;
                int dataOffset = chunk.DataOffset;
                int dataLength = chunk.DataLength;

                if (array.Length > dataArrayLength)
                {
                    arrayOffset = array.Length - dataArrayLength;
                }
                else if (dataArrayLength > array.Length)
                {
                    dataOffset += (dataArrayLength - array.Length) * elementSize;
                    dataLength = array.Length * elementSize;
                }

                Buffer.BlockCopy(data, dataOffset, array, arrayOffset * elementSize, dataLength);
            }
        }
    }

    static class Extensions
    {
        // This is overload the extension method in Space Engineers which is not allowed in program blocks 
        public static bool Contains(this string text, string value, StringComparison comparison) => text.IndexOf(value, comparison) != -1;

        public static Point BottomLeft(this Rectangle rectangle) => new Point(rectangle.X, rectangle.Bottom);
        public static Point BottomRight(this Rectangle rectangle) => new Point(rectangle.Right, rectangle.Bottom);

        // Space Engineers prohibits the Predicate Type >:( 
        public static int FindIndex<T>(this List<T> list, Func<T, bool> predicate)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (predicate(list[i]))
                    return i;
            }

            return -1;
        }

        public static string[] SplitOnChar(this string _string, char separatorChar)
        {
            int separatorCount = 0;

            for (int i = 0; i < _string.Length; i++)
            {
                if (_string[i] == separatorChar)
                    separatorCount++;
            }

            if (separatorCount == 0)
                return new[] { _string };

            var parts = new string[separatorCount + 1];

            separatorCount = 0;
            int prevIndex = -1;

            for (int i = 0; i < _string.Length; i++)
            {
                if (_string[i] == separatorChar)
                {
                    int start = prevIndex + 1;
                    parts[separatorCount++] = _string.Substring(start, i - start);
                    prevIndex = i;
                }
            }

            parts[separatorCount++] = _string.Substring(prevIndex + 1, _string.Length - (prevIndex + 1));

            return parts;
        }

        public static bool AcceptsResourceType(this MyResourceSinkComponent sink, MyDefinitionId typeId)
        {
            foreach (var item in sink.AcceptedResources)
            {
                if (item == typeId)
                    return true;
            }

            return false;
        }

        // source.ResourceTypes is prohibited >:( 
        //public static bool OutputsResourceType(this MyResourceSourceComponent source, MyDefinitionId typeId) 
        //{ 
        //    foreach (var item in source.ResourceTypes) 
        //    { 
        //        if (item == typeId) 
        //            return true; 
        //    } 

        //    return false; 
        //} 

        public static bool ContainsString(this string[] strings, string value)
        {
            for (int i = 0; i < strings.Length; i++)
            {
                if (strings[i].Equals(value, StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

        public static void GetItemsOfType<T, TResult>(this List<T> list, List<TResult> outList) where TResult : T
        {
            outList.Clear();

            for (int i = 0; i < list.Count; i++)
            {
                var item = list[i];

                if (item is TResult)
                    outList.Add((TResult)item);
            }
        }

        public static void GetItemsOfType<T, TResult>(this List<T> list, List<TResult> outList, Func<TResult, bool> predicate) where TResult : T
        {
            outList.Clear();

            for (int i = 0; i < list.Count; i++)
            {
                var item = list[i];

                if (item is TResult && predicate((TResult)item))
                    outList.Add((TResult)item);
            }
        }

        public static StringBuilder AppendTimeSpan(this StringBuilder stringBuilder, TimeSpan timeSpan)
        {
            if (timeSpan.Days > 0)
            {
                stringBuilder.Append((int)timeSpan.TotalDays);
                stringBuilder.Append("d");

                if (timeSpan.Hours == 0 && timeSpan.Minutes == 0 && timeSpan.Seconds == 0)
                    stringBuilder.Append(" ");
            }

            if (timeSpan.Hours > 0 && timeSpan.Days < 2)
            {
                stringBuilder.Append(timeSpan.Hours);
                stringBuilder.Append("h");

                if (timeSpan.Minutes == 0 && timeSpan.Seconds == 0)
                    stringBuilder.Append(" ");
            }

            if (timeSpan.Minutes > 0 && timeSpan.Days == 0)
            {
                stringBuilder.Append(timeSpan.Minutes);
                stringBuilder.Append("m");

                if (timeSpan.Seconds == 0)
                    stringBuilder.Append(" ");
            }

            if (timeSpan.Hours == 0 && timeSpan.Days == 0)
            {
                stringBuilder.Append(timeSpan.Seconds);
                stringBuilder.Append("s");
            }

            return stringBuilder;
        }
    }
} 
