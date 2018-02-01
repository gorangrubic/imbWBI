// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ShopDataModel.cs" company="imbVeles" >
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
using imbACE.Network.tools;
using imbSCI.Core.attributes;
using imbSCI.Data.collection.graph;
using imbSCI.Data.interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using imbWBI.Core.BusinessDataModel.GeneralDataModel;
using imbWBI.Core.BusinessDataModel;

namespace imbWBI.Core.BusinessDataModel.ShopDataModel
{

    /// <summary>
    /// Shop Data Model
    /// </summary>
    /// <seealso cref="imbWBI.Core.BusinessDataModel.ShopDataModel.ShopWebDataElement" />
    /// <seealso cref="imbWBI.Core.BusinessDataModel.IDataModel" />
    public class ShopDataModel : ShopWebDataElement, IDataModel
    {
        
        /// <summary> Domain information </summary>
        [Category("Label")]
        [DisplayName("DomainInfo")] //[imb(imbAttributeName.measure_letter, "")]
        [Description("Domain information")] // [imb(imbAttributeName.reporting_escapeoff)]
        public domainAnalysis DomainInfo { get; set; } = default(domainAnalysis);




    }

}