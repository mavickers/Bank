using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LightPath.Bank
{
    public class EmbeddedResource
    {
        private Assembly _assembly;
        private byte[] _contents;
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
        public Dictionary<string, string> Attributes { get; set; } = new();
        public byte[] ByteContents => Exceptions != null ? null : IsCached
                                      ? _contents ?? (_contents = Helpers.GetEmbeddedBytes(Assembly, NameSpace, FileName))
                                      : Helpers.GetEmbeddedBytes(Assembly, NameSpace, FileName);
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

                return results.Any() ? results : null;
            }
        }
        public bool IsCached { get; set; }
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

            if (string.IsNullOrWhiteSpace(_facadeFileName) && string.IsNullOrWhiteSpace(_fileName))
            {
                Url = string.Empty;

                return;
            }

            Url = string.IsNullOrWhiteSpace(_facadeFileName)
                  ? $"/{Assembly.GetName().Name}/{NameSpace}/{FileName}"
                  : $"/{Assembly.GetName().Name}/{NameSpace}/{_facadeFileName}";
        }
    }
}
