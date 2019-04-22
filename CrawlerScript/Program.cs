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
        enum CrawlerStatus
        {
            Stop,
            GoUp,
            GoDown
        }

        int _count = 0;
        bool _init = false;
        IMyLandingGear _upperLandingGear;
        IMyLandingGear _lowerLandingGear;

        IMyLandingGear _portBaseLandingGear;
        IMyLandingGear _starboardBaseLandingGear;

        IMyExtendedPistonBase _piston1;
        IMyExtendedPistonBase _piston2;
        IMyExtendedPistonBase _piston3;

        CrawlerStatus _crawlerStatus;

        public Program()
        {
            var error = false;
            _upperLandingGear = GridTerminalSystem.GetBlockWithName("Lower Landing Gear") as IMyLandingGear;
            _lowerLandingGear = GridTerminalSystem.GetBlockWithName("Upper Landing Gear") as IMyLandingGear;

            _portBaseLandingGear = GridTerminalSystem.GetBlockWithName("Port Base Landing Gear") as IMyLandingGear;
            _starboardBaseLandingGear = GridTerminalSystem.GetBlockWithName("Starboard Base Landing Gear") as IMyLandingGear;

            _piston1 = GridTerminalSystem.GetBlockWithName("Piston 1") as IMyExtendedPistonBase;
            _piston2 = GridTerminalSystem.GetBlockWithName("Piston 2") as IMyExtendedPistonBase;
            _piston3 = GridTerminalSystem.GetBlockWithName("Piston 3") as IMyExtendedPistonBase;

            if (_upperLandingGear == null)
            {
                Echo("No top landing gear");
                error = true;
            }

            if (_lowerLandingGear == null)
            {
                Echo("No bottom landing gear");
                error = true;
            }

            if (_portBaseLandingGear == null)
            {
                Echo("No port landing gear");
                error = true;
            }

            if (_starboardBaseLandingGear == null)
            {
                Echo("No starboard landing gear");
                error = true;
            }

            if (_piston1 == null)
            {
                Echo("No piston #1");
                error = true;
            }

            if (_piston2 == null)
            {
                Echo("No piston #2");
                error = true;
            }

            if (_piston3 == null)
            {
                Echo("No piston #3");
                error = true;
            }

            if (error)
            {
                Echo("Errors found. Please fix and recompile");
                return;
            }

            Runtime.UpdateFrequency = UpdateFrequency.Update10;

            _upperLandingGear.AutoLock = false;
            _upperLandingGear.Lock();

            _lowerLandingGear.AutoLock = false;
            _lowerLandingGear.Lock();

            _portBaseLandingGear.AutoLock = false;
            _starboardBaseLandingGear.AutoLock = false;

            _crawlerStatus = CrawlerStatus.Stop;

            _init = true;
            Echo($"Crawler startup @ {DateTime.Now}");
        }

        PistonStatus oldStatus;

        public void Main(string argument, UpdateType updateSource)
        {
            if (!_init)
                return;

            if ((updateSource & UpdateType.Update10) == UpdateType.Update10)
            {
                HandleUpdate();
            }

            if ((updateSource & UpdateType.Trigger) == UpdateType.Trigger)
            {
                HandleTrigger(argument);
            }

            if ((updateSource & UpdateType.Terminal) == UpdateType.Terminal)
            {
                Echo($"Count: {_count}");
            }
        }

        void HandleTrigger(string argument)
        {
            switch (argument)
            {
                case "GoUp":
                    _crawlerStatus = CrawlerStatus.GoUp;
                    _lowerLandingGear.Lock();
                    _upperLandingGear.Unlock();
                    ExtendPistons();
                    break;

                case "GoDown":
                    _crawlerStatus = CrawlerStatus.GoDown;
                    _upperLandingGear.Lock();
                    _lowerLandingGear.Unlock();
                    ExtendPistons();
                    break;

                case "Stop":
                    _crawlerStatus = CrawlerStatus.Stop;
                    break;

                default:
                    Echo($"Invalid option: {argument}\n");
                    break;
            }
        }

        private void HandleUpdate()
        {
            return;

            ////if (_crawlerStatus == CrawlerStatus.Stop)
            ////    return;

            ////if (_crawlerStatus == CrawlerStatus.GoUp)
            ////{
            ////    if ((_piston1.Status == _piston2.Status) & (_piston1.Status == _piston3.Status))
            ////    {
            ////        if (_piston1.Status != oldStatus)
            ////        {
            ////            oldStatus = _piston1.Status;
            ////            Echo("Action");
            ////            switch (oldStatus)
            ////            {
            ////                case PistonStatus.Stopped:
            ////                    break;

            ////                case PistonStatus.Extending:
            ////                    break;

            ////                case PistonStatus.Extended:
            ////                    _upperLandingGear.Lock();
            ////                    _lowerLandingGear.Unlock();
            ////                    RetractPistons();
            ////                    break;

            ////                case PistonStatus.Retracting:
            ////                    break;

            ////                case PistonStatus.Retracted:
            ////                    _lowerLandingGear.Lock();
            ////                    _upperLandingGear.Unlock();
            ////                    ExtendPistons();
            ////                    _count++;
            ////                    break;
            ////            }
            ////        }
            ////    }
            ////}

            ////if (_crawlerStatus == CrawlerStatus.GoDown)
            ////{
            ////    if ((_piston1.Status == _piston2.Status) & (_piston1.Status == _piston3.Status))
            ////    {
            ////        if (_piston1.Status != oldStatus)
            ////        {
            ////            oldStatus = _piston1.Status;
            ////            Echo("Action");
            ////            switch (oldStatus)
            ////            {
            ////                case PistonStatus.Stopped:
            ////                    break;

            ////                case PistonStatus.Extending:
            ////                    break;

            ////                case PistonStatus.Extended:
            ////                    _upperLandingGear.Lock();
            ////                    _lowerLandingGear.Unlock();
            ////                    RetractPistons();
            ////                    break;

            ////                case PistonStatus.Retracting:
            ////                    break;

            ////                case PistonStatus.Retracted:
            ////                    _lowerLandingGear.Lock();
            ////                    _upperLandingGear.Unlock();
            ////                    ExtendPistons();
            ////                    _count++;
            ////                    break;
            ////            }
            ////        }
            ////    }
            ////}
        }

        void RetractPistons()
        {
            _piston1.Retract();
            _piston2.Retract();
            _piston3.Retract();
        }

        void ExtendPistons()
        {
            _piston1.Extend();
            _piston2.Extend();
            _piston3.Extend();
        }
    }
}