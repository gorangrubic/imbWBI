// --------------------------------------------------------------------------------------------------------------------
// <copyright file="experimentNotes.cs" company="imbVeles" >
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
using System.ComponentModel;
using imbSCI.Core.attributes;
using System.Xml.Serialization;
using System.IO;
using imbSCI.Core.files;

namespace imbWBI.Core.WebClassifier.experiment
{


    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="imbACE.Core.core.builderForLog" />
    public class experimentNotes:builderForLog
    {
        public static Boolean isTheFirst { get; set; } = true;

        public folderNode folder { get; set; }

        public String description { get; set; }

        public experimentNotes(folderNode __folder, String _description)
        {
            folder = __folder;
            description = _description;

            autoFlushLength = 800000;
            autoFlushDisabled = true;
            if (isTheFirst)
            {
                isTheFirst = false;
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
                
            }

        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = (Exception)e.ExceptionObject;
            ex.LogException("Unhandled exception", "EX");

            log("Runtime terminating:" + e.IsTerminating);
        }

        public aceDictionarySet<String, String> notes { get; set; } = new aceDictionarySet<string, string>();

        public List<String> GetNotes(kFoldValidationCase validationCase)
        {
            return notes[validationCase.name].ToList();
        }
        public void AddNote(String note, kFoldValidationCase validationCase)
        {
            notes[validationCase.name].Add(note);
            log(validationCase.name + "> " + note);

        }


        /// <summary>
        /// Logs the exception.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="ex">The ex.</param>
        /// <param name="prefix">The prefix.</param>
        /// <param name="skipSave">if set to <c>true</c> [skip save].</param>
        public void LogException(String context, Exception ex, String prefix = "", Boolean skipSave=false)
        {

            if (!skipSave) AppendHorizontalLine();
            log(prefix + context);
            AppendLine(prefix + " > " + ex.Message + "");
            AppendLine(prefix + " > " + ex.StackTrace + "");

            if (ex.InnerException != null)
            {
                LogException("Inner exception", ex.InnerException, prefix + " > ", true);
            }

            if (ex.Exceptions() != null)
            {

                foreach (Exception exsub in ex.Exceptions())
                {
                    if (exsub != null)
                    {
                        AppendLine(prefix + " > InnerException [" + exsub.Message + "]");
                        AppendLine(prefix + " > > " + exsub.StackTrace);


                    }
                }
            }

            if (!skipSave)
            {
                AppendHorizontalLine();
                SaveNote();
            }
        }

        /// <summary>
        /// Saves the note into assigned folder. Default name: note.txt
        /// </summary>
        /// <param name="name">The name.</param>
        public void SaveNote(string name = "")
        {
            if (name.isNullOrEmpty())
            {
                name = "note";
            }
            name = name.ensureEndsWith(".txt");

            

            string path = folder.pathFor(name, imbSCI.Data.enums.getWritableFileMode.overwrite, description).getWritableFile().FullName;
            // aceCommonTypes.reporting.reportOutputQuickTools.saveMarkdownToPDF

            ContentToString().saveStringToFile(path);
        }
    }

}