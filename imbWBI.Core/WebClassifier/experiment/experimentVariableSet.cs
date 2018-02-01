// --------------------------------------------------------------------------------------------------------------------
// <copyright file="experimentVariableSet.cs" company="imbVeles" >
//
// Copyright (C) 2018 imbVeles
//
// This program is free software: you can redistribute it and/or modify
// it under the +terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see http://www.gnu.org/licenses/. 
// </copyright>
// <summary>
// Project: imbWBI.Core
// Author: Goran Grubic
// ------------------------------------------------------------------------------------------------------------------
// Project web site: http://blog.veles.rs
// GitHub: http://github.com/gorangrubic
// Mendeley profile: http://www.mendeley.com/profiles/goran-grubi2/
// ORCID ID: http://orcid.org/0000-0003-2673-9471
// Email: hardy@veles.rs
// </summary>
// ------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace imbWBI.Core.WebClassifier.experiment
{
    public class experimentVariableSet
    {
        public experimentVariableSet()
        {
            Flags.Add("TW", new List<string>());
            Flags.Add("TC", new List<string>());
            Flags.Add("RX", new List<string>());
            Flags.Add("DFC", new List<string>());
            Flags.Add("LPF", new List<string>());
        }

        public Dictionary<String, List<String>> Flags { get; set; } = new Dictionary<string, List<string>>();


        public List<String> TWFlags
        {
            get { return Flags["TW"]; }
        }
        public List<String> TCFlags { get; set; } = new List<string>();
        public List<String> RXFlags { get; set; } = new List<string>();

        public List<String> DFCFlags { get; set; } = new List<string>();
        public List<String> LPFFlags { get; set; } = new List<string>();

        public List<Boolean> IDFOn { get; set; } = new List<bool>();


        public Int32 STXStart { get; set; } = 2;
        public Int32 STXEnd { get; set; } = 8;
    }
}
