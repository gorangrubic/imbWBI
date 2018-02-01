// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NamespaceDoc.cs" company="imbVeles" >
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
using System.Linq;
using System.Collections.Generic;
using imbACE.Network.tools;
using imbSCI.Core.attributes;
using imbSCI.Data.collection.graph;
using imbSCI.Data.interfaces;
using System.ComponentModel;
using System.Text;

namespace imbWBI.Core.BusinessDataModel.ShopDataModel
{

    /// <summary>
    /// <para>Graph-directed graph tree model of an online shop</para>
    /// </summary>
    /// <remarks>
    /// <para>Holds structrured data on a harvested on-line shop</para>
    /// <list type="bullet">
    /// 	<listheader>
    ///			<term>Main elements</term>
    ///			<description>Elements in the model hierarchy</description>
    ///		</listheader>
    ///		<item>
    ///			<term><see cref="ShopDataModel"/></term>
    ///			<description>Root element</description>
    ///		</item>
    ///		<item>
    ///			<term><see cref="ShopCategory"/></term>
    ///			<description>Contains data on a category in on-line shop portfolio</description>
    ///		</item>
    ///		<item>
    ///			<term><see cref="ShopProduct"/></term>
    ///			<description>Describes a product, may contain subnodes like <see cref="WebDataImage"/>, <see cref="WebDataTechnicalProperty"/>...</description>
    ///		</item>
    /// </list>
    /// <list type="bullet">
    ///     <listheader>
    ///			<term>Additional elements</term>
    ///			<description>Multimedia elements and data</description>
    ///		</listheader>
    ///		<item>
    ///			<term><see cref="WebDataImage"/></term>
    ///			<description>Represents an image harvested from product or category page</description>
    ///		</item>
    ///		<item>
    ///			<term><see cref="WebDataTechnicalProperty"/></term>
    ///			<description>A technical property of a product</description>
    ///		</item>
    ///		<item>
    ///			<term><see cref="WebDataVideo"/></term>
    ///			<description>Embedded video link</description>
    ///		</item>
    /// </list>
    /// </remarks>
    /// <seealso cref="WebHarvester.HarvesterProject" />
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    class NamespaceDoc
    {
    }

}