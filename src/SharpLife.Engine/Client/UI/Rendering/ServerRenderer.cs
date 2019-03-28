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

using SharpLife.Engine.Client.UI.Rendering.Models;
using System.Numerics;

namespace SharpLife.Engine.Client.UI.Rendering
{
    /// <summary>
    /// A renderer that does no rendering
    /// </summary>
    internal sealed class ServerRenderer : IRenderer
    {
        //TODO: should throw NotSupportedException
        public Vector3 SkyColor { get; set; }

        public Vector3 SkyNormal { get; set; }

        public IRendererModels Models { get; } = new ServerRendererModels();

        public Scene Scene => null;
    }
}
