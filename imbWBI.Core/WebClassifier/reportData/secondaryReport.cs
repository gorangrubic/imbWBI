// --------------------------------------------------------------------------------------------------------------------
// <copyright file="secondaryReport.cs" company="imbVeles" >
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
using imbNLP.PartOfSpeech.TFModels.semanticCloud;
using imbSCI.Core.attributes;
using imbSCI.Core.files;
using imbSCI.Core.files.folders;
using imbSCI.Core.reporting;
using imbSCI.Core.extensions.table.dynamics;
using imbSCI.Core.extensions.table.style;
using imbSCI.Core.extensions.table;
using imbSCI.Core.extensions.table.core;
using imbSCI.DataComplex.tables;
using imbWBI.Core.WebClassifier.experiment;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Data;
using imbACE.Core;
using System.Xml.Serialization;
using imbSCI.Data;
using imbSCI.DataComplex.extensions.data.modify;
using System.Threading.Tasks;
using imbSCI.Graph.Converters;
using imbSCI.Graph.DOT;

namespace imbWBI.Core.WebClassifier.reportData
{



    /// <summary>
    /// Data structure describing multiple experiments, their key features and results
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///     <item>
    ///         Table of settings and key results
    ///     </item>
    ///     <item>
    ///         Experiment - feature graph
    ///     </item>
    /// </list>
    /// </remarks>
    public class secondaryReport
    {
        [XmlIgnore]
        public folderNode folder { get; set; }

        public secondaryReport()
        {

        }

        public secondaryReport(folderNode _folder)
        {
            folder = _folder;
        }

        public List<secondaryReportOnExperiment> items { get; set; } = new List<secondaryReportOnExperiment>();

        [XmlIgnore]
        public List<secondaryReportOnFVE> allItems { get; set; } = new List<secondaryReportOnFVE>();


        [XmlIgnore]
        public List<secondaryReportOnFVE> allTopItems { get; set; } = new List<secondaryReportOnFVE>();




        /// <summary>
        /// Scans the subdirectories of given folder and creates summaries for each experiment group, where groups are formed following the structure of subdirectory tree
        /// </summary>
        /// <param name="_folder">The folder.</param>
        /// <param name="reportSubDirectory">The report sub directory - name of the subdirectory where the summary secondary report is stored</param>
        /// <param name="logger">The logger.</param>
        public static void ScanSubdirectories(folderNode _folder, String reportSubDirectory, ILogBuilder logger)
        {
            List<String> files = _folder.findFiles("experimentSetup.xml", SearchOption.AllDirectories);

            List<DirectoryInfo> directories = new List<DirectoryInfo>();

            Dictionary<String, DirectoryInfo> dirs = new Dictionary<string, DirectoryInfo>();

            foreach (String fl in files)
            {
                DirectoryInfo di = new DirectoryInfo(Path.GetDirectoryName(fl));
                if (di.Parent != null)
                {
                    if (!dirs.ContainsKey(di.Parent.FullName))
                    {
                        dirs.Add(di.Parent.FullName, di.Parent);
                    }
                }
            }

            logger.log("Experiment groups detected: " + dirs.Count);

            //foreach (var pair in dirs)
            //{
                
               
            //}

            Parallel.ForEach(dirs, pair =>
            {
                var secRep = new secondaryReport(pair.Value);
                secRep.Load(pair.Value, logger);
                secRep.GetAndSaveDataTable(reportSubDirectory, logger);
            });

        }





        /// <summary>
        /// Loads the specified folder.
        /// </summary>
        /// <param name="folder">The folder - to be scanned for experiment data</param>
        /// <param name="logger">The logger.</param>
        public void Load(folderNode _folder, ILogBuilder logger)
        {
            folder = _folder;
            try
            {
                var experimentFiles = folder.findFiles("experimentSetup.xml", SearchOption.AllDirectories, false);

                foreach (var path in experimentFiles)
                {
                    try
                    {
                        var exp = new secondaryReportOnExperiment(path, logger, folder);
                        items.Add(exp);
                        allItems.AddRange(exp.items);
                        allTopItems.Add(exp.topPerformer);
                      //  logger.log("Experiment report processed [" + path + "] : [" + allItems.Count + "]");
                    }
                    catch (Exception ex)
                    {
                        logger.log(ex.LogException("secondaryReport.Load", "SECREP_LOAD=>" + path));
                    }
                }
            }
            catch (Exception ex)
            {

                logger.log(ex.LogException("secondaryReport.Load","SECREP_LOAD"));
            }
            
        }

        protected Dictionary<Int32, DataTableTypeExtended<secondaryReportOnFVE>> tablesByFolds = new Dictionary<int, DataTableTypeExtended<secondaryReportOnFVE>>();

        protected List<String> tops_inFolds = new List<String>();

        protected Dictionary<Int32,String> top_inFolds = new Dictionary<Int32, String>();

        protected Dictionary<Int32, secondaryReportOnFVE> top_inFolds_instance = new Dictionary<Int32, secondaryReportOnFVE>();

        protected Dictionary<Int32, Double> top_F1inFolds = new Dictionary<Int32, Double>();

        /// <summary>
        /// Gets the table.
        /// </summary>
        /// <param name="folds">The folds.</param>
        /// <returns></returns>
        public DataTableTypeExtended<secondaryReportOnFVE> GetTable(Int32 folds)
        {
            if (!tablesByFolds.ContainsKey(folds))
            {
                DataTableTypeExtended<secondaryReportOnFVE> dataTableExtended = new DataTableTypeExtended<secondaryReportOnFVE>("SecondaryReport", "Experiments with k=[" + folds + "] cross-validation");
                tablesByFolds.Add(folds, dataTableExtended);
                top_inFolds_instance.Add(folds, new secondaryReportOnFVE());
                top_inFolds.Add(folds, "");
                top_F1inFolds.Add(folds, Double.MinValue);

            }
            return tablesByFolds[folds];
        }


        public Boolean DoGenerateGraph { get; set; } = true;


        /// <summary>
        /// Generates the report set directory readme file.
        /// </summary>
        /// <param name="filename">The filename.</param>
        public void GenerateParentReadmeFile(String filename = "experiments_readme.txt")
        {
            folderNode node = folder;

            //if (folder.parent != null)
            //{
            //    node = folder.parent as folderNode;
            //}
            //else
            //{
            //    DirectoryInfo di = folder;
            //    DirectoryInfo dip = di.Parent;
            //    node = dip;
            //    node.caption = dip.Name;
            //    node.description = "Report data set on group of experiments";
            //}

            StringBuilder sb = new StringBuilder();

            // <--------- ---- ---- -- ------ DESCRIPTION GENERATION
            String ln = "# Report package [" + node.caption + "]";
            sb.AppendLine(ln);
            String line = "-".Repeat(ln.Length);
            sb.AppendLine(line);


            sb.AppendLine("This directory contains integral reports on performed experiments on web site classification, done with imbWBI library - part of imbVeles Framework.");
            sb.AppendLine("Each subdirectory contains reports on one particular configuration, tested on a range of semantic term expansion (Stx) values.");
            sb.AppendLine(line);
            sb.AppendLine();
            sb.AppendLine("Main directory structure:");
            sb.AppendLine("-- exp_[experient code name]");
            sb.AppendLine("-- exp_[experient code name]");
            sb.AppendLine("--            ...           ");
            sb.AppendLine("-- Summary");
            sb.AppendLine();
            sb.AppendLine(line);
            sb.AppendLine();
            sb.AppendLine("## Experiment subdirectories");
            sb.AppendLine("In the subdirectories named exp_[experiment code name] you will find summary text, spreadsheet and XML data files - describing the experiment.");
            sb.AppendLine("Wherever an Excel spreadsheet file is generated (in XLSX format), you'll find a subfolder named [data] where the same data is exported in Comma Separated Values (CSV) format for easier consumption by 3rd party software.");
            sb.AppendLine("-- In Excel spreadsheet files you'll find second sheet called LEGEND, where each column in the report is explained. The same column descriptions are saved in plain text format within [data] subfolders.");
            sb.AppendLine("In each subdirectory of this data set, across the complete directory tree, you will find [directory_readme.txt] where content of the subdirectory is described.");
            sb.AppendLine("Exact content of the experiment report subdirectores varies depending on reporting options used and version of the software.");
            sb.AppendLine();
            sb.AppendLine("However, there is general directory tree structure:");
            sb.AppendLine("-- exp_[experient code name]");
            sb.AppendLine("-- -- [name of FVE]_[sample randomization tag]_E[Stx]                    <- directory with report on experiment version, performed with Stx number of semantic term expansion steps");
            sb.AppendLine("-- -- -- [name of FVE]_[sample randomization tag]_E[Stx]00[fold id]      <- directory with report on [fold id] fold of k-fold schema used");
            sb.AppendLine("-- -- -- -- cases                                                        <- XML serialized data on Category Knowledge, constructed in this fold (from the training sample subset)");
            sb.AppendLine("-- -- General                                                            <- general report on the complete data set, different in scope depending on reporting options used");
            sb.AppendLine("-- -- SharedKnowledge                                                    <- XML serialized data (Lemma Tables) on all Cases in the data set");
            sb.AppendLine("-- -- errors                                <- Here you might find logs on exceptions, if an error during execution happen. ");
            sb.AppendLine("                                            | However, if the note.txt file is not empty - it should have earlier time/date creation stamp then the rest of the report.");
            sb.AppendLine("                                            | Only reports with error notes that should exist in this data set are the ones where Semantic Cloud");
            sb.AppendLine("                                            | failed to be created because of small amount of training data in the fold.");
            sb.AppendLine();
            sb.AppendLine("## Summary directory");
            sb.AppendLine("In the [Summary] directory, you will find aggregated overview reports on all experiments in this group. The reports are created separately for each [k] number of k-fold schemas.");
            sb.AppendLine("These reports are given in spreadsheet format (Excel file, and CSV in [data] subfolder) and in form of native XML serialized objects.");
            sb.AppendLine("Additional remarks:");
            sb.AppendLine("-- Reports may contain some additional metrics, subreports and other records, not mentioned nor explained in the research article. Like: ModelMetrics.xml. ");
            sb.AppendLine("-- These are some unfinished ideas, models for FVE evaluation that, at the end, were not used for research conclusions nor system validation.");
            sb.AppendLine();
            Double f1 = Double.MinValue;
            secondaryReportOnFVE topFVE = null;

            sb.AppendLine(line);

            sb.AppendLine("## List of the experiments contained in this data set");

            Int32 c = 1;
            foreach (var item in items)
            {
                ln = "[" + c.ToString("D2") + "] " + item.experiment.name + "               (sub experiments: " + item.items.Count.ToString() + ")";
                sb.AppendLine(ln);

                if (f1 < item.topPerformer.F1Score)
                {
                    topFVE = item.topPerformer;
                    f1 = item.topPerformer.F1Score;
                }

                c++;
            }

            

            sb.AppendLine(line);
            sb.AppendLine();
            sb.AppendLine("imbVeles Framework | imbWBI | GNU GPL v3.0 | http://blog.veles.rs | Goran Grubić | hardy@veles.rs");
            sb.AppendLine(line);
            sb.AppendLine("File generated: " + DateTime.Now.ToLongDateString() + ", " + DateTime.Now.ToLongTimeString());
            
            


            // <---------

            String p = node.pathFor(filename, imbSCI.Data.enums.getWritableFileMode.overwrite, "ReadMe file describing experiment reports contained in this report set.");

            File.WriteAllText(p, sb.ToString());

            node.AttachSubfolders();

            if (DoGenerateGraph)
            {
                var dgml = node.GetDirectedGraph(false, false, false, 3);
                dgml.Layout = imbSCI.Graph.DGML.enums.GraphLayoutEnum.DependencyMatrix;
                dgml.GraphDirection = imbSCI.Graph.DGML.enums.GraphDirectionEnum.LeftToRight;
                dgml.Save(node.pathFor("directoryGraph.dgml", imbSCI.Data.enums.getWritableFileMode.overwrite, "Directory structure Directed Graph in Microsoft DGML format - open in Visual Studio", true));

                //var dot = dgml.ConvertToDOT();

                //dot.Save(node.pathFor("directoryGraph.dot", imbSCI.Data.enums.getWritableFileMode.overwrite, "Directory structure Directed Graph in GraphVIZ DOT graph language format", true));
            }
            node.generateReadmeFiles(appManager.AppInfo);


        }



        /// <summary>
        /// Creates the summary reports
        /// </summary>
        /// <param name="prefix">The prefix.</param>
        /// <param name="logger">The logger.</param>
        public void GetAndSaveDataTable(String prefix, ILogBuilder logger)
        {
            try
            {
                folderNode repFolder = folder.Add(prefix, prefix + " Secondary report", "Summary report on multiple experiments [" + items.Count+ "], data extracted from:" + folder.path + " experiment report folder");

                foreach (var item in allItems)
                {
                    GetTable(item.Folds);
                }


                
                //String top_hash = "";
                List<String> top_hashes = new List<string>();



                foreach (var item in allTopItems)
                {
                    Double F1 = top_F1inFolds[item.Folds];
                    if (item.F1Score > F1)
                    {
                        top_F1inFolds[item.Folds] = item.F1Score;
                        top_inFolds[item.Folds] = item.UID;
                    }

                    top_hashes.Add(item.UID);
                }

                foreach (var item in allTopItems)
                {
                    if (top_inFolds[item.Folds] == item.UID)
                    {
                        top_inFolds_instance[item.Folds] = item;
                        item.Comment = "<-- best globaly";
                    }
                }
                Boolean kFoldPlural = tops_inFolds.Count > 1;
                String kFoldInsert = "";
                foreach (var flds in top_inFolds)
                {
                    kFoldInsert = kFoldInsert.add(flds.Key.ToString(), ", ");
                }
                kFoldInsert = " k=[" + kFoldInsert + "] k-fold validation shema";
                if (kFoldPlural) kFoldInsert += "s";

                folder.caption = "[" + folder.caption + "] Experimental session";
                folder.description = "Experiment report package with [" + allItems.Count + "] reports on experiments performed in " + kFoldInsert + ".";



                Int32 c = 0;

                foreach (var item in items)
                {
                    
                    foreach (var it in item.items)
                    {
                        if (top_hashes.Contains(it.UID))
                        {
                            if (it.Comment.isNullOrEmpty())
                            {
                                it.Comment = "<-- best locally";
                            }
                        }
                        if (it.F1Score > 0)
                        {
                            c++;
                            GetTable(it.Folds).AddRow(it);
                            
                        }
                    }
                   // dataTableExtended.AddLineRow();

                }

                foreach (var tPair in tablesByFolds)
                {
                    var dataTableExtended = tPair.Value;

                    dataTableExtended.GetRowMetaSet().SetStyleForRowsWithValue<String>(DataRowInReportTypeEnum.dataHighlightB, "UID", top_inFolds[tPair.Key]);

                    dataTableExtended.GetRowMetaSet().SetStyleForRowsWithValue<String>(DataRowInReportTypeEnum.dataHighlightA, "UID", top_hashes);


                    dataTableExtended.AddExtra("Summary report on experiments done with k=[" + tPair.Key + "] cross validation.");


                    dataTableExtended.GetReportAndSave(repFolder, appManager.AppInfo, prefix + "_k" + tPair.Key + "_summary", true, true);

                    String pathForTable = repFolder.pathFor(prefix + "_k" + tPair.Key + "_summary.xml", imbSCI.Data.enums.getWritableFileMode.overwrite, "Serialized DataTable with data collected from [" + items.Count + "] experiment reports");

                    dataTableExtended.saveObjectToXML(pathForTable);
                }

                foreach (var iPair in top_inFolds_instance)
                {
                    String pathForTable = repFolder.pathFor(prefix + "_k" + iPair.Key + "_bestCase.xml", imbSCI.Data.enums.getWritableFileMode.overwrite, "Serialized object with report about the best performing FVE model [" + iPair.Value.F1Score.ToString("F5") + "] experiment reports");

                    iPair.Value.saveObjectToXML(pathForTable);
                }
               

            String pathForObject = repFolder.pathFor(prefix + "_report_summary.xml", imbSCI.Data.enums.getWritableFileMode.overwrite, "Serialized [secondaryReport] object with summary data collected from [" + items.Count + "] experiment reports");

            this.saveObjectToXML(pathForObject);

                repFolder.generateReadmeFiles(appManager.AppInfo);

                GenerateParentReadmeFile();

            }
            catch (Exception ex)
            {

                logger.log(ex.LogException("secondaryReport.Load", "SECREP_LOAD"));
            }
           // return null;
        }
    }
}
