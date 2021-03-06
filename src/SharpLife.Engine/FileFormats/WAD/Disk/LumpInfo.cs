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

using System.Text;

namespace SharpLife.Engine.FileFormats.WAD.Disk
{
    internal unsafe struct LumpInfo
    {
#pragma warning disable CS0649
        public int filepos;
        public int disksize;
        public int size;                // uncompressed
        public byte type;
        public byte compression;
        public byte pad1, pad2;
        public fixed byte name[WADConstants.MaxTextureNameLength + 1];      // must be null terminated
#pragma warning restore CS0649

        public string Name
        {
            get
            {
                fixed (byte* pName = name)
                {
                    return Encoding.UTF8.GetString(pName, WADConstants.MaxTextureNameLength + 1);
                }
            }
        }
    }
}
