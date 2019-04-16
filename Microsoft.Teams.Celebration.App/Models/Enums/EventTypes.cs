﻿// <copyright file="EventTypes.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Celebration.App.Models.Enums
{
    /// <summary>
    /// Store type of events.
    /// </summary>
    public enum EventTypes
    {
        /// <summary>
        /// Any event except Birthday and Anniversary.
        /// </summary>
        Other,

        /// <summary>
        /// Birthday.
        /// </summary>
        Birthday,

        /// <summary>
        /// Anniversary.
        /// </summary>
        Anniversary,
    }
}