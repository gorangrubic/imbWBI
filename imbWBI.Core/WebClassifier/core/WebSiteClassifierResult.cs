// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WebSiteClassifierResult.cs" company="imbVeles" >
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
using imbWBI.Core.WebClassifier.experiment;

namespace imbWBI.Core.WebClassifier.core
{


    public class WebSiteClassifierResult:Dictionary<Int32, WebSiteResultForClass>
    {
        public DocumentSetClasses setClassCollection { get; protected set; }

        public Dictionary<String, WebSiteClassifierResult> postResults { get; set; } = new Dictionary<string, WebSiteClassifierResult>();

        public Int32 CountFeatureVectors()
        {
            var first = this.FirstOrDefault();
            Int32 vl = Count * first.Value.GetCount(true);
            return vl;
        }


        public WebSiteClassifierResult(DocumentSetClasses _setClassCollection, IFVExtractorSettings _settings)
        {
            setClassCollection = _setClassCollection;

            foreach (var pair in setClassCollection.GetClasses())
            {
                WebSiteResultForClass resUnit = new WebSiteResultForClass(_settings.featureVectors);
                resUnit.classID = pair.classID;
                this.Add(pair.classID, resUnit);
            }

            
        }

        public List<Double> GetClassScoreValues()
        {
            var vl = new List<double>();
            foreach (var v in Values)
            {
                vl.Add(v.classScore);
            }

            return vl;
        }

        public List<Double> GetValues(Boolean onlyActiveFVs=true)
        {
            var vl = new List<double>();
            foreach (var v in Values)
            {
                vl.AddRange(v.GetValues(onlyActiveFVs));
            }
            
            return vl;
        }

        public void SetValues(Double[] cs)
        {
            for (int i = 0; i < cs.Length; i++)
            {
                this[i].classScore = cs[i];
            }
            selected = GetClassWithHighestScore();
        }

        public void SetValues(Int32 selectedClassID)
        {
            selected = null;
            foreach (var pair in this)
            {
                if (pair.Key == selectedClassID)
                {
                    pair.Value.classScore = 1;
                    selected = setClassCollection[pair.Key];
                } else
                {
                    pair.Value.classScore = 0;
                }
            }
        }

        public IDocumentSetClass selected { get; set; }

        public IDocumentSetClass GetClassWithHighestScore()
        {
            Int32 output = -1;
            Double highest = Double.MinValue;

            foreach (var pair in this)
            {
                if (pair.Value.classScore > highest)
                {
                    output = pair.Key;
                    highest = pair.Value.classScore;
                }
            }

            if (output == -1) return null;

            return setClassCollection[output];
        }
    }

}