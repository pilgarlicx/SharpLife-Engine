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

using SharpLife.Engine.Models.Rendering;

namespace SharpLife.Engine.Models.MDL.Rendering
{
    public unsafe struct StudioModelRenderData
    {
        public StudioModel Model;

        public SharedModelRenderData Shared;

        public double CurrentTime;

        public uint Sequence;

        public float LastTime;

        public float Frame;

        public float FrameRate;

        public uint Body;

        public uint Skin;

        public BoneData BoneData;

        public int RenderFXLightMultiplier;
    }
}
