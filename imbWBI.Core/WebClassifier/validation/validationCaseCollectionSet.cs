// --------------------------------------------------------------------------------------------------------------------
// <copyright file="validationCaseCollectionSet.cs" company="imbVeles" >
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

namespace imbWBI.Core.WebClassifier.validation
{

    /// <summary>
    /// Per <see cref="IDocumentSetClass"/> collection of <see cref="validationCaseCollection"/>
    /// </summary>
    /// <seealso cref="System.Collections.Generic.List{imbWBI.Core.WebClassifier.validation.validationCaseCollection}" />
    public class validationCaseCollectionSet:List<validationCaseCollection>
    {

        public void connectContext(kFoldValidationCollection _master, kFoldValidationCase _case)
        {
            kFoldMaster = _master;
            kFoldCase = _case;
            foreach (var col in this)
            {
                col.kFoldCase = _case;
                col.kFoldMaster = _master;
            }
        }

        [XmlIgnore]
        public kFoldValidationCollection kFoldMaster { get; set; }

        [XmlIgnore]
        public kFoldValidationCase kFoldCase { get; set; }


        public validationCaseCollection this[String className]
        {
            get
            {
                foreach (var vc in this)
                {
                    if (vc.className == className)
                    {
                        return vc;
                    }
                }
                return null;
            }
        }

        public validationCaseCollection Add(String className, List<String> items)
        {
            validationCaseCollection newColl = new validationCaseCollection(className, items);
            Add(newColl);
            return newColl;
        }

        public validationCaseCollectionSet()
        {

        }
    }

}