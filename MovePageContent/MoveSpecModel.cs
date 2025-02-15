﻿//  <copyright file="MoveSpecModel.cs" company="Global Graphics Software Ltd">
//      Copyright (c) 2021 Global Graphics Software Ltd. All rights reserved.
//  </copyright>
//  <summary>
//  This example is provided on an "as is" basis and without warranty of any kind.
//  Global Graphics Software Ltd. does not warrant or make any representations
//  regarding the use or results of use of this example.
//  </summary>
// -----------------------------------------------------------------------

using JawsMako;

namespace MovePageContent
{
    public class MoveSpecModel
    {
        public FRect Source { get; set; }
        public FRect Target { get; set; }
        public FRect KeyPos { get; set; }
        public string KeyMatch { get; set; }
    }
}
