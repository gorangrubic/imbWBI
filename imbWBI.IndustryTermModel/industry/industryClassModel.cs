// --------------------------------------------------------------------------------------------------------------------
// <copyright file="industryClassModel.cs" company="imbVeles" >
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
using imbACE.Core;
using imbMiningContext;
using imbMiningContext.MCRepository;
// using imbMiningContext.TFModels.WLF_ISF;
using imbNLP.PartOfSpeech.pipelineForPos.subject;
using imbSCI.Core.files.fileDataStructure;
using imbSCI.Core.files.folders;
using imbWBI.Core.WebClassifier.cases;
using imbWBI.Core.WebClassifier.wlfClassifier;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace imbWBI.IndustryTermModel.industry
{
    /// <summary>
    /// Represents one industry area: having its TF-IDF table, MC repository, sample list
    /// </summary>
    [fileStructure(nameof(name), fileStructureMode.subdirectory, fileDataFilenameMode.propertyValue, fileDataPropertyOptions.textDescription)]
    public class industryClassModel: fileDataStructure, IFileDataStructure, IDocumentSetClass
    {
        public industryClassModel(String _name, String _tla, String _desc)
        {
            name = _name;
            description = _desc;
            treeLetterAcronim = _tla;
            repoFolderRoot = imbMCManager.GetMCRepoNode();
            

        }

        public industryClassModel() {

            repoFolderRoot = imbMCManager.GetMCRepoNode();
        }


        public List<pipelineTaskMCSiteSubject> FilterSites(List<pipelineTaskMCSiteSubject> input)
        {
            List<pipelineTaskMCSiteSubject> output = new List<pipelineTaskMCSiteSubject>();

            foreach (pipelineTaskMCSiteSubject site in input)
            {
                foreach (String s in WebSiteSample)
                {
                    if (s.Contains(site.MCSite.domainInfo.domainRootName))
                    {
                        output.Add(site);
                        break;
                    }
                }
            }

            return output;
        }



        [XmlIgnore]
        public folderNode repoFolderRoot { get; set; }

        public String name { get; set; } = "industry_model";

        public String description { get; set; } = "";

        public String treeLetterAcronim { get; set; } = "icm";

        [Category("Label")]
        [DisplayName("Sample in industry")]
        [Description("List of sites that are in the industry")]
        [XmlIgnore]
        [fileData(fileDataFilenameMode.memberInfoName, fileDataPropertyMode.text, fileDataPropertyOptions.none)]
        public List<String> WebSiteSample { get; set; } = new List<String>();


        private imbMCRepository _MCRepository = null;

        [XmlIgnore]
        public imbMCRepository MCRepository { get
            {

                if (_MCRepository == null)
                {
                    _MCRepository = MCRepositoryName.LoadDataStructure<imbMCRepository>(repoFolderRoot);
                    _MCRepository.name = MCRepositoryName;
                }
                return _MCRepository;
            }
            set {
                _MCRepository = value;
            } }

        


        public int classID { get; set; } = 0;

        [XmlIgnore]
        public DocumentSetClasses parent { get; set; }



        [XmlIgnore]
        public String MCRepositoryName
        {
            get
            {
                return name + "_" + treeLetterAcronim;
            }
        }

        public override void OnBeforeSave()
        {
            if (MCRepository != null)
            {
                MCRepository.SaveDataStructure(repoFolderRoot);
            }
            folder.generateReadmeFiles(appManager.AppInfo);
        }

        public override void OnLoaded()
        {
           MCRepository = MCRepositoryName.LoadDataStructure<imbMCRepository>(repoFolderRoot);
           MCRepository.name = MCRepositoryName;
        }

        
    }
}
