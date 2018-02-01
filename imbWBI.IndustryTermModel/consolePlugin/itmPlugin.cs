// --------------------------------------------------------------------------------------------------------------------
// <copyright file="itmPlugin.cs" company="imbVeles" >
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace imbWBI.IndustryTermModel.consolePlugin
{

    using System;
    using System.ComponentModel.DataAnnotations;
    using System.IO;
    using imbACE.Core;
    using imbACE.Core.application;
    using imbACE.Core.plugins;
    using imbACE.Core.operations;
    using imbACE.Services.application;
    using imbACE.Services.console;
    using imbACE.Services.consolePlugins;
    using imbSCI.Core.extensions.data;
    using imbSCI.Core.extensions.enumworks;
    using imbSCI.Data;
    using imbSCI.Core.files.search;
    using imbWBI.IndustryTermModel.industry;
    using System.ComponentModel;
    using imbSCI.Core.files.folders;
    using imbSCI.Core.files.fileDataStructure;
    using imbMiningContext.TFModels.ILRT;
    using imbACE.Core.core;
    using imbACE.Services.terminal.dialogs.core;
    using imbACE.Services.terminal.dialogs;
    using imbACE.Services;
    using imbACE.Services.textBlocks.input;
    using imbACE.Services.textBlocks.smart;
    using imbSCI.Core.extensions.text;
    using imbNLP.PartOfSpeech.pipeline.models;
    using imbMiningContext.MCRepository;
    using imbWEM.Core.consolePlugin;
    using imbMiningContext;
    using imbSCI.Core.extensions.io;
    using System.Data;
    using imbSCI.Core.reporting.template;
    using imbNLP.PartOfSpeech.nlpTools;
    using imbNLP.PartOfSpeech.pipeline.machine;
    using imbNLP.PartOfSpeech.pipelineForPos.subject;
    using imbNLP.PartOfSpeech.pipelineForPos.render;
    using imbSCI.Data.enums;
    using imbSCI.Data.collection;

    using imbSCI.DataComplex.tables;
    using imbSCI.DataComplex.tables.extensions;
    using imbSCI.DataComplex.extensions.data.formats;
    using imbSCI.DataComplex.tf_idf;
    using imbNLP.PartOfSpeech.flags.token;
    using imbWBI.Core.WebClassifier.pipelineMCRepo;
    using imbNLP.Data;
    using imbNLP.PartOfSpeech.resourceProviders.multitext;
    using imbNLP.PartOfSpeech.lexicUnit;
    using imbNLP.PartOfSpeech.resourceProviders.core;
    using imbSCI.Core.math;
    using imbNLP.PartOfSpeech.TFModels.webLemma;
    // using imbMiningContext.TFModels.WLF_ISF;
    using imbNLP.PartOfSpeech.TFModels.industryLemma;
    using imbWBI.Core.WebClassifier.wlfClassifier;
    using imbNLP.PartOfSpeech.pipeline.core;
    using imbSCI.Core.extensions.table;
    using imbWBI.Core.WebClassifier.core;
    using imbNLP.PartOfSpeech.decomposing.chunk;


    using imbNLP.PartOfSpeech.pipelineForPos.subject;
    using imbNLP.PartOfSpeech.TFModels.webLemma.core;
    using imbWBI.Core.WebClassifier.cases;
    using imbWBI.Core.WebClassifier.validation;
    using imbNLP.PartOfSpeech.flags.basic;
    using imbWBI.Core.WebClassifier.experiment;
    using imbWBI.Core.WebClassifier.pipelineMCRepo.model;
    using imbSCI.Core.files;
    using imbWBI.Core.WebClassifier.reportData;




    /// <summary>
    /// Plugin for imbACE console - itmPlugin
    /// </summary>
    /// <seealso cref="imbACE.Services.consolePlugins.aceConsolePluginBase" />
    public class itmPlugin : aceConsolePluginBase
    {
        /// <summary>
        /// Gets or sets the folder.
        /// </summary>
        /// <value>
        /// The folder.
        /// </value>
        protected folderNode folder { get; set; }

        /// <summary>
        /// Gets or sets the project.
        /// </summary>
        /// <value>
        /// The project.
        /// </value>
        public industryTermModelProject project { get; set; }


        /// <summary>
        /// Name of the crawlJobPlugin at parent
        /// </summary>
        /// <value>
        /// The plugin name wem.
        /// </value>
        protected String pluginName_WEM { get; set; } = "";


        /// <summary>
        /// Name of the MCManager plugin at parent
        /// </summary>
        /// <value>
        /// The plugin name MCM.
        /// </value>
        protected String pluginName_MCM { get; set; } = "";


        /// <summary>
        /// Initializes a new instance of the <see cref="itmPlugin"/> class.
        /// </summary>
        /// <param name="__parent">The parent.</param>
        public itmPlugin(IAceAdvancedConsole __parent) : base(__parent, "itmPlugin", "This is imbACE advanced console plugin for itmPlugin")
        {

            folder = appManager.Application.folder_projects.createDirectory(nameof(itmPlugin), "Projects of Industry Term Model console plugin", false);
            _output = new builderForLog();

            imbSCI.Core.screenOutputControl.logToConsoleControl.setAsOutput(_output, "itm");

            manager = new experimentManager(appManager.Application.folder_reports);
           

        }


        protected void prepareForScripting()
        {
            var props = parent.GetType().GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly);
            foreach (var pi in props)
            {
                if (pi.PropertyType == typeof(crawlJobPlugin))
                {
                    pluginName_WEM = pi.Name;
                }

                if (pi.PropertyType == typeof(imbMCManager))
                {
                    pluginName_MCM = pi.Name;
                }
            }
        }



        [Display(GroupName = "run", Name = "Open", ShortName = "", Description = "Sets the current Industry Term Model projects")]
        [aceMenuItem(aceMenuItemAttributeRole.ExpandedHelp, "It will try to load the project or to create new under specified name")]
        /// <summary>Sets the current Industry Term Model projects</summary> 
        /// <remarks><para>It will try to load the project or to create new under specified name</para></remarks>
        /// <param name="name">Name of project to load</param>
        /// <param name="preset">if true it will create project from preset if project doesn't exist already</param>
        /// <seealso cref="aceOperationSetExecutorBase"/>
        public void aceOperation_runOpen(
            [Description("Name of project to load")] String name = "itm01",
            [Description("if true it will create project from preset if project doesn't exist already")] Boolean preset = true)
        {
            output.log("Project [" + name + "] initializing");

            if (Directory.Exists(folder.pathFor(name)))
            {
                project = fileDataStructureExtensions.LoadDataStructure<industryTermModelProject>(name, folder, output);
            } else
            {
                project = new industryTermModelProject(name, "Industry term model project", folder);
                

                project.SaveDataStructure(folder, output);

            }

            output.log("Project [" + project.name + "] ready");
        }


        public experimentSetup experiment { get; set; } = null;

        public experimentManager manager { get; set; }

        [Display(GroupName = "run", Name = "GetExperiment", ShortName = "", Description = "Provides new experiment or loads existing")]
        [aceMenuItem(aceMenuItemAttributeRole.ExpandedHelp, "It will load or create new experiment and set is as the active one, ready for execution")]
        /// <summary>Provides new experiment or loads existing</summary> 
        /// <remarks><para>It will load or create new experiment and set is as the active one, ready for execution</para></remarks>
        /// <param name="name">name of the experiment</param>
        /// <param name="makeDefault">if true it will create experiment setup with default settings (if experiment not found)</param>
        /// <param name="debug">if true it will provide more verbose output</param>
        /// <seealso cref="aceOperationSetExecutorBase"/>
        public void aceOperation_runGetExperiment(
            [Description("name of the experiment")] String name = "Exp01",
            [Description("if true it will create experiment setup with default settings (if experiment not found)")] Boolean makeDefault = true,
            [Description("if true it will provide more verbose output")] Boolean debug = true)
        {

            
            if (project == null)
            {
                output.log("No _project_ loaded!! Call _Open_ command first, to load project.");
                return;
            }

            experiment = project.GetOrMakeExperiment(name, makeDefault);
            experimentContext = new experimentExecutionContext();

            project.SaveDataStructure(folder, output);
        }


        public experimentExecutionContext experimentContext { get; set; }






        [Display(GroupName = "run", Name = "ExperimentRange", ShortName = "", Description = "What is purpose of this?")]
        [aceMenuItem(aceMenuItemAttributeRole.ExpandedHelp, "What it will do?")]
        /// <summary>What is purpose of this?</summary> 
        /// <remarks><para>What it will do?</para></remarks>
        /// <param name="word">--</param>
        /// <param name="steps">--</param>
        /// <param name="debug">--</param>
        /// <seealso cref="aceOperationSetExecutorBase"/>
        public void aceOperation_runExperimentRange(
            [Description("Term Weight Flag for HTML Tags")] String TW = "std",
            [Description("Term Category")] String TC = "std",
            [Description("Redux option")] String RX = "div",
            [Description("DFC values")] String DFC = "1.5,2.0,3.5,5.0,7.5,10",
            [Description("LPF values")] String LPF = "2",
            [Description("IDF turn on and off")] Boolean IDFOn = true,
            [Description("FVE name")] String fve = "CSSRM",
            [Description("Shell name")] String shell = "k1",
            [Description("Stx start")] Int32 stxStart = 2,
            [Description("Stx end")] Int32 stxEnd = 6,
            [Description("Strict POS Policy")] Boolean StrictPOS = false,
            [Description("Should generate general report on classes")] Boolean Report = false,
            [Description("Overrides sample randomization, 0:no change, 1:randomize true, -1:randomize false")] Int32 rnd = 0,
            [Description("Experiment name")] String exName = "*",
            [Description("Name of classifier compound")] String classifier = "default"
            )
        {

            StringBuilder sb = new StringBuilder();

            


            List<Double> DFCs = new List<double>();

            foreach (String dfc in DFC.SplitSmart(","))
            {
                DFCs.Add(Convert.ToDouble(dfc));
            }


            List<Int32> LPFs = new List<Int32>();


            foreach (String dfc in LPF.SplitSmart(","))
            {
                LPFs.Add(Convert.ToInt32(dfc));
            }

            String DFCSpan = DFCs.Min().ToString("F2") + "-" + DFCs.Max().ToString("F2");
            String LPFSpan = LPFs.Min().ToString() + "-" + LPFs.Max().ToString();


            String matrixName = $"W{TW}C{TC}X{RX}_" + "IDF" + IDFOn.ToString() + DFCSpan + LPFSpan + "_SP" + StrictPOS.ToString();
            matrixName = matrixName.Replace(".", "_").getCleanFilePath();


            sb.AppendLine($"// --------------------------------------------------------------------- //");
            sb.AppendLine($"itm.Open \"itm01\";");


                foreach (Int32 lpf in LPFs)
                {

                    foreach (Double dfc in DFCs)
                    {
                        sb.AppendLine($"// --------------------------------------------------------------------- //");
                        sb.AppendLine($"itm.Compose shell=\"{shell}\";classifiers=\"{classifier}\";fve=\"{fve}\";");
                        sb.AppendLine($"itm.Modify TW=\"{TW}\";TC=\"{TC}\";RX=\"{RX}\";LPF={lpf};DFC=\"" + dfc.ToString("F2") + $"\";IDFOn={IDFOn};StrictPOS={StrictPOS};");
                        sb.AppendLine($"itm.CloneFVEOnSTX name = \"{exName}\"; start = {stxStart}; end = {stxEnd}; code = \"*\"; kFold = -1; rnd = -1;");
                        sb.AppendLine($"itm.Experiment classReporting=false;caseReporting=false;resultReporting=true;makeGeneralReport={Report};");
                    }
                }



            //sb.AppendLine("itm.Summary prefix = \"exp_SM02\";");

            sb.AppendLine($"// --------------------------------------------------------------------- //");

            sb.AppendLine($"itm.Save;");

            IAceAdvancedConsole console = parent as IAceAdvancedConsole;
            String scriptName = matrixName.add("ace", ".");

            console.workspace.saveScript(scriptName, sb.ToString().SplitSmart(Environment.NewLine));

           // sb.ToString().saveStringToFile(console.workspace.folder.pathFor(scriptName, getWritableFileMode.overwrite, "Automatically created matrix script [" + matrixName + "]"));

            console.executeCommand("exe "+scriptName);

        }







        [Display(GroupName = "run", Name = "ExperimentMatrix", ShortName = "", Description = "Automatically creates 3x3 matrix of experiments variating TW, TC and RX options")]
        [aceMenuItem(aceMenuItemAttributeRole.ExpandedHelp, "Creates experiment matrix using LPF, DFC, IDFOn specified")]
        /// <summary>
        /// Automatically creates 3x3 matrix of experiments variating TW, TC and RX options
        /// </summary>
        /// <param name="LPF">Low pass frequecy--</param>
        /// <param name="DFC">Document Frequency Correction</param>
        /// <param name="IDFOn">IDF on / off</param>
        /// <param name="fve">The fve.</param>
        /// <param name="shell">The shell.</param>
        /// <remarks>
        /// Creates experiment matrix using LPF, DFC, IDFOn specified
        /// </remarks>
        /// <seealso cref="aceOperationSetExecutorBase" />
        public void aceOperation_runExperimentMatrix(
            [Description("Low pass frequecy--")] Int32 LPF = 2,
            [Description("Document Frequency Correction")] Double DFC = 5,
            [Description("IDF on / off")] Boolean IDFOn = true,
            [Description("FVE name")] String fve = "CSSRM",
            [Description("Shell name")] String shell = "k1")
        {

            StringBuilder sb = new StringBuilder();

            List<String> TWFlags = new List<string>() { "std", "bst", "off" };
            List<String> TCFlags = new List<string>() { "std", "bst", "off" };
            List<String> RXFlags = new List<string>() { "div", "sqr", "off" };

            String matrixName = $"LPF{LPF.ToString()}DFC" + DFC.ToString("F2") + "IDFOn" + IDFOn.ToString();

            sb.AppendLine($"// --------------------------------------------------------------------- //");
            sb.AppendLine($"itm.Open \"itm01\";");

            foreach (String tw in TWFlags)
            {
                foreach (String tc in TCFlags)
                {

                    foreach (String rx in RXFlags)
                    {
                        sb.AppendLine($"// --------------------------------------------------------------------- //");
                        sb.AppendLine($"itm.Compose shell=\"{shell}\";classifiers=\"default\";fve=\"{fve}\";");
                        sb.AppendLine($"itm.Modify TW=\"{tw}\";TC=\"{tc}\";RX=\"{rx}\";LPF={LPF};DFC=\"" + DFC.ToString("F2") + $"\";IDFOn={IDFOn};");
                        sb.AppendLine($"itm.CloneFVEOnSTX name = \"*\"; start = 2; end = 7; code = \"*\"; kFold = -1; rnd = -1;");
                        sb.AppendLine($"itm.Experiment classReporting=false;caseReporting=false;resultReporting=true;makeGeneralReport=false;");
                    }
                }

            }

            //sb.AppendLine("itm.Summary prefix = \"exp_SM02\";");

            sb.AppendLine($"// --------------------------------------------------------------------- //");

            sb.AppendLine($"itm.Save;");
            matrixName = matrixName.Replace(".", "_");

            IAceAdvancedConsole console = parent as IAceAdvancedConsole;
            String scriptName = matrixName.add("ace", ".");
            console.workspace.saveScript(scriptName, sb.ToString().SplitSmart(Environment.NewLine));

           // sb.ToString().saveStringToFile(console.workspace.folder.pathFor(scriptName, getWritableFileMode.overwrite, "Automatically created matrix script [" + matrixName + "]"));

            console.executeCommand("exe " + scriptName);
        }





        [Display(GroupName = "run", Name = "Compose", ShortName = "", Description = "Utilize composite template system to create an experiment instance")]
        [aceMenuItem(aceMenuItemAttributeRole.ExpandedHelp, "What it load component specified and set the experiment as current")]
        /// <summary>Utilize composite template system to create an experiment instance</summary> 
        /// <remarks><para>What it load component specified and set the experiment as current</para></remarks>
        /// <param name="schell">Name of experiment schell definition from composite templates sub folder of the project</param>
        /// <param name="classifiers">Name of classifier set XML file from composite template sub folder of the project</param>
        /// <param name="fve">Name of the Feature Vector Extractor XML file from composite template sub folder of the project</param>
        /// <seealso cref="aceOperationSetExecutorBase"/>
        public void aceOperation_runCompose(
            [Description("Name of experiment schell definition from composite templates sub folder of the project")] String shell = "default",
            [Description("Name of classifier set XML file from composite template sub folder of the project")] String classifiers = "default",
            [Description("Name of the Feature Vector Extractor XML file from composite template sub folder of the project")] String fve = "default")
        {

            if (project == null)
            {
                output.log("No _project_ loaded!! Call _Open_ command first, to load project.");
                return;
            }


            experiment = project.compositeTemplate.composeExperiment(fve, shell, classifiers, output);
        }




        [Display(GroupName = "run", Name = "Modify", ShortName = "", Description = "Adjusting experiment settings")]
        [aceMenuItem(aceMenuItemAttributeRole.ExpandedHelp, "Modifies current experiment")]
        /// <summary>
        /// Adjusting experiment settings
        /// </summary>
        /// <param name="TW">Term Weight Flag for HTML Tags</param>
        /// <param name="TC">The tc.</param>
        /// <param name="RX">The rx.</param>
        /// <param name="LPF">The LPF.</param>
        /// <param name="DFC">DF Correction</param>
        /// <param name="IDFOn">IDF turn on and off</param>
        /// <param name="StrictPOS">if set to <c>true</c> [strict position].</param>
        /// <remarks>
        /// Modifies current experiment
        /// </remarks>
        /// <seealso cref="aceOperationSetExecutorBase" />
        public void aceOperation_runModify(
            [Description("Term Weight Flag for HTML Tags")] String TW = "std",
            [Description("Term Category")] String TC = "std",
            [Description("Redux option")] String RX = "div",
            [Description("Term Weight Flag for HTML Tags")] Int32 LPF= 2,
            [Description("DF Correction")] Double DFC = 1.1,
            [Description("IDF turn on and off")] Boolean IDFOn = true,
            [Description("Strict POS Policy")] Boolean StrictPOS = false
            )
        {
            if (project == null)
            {
                output.log("No _project_ loaded!! Call _Open_ command first, to load project.");
                return;
            }

            String experimentName = experiment.name;

            experiment.featureVectorExtractors_semantic.FirstOrDefault().SetTW(TW, DFC, IDFOn);

            experiment.featureVectorExtractors_semantic.FirstOrDefault().CloudConstructor.settings.SetTC(TC);

            experiment.featureVectorExtractors_semantic.FirstOrDefault().settings.semanticCloudFilter.SetRedux(RX, LPF);

            experiment.featureVectorExtractors_semantic.FirstOrDefault().termTableConstructor.settings.strictPosTypePolicy = StrictPOS;

            experimentName = "TW" + TW + "_TC" + TC + "_RX" + RX + "DFC" + DFC.ToString("F2").Replace(".", "_") + "LP" + LPF + "_IDF" + IDFOn;

            experiment.name = experimentName;
       

        }




        [Display(GroupName = "run", Name = "MakeTemplate", ShortName = "", Description = "Ensures consistant settings accross FVE models in the experiment")]
        [aceMenuItem(aceMenuItemAttributeRole.ExpandedHelp, "It will take first FVE model, and make clones with different Semantic Expansion setting - other FVEs will be removed.")]
        /// <summary>
        /// Aces the operation run clone fve on STX.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <param name="code">The code.</param>
        /// <param name="kFold">The k fold.</param>
        /// <param name="rnd">The random.</param>
        /// <param name="rename">The rename.</param>
        public void aceOperation_runCloneFVEOnSTX(
            [Description("Name of the experiment to process, or * to use currently selected")] String name = "*",
            [Description("Starting value for Stx")] Int32 start = 3,
            [Description("Ending value for Sts")] Int32 end = 8,
            [Description("3 or 4 letter code indicating how the settings are different then in other experiments,* means sufix will be created automatically according to other modifications")] String code = "",
            [Description("number of k-folds, when specified it overrides settings of the experiment")] Int32 kFold = -1,
            [Description("Overrides sample randomization, 0:no change, 1:randomize true, -1:randomize false")] Int32 rnd = 0,
            [Description("Renames the experiment, if * the name is set automatically")] String rename = ""
            )
        {
            if (project == null)
            {
                output.log("No _project_ loaded!! Call _Open_ command first, to load project.");
                return;
            }

            if (name != "*")  experiment = project.GetOrMakeExperiment(name, true);

            
            experimentContext = new experimentExecutionContext();
            semanticFVExtractor model = experiment.models.First() as semanticFVExtractor;

            Int32 initialSteps = model.settings.caseTermExpansionSteps;

            String needle = "<caseTermExpansionSteps>" + initialSteps + "</caseTermExpansionSteps>";

            if (kFold > -1)
            {
                output.log("Overriding K-fold crossvalidation settings in experiment from [" + experiment.validationSetup.k + "] to [" + kFold + "]");
                experiment.validationSetup.k = kFold;
                //imbACE.Services.terminal.aceTerminalInput.doBeepViaConsole(3200, 500, 1);
            }

            if (rnd < 0) experiment.validationSetup.randomize = false;
            if (rnd > 0) experiment.validationSetup.randomize = true;

            if (code == "*")
            {
                code = "";
                if (kFold > -1)
                {
                    code += "k" + kFold;
                }

                if (rnd < 0) code += "rnd_off";
                if (rnd > 0) code += "rnd_on";
            }
            experiment.derivedFrom = experiment.name;
            experiment.RemoveAllModelsExcept();

            if (rename != "")
            {
                experiment.name = rename;
            }
            if (code != "")
            {
                experiment.name = experiment.name.add(code, "_");
            }

            String serializedModel = objectSerialization.ObjectToXML(model);


            for (Int32 i = start; i < end; i++)
            {
                String newModelXML = serializedModel;
                newModelXML = newModelXML.Replace(needle, "<caseTermExpansionSteps>" + i + "</caseTermExpansionSteps>");
                semanticFVExtractor newModel = objectSerialization.ObjectFromXML<semanticFVExtractor>(newModelXML);
                newModel.name = newModel.GetShortName(code);

                newModel.CloudConstructor.settings.documentSetFreqLowLimit = 1;

                newModel.description = newModel.GetShortDescription();
                output.log("-- created model: " + newModel.name);
                experiment.featureVectorExtractors_semantic.Add(newModel);
                
            }

            experiment.deploy();

        }




        [Display(GroupName = "run", Name = "Summary", ShortName = "", Description = "Scans report folders to produce summary report on all experiments")]
        [aceMenuItem(aceMenuItemAttributeRole.ExpandedHelp, "Scans for experimentSetup.xml files and collects all FVE data into single table")]
        /// <summary>
        /// Scans report folders to produce summary report on all experiments
        /// </summary>
        /// <param name="subfolder">Subfolder within report directory</param>
        /// <param name="reportName">The prefix.</param>
        /// <remarks>
        /// Scans for experimentSetup.xml files and collects all FVE data into single table
        /// </remarks>
        /// <seealso cref="aceOperationSetExecutorBase" />
        public void aceOperation_runSummary(
             [Description("Prefix to insert in name")] String reportName = "Summary",
            [Description("Subfolder within report directory")] String subfolder = ""
            )
        {
            folderNode folderToReport = appManager.Application.folder_reports;

            if (!subfolder.isNullOrEmpty()) {
                DirectoryInfo di = appManager.Application.folder_reports;
                if (Directory.Exists(di.FullName + Path.DirectorySeparatorChar + subfolder))
                {
                    folderToReport = appManager.Application.folder_reports[subfolder];
                }
                else
                {
                    output.log("Directory : [" + di.FullName + Path.DirectorySeparatorChar + subfolder + "] not found!");
                }
                
            }

            secondaryReport.ScanSubdirectories(folderToReport, reportName, output);
            
        }





        [Display(GroupName = "run", Name = "Experiment", ShortName = "", Description = "Executes currently selected experiment")]
        [aceMenuItem(aceMenuItemAttributeRole.ExpandedHelp, "It will create experiment execution context and run")]
        /// <summary>
        /// Executes currently selected experiment
        /// </summary>
        /// <param name="classReporting">If true it will create additional report on each industry / category</param>
        /// <param name="caseReporting">If true it will create additional report for each processed case</param>
        /// <param name="resultReporting">if true it will create additional report for each validation case</param>
        /// <param name="makeGeneralReport">if set to <c>true</c> [make general report].</param>
        /// <param name="sufix">The sufix.</param>
        /// <remarks>
        /// It will create experiment execution context and run
        /// </remarks>
        /// <seealso cref="aceOperationSetExecutorBase" />
        public void aceOperation_runExperiment(
            [Description("If true it will create additional report on each industry / category")] Boolean classReporting = true,
            [Description("If true it will create additional report for each processed case")] Boolean caseReporting = true,
            [Description("if true it will create additional report for each validation case")] Boolean resultReporting = true,
            [Description("if true it will create General reports")] Boolean makeGeneralReport = true,
            [Description("Adds specified sufix to experiment report folder name")] String sufix = ""
            )
        {
            var startLength = output.Length;

            if (project == null)
            {
                output.log("No _project_ loaded!! Call _Open_ command first, to load project.");
                return;
            }
            if (experiment == null)
            {
                output.log("No _experiment_ selected!! Call _GetExperiment_ command first, to get experiment.");
                return;
            }
            
            var tools = GetTools();
            tools.DoReport = caseReporting;
            tools.DoClassReport = classReporting;
            tools.DoResultReporting = resultReporting;
            tools.operation.doCreateDiagnosticMatrixAtStart = makeGeneralReport;
            tools.operation.doFullDiagnosticReport = makeGeneralReport;
          

            experimentContext.SetExecutionContext(manager, experiment, tools, project.industries, sufix, project.chunkComposer, project.masterExtractor, output);
            //project.SaveDataStructure();

            manager.runExperiment(experimentContext);

            project.DescribeSelf().ForEach(x => experimentContext.notes.AppendLine(x));
            experimentContext.notes.SaveNote();

            //experimentContext.notes.

            String experiment_log = output.GetContent(startLength, output.Length);
            String log_path = experimentContext.folder.pathFor("log.txt", getWritableFileMode.overwrite, "Complete log output of this experiment session");
            File.WriteAllText(log_path, experiment_log);

            project.SaveDataStructure();
        }








        [Display(GroupName = "run", Name = "SingleFoldTest", ShortName = "", Description = "Trains the classifier and performs classification over complete industry classes, used for debug purposes")]
        [aceMenuItem(aceMenuItemAttributeRole.ExpandedHelp, "It will use complete sample of the class to train classifier, later it will run classification on the same class")]
        /// <summary>Trains the classifier and performs classification over complete industry classes, used for debug purposes</summary> 
        /// <remarks><para>It will use complete sample of the class to train classifier, later it will run classification on the same class</para></remarks>
        /// <param name="industry">Name of the industry class to perform the operation over</param>
        /// <param name="model">Model to use for training and classification</param>
        /// <param name="debug">if true it will create extensive debug information</param>
        /// <seealso cref="aceOperationSetExecutorBase"/>
        public void aceOperation_runSingleFoldTest(
            [Description("Test name prefix")] String prefix = "",
            [Description("Model to use for training and classification")] itmModelEnum model = itmModelEnum.chunkTFIDF,
            [Description("if true it will create extensive debug information")] Boolean report = false,
            [Description("if true it will create extensive debug information")] Boolean debug = true)
        {
            
            //if (prefix.isNullOrEmpty()) prefix = project.validationSetup.name;
            //prefix = prefix + model.ToString();
            //kFoldValidationCollection validationCases = project.industries.BuildValidationCases(prefix, 1, debug, output);
            
            
            //tools.classifier.DoTraining(validationCases[0], tools, output);

            //var results = tools.classifier.DoClassification(validationCases[0], tools, output);

            
            
            

        }








        [Display(GroupName = "run", Name = "Save", ShortName = "", Description = "Saves the currently selected Industry Term Model project")]
        [aceMenuItem(aceMenuItemAttributeRole.ExpandedHelp, "It will call for save of the currently selected Industry Term Model project")]
        /// <summary>Saves the currently selected Industry Term Model project</summary> 
        /// <remarks><para>It will call for save of the currently selected Industry Term Model project</para></remarks>
        /// <seealso cref="aceOperationSetExecutorBase"/>
        public void aceOperation_runSave()
        {
            if (project != null)
            {
                project.SaveDataStructure(folder, output);
            } else
            {
                output.log("Error: no current project set");
            }

            appManager.Application.folder.generateReadmeFiles(appManager.AppInfo);
            
            
        }


       


        private folderNode _cacheFolder;
        private folderNode _reportFolder;




        /// <summary>
        /// Gets the tools.
        /// </summary>
        /// <param name="process">The component.</param>
        /// <returns></returns>
        public classifierTools GetTools()
        {
            classifierTools tools = new classifierTools();

         //   tools.reportFolder = GetReportFolder(process);

           // tools.cacheFolder = GetCacheFolder(process, tools.reportFolder);

            tools.DoUseExistingKnowledge = project.operationSettings.doUseExistingKnowledge;
            tools.DoUseLexicResourceCache = project.operationSettings.DoUseCachedLemmaResource;
            tools.operation = project.operationSettings;
            
            tools.model = GetModelStageOne();
            
            //tools.classifier = process.GetClassifierInstance();
            //tools.classifier.classes = project.industries;

            tools.output = output;
            tools.setup_multitext_lex = project.nlpRepoProcessSetup.setup_multitext_lex;
            tools.setup_multitext_specs = project.nlpRepoProcessSetup.setup_multitext_specs;
            tools.setup_multitext_alt = project.nlpRepoProcessSetup.setup_multitext_alt;

            
            return tools;


        }




        

       
        private IPipelineModel modelStageOne = null;
        public IPipelineModel GetModelStageOne()
        {
            if (modelStageOne == null)
            {
                mcRepoProcessModel pModel = new mcRepoProcessModel(); // nlpTypeManager.main.modelTypeManager.GetInstance(pipeline, output); //

                pModel.logger = output;
                pModel.setup = project.nlpRepoProcessSetup;
                pModel.constructionProcess();

                modelStageOne = pModel;
            }
            return modelStageOne;
        }

        public cnt_level GetSubjectTFIDFLevel(itmModelEnum process)
        {
            switch (process)
            {
                case itmModelEnum.chunkTFIDF:
                    return cnt_level.mcChunk;
                    break;
            }
            return cnt_level.mcToken;
        }









        [Display(GroupName = "make", Name = "CrawlScript", ShortName = "", Description = "Creates script to perform (re)build MCRepository crawl")]
        [aceMenuItem(aceMenuItemAttributeRole.ExpandedHelp, "Creates script that prepares crawler, crawl job and performs the crawl")]
        /// <summary>
        /// Performs crawl to (re) build MCRepository
        /// </summary>
        /// <param name="name">What component of the model to crawl</param>
        /// <param name="clearRepo">If true it will clear existing MC Repo</param>
        /// <param name="debug">If true it will show some extra information during the process</param>
        /// <param name="autorun">If true it will autorun created script without asking user</param>
        /// <remarks>
        /// Prepares crawler, crawl job and runs the crawl
        /// </remarks>
        /// <seealso cref="aceOperationSetExecutorBase" />
        public void aceOperation_makeCrawlScript(
            [Description("What component of the model to crawl")] String name = "inIndustry",
            [Description("If true it will clear existing MC Repo")] Boolean clearRepo = false,
            [Description("If true it will show some extra information during the process")] Boolean debug = true,
            [Description("If true it will autorun created script without asking user")] Boolean autorun = true)
        {
            prepareForScripting();

            String scriptName = "";
            imbMCRepository targetRepo = null;
            List<String> sampleList = null;
            IAceAdvancedConsole console = parent as IAceAdvancedConsole;

            folderNode targetFolder = folder;
            if (console != null)
            {
                targetFolder = console.workspace.folder;
                if (debug) output.log("Using console's workspace folder: " + targetFolder.path);
            }

            
            IDocumentSetClass documentSetClass = project.industries.GetClass(name, output);

            scriptName = documentSetClass.name + "_crawl";
            
            targetRepo = documentSetClass.MCRepository;
            sampleList = documentSetClass.WebSiteSample; // project.SampleInBusiness;

            
            if (clearRepo)
            {
                if (debug) output.log("Clearing repository: " + targetRepo.name);
                targetRepo.DeleteAll(output);
                targetRepo.Save(output);
            }

            String scriptFileInfo = targetFolder.findFile(scriptName + ".ace", SearchOption.AllDirectories);
            if (File.Exists(scriptFileInfo))
            {
                if (debug) output.log("Deleting existing script file at: " + scriptFileInfo);
                File.Delete(scriptFileInfo);
            }

            scriptFileInfo = targetFolder.findFile(scriptName + ".txt", SearchOption.AllDirectories);
            if (File.Exists(scriptFileInfo))
            {
                if (debug) output.log("Deleting existing sample file at: " + scriptFileInfo);
                File.Delete(scriptFileInfo);
            }

            String sampleFilePath = targetFolder.pathFor(scriptName + ".txt", imbSCI.Data.enums.getWritableFileMode.existing);
            saveBase.saveContentOnFilePath(sampleList, sampleFilePath);
            if (debug) output.log("Sample list exported to: " + sampleFilePath);


            // <----------- preparing meta data
            PropertyCollection meta = new PropertyCollection();
            meta[nameof(itmScript.wem)] = pluginName_WEM;
            meta[nameof(itmScript.mcm)] = pluginName_MCM;
            meta[nameof(itmScript.date)] = DateTime.Now.ToShortDateString();
            meta[nameof(itmScript.sample_file)] = sampleFilePath;
            meta[nameof(itmScript.component)] = name;
            meta[nameof(itmScript.project_name)] = project.name;
            meta[nameof(itmScript.project_path)] = project.folder.path;
            meta[nameof(itmScript.debug)] = debug;
            meta[nameof(itmScript.crawler)] = project.crawlSetup.Crawler;

            meta[nameof(itmScript.LT_t)] = project.crawlSetup.crawlerSettings.limitIterationNewLinks;
            meta[nameof(itmScript.I_max)] = project.crawlSetup.crawlerSettings.limitIterations;
            meta[nameof(itmScript.PL_max)] = project.crawlSetup.crawlerSettings.limitTotalPageLoad;
            meta[nameof(itmScript.PS_c)] = project.crawlSetup.crawlerSettings.primaryPageSetSize;

            meta[nameof(itmScript.pLanguage)] = project.crawlSetup.PrimaryLanguage.ToString();
            meta[nameof(itmScript.sLanguage)] = project.crawlSetup.SecondaryLanguage.ToString();


            meta[nameof(itmScript.TC_max)] = project.crawlSetup.crawlerJobEngineSettings.TC_max;
            meta[nameof(itmScript.Tdl_max)] = project.crawlSetup.crawlerJobEngineSettings.Tdl_max;
            meta[nameof(itmScript.Tcjl_max)] = project.crawlSetup.crawlerJobEngineSettings.Tcjl_max;
            meta[nameof(itmScript.Tll_max)] = project.crawlSetup.crawlerJobEngineSettings.Tll_max;
            meta[nameof(itmScript.indexID)] = project.name;
            meta[nameof(itmScript.sessionID)] = project.name + "_" + name;
            meta[nameof(itmScript.repo_name)] = targetRepo.name;

            String outputFilePath = targetFolder.pathFor(scriptName + ".ace", getWritableFileMode.existing);

            String templatePath = appManager.Application.folder_resources.findFile("mc_repo_script.ace", SearchOption.AllDirectories);
            String templateCode = File.ReadAllText(templatePath);

            if (debug) output.log("Script template loaded: " + templatePath);

            stringTemplate template = new stringTemplate(templateCode);
            String scriptCode = template.applyToContent(meta);

            aceConsoleScript script = new aceConsoleScript(outputFilePath, false);
            script.AppendUnique(scriptCode.SplitSmart(Environment.NewLine, "", true, false));
            script.Save(output);

            if (debug) output.log("Script created with [" + script.Count() + "] lines");

            output.log("Script created at [" + outputFilePath + "]");

            dialogMenuItem action = dialogMenuItem.Run;

            if (!autorun)
            {
             action=dialogs.openDialogWithOptions<dialogMenuItem>(new dialogMenuItem[] { dialogMenuItem.Run, dialogMenuItem.Print, dialogMenuItem.Done }, "Script created", "Select action to perform", dialogStyle.greenDialog, dialogSize.mediumBox);
            }
            switch (action)
            {
                case dialogMenuItem.Run:
                    if (console != null)
                    {
                        console.executeScript(script, 1);
                    } else
                    {
                        output.log("Script execution failed: parent of this plugin is not IAceAdvancedConsole object");
                    }
                    break;
                case dialogMenuItem.Print:

                    output.AppendLine("Showing script content:");
                    script.getContentLines().ForEach(x => output.AppendLine(x));

                    break;
                case dialogMenuItem.Done:
                    output.log("Done");
                    
                    break;
            }

            output.AppendLine("To execute script later call: _exe_ \"scriptfilename.ace\";");
        }




        /// <summary>
        /// Imports the file from.
        /// </summary>
        /// <param name="propName">Name of the property.</param>
        /// <param name="propValue">The property value.</param>
        /// <param name="verbose">if set to <c>true</c> [verbose].</param>
        protected void importFileFrom(String propName, List<String> propValue, Boolean verbose)
        {
            propName = propName.imbTitleCamelOperation(false);
            dialogMenuItem dialogResponse = dialogs.openDialogWithOptions<dialogMenuItem>(new dialogMenuItem[] { dialogMenuItem.Yes, dialogMenuItem.No },
                   $"Do you want to import {propName} sample from file?", $"{propName} sample list has [" + propValue.Count + "] entires. It will open select file dialog to import list from.");

            String importPath = "";

            switch (dialogResponse)
            {
                case dialogMenuItem.Yes:
                    importPath = dialogs.openSelectFile(dialogSelectFileMode.selectFileToOpen, "*.txt", appManager.Application.folder_projects.path, $"Select txt file to {propName} list of domains from");
                    Int32 c = propValue.Count;
                    if (File.Exists(importPath))
                    {
                        var lns = File.ReadAllLines(importPath);
                        propValue.AddRange(lns);

                        c = c - propValue.Count;

                        if (verbose) output.log("Imported [" + c + "] items in " + propName);
                    }
                    break;
                case dialogMenuItem.No:
                    break;
            }
        }


        [Display(GroupName = "run", Name = "Setup", ShortName = "", Description = "Performs basic setup procedure, should be called after new project created")]
        [aceMenuItem(aceMenuItemAttributeRole.ExpandedHelp, "It will ask user to insert or confirm each property in configuration modules: crawlSetup, nlpRepoProcessSetup, etc...")]
        /// <summary>Performs basic setup procedure, should be called after new project created</summary> 
        /// <remarks><para>It will ask user to insert or confirm each property in configuration modules: crawlSetup, nlpRepoProcessSetup, etc...</para></remarks>
        /// <param name="verbose">Provides additional hints and explanations during the setup process</param>
        /// <seealso cref="aceOperationSetExecutorBase"/>
        public void aceOperation_runSetup(
            [Description("Provides additional hints and explanations during the setup process")] Boolean verbose = true)
        {

           // if (project == null)
           // {
           //     output.log("No project selected! Call _Open_ operation first");
           //     return;
           // }
           // if (verbose) output.log("Project [" + project.name + "] setup started");

           // if (!project.SampleInIndustry.Any()) importFileFrom(nameof(project.SampleInIndustry), project.SampleInIndustry, verbose);
           // if (!project.SampleInBusiness.Any()) importFileFrom(nameof(project.SampleInBusiness), project.SampleInBusiness,verbose);
           // if (!project.SampleIrrelevant.Any()) importFileFrom(nameof(project.SampleIrrelevant), project.SampleIrrelevant,verbose);

           // project.crawlSetup = dialogs.openEditProperties(project.crawlSetup, "Crawl Setup", "Settings defining MCRepository construction crawl") as industryTermModelCrawlSetup;

           // project.nlpRepoProcessSetup = dialogs.openEditProperties(project.nlpRepoProcessSetup, "NLP MCRepo processing", "Settings of MCRepository NLP processing") as mcRepoProcessModelSetup;


           //if (verbose) output.log("Setup finished");

           // project.SaveDataStructure(folder, output);
        }





        [Display(GroupName = "run", Name = "Diagnostic", ShortName = "", Description = "Evaluates current Industry Term Model project's state")]
        [aceMenuItem(aceMenuItemAttributeRole.ExpandedHelp, "It will check in which phase the current project is, and if it is ready for application")]
        /// <summary>Evaluates current Industry Term Model project's state</summary> 
        /// <remarks><para>It will check in which phase the current project is, and if it is ready for application</para></remarks>
        /// <param name="verbose">If true it will produce more in-detail report on the state of the current project</param>
        /// <seealso cref="aceOperationSetExecutorBase"/>
        public itmStateEnum aceOperation_runDiagnostic(
            [Description("If true it will produce more in-detail report on the state of the current project")] Boolean verbose = true)
        {
            IAceAdvancedConsole console = parent as IAceAdvancedConsole;
            console.cls();

            output.AppendHeading("Industry Term Model Project diagnostic", 1);


            if (project == null)
            {
                output.log("There is no currently selected project");
                output.log("Call _Open_ operation first");
                return itmStateEnum.notReady;
            }

            List<String> stepsToDo = new List<string>();
            itmStateEnum state = itmStateEnum.initiated;

            //if (project.SampleInIndustry.Any())
            //{
            //    state |= itmStateEnum.inIndustrySampleReady;
            //    if (verbose) output.log("Web site list of InIndustry [" + project.SampleInIndustry.Count + "] ready");
            //} else
            //{
            //    state |= itmStateEnum.notReady;
            //    if (verbose) output.log("Web site list of InIndustry sample is emptry!");
            //    stepsToDo.Add("Edit SampleInIndustry.txt file from the project directory and add domains. One domain per line.");
            //}

            //if (project.SampleInBusiness.Any())
            //{
            //    state |= itmStateEnum.inBusinessSampleReady;
            //    if (verbose) output.log("Web site list of InBusiness [" + project.SampleInBusiness.Count + "] ready" );
            //}
            //else
            //{
            //    state |= itmStateEnum.notReady;
            //    if (verbose) output.log("Web site list of InBusiness sample is emptry!");
            //    stepsToDo.Add("Edit SampleInBusiness.txt file from the project directory and add domains. One domain per line.");
            //}

            //if (project.SampleIrrelevant.Any())
            //{
            //    state |= itmStateEnum.irrelevantSampleReady;
            //    if (verbose) output.log("Web site list of InIrrelevant [" + project.SampleIrrelevant.Count + "] ready");
            //}
            //else
            //{
            //    state |= itmStateEnum.notReady;
            //    if (verbose) output.log("Web site list of InIrrelevant sample is emptry!");
            //    stepsToDo.Add("Edit SampleIrrelevant.txt file from the project directory and add domains. One domain per line.");
            //}


            
            //var inIndustry = project.MCRepoInIndustry.GetAllWebSites();

            //if (inIndustry.Any())
            //{
            //    state |= itmStateEnum.inIndustryRepoReady;
            //    if (verbose) output.log("InIndustry MCRepository has [" + inIndustry.Count + "] web sites");

            //} else
            //{
            //    state |= itmStateEnum.notReady;
            //    if (verbose) output.log("InIndustry MCRepository is empty");
            //    stepsToDo.Add("Call _Setup_ operation (on this plugin) to customize crawlSetup and nlpRepoProcessSetup - or edit configuration XML files directly.");
            //    stepsToDo.Add("Call _RunCrawl itmComponentEnum.inIndustry_ to create MCRepo for _InIndustry_");
            //}


            //var inBusiness = project.MCRepoInBusiness.GetAllWebSites();

            //if (inBusiness.Any())
            //{
            //    state |= itmStateEnum.inBusinessRepoReady;
            //    if (verbose) output.log("InBusiness MCRepository has [" + inBusiness.Count + "] web sites");
                
            //}
            //else
            //{
            //    state |= itmStateEnum.notReady;
            //    if (verbose) output.log("InBusiness MCRepository is empty");
            //    stepsToDo.Add("Call _RunCrawl itmComponentEnum.inBusiness_ to create MCRepo for _InBusiness_");
            //}


            //var irrelevant = project.MCRepoIrrelevant.GetAllWebSites();

            //if (irrelevant.Any())
            //{
            //    state |= itmStateEnum.irrelevantRepoReady;
            //    if (verbose) output.log("Irrelevant MCRepository has [" + irrelevant.Count + "] web sites");
            //}
            //else
            //{
            //    state |= itmStateEnum.notReady;
            //    if (verbose) output.log("Irrelevant MCRepository is empty");
            //    stepsToDo.Add("Call _RunCrawl itmComponentEnum.irrelevant_ to create MCRepo for _Irrelevant_");
            //}



            if (state.HasFlag(itmStateEnum.notReady))
            {
                output.log("Industry Term Model Project _not ready_ for application");
            } else
            {
                output.log("Industry Term Model Project _ready_ for application");
            }

            if (stepsToDo.Any())
            {
                output.AppendLine("# Advised steps to take:");
                Int32 c = 1;
                foreach (String step in stepsToDo)
                {
                    output.AppendLine(c.ToString("D3") + " " + step);
                    c++;
                }
                output.AppendLine("---");
            }


            // <--------------------- conclusion
            var stateFlags = state.getEnumListFromFlags();

            output.AppendLine("State flags:");
            foreach (itmStateEnum en in stateFlags)
            {
                output.Append("[" + en.toString() + "]");
            }
            output.AppendLine();

            return state;
        }



    }


}

