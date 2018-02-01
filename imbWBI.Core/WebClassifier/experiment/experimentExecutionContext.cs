// --------------------------------------------------------------------------------------------------------------------
// <copyright file="experimentExecutionContext.cs" company="imbVeles" >
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
using imbACE.Core.core;
using imbMiningContext.TFModels.ILRT;
using imbSCI.Core.files.folders;
using imbSCI.Core.reporting;
using imbWBI.Core.WebClassifier.cases;
using imbWBI.Core.WebClassifier.core;
using imbWBI.Core.WebClassifier.validation;
using imbWBI.Core.WebClassifier.wlfClassifier;
using imbWEM.Core.project;
using System.Text;
using imbSCI.Data;
using System.Data;
using imbSCI.Core.extensions.table;
using imbNLP.PartOfSpeech.decomposing.chunk;
using imbNLP.PartOfSpeech.TFModels.webLemma;
using imbWBI.Core.WebClassifier.reportData;

namespace imbWBI.Core.WebClassifier.experiment
{

    /// <summary>
    /// Experiment execution context, holding all relevant information on the running experiment
    /// </summary>
    public class experimentExecutionContext
    {
        public experimentExecutionContext()
        {

        }


        public void AddExperimentInfo(DataTable output)
        {
            output.SetAdditionalInfoEntry("Experiment", setup.name);
            output.AddExtra("Experiment: " + setup.name);
            output.AddExtra("Description: " + setup.description);
        }

        public experimentNotes errorNotes { get; set; } 

        public folderNode errorNotesFolder { get; set; }


        /// <summary>
        /// Sets the execution context.
        /// </summary>
        /// <param name="_manager">The manager.</param>
        /// <param name="_setup">The setup.</param>
        /// <param name="_tools">The tools.</param>
        /// <param name="_classes">The classes.</param>
        /// <param name="sufix">The sufix.</param>
        /// <param name="chunker">The chunker.</param>
        /// <param name="_masterExtractor">The master extractor.</param>
        /// <param name="_logger">The logger.</param>
        public void SetExecutionContext(experimentManager _manager, experimentSetup _setup, classifierTools _tools, DocumentSetClasses _classes, String sufix, chunkComposerBasic chunker, semanticFVExtractor _masterExtractor, ILogBuilder _logger=null)
        {
            if (_logger == null)
            {
                _logger = new builderForLog();
                aceLog.consoleControl.setAsOutput(_logger, _setup.name);
            }
            logger = _logger;
            chunkComposer = chunker;
            setup = _setup;
            tools = _tools;
            tools.context = this;
            classes = _classes;
           // masterConstructor = _masterExtractor.termTableConstructor;

           

            masterExtractor = _setup.featureVectorExtractors_semantic.First();
            masterConstructor = masterExtractor.termTableConstructor;
            manager = _manager;
            String expContextName = "exp_" + setup.name.add(sufix, "_");

            folder = manager.folder.Add(expContextName, "Experiment " + setup.name, "Directory with all information on the experiment [" + setup.name + "]");
            errorNotesFolder = folder.Add("errors", "Error logs", "Directory with error reports produced if an exception occours. Normally, if everything was ok this folder should have only two files inside: directory_readme.txt and empty: note.txt).");
            errorNotes = new experimentNotes(errorNotesFolder, "Notes (logs) about critical and non-critical errors that happen during experiment execution. If everything was ok - this file should remain empty");

            notes = new experimentNotes(folder, "Notes on experiment setup and execution log");
            aceLog.consoleControl.setAsOutput(notes, "Notes");

            notes.log("Experiment [" + expContextName + "] initiated");
            notes.AppendLine("About: " + setup.description);

            notes.AppendHorizontalLine();

            

            notes.SaveNote();
            notes.AppendHeading("Feature extraction models");

            var lnsc = chunkComposer.DescribeSelf();
            lnsc.ForEach(x => notes.AppendLine(x));
            notes.AppendLine(" - ");


            List<String> mdn = new List<string>();
            foreach (var md in setup.models)
            {
                if (mdn.Contains(md.name))
                {
                    md.name += "_" + mdn.Count.ToString();
                } else
                {
                    mdn.Add(md.name);
                }


            }

            foreach (var md in setup.models)
            {
                String prefix = md.name;
                md.classes = classes;
                md.BuildFeatureVectorDefinition();

                var lns = md.DescribeSelf();
                lns.ForEach(x => notes.AppendLine(x));

               
                
                kFoldValidationCollection validationCases = classes.BuildValidationCases(prefix, setup.validationSetup.k, tools.DoDebug, logger, folder, setup.validationSetup.randomize);
                validationCases.pipelineCollection = pipelineCollection;

                validationCases.connectContext(this, md);
                
                validationCollections.Add(md.name, validationCases);

                
                //md.postClassifiers = setup.classifiers;

            }

           
        }

        public experimentReport mainReport { get; set; }

        public DocumentSetPipelineCollection pipelineCollection { get; set; } = new DocumentSetPipelineCollection();

        public ILogBuilder logger { get; set; } = null;

        public experimentManager manager { get; set; }

        public experimentSetup setup { get; set; } = null;

        public experimentNotes notes { get; set; }

        public folderNode folder { get; set; }

        public chunkComposerBasic chunkComposer { get; set; }

        public semanticFVExtractor masterExtractor { get; set; }

        public wlfConstructorTFIDF masterConstructor { get; set; }

        public classifierTools tools { get; set; }

        public DocumentSetClasses classes { get; set; }

       

        public Dictionary<String, kFoldValidationCollection> validationCollections { get; set; } = new Dictionary<String, kFoldValidationCollection>();
    }

}