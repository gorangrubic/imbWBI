// --------------------------------------------------------------------------------------------------------------------
// <copyright file="industryTermModelProject.cs" company="imbVeles" >
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
// Project: imbWBI.IndustryTermModel
// Author: Goran Grubic
// ------------------------------------------------------------------------------------------------------------------
// Project web site: http://blog.veles.rs
// GitHub: http://github.com/gorangrubic
// Mendeley profile: http://www.mendeley.com/profiles/goran-grubi2/
// ORCID ID: http://orcid.org/0000-0003-2673-9471
// Email: hardy@veles.rs
// </summary>
// ------------------------------------------------------------------------------------------------------------------
using imbSCI.Core.files.fileDataStructure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Xml.Serialization;


// using imbMiningContext.TFModels.WLF_ISF;
using imbMiningContext.TFModels.ILRT;
using imbMiningContext.MCRepository;
using imbNLP.Data;
using imbWEM.Core.crawler.engine;
using imbNLP.PartOfSpeech.pipeline.models;
using imbSCI.Core.files.folders;
using imbMiningContext;
using System.IO;
using imbACE.Core;
using imbWBI.Core.WebClassifier.pipelineMCRepo;
using imbWBI.Core.WebClassifier.wlfClassifier;
using imbSCI.Core.reporting;
using imbACE.Core.core;
using imbWBI.Core.WebClassifier.validation;
using imbWBI.Core.WebClassifier.cases;
using imbSCI.Data.data.sample;
using imbWBI.Core.WebClassifier.experiment;
using imbWBI.Core.WebClassifier.core;
using imbNLP.PartOfSpeech.decomposing.chunk;
using imbNLP.PartOfSpeech.TFModels.webLemma;
using imbNLP.PartOfSpeech.TFModels.semanticCloud;
using imbSCI.Core.extensions.data;
using imbSCI.Data;
using imbWBI.Core.WebClassifier.pipelineMCRepo.model;
using imbSCI.Graph.Converters;

namespace imbWBI.IndustryTermModel.industry
{


    /// <summary>
    /// Project class for industryTermModel
    /// </summary>
    /// <seealso cref="imbSCI.Core.files.fileDataStructure.fileDataStructure" />
    [fileStructure(nameof(name), fileStructureMode.subdirectory,
    fileDataFilenameMode.propertyValue, fileDataPropertyOptions.textDescription)]
    public class industryTermModelProject : fileDataStructure, IFileDataStructure
    {
        [fileData(fileDataFilenameMode.memberInfoName, fileDataPropertyMode.XML, fileDataPropertyOptions.none)]
        [XmlIgnore]
        [Description("Primary chunk composer")]
        public chunkComposerBasic chunkComposer { get; set; } = new chunkComposerBasic();

        [fileData(fileDataFilenameMode.memberInfoName, fileDataPropertyMode.XML, fileDataPropertyOptions.none)]
        [XmlIgnore]
        [Description("Primary TF-IDF constructor dealing with diagnostic reporting and/or complete sample")]
        public wlfConstructorTFIDF masterTFIDF { get; set; } = new wlfConstructorTFIDF();

        //[fileData(fileDataFilenameMode.memberInfoName, fileDataPropertyMode.XML, fileDataPropertyOptions.none)]
        //[XmlIgnore]
        //[Description("Primary TF-IDF constructor dealing with diagnostic reporting and/or complete sample")]
        //public chunkConstructorTF masterChunkTFIDF { get; set; } = new chunkConstructorTF();

        //[fileData(fileDataFilenameMode.memberInfoName, fileDataPropertyMode.XML, fileDataPropertyOptions.none)]
        //[XmlIgnore]
        //[Description("Primary TF-IDF constructor dealing with diagnostic reporting and/or complete sample")]
        //public cloudConstructor masterCloudConstructor { get; set; } = new cloudConstructor();




        [fileData(fileDataFilenameMode.memberInfoName, fileDataPropertyMode.XML, fileDataPropertyOptions.none)]
        [XmlIgnore]
        [Description("Primary extractor")]
        public semanticFVExtractor masterExtractor { get; set; } = new semanticFVExtractor();


        [XmlIgnore]
        public experimentCompositeTemplate compositeTemplate {get; set; }


        [XmlIgnore]
        public DocumentSetClasses industries { get; set; } = new DocumentSetClasses();


        [XmlIgnore]
        public kFoldValidationCollection validationCases { get; set; } = new kFoldValidationCollection();


        /// <summary>
        /// Initializes a new instance of the <see cref="industryTermModelProject"/> class.
        /// </summary>
        public industryTermModelProject()
        {
            chunkComposer.settings.setDefaults();
            repoFolderRoot = imbMCManager.GetMCRepoNode(); //appManager.Application.folder.createDirectory(imbMCManager.MCRepo_DefaultDirectoryName, "Repository directory", false);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="industryTermModelProject"/> class.
        /// </summary>
        /// <param name="__name">The name.</param>
        /// <param name="__description">The description.</param>
        /// <param name="__parentNode">The parent node.</param>
        public industryTermModelProject(String __name, String __description, folderNode __parentNode)
        {
            //MCManager = new imbMCManager();
            repoFolderRoot = imbMCManager.GetMCRepoNode(); // appManager.Application.folder.createDirectory(imbMCManager.MCRepo_DefaultDirectoryName, "Repository directory", false);

            name = __name;
            description = __description;


            var parentFolder = __parentNode;
            folder = parentFolder.createDirectory(name, description, false);

            OnLoaded();
        }

        [XmlIgnore]
        public folderNode repoFolderRoot { get; set; }


        /// <summary> Name of the model project </summary>
        [Category("Label")]
        [DisplayName("name")] //[imb(imbAttributeName.measure_letter, "")]
        [Description("Name of the model project")] // [imb(imbAttributeName.reporting_escapeoff)]
        public String name { get; set; } = "ITM01";

        /// <summary>
        /// Configuration of the MCRepository crawl buildup
        /// </summary>
        /// <value>
        /// The crawl setup.
        /// </value>
        [fileData(fileDataFilenameMode.memberInfoName, fileDataPropertyMode.XML, fileDataPropertyOptions.none)]
        [XmlIgnore]
        [Description("Configuration of the MCRepository build-up crawl")]
        public industryTermModelCrawlSetup crawlSetup { get; set; } = new industryTermModelCrawlSetup();

        [fileData(fileDataFilenameMode.memberInfoName, fileDataPropertyMode.XML, fileDataPropertyOptions.itemAsFile)]
        [XmlIgnore]
        [Description("List of experiment definitions for this project")]
        public List<experimentSetup> experiment { get; set; } = new List<experimentSetup>();


        [fileData(fileDataFilenameMode.memberInfoName, fileDataPropertyMode.XML, fileDataPropertyOptions.none)]
        [XmlIgnore]
        [Description("NLP MCRepository processing setup")]
        public mcRepoProcessModelSetup nlpRepoProcessSetup { get; set; } = new mcRepoProcessModelSetup();

        [fileData(fileDataFilenameMode.memberInfoName, fileDataPropertyMode.XML, fileDataPropertyOptions.none)]
        [XmlIgnore]
        [Description("K-Fold validation")]
        public kFoldValidationSetup validationSetup { get; set; } = new kFoldValidationSetup();

        public operationsSetup operationSettings { get; set; } = new operationsSetup();


      //  public itmModelEnum modelToUse { get; set; } = itmModelEnum.chunkTFIDF;

        public List<String> DescribeSelf()
        {
            List<String> output = new List<string>();

            output.Add("# Industry Model Project [" + name + "]");
            if (!description.isNullOrEmpty()) {
                output.Add(" >  " + description);
            }

            output.AddRange(crawlSetup.DescribeSelf());

            output.Add("## Page Language Filter settings");
            String lang = "";
            nlpRepoProcessSetup.pageFilterSettings.testLanguages.ForEach(x => lang.add(x.ToString(), ","));
            output.Add(" > Dictionaries used for detection: " + lang);
            output.Add(" > Limit of valid pages per site: " + nlpRepoProcessSetup.target_languagePagePerSite);
            

            return output;
        }

        public experimentSetup GetOrMakeExperiment(String exp_name, Boolean useDefaultPreset)
        {
            experimentSetup exp = experiment.FirstOrDefault(x => x.name == exp_name);
            if (exp == null)
            {
                if (useDefaultPreset)
                {
                    exp = experimentSetup.GetDefaultExperimentSetup();
                    logger.log("New experiment from default preset created [" + exp_name + "] for project [" + name + "]");
                } else
                {
                    exp = new experimentSetup();
                    logger.log("New experiment, with basic setup, created [" + exp_name + "] for project [" + name + "]");
                }
                exp.name = exp_name;
                experiment.Add(exp);
                

            }
            return exp;
        }


        public void DefineDefaultIndustries()
        {
            industries.Clear();
            industries.Add(new industryClassModel("heating", "heat", "Manufacturing industry producing stoves and heating systems"));
            industries.Add(new industryClassModel("cooling", "vac", "Manufacturing industry producing ventilation and climatization equipment and POS climatized equipment"));
            industries.Add(new industryClassModel("energetics", "ener", "Manufacturing industry producing high voltage systems and other electrical power distribution equipment"));
            industries.Add(new industryClassModel("constructions", "const", "Metal construction industry"));
            industries.Add(new industryClassModel("outsiders", "out", "Web sites of irrelevant businesses and media portals"));


        }

        
       

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string description { get; set; }

        [XmlIgnore]
        public builderForLog logger { get; set; } 

        /// <summary>
        /// Called when object is loaded
        /// </summary>
        public override void OnLoaded()
        {
            logger = new builderForLog();
            aceLog.consoleControl.setAsOutput(logger,  name);

            industries.OnLoad<industryClassModel>(folder, logger);

            experiment.ForEach(x => x.deploy());


            compositeTemplate = new experimentCompositeTemplate(folder);

            
            //validationCases.OnLoad(folder, logger);

            //if (!industries.GetClasses().Any())
            //{
            //    DefineDefaultIndustries();
            //}

        }

        /// <summary>
        /// Called when before saving the data structure
        /// </summary>
        public override void OnBeforeSave()
        {
            experiment.ForEach(x => x.deploy());
            industries.OnBeforeSave<industryClassModel>(logger);

            
            var graph = folder.GetDirectedGraph(true, true, true, 100);
            graph.Save(folder.pathFor("projectGraph.dgml", imbSCI.Data.enums.getWritableFileMode.overwrite, "Directed Graph of the project [" + name + "] in Microsoft DGML format"));

            folder.generateReadmeFiles(appManager.AppInfo);


            //validationCases.OnBeforeSave(logger);
        }
    }
}
