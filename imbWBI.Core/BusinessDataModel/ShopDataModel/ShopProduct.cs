// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ShopProduct.cs" company="imbVeles" >
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

namespace imbWBI.Core.BusinessDataModel.ShopDataModel
{

    /// <summary>
    /// Data model for a product, harvested from online shop
    /// </summary>
    /// <seealso cref="imbWBI.Core.BusinessDataModel.ShopDataModel.WebDataElement" />
    public class ShopProduct : WebDataElement
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="ShopProduct"/> class.
        /// </summary>
        public ShopProduct() { }

        /// <summary> Price attached to the product </summary>
        [Category("Sales")]
        [DisplayName("Price")]
        [imb(imbAttributeName.measure_letter, "")]
        [imb(imbAttributeName.reporting_valueformat, "F2")]
        [imb(imbAttributeName.reporting_columnWidth, 10)]
        [Description("Price attached to the product")] // [imb(imbAttributeName.measure_important)][imb(imbAttributeName.reporting_escapeoff)]
        public Double Price { get; set; } = default(Double);


        /// <summary> Price currency sign - if detected </summary>
        [Category("Price")]
        [DisplayName("Currency")] //[imb(imbAttributeName.measure_letter, "")]
        [Description("Price currency sign - if detected")] // [imb(imbAttributeName.reporting_escapeoff)]
        [imb(imbAttributeName.reporting_columnWidth, 5)]
        public String Currency { get; set; } = default(String);
        
        /// <summary> Brand name or identification </summary>
        [Category("Sales")]
        [DisplayName("Brand")] //[imb(imbAttributeName.measure_letter, "")]
        [Description("Brand name or identification")] // [imb(imbAttributeName.reporting_escapeoff)]
        [imb(imbAttributeName.reporting_columnWidth, 10)]
        public String Brand { get; set; } = default(String);
        
    }

}