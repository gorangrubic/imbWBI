// --------------------------------------------------------------------------------------------------------------------
// <copyright file="kFoldValidationCollection.cs" company="imbVeles" >
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
using imbSCI.DataComplex.tables;
using imbSCI.Core.math;
using imbWBI.Core.WebClassifier.experiment;
using imbWBI.Core.WebClassifier.wlfClassifier;
using imbWBI.Core.WebClassifier.core;
using System.Data;
using imbACE.Network.extensions;
using imbSCI.Core.extensions.table;
using imbSCI.Data.collection.nested;
using imbACE.Core.core;

namespace imbWBI.Core.WebClassifier.validation
{

    public class kFoldValidationCollection
    {

        [XmlIgnore]
        public DocumentSetPipelineCollection pipelineCollection { get; set; }

        [XmlIgnore]
        public WebFVExtractorKnowledgeLibrary knowledgeLibrary { get; set; } 


        public kFoldValidationCollection()
        {

        }


        /// <summary>
        /// The sample matrix :: list of web sites for each class
        /// </summary>
        [XmlIgnore]
        public Dictionary<IDocumentSetClass, List<String>> sampleMatrix = new Dictionary<IDocumentSetClass, List<string>>();

        public void connectContext(experimentExecutionContext _context, IWebFVExtractor _fve)
        {
            
            context = _context;
            extractor = _fve;

            caseFolder = folder.Add("cases", "Cases", "Directory with serialized DocumentSetCase knowledge, shared among k-fold validation folds for faster execution.");

            foreach (kFoldValidationCase setCase in items)
            {
                setCase.context = context;
                setCase.extractor = _fve;
                setCase.trainingCases.connectContext(this, setCase);
                setCase.evaluationCases.connectContext(this, setCase);
            }

            knowledgeLibrary = new WebFVExtractorKnowledgeLibrary(this);
        }

        public String DescribeSampleDistribution(ILogBuilder modelNotes)
        {
            if (modelNotes == null) modelNotes = new builderForLog();

            var l = modelNotes.Length;
            foreach (var vc in GetCases())
            {
                modelNotes.AppendHeading("Fold: " + vc.name, 2);

                var categoryCaseList = new aceDictionarySet<String, String>();

                foreach (validationCaseCollection vcc in vc.trainingCases)
                {
                    foreach (string vccs in vcc)
                    {
                        categoryCaseList.Add(vcc.className, "[T] " + vccs);
                    }
                }

                foreach (validationCaseCollection vcc in vc.evaluationCases)
                {
                    foreach (string vccs in vcc)
                    {
                        categoryCaseList.Add(vcc.className, "[E] " + vccs);
                    }
                }

                foreach (var k in categoryCaseList.Keys)
                {
                    modelNotes.AppendHeading("Category: " + k, 3);
                    foreach (var s in categoryCaseList[k])
                    {
                        modelNotes.AppendLine(s);
                    }
                }
            }
            
            SampleDistributionNote = modelNotes.GetContent(l);
            SampleDistributionHash = md5.GetMd5Hash(SampleDistributionNote);
            return SampleDistributionNote;
        }


        /// <summary>
        /// Sample description - used for experiment note and for <see cref="SampleDistributionHash"/>
        /// </summary>
        /// <value>
        /// The sample distribution note.
        /// </value>
        public String SampleDistributionNote { get; set; } = "";

        /// <summary>
        /// MD5 hash of <see cref="SampleDistributionNote"/>
        /// </summary>
        /// <value>
        /// The sample distribution hash.
        /// </value>
        public String SampleDistributionHash { get; set; } = "";


        [XmlIgnore]
        public experimentExecutionContext context { get; set; }

        [XmlIgnore]
        public IWebFVExtractor extractor { get; set; }

        [XmlIgnore]
        public List<IWebPostClassifier> postClassifiers { get; set; }

        [XmlIgnore]
        public folderNode folder { get; set; }


        public void Clear()
        {
            valCaseNames.Clear();
            items.Clear();
           // folderRoot.deleteFiles();

        }

        ///// <summary>
        ///// Gets the validation results table for complete collection
        ///// </summary>
        ///// <returns>DataTable</returns>
        //public DataTable GetValidationTable()
        //{

        //    String repTitle = context.setup.name + " - " + featureVectorExtractor.name;
        //    String repName = repTitle.getCleanFileName();

        //    DataTable table = new DataTable(repName);
         
        //    objectTable<DocumentSetCaseCollectionReport> output = new objectTable<DocumentSetCaseCollectionReport>(nameof(DocumentSetCaseCollectionReport.kFoldCase), repName);

            
        //    foreach (IWebPostClassifier classifier in context.setup.classifiers)
        //    {
                

        //        foreach (kFoldValidationCase valCase in items)
        //        {
        //            var r = valCase.testReport[classifier];
                   

        //            i++;
        //            output.Add(r);
        //        }
        //        avgReport.DivideValues(i);
        //        output.Add(avgReport);
        //    }



        //    var dt = output.GetDataTable(null, repName);
        //    dt.GetTitle(repTitle);


        //    return dt;

        //}

        public kFoldValidationCase CreateNew(String valCaseName)
        {
            kFoldValidationCase valCase = new kFoldValidationCase();
            valCase.name = valCaseName;
            valCase.id = items.Count;
            DeployCase(valCase);
            items.Add(valCase);
            return valCase;
        }

        public void DeployCase(kFoldValidationCase valCase, folderNode folderOverride = null)
        {
            var fl = folder;
            if (folderOverride != null) fl = folderOverride;
            valCase.kFoldMaster = this;
            valCase.trainingCases.kFoldMaster = this;
            valCase.evaluationCases.kFoldMaster = this;
            valCase.evaluationCases.kFoldCase = valCase;
            valCase.trainingCases.kFoldCase = valCase;
            valCase.folder = folder.Add(valCase.name, valCase.name, "Operational files and reports for k-fold [" + valCase.name + "]");
            valCase.caseFolder = valCase.folder.Add("cases", "Cases", "Repository with knowledge on cases");
            valCase.caseSampleFolder = valCase.caseFolder.Add("microAnalysis", "Micro-analysis of FV Extraction", "Randomly picked cases for Micro-analysis of FV Extraction - similarity computation between a case and a category");
        }

        public kFoldValidationCase this[Int32 i]
        {
            get
            {
                return items[i];
            }
        }

        public kFoldValidationCase GetDiagnosticCase(DocumentSetClasses classes)
        {
            kFoldValidationCase valCase = new kFoldValidationCase();
            valCase.name = "Diagnostic";
            valCase.id = items.Count;
            DeployCase(valCase);
            foreach (IDocumentSetClass c in classes.GetClasses()) {
                valCase.trainingCases[c.classID].AddRange(c.WebSiteSample);
                valCase.evaluationCases[c.classID].AddRange(c.WebSiteSample);
            }
            
            
            return valCase;
        }


        public List<kFoldValidationCase> GetCases()
        {
            return items;
        }

        public List<String> valCaseNames { get; set; } = new List<string>();

        private List<kFoldValidationCase> _items = new List<kFoldValidationCase>();
        /// <summary>
        /// 
        /// </summary>
        protected List<kFoldValidationCase> items
        {
            get
            {
                //if (_items == null)_items = new Dictionary<, kFoldValidationCase>();
                return _items;
            }
            set { _items = value; }
        }

        public folderNode caseFolder { get; set; }

        public void OnLoad(folderNode folder, ILogBuilder output)
        {
            if (folder != null) this.folder = folder;
            foreach (String className in valCaseNames)
            {
                kFoldValidationCase setClass = className.LoadDataStructure<kFoldValidationCase>(this.folder, output);
                setClass.kFoldMaster = this;
                setClass.evaluationCases.kFoldMaster = this;
                setClass.trainingCases.kFoldMaster = this;
                setClass.id = items.Count;
                DeployCase(setClass);
                items.Add(setClass);
            }
        }


        public void OnBeforeSave(ILogBuilder output) 
        {
            foreach (kFoldValidationCase setClass in items)
            {
                setClass.SaveDataStructure<kFoldValidationCase>(folder, output);
            }
        }

    }

}