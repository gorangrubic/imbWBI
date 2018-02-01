// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WebClassifierResultSet.cs" company="imbVeles" >
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
using System.Threading;
using imbACE.Core.core;

namespace imbWBI.Core.WebClassifier.core
{

    public class WebClassifierResultSet
    {

        public DocumentSetClasses setClassCollection { get; protected set; }

        public WebSiteClassifierResult featureVectors { get; protected set; }


        private Object classifiersLock = new Object();

        public WebClassifierResultSet(DocumentSetClasses _setClassCollection, experimentExecutionContext _context, IFVExtractorSettings _extractorSettings)
        {
            setClassCollection = _setClassCollection;
            featureVectors = new WebSiteClassifierResult(_setClassCollection, _extractorSettings);

            items.Add(INPUTSET_NAME, featureVectors);

            List<IWebPostClassifier> classifiers = new List<IWebPostClassifier>();

            for (int i = 0; i < _context.setup.classifiers.Count; i++)
            {
                classifiers.Add(_context.setup.classifiers[i]);
            }

            Thread.Sleep(100);

            if (classifiers.Count < _context.setup.classifiers.Count)
            {
                aceLog.log("::: MULTITHREADING --- CLASSIFIERS COUNT MISTMATCHED --- AUTOCORRECTION APPLIED :::");
                for (int i = classifiers.Count-1; i < _context.setup.classifiers.Count; i++)
                {
                    classifiers.Add(_context.setup.classifiers[i]);
                }
            }

            foreach (var cs in classifiers)
            {
                items.Add(cs.name, new WebSiteClassifierResult(_setClassCollection, _extractorSettings));
            }


        }

        public const String INPUTSET_NAME = "FeatureVectors";


        public WebSiteClassifierResult this[IWebPostClassifier classifier]
        {
            get
            {
                return items[classifier.name];
            }
        }

        public WebSiteClassifierResult this[String key]
        {
            get
            {
                return items[key];
            }
        }

        private Dictionary<String, WebSiteClassifierResult> _items = new Dictionary<String, WebSiteClassifierResult>();
        /// <summary>
        /// 
        /// </summary>
        protected Dictionary<String, WebSiteClassifierResult> items
        {
            get
            {
                //if (_items == null)_items = new Dictionary<String, WebSiteClassifierResult>();
                return _items;
            }
            set { _items = value; }
        }


    }

}