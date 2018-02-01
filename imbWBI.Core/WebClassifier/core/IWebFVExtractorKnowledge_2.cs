// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IWebFVExtractorKnowledge_2.cs" company="imbVeles" >
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
using System.Text;
using imbWBI.Core.WebClassifier.wlfClassifier;
using imbWBI.Core.WebClassifier.cases;
using imbSCI.Core.files.folders;
// using imbMiningContext.TFModels.WLF_ISF;
using imbSCI.Core.reporting;

namespace imbWBI.Core.WebClassifier.core
{

    public interface IWebFVExtractorKnowledge
    {
        String name { get; set; }
        folderNode folder { get; set; }
        String relatedItemPureName { get; set; }
        /// <summary>
        /// Sets knowledge object reference folder and then calls <see cref="OnLoaded()"/>
        /// </summary>
        /// <param name="_folder">The folder.</param>
        Boolean Deploy(folderNode _folder, ILogBuilder logger);

        /// <summary>
        /// There is no reason to call this manually. Use <see cref="Deploy(folderNode)"/>, it will call this method once folder is set
        /// </summary>
        void OnLoaded();

        void OnBeforeSave();

        /// <summary>
        /// Checks if this knowledge unit requires anything to be built
        /// </summary>
        /// <returns></returns>
        bool ShouldBuildAny();

        void SetRebuild(Boolean to = true);

        WebFVExtractorKnowledgeType type { get; set; }

        
    }

}