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

namespace SharpLife.Engine.Client.UI.Rendering.Utility
{
    /// <summary>
    /// Texture formats to convert from palette based to <see cref="SixLabors.ImageSharp.PixelFormats.Rgba32"/>
    /// <see cref="ImageConversionUtils.ConvertIndexedToRgba32(SixLabors.ImageSharp.PixelFormats.Rgb24[], byte[], int, int)"/>
    /// </summary>
    public enum TextureFormat
    {
        /// <summary>
        /// Convert a 256 color image by performing direct lookup
        /// </summary>
        Normal = 0,

        /// <summary>
        /// Single color is transparent
        /// </summary>
        AlphaTest,

        /// <summary>
        /// Transparency is determined by index in table
        /// </summary>
        IndexAlpha,
    }
}
