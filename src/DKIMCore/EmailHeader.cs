﻿/*
 * DKIM.Net
 * 
 * Copyright (C) 2011 Damien McGivern, damien@mcgiv.com
 * 
 * 
 * 
 * */
namespace DKIMCore
{
    /// <summary>
    /// Stores the origional header key and value and wether or not the value is folded.
    /// </summary>
    internal class EmailHeader
    {
        /// <summary>
        /// Header Key
        /// </summary>
        public string Key;


        /// <summary>
        /// Header Value
        /// </summary>
        public string Value;


        /// <summary>
        /// Indicates that the value is folded over multiple lines.
        /// </summary>
        public bool FoldedValue;
        public override string ToString()
        {
            return $"{Key}:{Value}";
        }
    }
}
