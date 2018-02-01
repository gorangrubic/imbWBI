// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WrapperModelGraph.cs" company="imbVeles" >
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
using imbSCI.Data.collection.graph;
using System.Text;
using System.Data;
using System.Xml.Serialization;
using imbSCI.Core.files.folders;
using imbSCI.Core.extensions.enumworks;
using imbSCI.Core.extensions.data;
using imbSCI.Core.extensions;
using imbSCI.Core;
using imbSCI.Data;
using imbACE.Core.core.exceptions;
using HtmlAgilityPack;
using imbWBI.Core.BusinessDataModel;
using imbWBI.Core.WebHarvester.task;

namespace imbWBI.Core.WebHarvester.wrapper
{






    /// <summary>
    /// Wrapper graph model for a page to extract data from
    /// </summary>
    /// <seealso cref="imbSCI.Data.collection.graph.graphNodeCustom" />
    public abstract class WrapperModelGraph : graphNodeCustom, IWrapperModelGraph //where T:IDataModelElement, new()
    {


        /// <summary>
        /// Perfomes data extraction
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public abstract WrapperResponse doExtract(WrapperRequest request);
         

        protected override bool doAutorenameOnExisting => true;

        protected override bool doAutonameFromTypeName => true;

        
        private folderNode _folder;
        /// <summary> Directory to store collected content into </summary>
        public folderNode folder
        {
            get
            {
                if (_folder == null)
                {
                    
                    _folder = designateTargetFolder();
                }
                return _folder;
            }
            set
            {
                _folder = value;
                
            }
        }

        protected folderNode designateTargetFolder()
        {
            if (parent == null)
            {
                throw new aceGeneralException("A root wrapper model has no target folder defined!", null, this, "No target folder [" + name + "]");
            }
            if (useOwnFolder(type))
            {
                return modelParent.folder.Add(name, "Content of [" + name + "]", "Web content retrieved by [" + name + "] wrapper node. " + description);
            }
            else
            {
                return modelParent.folder;
            }
        }


        /// <summary>
        /// Optional description - for human interpretation
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public String description { get; set; } = "";

        /// <summary>
        /// XPath associated with the harvest node
        /// </summary>
        /// <value>
        /// The x path.
        /// </value>
        public String XPath { get; set; } = "";

        /// <summary>
        /// Gets the model root.
        /// </summary>
        /// <value>
        /// The model root.
        /// </value>
        [XmlIgnore]
        public IWrapperModelGraph modelRoot
        {
            get
            {
                return root as IWrapperModelGraph;
            }
        }

        [XmlIgnore]
        public IWrapperModelGraph modelParent
        {
            get
            {
                return parent as IWrapperModelGraph;
            }
        }

        protected Boolean useOwnFolder(WrapperTypeEnum _type) 
        {
            //List<WrapperTypeEnum> types = type.getEnumListFromFlags<WrapperTypeEnum>();

            switch (_type) {
                case WrapperTypeEnum.rootModel:
                    return true;
                    break;
                case WrapperTypeEnum.htmlHarvest:
                    return false;
                    break;
                case WrapperTypeEnum.imageHarvest:
                    return true;
                    break;
                case WrapperTypeEnum.linkHarvest:
                    return false;
                    break;
                case WrapperTypeEnum.tableHarvest:
                    return false;
                    break;
                case WrapperTypeEnum.none:
                    return false;
                    break;
                case WrapperTypeEnum.textHarvest:
                    return false;
                    break;
                    
            }
            return false;
        }

        /// <summary>
        /// Kind of data to produce
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public WrapperTypeEnum type { get; set; } = WrapperTypeEnum.none;

        /// <summary>
        /// Adds new wrapper node
        /// </summary>
        /// <param name="_XPath">The x path.</param>
        /// <param name="_type">The type.</param>
        /// <param name="_description">The description.</param>
        /// <param name="_name">The name.</param>
        /// <returns></returns>
        public TWrapper AddWrapper<TWrapper>(String _XPath, WrapperTypeEnum _type, String _description, String _name = "") where TWrapper:IWrapperModelGraph, new()
        {
            TWrapper output = new TWrapper();

            if (_name == "") {
                output.name = _type.ToString();
            } else {
                output.name = _name;
            }
            output.type = _type;
            output.XPath = _XPath;
            output.description = _description;

            Add(output);

            return output;

        }

        /// <summary>
        /// The constructor for main (root) wrapper model node
        /// </summary>
        /// <param name="parentFolder">The folder.</param>
        /// <param name="_name">The name.</param>
        /// <param name="_description">The description.</param>
        protected WrapperModelGraph(folderNode parentFolder, String _name, String _description)
        {
            name = _name;
            if (_description.isNullOrEmpty())
            {
                description = "primary wrapper node - the root";
            }
            else
            {
                description = _description;
            }
            
            type = WrapperTypeEnum.rootModel;

            _folder = parentFolder.Add(name, "Wrapper model " + _name, "Directory with content harvested by WrapperModel [" + name + "]. " + _description);
        }


        /// <summary>
        /// FOR SERIALIZATION --- USE <see cref="AddWrapper(string, WrapperTypeEnum, string, string)"/> to add subnodes, or <see cref="WrapperModelGraph(folderNode _folder)"/> for new model
        /// </summary>
        protected WrapperModelGraph()
        {
        }
    }

}