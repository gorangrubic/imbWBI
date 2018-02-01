// --------------------------------------------------------------------------------------------------------------------
// <copyright file="imbWBIToolConsole.cs" company="imbVeles" >
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
// Project: imbWBI.Tools
// Author: Goran Grubic
// ------------------------------------------------------------------------------------------------------------------
// Project web site: http://blog.veles.rs
// GitHub: http://github.com/gorangrubic
// Mendeley profile: http://www.mendeley.com/profiles/goran-grubi2/
// ORCID ID: http://orcid.org/0000-0003-2673-9471
// Email: hardy@veles.rs
// </summary>
// ------------------------------------------------------------------------------------------------------------------
namespace imbWBI.Tools
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using imbACE.Services.console;
    using imbWBI.IndustryTermModel.consolePlugin;
    using imbWEM.Core.consolePlugin;
    using imbNLP.Data.semanticLexicon;
    using imbNLP.Data;
    using imbWEM.Core;
    using imbWEM.Core.console;
    using imbSCI.Data;
    using imbSCI.Core.extensions.data;
    using imbACE.Core;
    using imbMiningContext;
    using System.ComponentModel.DataAnnotations;
    using imbACE.Core.core.exceptions;

    public class imbWBIToolConsole : aceAdvancedConsole<imbWBIToolState, imbWBIToolWorkspace>
    {
        public override string consoleTitle { get { return "imbWBITool Console"; } }

        public imbWBIToolConsole() : base()
        {


        }




        protected imbMCManager _mcm = null;
        /// <summary>
        /// Embedded plugin for MCRepository manipulations
        /// </summary>
        [Display(GroupName = "Plugin", Name = "mcm", Description = "Embedded plugin")]
        public imbMCManager mcm
        {
            get
            {
                if (_mcm == null) _mcm = new imbMCManager(this);
                return _mcm;
            }
        }



        protected itmPlugin _itm = null;

        /// <summary>
        /// Industry Term Model plugin
        /// </summary>
        /// <value>
        /// The itm.
        /// </value>
        public itmPlugin itm
        {
            get
            {
                if (_itm == null) _itm = new itmPlugin(this);
                return _itm;
            }
        }


        private crawlJobPlugin _wem = null;
        /// <summary>
        /// Gets the crawlJob plugin
        /// </summary>
        /// <value>
        /// The wem.
        /// </value>
        public crawlJobPlugin wem
        {
            get
            {
                if (_wem == null) _wem = new crawlJobPlugin(this);
                return _wem;
            }
        }


        private semanticLexiconManager _manager;
        /// <summary>
        /// Semantic lexicon manager
        /// </summary>
        protected semanticLexiconManager manager
        {
            get
            {
                if (_manager == null) _manager = semanticLexiconManager.manager;
                return _manager;
            }
            set { _manager = value; }
        }


        /// <summary>
        /// Gets the workspace.
        /// </summary>
        /// <value>
        /// The workspace.
        /// </value>
        public override imbWBIToolWorkspace workspace
        {
            get
            {
                if (_workspace == null)
                {
                    _workspace = new imbWBIToolWorkspace(this);
                }
                return _workspace;
            }
        }

        public override aceCommandConsoleIOEncode encode
        {
            get
            {
                return aceCommandConsoleIOEncode.dos;
            }
        }

        public override void onStartUp()
        {
            try
            {
                imbWEMManager.prepare(null);
            } catch (Exception ex)
            {
                aceGeneralException axe = new aceGeneralException("Web Exploration Module failed to prepare on WBI console start-up: " + ex.Message, ex, this, "WEM prepare call failed: " + ex.Message);
                throw axe;
            }

            base.onStartUp();

            output.log("Plugin: " + itm.name);

            output.log("Plugin: " + wem.name);

            imbNLP.PartOfSpeech.nlpTools.nlpTypeManager.main.prepare();

            //

            // imbLanguageFrameworkManager.Prepare();

            //manager.workspaceFolderPath = workspace.folder.path;

            //log("Preparing Semantic Lexicon manager", true);

            //manager.prepare();
            //log("Preparing Semantic Lexicon cache", true);
            //manager.prepareCache(output, workspace.folder);
            //manager.constructor.startConstruction(workspace.folder[ACFolders.constructor].path.removeEndsWith("\\"));

            //   Program app = appManager.Application as Program;


            // imbWEMManager.index.isFullTrustMode = imbWEMManager.settings.indexEngine.doIndexFullTrustMode;

            //if (app.args.Any())
            //{
            //    string line = app.args.toCsvInLine(" ");
            //    executeCommand(line);
            //}
            //else
            //{
            //    var script = workspace.loadScript("autoexec.ace");
            //    executeScript(script);
            //}
        }

        protected override void doCustomSpecialCall(aceCommandActiveInput input)
        {

        }
    }

}