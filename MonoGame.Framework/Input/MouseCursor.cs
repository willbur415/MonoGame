// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Input
{
    /// <summary>
    /// Describes a mouse cursor.
    /// </summary>
    public enum MouseCursor
    {
        /// <summary>
        /// Gets the default arrow cursor.
        /// </summary>
        Arrow,

        /// <summary>
        /// Gets the cursor that appears when the mouse is over text editing regions.
        /// </summary>
        IBeam,

        /// <summary>
        /// Gets the waiting cursor that appears while the application/system is busy.
        /// </summary>
        Wait,

        /// <summary>
        /// Gets the crosshair ("+") cursor.
        /// </summary>
        Crosshair,

        /// <summary>
        /// Gets the cross between Arrow and Wait cursors.
        /// </summary>
        WaitArrow,

        /// <summary>
        /// Gets the northwest/southeast ("\") cursor.
        /// </summary>
        SizeNWSE,

        /// <summary>
        /// Gets the northeast/southwest ("/") cursor.
        /// </summary>
        SizeNESW,

        /// <summary>
        /// Gets the horizontal west/east ("-") cursor.
        /// </summary>
        SizeWE,

        /// <summary>
        /// Gets the vertical north/south ("|") cursor.
        /// </summary>
        SizeNS,

        /// <summary>
        /// Gets the size all cursor which points in all directions.
        /// </summary>
        SizeAll,

        /// <summary>
        /// Gets the cursor that points that something is invalid, usually a cross.
        /// </summary>
        No,

        /// <summary>
        /// Gets the hand cursor, usually used for web links.
        /// </summary>
        Hand,

        /// <summary>
        /// Try to use a custom Texture2D.
        /// </summary>
        Texture
    }
}
