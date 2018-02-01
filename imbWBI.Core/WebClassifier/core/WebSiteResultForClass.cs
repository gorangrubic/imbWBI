// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WebSiteResultForClass.cs" company="imbVeles" >
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
using System.Collections.Concurrent;

namespace imbWBI.Core.WebClassifier.core
{

    public class WebSiteResultForClass
    {
        
        public WebSiteResultForClass(FeatureVectorDefinitionSet featureVectors)
        {
            FVDefinitions = featureVectors;
            foreach (var FV in FVDefinitions.serialization)
            {
                SetValue(FV.id, 0);
            }
        }

        /// <summary>
        /// Returns values for feature vectors
        /// </summary>
        /// <param name="onlyActiveFVs">if set to <c>true</c> [only active f vs].</param>
        /// <returns></returns>
        public List<Double> GetValues(Boolean onlyActiveFVs=true)
        {
            List<Double> vl = new List<double>();

            foreach (var FV in FVDefinitions.serialization)
            {
                if (FV.isActive || !onlyActiveFVs)
                {
                    vl.Add(vectors[FV.id]);
                }
            }

            return vl;
        }

        public Int32 GetCount(Boolean onlyActiveFVs = true)
        {
            Int32 vl = 0;
            foreach (var FV in FVDefinitions.serialization)
            {
                if (FV.isActive || !onlyActiveFVs)
                {
                    vl = vl + 1;
                }
            }
            return vl;
        }

        protected Double GetValue(Int32 id)
        {
            return vectors[id];
        }

        protected Double SetValue(Int32 id, Double val)
        {
            vectors[id] = val;
            return vectors[id];
        }

        public Double this[String name] { get { return GetValue(FVDefinitions[name].id); } set { SetValue(FVDefinitions[name].id,value); } }

        public Double this[Int32 id] { get { return GetValue(id); } set { SetValue(id, value); } }

        public Double this[FeatureVectorDefinition fv]
        { get { return GetValue(fv.id); }
            set { SetValue(fv.id, value); } }

        //public void SetValues(Double[] vl)
        //{
        //    for (int i = 0; i < cs.Length; i++)
        //    {
        //        this[i].score = cs[i];
        //    }
        //}

        public Double classScore { get; set; } = 0;

        public FeatureVectorDefinitionSet FVDefinitions { get; set; } 

        public Int32 classID { get; set; }

        //public Double score { get; set; } = 0;

        protected ConcurrentDictionary<Int32, Double> vectors { get; set; } = new ConcurrentDictionary<int, double>();

        public Int32 termMatched { get; set; } = 0;

    }

}