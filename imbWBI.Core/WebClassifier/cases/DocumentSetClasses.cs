// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DocumentSetClasses.cs" company="imbVeles" >
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
using imbSCI.Core.files.folders;
using imbSCI.Core.math;
using imbSCI.Core.reporting;
using imbSCI.Data;
using imbWBI.Core.WebClassifier.core;
using System.Text;
using System.Xml.Serialization;
using imbSCI.Core.files.fileDataStructure;
using System.IO;
using imbWBI.Core.WebClassifier.validation;
using imbSCI.Data.data.sample;
using imbSCI.Data.collection.nested;

namespace imbWBI.Core.WebClassifier.cases
{

    public class DocumentSetClasses
    {

        public DocumentSetClasses()
        {

        }

        /// <summary>
        /// Builds the validation cases.
        /// </summary>
        /// <param name="basename">The basename.</param>
        /// <param name="k">The k.</param>
        /// <param name="debug">if set to <c>true</c> [debug].</param>
        /// <param name="output">The output.</param>
        /// <returns></returns>
        public kFoldValidationCollection BuildValidationCases(String basename, Int32 k, Boolean debug, ILogBuilder output = null, folderNode folderOverride = null, Boolean randomize = false)
        {
            kFoldValidationCollection validationCases = new kFoldValidationCollection();

            folderNode folderToUse = folderRoot;
            if (folderOverride != null) folderToUse = folderOverride;

            validationCases.folder = folderToUse.Add(basename, basename, basename + " " + k + "-fold validation");
           
            validationCases.Clear();

            var classes = GetClasses();

            validationCases.sampleMatrix = new Dictionary<IDocumentSetClass, List<string>>();
            

            foreach (IDocumentSetClass cl in classes)
            {
                validationCases.sampleMatrix.Add(cl, cl.WebSiteSample.ToList());
                //   sampling.takeOrder = samplingOrderEnum.randomSuffle;
            }


            samplingSettings sampling = new samplingSettings();
            sampling.parts = k;
            sampling.takeOrder = samplingOrderEnum.ordinal;

            if (randomize)
            {
                foreach (IDocumentSetClass cl in classes)
                {
                    validationCases.sampleMatrix[cl].Randomize();
                    //sampleMatrix.Add(cl, cl.WebSiteSample.ToList());
                    //   sampling.takeOrder = samplingOrderEnum.randomSuffle;
                }
            }



            for (int i = 0; i < k; i++)
            {
                var valCase = validationCases.CreateNew(basename + i.ToString("D3"));

                

                foreach (IDocumentSetClass cl in classes)
                {
                    List<String> sample = validationCases.sampleMatrix[cl].ToList();

                    //if (randomize)
                    //{
                    //    sample.Randomize();
                    //}

                    //Int32 foldSize = sample.Count() / k;




                    if (k > 1)
                    {
                        
                        

                        sampling.skip = i;

                        var eval = new sampleTake<String>(sample, sampling);

                        sample = eval.GetRestOfSource();

                        valCase.trainingCases.Add(cl.name, sample);
                        valCase.evaluationCases.Add(cl.name, eval);
                    } else
                    {
                        valCase.trainingCases.Add(cl.name, sample);
                        valCase.evaluationCases.Add(cl.name, sample);
                    }

                    if (output != null) if (debug) output.AppendLine("Case [" + valCase.name + "] for [" + cl.name + "] have training[" + valCase.trainingCases[cl.name].Count + "] and eval[" + valCase.evaluationCases[cl.name].Count + "]");

                }

                if (output != null)
                {
                    
                    output.log("k-fold validation case [" + valCase.name + "] created for [" + valCase.trainingCases.Count + "] industries");
                }

            }

            //validationCases.OnLoad(null, output);

            validationCases.OnBeforeSave(output);
            return validationCases;
        }


        public List<IDocumentSetClass> GetClasses()
        {
            return items;
        }

        public IDocumentSetClass GetClass(String className, ILogBuilder logger)
        {
            IDocumentSetClass output = null;

            foreach (IDocumentSetClass cl in items)
            {
                if (cl.name == className)
                {
                    logger.log("Class found by name");
                    output = cl;
                    break;
                }
                if (cl.treeLetterAcronim == className)
                {
                    logger.log("Class found by TLA");
                    output = cl;
                    break;
                }
            }

            if (output == null)
            {
                logger.log("No class found under [" + className + "] name nor tree letter accronim");
            }

            return output;
        }


        public void Clear()
        {
            classNames.Clear();
            items.Clear();
        }

        public void Add(IDocumentSetClass setClass)
        {
            classNames.Add(setClass.name);
            setClass.classID = items.Count;
            setClass.parent = this;
            items.Add(setClass);
        }

        public IDocumentSetClass this[Int32 _id]
        {
            get
            {
                foreach (IDocumentSetClass setClass in items)
                {
                    if (setClass.classID == _id) return setClass;
                }
                return null;
            }
        }

        public IDocumentSetClass this[String _name]
        {
            get
            {
                foreach (IDocumentSetClass setClass in items)
                {
                    if (setClass.name == _name) return setClass;
                }
                return null;
            }
        }


        public List<String> classNames { get; set; } = new List<String>();

        [XmlIgnore]
        public folderNode folderRoot { get; set; }

        private List<IDocumentSetClass> _items = new List<IDocumentSetClass>();
        /// <summary>
        /// 
        /// </summary>
        protected List<IDocumentSetClass> items
        {
            get
            {
                //if (_items == null)_items = new Dictionary<Int32, IDocumentSetClass>();
                return _items;
            }
            set { _items = value; }
        }
        public const String DESC_ListOfIndustries = "List of industries / categories - used by this project";
        public void OnLoad<T>(folderNode folder, ILogBuilder output) where T:IDocumentSetClass, new()
        {
            folderRoot = folder;

            String pt = folder.pathFor("DocumentSetClasses.txt", imbSCI.Data.enums.getWritableFileMode.newOrExisting, DESC_ListOfIndustries);
            if (File.Exists(pt))
            {
                classNames = File.ReadAllLines(pt).ToList();
            }

            foreach (String className in classNames)
            {
                IDocumentSetClass setClass = className.LoadDataStructure<T>(folderRoot, output);
                setClass.classID = items.Count;
                setClass.parent = this;
                items.Add(setClass);
            }
        }


        public void OnBeforeSave<T>(ILogBuilder output) where T : IDocumentSetClass, new()
        {
            String pt = folderRoot.pathFor("DocumentSetClasses.txt", imbSCI.Data.enums.getWritableFileMode.newOrExisting, DESC_ListOfIndustries);
            File.WriteAllLines(pt, classNames);

            foreach (T setClass in items)
            {
                setClass.SaveDataStructure<T>(folderRoot, output);
            }
        }

        
        //public DocumentSetClass AddWLFAndClass(webLemmaTermTable wlfTable, Int32 classID, String className, ILogBuilder logger)
        //{
        //    if (!this.ContainsKey(classID))
        //    {
        //        DocumentSetClass newClass = new DocumentSetClass();
        //        newClass.classID = classID;
        //        newClass.className = className;
        //        newClass.wlfTable = wlfTable;

        //        this.Add(classID, newClass);
        //        wlfTable.ReadOnlyMode = true;
        //        wlfTable.prepareForUse(logger);

        //        logger.log("Class [" + classID + "] " + wlfTable.name);
        //        newClass.parent = this;
        //        return newClass;
        //    } else
        //    {
        //        return this[classID];
        //    }



        //}
    }

}