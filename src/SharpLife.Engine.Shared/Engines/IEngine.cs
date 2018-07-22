﻿/***
*
*	Copyright (c) 1996-2001, Valve LLC. All rights reserved.
*	
*	This product contains software technology licensed from Id 
*	Software, Inc. ("Id Technology").  Id Technology (c) 1996 Id Software, Inc. 
*	All Rights Reserved.
*
*   This source code contains proprietary and confidential information of
*   Valve LLC and its suppliers.  Access to this code is restricted to
*   persons who have executed a written SDK license with Valve.  Any access,
*   use or distribution of this code by or to any unlicensed person is illegal.
*
****/

using SharpLife.CommandSystem;
using SharpLife.Engine.Shared.Configuration;
using SharpLife.Engine.Shared.UI;
using SharpLife.Engine.Shared.Utility;
using SharpLife.FileSystem;
using SharpLife.Utility;
using System;

namespace SharpLife.Engine.Shared.Engines
{
    /// <summary>
    /// Manages top level engine components (client, server) and shared components
    /// </summary>
    public interface IEngine
    {
        /// <summary>
        /// Gets the command line passed to the engine
        /// </summary>
        ICommandLine CommandLine { get; }

        /// <summary>
        /// Gets the filesystem used by the engine
        /// </summary>
        IFileSystem FileSystem { get; }

        /// <summary>
        /// Gets the game directory that this game was loaded from
        /// </summary>
        string GameDirectory { get; }

        /// <summary>
        /// Gets the command system
        /// </summary>
        IConCommandSystem CommandSystem { get; }

        /// <summary>
        /// Gets the user interface component
        /// This component is optional and should be created only if needed
        /// </summary>
        IUserInterface UserInterface { get; }

        /// <summary>
        /// Gets the engine time
        /// </summary>
        IEngineTime EngineTime { get; }

        /// <summary>
        /// The engine configuration
        /// </summary>
        EngineConfiguration EngineConfiguration { get; }

        /// <summary>
        /// The game configuration
        /// </summary>
        GameConfiguration GameConfiguration { get; }

        /// <summary>
        /// Gets the date that the engine was built
        /// </summary>
        DateTimeOffset BuildDate { get; }

        /// <summary>
        /// Creates the user interface if it does not exist
        /// </summary>
        IUserInterface CreateUserInterface();

        void Run(string[] args, HostType hostType);
    }
}
