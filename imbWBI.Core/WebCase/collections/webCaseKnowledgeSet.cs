﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="webCaseKnowledgeSet.cs" company="imbVeles" >
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace imbWBI.Core.WebCase.collections
{
    public class webProjectKnowledgeSet
    {


        /// <summary> Name of the project that collected this knowledge set </summary>
        [Category("Label")]
        [DisplayName("name")] //[imb(imbAttributeName.measure_letter, "")]
        [Description("Name of the project that collected this knowledge set")] // [imb(imbAttributeName.reporting_escapeoff)]
        public String name { get; set; } = "";

        public void Add(webCaseKnowledge webCase)
        {
            webCases.Add(webCase);
        }

        public List<webCaseKnowledge> GetCases()
        {
            return webCases.ToList();
        }
        

        public ConcurrentBag<webCaseKnowledge> webCases { get; set; } = new ConcurrentBag<webCaseKnowledge>();


    }

    public class webCaseKnowledgeSet
    {


    }
}
