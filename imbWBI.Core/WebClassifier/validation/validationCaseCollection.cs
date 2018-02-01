// --------------------------------------------------------------------------------------------------------------------
// <copyright file="validationCaseCollection.cs" company="imbVeles" >
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
using imbSCI.Core.files.fileDataStructure;
using imbSCI.Core.files.folders;
using imbSCI.Core.reporting;
using imbWBI.Core.WebClassifier.cases;
using System.Text;
using System.Xml.Serialization;
using imbNLP.PartOfSpeech.pipelineForPos.subject;
using imbACE.Network.extensions;
using imbACE.Core.core;

namespace imbWBI.Core.WebClassifier.validation
{

    public class validationCaseCollection:List<String>
    {
        public String className { get; set; } = "";

        [XmlIgnore]
        public kFoldValidationCollection kFoldMaster { get; set; }

        [XmlIgnore]
        public kFoldValidationCase kFoldCase { get; set; }





        public List<pipelineTaskMCSiteSubject> FilterSites(List<pipelineTaskMCSiteSubject> input)
        {
            List<pipelineTaskMCSiteSubject> output = new List<pipelineTaskMCSiteSubject>();
            Int32 c = 0;
            foreach (pipelineTaskMCSiteSubject site in input)
            {
                foreach (String s in this)
                {
                    var sc = pipelineSubjectTools.GetCleanCaseName(s);
                    if (sc.Equals(site.name))
                    {
                        c++;
                     //   aceLog.log("[" + c.ToString() + "] Case [" + sc + "] added to [" + className + "] at [" + kFoldCase.name + "]");
                        output.Add(site);

                        break;
                    }
                }
            }



            return output;
        }

        public validationCaseCollection(String _className, List<String> _list)
        {
            className = _className;
            AddRange(_list);
        }

        public validationCaseCollection()
        {

        }



    }

}