// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HarvesterExecutionContext.cs" company="imbVeles" >
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
using imbSCI.Core.files.folders;
using imbWBI.Core.BusinessDataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace imbWBI.Core.WebHarvester
{
    /// <summary>
    /// Execution context
    /// </summary>
    public class HarvesterExecutionContext
    {

        /// <summary>
        /// Gets or sets the data model.
        /// </summary>
        /// <value>
        /// The data model.
        /// </value>
        public IDataModel dataModel { get; protected set; }

        /// <summary>
        /// Gets or sets the folder.
        /// </summary>
        /// <value>
        /// The folder.
        /// </value>
        public folderNode folder { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HarvesterExecutionContext"/> class.
        /// </summary>
        /// <param name="_project">The project.</param>
        /// <param name="_unit">The unit.</param>
        /// <param name="_parentFolder">The parent folder.</param>
        public HarvesterExecutionContext(HarvesterProject _project, HarvesterUnit _unit, folderNode _parentFolder)
        {
            project = _project;
            unit = _unit;
            folder = _parentFolder.Add(_project.name, _project.name + " output", _project.description);

        }
        /// <summary>
        /// Gets or sets the project.
        /// </summary>
        /// <value>
        /// The project.
        /// </value>
        public HarvesterProject project { get; protected set; }

        /// <summary>
        /// Gets or sets the unit.
        /// </summary>
        /// <value>
        /// The unit.
        /// </value>
        public HarvesterUnit unit { get; protected set; }
    }
}
