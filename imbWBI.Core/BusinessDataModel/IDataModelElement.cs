// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDataModelElement.cs" company="imbVeles" >
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
using imbSCI.Data.collection.graph;
using imbSCI.Data.interfaces;
using System.Text;
using imbWBI.Core.BusinessDataModel.GeneralDataModel;
using imbWBI.Core.BusinessDataModel.ShopDataModel;

namespace imbWBI.Core.BusinessDataModel
{

    /// <summary>
    /// Interface to <see cref="WebDataElement"/>
    /// </summary>
    /// <seealso cref="imbSCI.Data.collection.graph.IGraphNode" />
    /// <seealso cref="imbSCI.Data.interfaces.IObjectWithNameAndDescription" />
    public interface IDataModelElement:IGraphNode, IObjectWithNameAndDescription
    {
        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        String description { get; set; }

        /// <summary>
        /// Gets or sets the content of the description.
        /// </summary>
        /// <value>
        /// The content of the description.
        /// </value>
        List<String> DescriptionContent { get; set; }
        /// <summary>
        /// Gets or sets the source URL.
        /// </summary>
        /// <value>
        /// The source URL.
        /// </value>
        String SourceUrl { get; set; }
        /// <summary>
        /// Gets or sets the local URL.
        /// </summary>
        /// <value>
        /// The local URL.
        /// </value>
        String LocalUrl { get; set; }
        /// <summary>
        /// Gets or sets the local path.
        /// </summary>
        /// <value>
        /// The local path.
        /// </value>
        String LocalPath { get; set; }
    }

}