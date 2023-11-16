﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LightPath.Bank
{
    public class BankEmbeddedResource
    {
        private Assembly _assembly;
        private string _facadeFileName;
        private string _fileName;
        private string _nameSpace;

        public Assembly Assembly
        {
            get => _assembly;
            set
            {
                _assembly = value;
                SetUrl();
            }
        }
        /// <summary>
        /// Key/Value pairs of attributes to be added to the emitted tag.
        /// </summary>
        public Dictionary<string, string> Attributes { get; } = new();
        public byte[] Contents => (Exceptions?.Any() ?? false) ? null : BankHelpers.GetEmbeddedBytes(Assembly, NameSpace, FileName);
        /// <summary>
        /// Key/Value pairs of placeholder variables to be injected in text-based resources.
        /// </summary>
        public Dictionary<string, string> Variables { get; } = new();

        public string ContentType { get; set; }
        public List<Exception> Exceptions
        {
            get
            {
                var results = new List<Exception>();

                if (Assembly == null) results.Add(new NullReferenceException(nameof(Assembly)));
                if (string.IsNullOrWhiteSpace(NameSpace)) results.Add(new NullReferenceException(nameof(NameSpace)));
                if (string.IsNullOrEmpty(FileName)) results.Add(new NullReferenceException(nameof(FileName)));
                if (string.IsNullOrWhiteSpace(ContentType)) results.Add(new NullReferenceException(nameof(ContentType)));

                return results;
            }
        }
        public string FacadeFileName
        {
            get => _facadeFileName;
            set
            {
                _facadeFileName = value;
                SetUrl();
            }
        }
        public string FileName
        {
            get => _fileName;
            set
            {
                _fileName = value;
                SetUrl();
            }
        }
        public string NameSpace
        {
            get => _nameSpace;
            set
            {
                _nameSpace = value;
                SetUrl();
            }
        }
        public string Url { get; private set; }

        /// <summary>
        /// Value with which to prepend the Url when using RenderEmbeddedResource
        /// </summary>
        public string UrlRenderPrepend { get; set; }

        /// <summary>
        /// Set the Url property based on the value of the other properties
        /// </summary>
        /// <remarks>
        /// We want to set Url on property sets rather than compute it on a
        /// getter as the Url is used in request path comparisons in the middleware.
        /// </remarks>
        private void SetUrl()
        {
            if (Assembly == null || string.IsNullOrWhiteSpace(NameSpace))
            {
                Url = string.Empty;

                return;
            }

            var fileName = string.IsNullOrWhiteSpace(_facadeFileName) ? _fileName : _facadeFileName;

            Url = string.IsNullOrWhiteSpace(fileName) ? string.Empty : $"/{Assembly.GetName().Name}/{NameSpace}/{fileName}".Replace("//", "/");
        }
    }
}
