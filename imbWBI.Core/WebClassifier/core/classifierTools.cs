// --------------------------------------------------------------------------------------------------------------------
// <copyright file="classifierTools.cs" company="imbVeles" >
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
using imbNLP.PartOfSpeech.pipeline.core;
using imbNLP.PartOfSpeech.resourceProviders.core;
using imbNLP.PartOfSpeech.TFModels.webLemma.core;
using imbSCI.Core.files.folders;
using imbWBI.Core.WebClassifier.core;
using System.Text;
using imbACE.Core;
using System.IO;
using imbSCI.Core.extensions.data;
using imbSCI.Core.reporting;
using imbWBI.Core.WebClassifier.cases;
using imbSCI.Data.enums;
using imbMiningContext.TFModels.ILRT;
using imbWBI.Core.WebClassifier.experiment;

namespace imbWBI.Core.WebClassifier.core
{

    public class classifierTools
    {
        public classifierTools()
        {

        }

        public const String cacheLemmaResourcePath = "generalLemmaResourceCache.mtx";

        public experimentExecutionContext context { get; set; }

        private textResourceIndexBase _lemmaResource = null;

        public operationsSetup operation { get; set; }

        public void PreloadLemmaResource(ILogBuilder logger)
        {
            String p = cacheFolder.pathFor(cacheLemmaResourcePath, getWritableFileMode.none, "Extracted lexic resource with entries used in this experiment/fold");
            localLemmaResourcePath = p;
            lemmaResource = GetLemmaResource(p) as multitextIndexedResolver;
            
        }

        public Boolean isUsingGeneralCache = true;

        public void SetLemmaResource(IDocumentSetClass setClass)
        {
            
            String _localLemmaResourceName = setClass.name + "_general" + ".mtx";
          //  String _localLemmaResourcePath = 

            if (isUsingGeneralCache)
            {
                localLemmaResourcePath = cacheFolder.pathFor(cacheLemmaResourcePath, getWritableFileMode.newOrExisting, "Extracted lexic resource with entries used in this experiment/fold");
            }
            else
            {

                localLemmaResourcePath = cacheFolder.pathFor(_localLemmaResourceName, getWritableFileMode.newOrExisting, "Extracted lexic resource with entries used in this experiment/fold"); 
            }

            lemmaResource = GetLemmaResource(localLemmaResourcePath) as multitextIndexedResolver;
        }

        /// <summary>
        /// Saves the lexic resource cache subset, on default path in context of teh current experiment
        /// </summary>
        public void SaveCache()
        {
            String p = cacheFolder.pathFor(cacheLemmaResourcePath, getWritableFileMode.none, "Extracted lexic resource with entries used in this experiment/fold");
            if (!File.Exists(p))
            {
                lemmaResource.SaveUsedCache(p);
            }
        }

        public void SaveAndReset(IDocumentSetClass setClass)
        {
            if (IsSingleFold)
            {
                if (!isUsingGeneralCache)
                {
                    if (!localLemmaResourcePath.isNullOrEmpty())
                    {
                        if (!File.Exists(localLemmaResourcePath))
                        {
                            _lemmaResource.SaveUsedCache(localLemmaResourcePath, true);
                        }
                    }
                }
            }

            if (isUsingGeneralCache)
            {
            }
            else
            {
                if (!_lemmaResource.localCache.isNullOrEmpty())
                {
                    _lemmaResource = null;
                }
            }
        }

        /// <summary> </summary>
        public void SetReportAndCacheFolder(folderNode folder, Boolean reselect=false)
        {
            if (reselect)
            {
                reportFolder = null;
                cacheFolder = null;
            }
            //folderNode ch_root = folder.parent as folderNode;
            

            if (cacheFolder == null)
            {

                

                cacheFolder = folder.Add("cache", "Cached content", "Folder with cached lexic resouce partition, used during the experiment");
            }
        }

        
        public textResourceIndexBase GetLemmaResource(String localLemmaResource = "")
        {
            if (_lemmaResource == null)
            {
                String resPath = appManager.Application.folder_resources.findFile(setup_multitext_lex, SearchOption.AllDirectories);

                String altPath = appManager.Application.folder_resources.findFile(setup_multitext_alt, SearchOption.AllDirectories);


                String specPath = appManager.Application.folder_resources.findFile(setup_multitext_specs, SearchOption.AllDirectories);


                multitextIndexedResolver parser = new multitextIndexedResolver();
                
                
                if (DoUseLexicResourceCache)
                {

                    if (parser.localCache != localLemmaResource) parser.ResetIsLoaded();

                    if (!localLemmaResource.isNullOrEmpty())
                    {
                        if (File.Exists(localLemmaResource))
                        {

                            parser.localCache = localLemmaResource;
                        }
                        else
                        {
                            parser.localCache = "";
                        }

                    }
                    else
                    {
                        parser.localCache = "";
                    }
                } else
                {
                    
                }

                parser.Setup(resPath, specPath, altPath, output, textResourceIndexResolveMode.resolveOnQuery);

                _lemmaResource = parser;
            }
            return _lemmaResource;
        }

        public multitextIndexedResolver lemmaResource { get; set; }


        public String setup_multitext_lex { get; set; } = "";

        public String setup_multitext_specs { get; set; } = "";

        public String setup_multitext_alt { get; set; } = "";

        public ILogBuilder output { get; set; }
        
        public String localLemmaResourcePath { get; set; } = "";

        public folderNode cacheFolder { get; set; }

        public folderNode reportFolder { get; set; }
    
        public IWLFConstructor constructor { get; set; }

        public IWebFVExtractor classifier { get; set; }

        public IPipelineModel model { get; set; }

        public Boolean DoDebug { get; set; } = true;

        public Boolean DoReport { get; set; } = true;

        public Boolean DoUseExistingKnowledge { get; set; } = true;

        public Boolean DoUseLexicResourceCache { get; set; } = true;

        public Boolean IsSingleFold { get; set; } = false;
        public bool DoResultReporting { get; set; }
        public bool DoClassReport { get; set; }
    }

}