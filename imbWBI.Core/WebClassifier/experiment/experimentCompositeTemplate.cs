// --------------------------------------------------------------------------------------------------------------------
// <copyright file="experimentCompositeTemplate.cs" company="imbVeles" >
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
using imbSCI.Core.extensions.data;
using imbSCI.Core.extensions.io;
using imbSCI.Data;
using imbSCI.Data.collection.nested;
using System.Xml.Serialization;
using imbSCI.Core.files.fileDataStructure;
using imbNLP.PartOfSpeech.flags.basic;
using System.ComponentModel;
using imbSCI.Core.extensions.text;
using imbSCI.Core.files;
using imbACE.Core;

namespace imbWBI.Core.WebClassifier.experiment
{

    /// <summary>
    /// Experiment setup (<see cref="experimentSetup"/>
    /// </summary>
    public class experimentCompositeTemplate
    {
        public folderNode folder { get; set; }

        public folderNode folderForClassifierSets { get; set; }

        public folderNode folderForFeatureVectorExtractors { get; set; }

        public folderNode folderForExperimentShells { get; set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="experimentCompositeTemplate"/> class.
        /// </summary>
        /// <param name="parentFolder">The parent folder.</param>
        public experimentCompositeTemplate(folderNode parentFolder)
        {

            folder = parentFolder.Add("CompositeTemplates", "Composite Experiment Templates", "Directory with template objects for separate aspects of the experiment");

            folderForClassifierSets = folder.Add("Classifiers", "Classifier sets", "Directory with serialized templates on classifier sets");

            folderForFeatureVectorExtractors = folder.Add("FeatureVectorExtractor", "Classifier sets", "Directory with serialized templates of Feature Vector Extractors");

            folderForExperimentShells = folder.Add("Shells", "Experiment Shells", "Directory with shells - experiment settings outside classifiers and FVEs");

            checkDefaults();

            folder.generateReadmeFiles(appManager.AppInfo);

        }

        public const String NAMEFORDEFAULT = "default.xml";


        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public void checkDefaults()
        {
            experimentSetup setup = experimentSetup.GetDefaultExperimentSetup();

            

            

            if (folderForClassifierSets.findFile(NAMEFORDEFAULT).isNullOrEmpty())
            {
                setup = experimentSetup.GetDefaultExperimentSetup();
                setup.name = "default";
                setup.featureVectorExtractors_semantic.Clear();
                
                objectSerialization.saveObjectToXML(setup, folderForClassifierSets.pathFor(NAMEFORDEFAULT, imbSCI.Data.enums.getWritableFileMode.overwrite, "Default set of classifiers"));
            }

            if (folderForFeatureVectorExtractors.findFile(NAMEFORDEFAULT).isNullOrEmpty())
            {
                setup = experimentSetup.GetDefaultExperimentSetup();
                setup.name = "default";
                setup.classifiers_settings.Clear();
                objectSerialization.saveObjectToXML(setup.featureVectorExtractors_semantic.First(), folderForFeatureVectorExtractors.pathFor(NAMEFORDEFAULT, imbSCI.Data.enums.getWritableFileMode.overwrite, "Default FVE"));
            }

            setup = experimentSetup.GetDefaultExperimentSetup();

            setup.classifiers.Clear();
            setup.classifiers_settings.Clear();
            setup.featureVectorExtractors_semantic.Clear();


            if (folderForExperimentShells.findFile(NAMEFORDEFAULT).isNullOrEmpty())
            {
                objectSerialization.saveObjectToXML(setup, folderForExperimentShells.pathFor(NAMEFORDEFAULT, imbSCI.Data.enums.getWritableFileMode.overwrite, "Default setup schell"));
            }
        }



        /// <summary>
        /// Composes the experiment.
        /// </summary>
        /// <param name="fvePresetName">Name of the fve preset.</param>
        /// <param name="experimentShellName">Name of the experiment shell.</param>
        /// <param name="classifierSetName">Name of the classifier set.</param>
        /// <param name="logger">The logger.</param>
        /// <returns></returns>
        public experimentSetup composeExperiment(String fvePresetName, String experimentShellName, String classifierSetName, ILogBuilder logger)
        {
            var _fvePresetName = fvePresetName.ensureEndsWith(".xml");
            var _experimentShellName = experimentShellName.ensureEndsWith(".xml");
            var _classifierSetName = classifierSetName.ensureEndsWith(".xml");

            var fve = objectSerialization.loadObjectFromXML<semanticFVExtractor>(folderForFeatureVectorExtractors.pathFor(_fvePresetName), logger);

            var setup = objectSerialization.loadObjectFromXML<experimentSetup>(folderForExperimentShells.pathFor(_experimentShellName), logger);
            
            var classifiers = objectSerialization.loadObjectFromXML<experimentSetup>(folderForClassifierSets.pathFor(_classifierSetName), logger);

            //setup.classifiers.Clear();
            setup.classifiers_settings.Clear();

            setup.derivedFrom = setup.name;
            setup.name = experimentShellName + "_" + classifierSetName + "_" + fvePresetName;

            setup.featureVectorExtractors_semantic.Clear();

            setup.classifiers_settings.AddRange(classifiers.classifiers_settings);

            setup.setClassifiers();

            setup.featureVectorExtractors_semantic.Add(fve);

            return setup;
        }


        
    }

}