// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDocumentSetClass.cs" company="imbVeles" >
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
using imbMiningContext.MCDocumentStructure;
// using imbMiningContext.TFModels.WLF_ISF;
using imbNLP.PartOfSpeech.pipelineForPos.subject;
using imbSCI.Core.extensions.data;
using imbSCI.Core.math;
using imbSCI.Core.reporting;
using imbSCI.Data;
using imbWBI.Core.WebClassifier.core;
using System.Text;
using imbSCI.Core.files.fileDataStructure;
using imbMiningContext.MCRepository;

namespace imbWBI.Core.WebClassifier.cases
{



    public interface IDocumentSetClass: IFileDataStructure
    {
        Int32 classID { get; set; }

        String treeLetterAcronim { get; set; }

        //String name { get; set; }

        imbMCRepository MCRepository { get; set; }

        String MCRepositoryName { get;  }

        List<String> WebSiteSample { get; set; }


        List<pipelineTaskMCSiteSubject> FilterSites(List<pipelineTaskMCSiteSubject> input);

        DocumentSetClasses parent { get; set; }

       

    }

}