// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WebDataElement.cs" company="imbVeles" >
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
using imbSCI.Data.collection.graph;
using imbSCI.Data.interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace imbWBI.Core.BusinessDataModel.GeneralDataModel
{


    /// <summary>
    /// Basic data element of <see cref="ShopDataModel"/>
    /// </summary>
    /// <seealso cref="imbSCI.Data.collection.graph.graphNodeCustom" />
    /// <seealso cref="imbSCI.Data.interfaces.IObjectWithName" />
    public abstract class WebDataElement : graphNodeCustom, IObjectWithName
    {
        protected override bool doAutorenameOnExisting => false;

        protected override bool doAutonameFromTypeName => false;

        /// <summary> Code </summary>
        [Category("id")]
        [DisplayName("id")] //[imb(imbAttributeName.measure_letter, "")]
        [Description("Code")] // [imb(imbAttributeName.reporting_escapeoff)]
        public String id { get; set; } = default(String);

        /// <summary> Name </summary>
        [Category("Content")]
        [DisplayName("Name")] //[imb(imbAttributeName.measure_letter, "")]
        [Description("Name")] // [imb(imbAttributeName.reporting_escapeoff)]
        public new String name { get { return base.name; } set { base.name = value; } }


        /// <summary> Path relative to root directory of the output </summary>
        [Category("Label")]
        [DisplayName("LocalPath")] //[imb(imbAttributeName.measure_letter, "")]
        [Description("Path relative to root directory of the output")] // [imb(imbAttributeName.reporting_escapeoff)]
        public String LocalPath { get; set; } = "";




    }

    
}
