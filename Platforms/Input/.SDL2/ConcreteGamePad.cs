﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Platform.Utilities;

namespace Microsoft.Xna.Platform.Input
{
    // TODO: move GamePadDevice to Framework.Input library
    public abstract class GamePadDevice
    {
        public GamePadCapabilities Capabilities;

        public GamePadDevice()
        {
        }
    }

    public sealed class ConcreteGamePad : GamePadStrategy
    {
        private Sdl SDL { get { return Sdl.Current; } }


        // map GamePad indices (PlayerIndex) -> GamePadDevices
        private readonly Dictionary<int, SdlGamePadDevice> _gamepads = new Dictionary<int, SdlGamePadDevice>();
        // map Joystick instanceIDs -> gamepad indices (PlayerIndex)
        private readonly Dictionary<int, int> _indicesMap = new Dictionary<int, int>();

        // Default & SDL Xbox Controller dead zones
        // Based on the XInput constants
        public override float LeftThumbDeadZone { get { return 0.24f; } }
        public override float RightThumbDeadZone { get { return 0.265f; } }

        const Sdl.InitFlags SdlSubSystems = Sdl.InitFlags.GameController
                                          | Sdl.InitFlags.Haptic
                                          ;

        public ConcreteGamePad()
        {
            Sdl.Current.InitSubSystem(SdlSubSystems);;
            InitDatabase();
            InitDevices();
        }

        ~ConcreteGamePad()
        {
            foreach (SdlGamePadDevice sdlGamepad in _gamepads.Values)
                SDL.GAMECONTROLLER.Close(sdlGamepad.Handle);

            _gamepads.Clear();
            _indicesMap.Clear();

            Sdl.Current.QuitSubSystem(SdlSubSystems);

        }

        public override int PlatformGetMaxNumberOfGamePads()
        {
            return 16;
        }

        public override GamePadCapabilities PlatformGetCapabilities(int index)
        {
            if (_gamepads.TryGetValue(index, out SdlGamePadDevice sdlGamepad))
                return sdlGamepad.Capabilities;
                        
            return base.CreateGamePadCapabilities(
                    gamePadType: GamePadType.Unknown,
                    displayName: null,
                    identifier: null,
                    isConnected: false,
                    buttons: (Buttons)0,
                    hasLeftVibrationMotor: false,
                    hasRightVibrationMotor: false,
                    hasVoiceSupport: false
                );
        }

        private GamePadCapabilities InternalGetCapabilities(IntPtr handle)
        {
            GamePadType gamePadType = GamePadType.GamePad;
            string displayName = SDL.GAMECONTROLLER.GetName(handle);
            string identifier = SDL.JOYSTICK.GetGUID(SDL.GAMECONTROLLER.GetJoystick(handle)).ToString();
            bool isConnected = true;
            bool hasLeftVibrationMotor  = SDL.GAMECONTROLLER.HasRumble(handle) != 0;
            bool hasRightVibrationMotor = SDL.GAMECONTROLLER.HasRumble(handle) != 0;
            bool hasVoiceSupport = false;

            Buttons buttons = (Buttons)0;
            ParseCapabilities(handle, ref buttons);

            return base.CreateGamePadCapabilities(
                    gamePadType: gamePadType,
                    displayName: displayName,
                    identifier: identifier,
                    isConnected: isConnected,
                    buttons: buttons,
                    hasLeftVibrationMotor: hasLeftVibrationMotor,
                    hasRightVibrationMotor: hasRightVibrationMotor,
                    hasVoiceSupport: hasVoiceSupport
                );
        }

        private void ParseCapabilities(IntPtr gamecontroller, ref Buttons buttons)
        {
            IntPtr pStrMappings = IntPtr.Zero;
            try
            {
                pStrMappings = SDL.GAMECONTROLLER.SDL_GameControllerMapping(gamecontroller);
                if (pStrMappings == IntPtr.Zero)
                    return;

                string mappings = InteropHelpers.Utf8ToString(pStrMappings);

                for (int idx = 0; idx < mappings.Length;)
                {
                    if (MatchKey("a", mappings, ref idx))
                        buttons |= Buttons.A;
                    else
                    if (MatchKey("b", mappings, ref idx))
                        buttons |= Buttons.B;
                    else
                    if (MatchKey("x", mappings, ref idx))
                        buttons |= Buttons.X;
                    else
                    if (MatchKey("y", mappings, ref idx))
                        buttons |= Buttons.Y;
                    else
                    if (MatchKey("back", mappings, ref idx))
                        buttons |= Buttons.Back;
                    else
                    if (MatchKey("guide", mappings, ref idx))
                        buttons |= Buttons.BigButton;
                    else
                    if (MatchKey("start", mappings, ref idx))
                        buttons |= Buttons.Start;
                    else
                    if (MatchKey("dpleft", mappings, ref idx))
                        buttons |= Buttons.DPadLeft;
                    else
                    if (MatchKey("dpdown", mappings, ref idx))
                        buttons |= Buttons.DPadDown;
                    else
                    if (MatchKey("dpright", mappings, ref idx))
                        buttons |= Buttons.DPadRight;
                    else
                    if (MatchKey("dpup", mappings, ref idx))
                        buttons |= Buttons.DPadUp;
                    else
                    if (MatchKey("leftshoulder", mappings, ref idx))
                        buttons |= Buttons.LeftShoulder;
                    else
                    if (MatchKey("lefttrigger", mappings, ref idx))
                        buttons |= Buttons.LeftTrigger;
                    else
                    if (MatchKey("rightshoulder", mappings, ref idx))
                        buttons |= Buttons.RightShoulder;
                    else
                    if (MatchKey("righttrigger", mappings, ref idx))
                        buttons |= Buttons.RightTrigger;
                    else
                    if (MatchKey("leftstick", mappings, ref idx))
                        buttons |= Buttons.LeftStick;
                    else
                    if (MatchKey("rightstick", mappings, ref idx))
                        buttons |= Buttons.RightStick;
                    else
                    if (MatchKey("leftx", mappings, ref idx))
                        buttons |= Buttons.LeftThumbstickLeft | Buttons.LeftThumbstickRight;
                    else
                    if (MatchKey("lefty", mappings, ref idx))
                        buttons |= Buttons.LeftThumbstickDown | Buttons.LeftThumbstickUp;
                    else
                    if (MatchKey("rightx", mappings, ref idx))
                        buttons |= Buttons.RightThumbstickLeft | Buttons.RightThumbstickRight;
                    else
                    if (MatchKey("righty", mappings, ref idx))
                        buttons |= Buttons.RightThumbstickDown | Buttons.RightThumbstickUp;

                    if (idx < mappings.Length)
                    {
                        int nidx = mappings.IndexOf(',', idx);
                        if (nidx != -1)
                        {
                            idx = nidx + 1;
                            continue;
                        }
                    }
                    break;
                }
            }
            finally
            {
                if (pStrMappings != IntPtr.Zero)
                    SDL.SDL_Free(pStrMappings);
            }
        }

        private bool MatchKey(string match, string input, ref int startIndex)
        {
            int nIndex = startIndex;
            if (!Match(match, input, ref nIndex))
                return false;
            if (!Match(":", input, ref nIndex))
                return false;

            startIndex = nIndex;
            return true;
        }

        private bool Match(string match, string input, ref int startIndex)
        {
            if (input.Length - startIndex < match.Length)
                return false;

            int matchIndex = input.IndexOf(match, startIndex, match.Length);
            if (matchIndex != startIndex)
                return false;

            startIndex += match.Length;
            return true;
        }

        private float GetFromSdlAxis(int axis)
        {
            // SDL Axis ranges from -32768 to 32767, so we need to divide with different numbers depending on if it's positive
            if (axis < 0)
                return axis / 32768f;

            return axis / 32767f;
        }

        public override GamePadState PlatformGetState(int index, GamePadDeadZone leftDeadZoneMode, GamePadDeadZone rightDeadZoneMode)
        {
            if (_gamepads.TryGetValue(index, out SdlGamePadDevice sdlGamepad))
            {
                // Y gamepad axis is rotate between SDL and XNA
                GamePadThumbSticks thumbSticks =
                    base.CreateGamePadThumbSticks(
                        new Vector2(
                            GetFromSdlAxis(SDL.GAMECONTROLLER.GetAxis(sdlGamepad.Handle, Sdl.GameController.Axis.LeftX)),
                            GetFromSdlAxis(SDL.GAMECONTROLLER.GetAxis(sdlGamepad.Handle, Sdl.GameController.Axis.LeftY)) * -1f
                        ),
                        new Vector2(
                            GetFromSdlAxis(SDL.GAMECONTROLLER.GetAxis(sdlGamepad.Handle, Sdl.GameController.Axis.RightX)),
                            GetFromSdlAxis(SDL.GAMECONTROLLER.GetAxis(sdlGamepad.Handle, Sdl.GameController.Axis.RightY)) * -1f
                        ),
                        leftDeadZoneMode,
                        rightDeadZoneMode
                    );

                GamePadTriggers triggers = new GamePadTriggers(
                    GetFromSdlAxis(SDL.GAMECONTROLLER.GetAxis(sdlGamepad.Handle, Sdl.GameController.Axis.TriggerLeft)),
                    GetFromSdlAxis(SDL.GAMECONTROLLER.GetAxis(sdlGamepad.Handle, Sdl.GameController.Axis.TriggerRight))
                );

                GamePadButtons buttons =
                    new GamePadButtons(
                        ((SDL.GAMECONTROLLER.GetButton(sdlGamepad.Handle, Sdl.GameController.Button.A) == 1) ? Buttons.A : 0) |
                        ((SDL.GAMECONTROLLER.GetButton(sdlGamepad.Handle, Sdl.GameController.Button.B) == 1) ? Buttons.B : 0) |
                        ((SDL.GAMECONTROLLER.GetButton(sdlGamepad.Handle, Sdl.GameController.Button.Back) == 1) ? Buttons.Back : 0) |
                        ((SDL.GAMECONTROLLER.GetButton(sdlGamepad.Handle, Sdl.GameController.Button.Guide) == 1) ? Buttons.BigButton : 0) |
                        ((SDL.GAMECONTROLLER.GetButton(sdlGamepad.Handle, Sdl.GameController.Button.LeftShoulder) == 1) ? Buttons.LeftShoulder : 0) |
                        ((SDL.GAMECONTROLLER.GetButton(sdlGamepad.Handle, Sdl.GameController.Button.RightShoulder) == 1) ? Buttons.RightShoulder : 0) |
                        ((SDL.GAMECONTROLLER.GetButton(sdlGamepad.Handle, Sdl.GameController.Button.LeftStick) == 1) ? Buttons.LeftStick : 0) |
                        ((SDL.GAMECONTROLLER.GetButton(sdlGamepad.Handle, Sdl.GameController.Button.RightStick) == 1) ? Buttons.RightStick : 0) |
                        ((SDL.GAMECONTROLLER.GetButton(sdlGamepad.Handle, Sdl.GameController.Button.Start) == 1) ? Buttons.Start : 0) |
                        ((SDL.GAMECONTROLLER.GetButton(sdlGamepad.Handle, Sdl.GameController.Button.X) == 1) ? Buttons.X : 0) |
                        ((SDL.GAMECONTROLLER.GetButton(sdlGamepad.Handle, Sdl.GameController.Button.Y) == 1) ? Buttons.Y : 0) |
                        ((triggers.Left > 0f) ? Buttons.LeftTrigger : 0) |
                        ((triggers.Right > 0f) ? Buttons.RightTrigger : 0)
                    );

                GamePadDPad dPad =
                    new GamePadDPad(
                        (SDL.GAMECONTROLLER.GetButton(sdlGamepad.Handle, Sdl.GameController.Button.DpadUp) == 1) ? ButtonState.Pressed : ButtonState.Released,
                        (SDL.GAMECONTROLLER.GetButton(sdlGamepad.Handle, Sdl.GameController.Button.DpadDown) == 1) ? ButtonState.Pressed : ButtonState.Released,
                        (SDL.GAMECONTROLLER.GetButton(sdlGamepad.Handle, Sdl.GameController.Button.DpadLeft) == 1) ? ButtonState.Pressed : ButtonState.Released,
                        (SDL.GAMECONTROLLER.GetButton(sdlGamepad.Handle, Sdl.GameController.Button.DpadRight) == 1) ? ButtonState.Pressed : ButtonState.Released
                    );

                sdlGamepad.State = base.CreateGamePadState(thumbSticks, triggers, buttons, dPad,
                                                           packetNumber: sdlGamepad.PacketNumber);

                return sdlGamepad.State;
            }

            return new GamePadState();
        }

        public override bool PlatformSetVibration(int index, float leftMotor, float rightMotor, float leftTrigger, float rightTrigger)
        {
            if (_gamepads.TryGetValue(index, out SdlGamePadDevice sdlGamepad))
            {
                return SDL.GAMECONTROLLER.Rumble(sdlGamepad.Handle, (ushort)(65535f * leftMotor),
                           (ushort)(65535f * rightMotor), uint.MaxValue) == 0 &&
                       SDL.GAMECONTROLLER.RumbleTriggers(sdlGamepad.Handle, (ushort)(65535f * leftTrigger),
                           (ushort)(65535f * rightTrigger), uint.MaxValue) == 0;
            }

            return false;
        }

        private void InitDatabase()
        {
            using (Stream stream = typeof(ConcreteGamePad).Assembly.GetManifestResourceStream("gamecontrollerdb.txt"))
            {
                if (stream != null)
                {
                    try
                    {
                        byte[] data = new byte[stream.Length];
                        stream.Read(data, 0, data.Length);
                        GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
                        try
                        {
                            IntPtr pRWops = SDL.RwFromMem(data, data.Length);
                            SDL.GAMECONTROLLER.AddMappingFromRw(pRWops, 1);
                        }
                        finally
                        {
                            handle.Free();
                        }
                    }
                    catch { }
                }
            }
        }

        private void InitDevices()
        {
            int numJoysticks = SDL.JOYSTICK.NumJoysticks();
            for (int deviceIndex = 0; deviceIndex < numJoysticks; deviceIndex++)
            {
                if (SDL.GAMECONTROLLER.IsGameController(deviceIndex) == 1)
                    AddDevice(deviceIndex);
            }
        }

        internal void AddDevice(int deviceIndex)
        {
            IntPtr handle = SDL.GAMECONTROLLER.Open(deviceIndex);
            IntPtr joystickHandle = SDL.GAMECONTROLLER.GetJoystick(handle);
            int instanceID = SDL.JOYSTICK.InstanceID(joystickHandle);

            if (_indicesMap.ContainsKey(instanceID))
                return;

            int index = 0;
            while (_gamepads.ContainsKey(index))
                index++;

            SdlGamePadDevice sdlGamepad = new SdlGamePadDevice(instanceID, handle);
            sdlGamepad.Capabilities = InternalGetCapabilities(handle);

            _gamepads.Add(index, sdlGamepad);

            _indicesMap[instanceID] = index;
        }

        internal void RemapDevice(int instanceID)
        {
        }

        internal void RemoveDevice(int instanceID)
        {
            if (_indicesMap.TryGetValue(instanceID, out int index))
            {
                if (_gamepads.TryGetValue(index, out SdlGamePadDevice sdlGamepad))
                {
                    _gamepads.Remove(index);
                    SDL.GAMECONTROLLER.Close(sdlGamepad.Handle);

                    _indicesMap.Remove(instanceID);
                }
            }
        }

        internal void UpdatePacketInfo(int instanceID, uint packetNumber)
        {
            if (_indicesMap.TryGetValue(instanceID, out int index))
            {
                if (_gamepads.TryGetValue(index, out SdlGamePadDevice sdlGamepad))
                {
                    sdlGamepad.PacketNumber = (packetNumber < int.MaxValue)
                                            ? (int)packetNumber
                                            : (int)(packetNumber - (uint)int.MaxValue);
                }
            }
        }

    }
}
