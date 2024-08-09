using LightPath.Bank.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LightPath.Bank
{
    public class BankEmbeddedResource
    {
        private Guid _instanceId = Guid.NewGuid();
        private Assembly _assembly;
        private string _facadeFileName;
        private string _fileName;
        private string _nameSpace;
        private string _resourceKey;
        private string _urlPrepend;
        private byte[] _contents;
        

        public Assembly Assembly
        {
            get => _assembly;
            set
            {
                _assembly = value;
                SetLocation();
            }
        }
        /// <summary>
        /// Key/Value pairs of attributes to be added to the emitted tag.
        /// </summary>
        public Dictionary<string, string> Attributes { get; } = new();
        public List<IBankAssetContentProcessor> ContentProcessors;
        public byte[] Contents
        {
            get
            {
                if (Exceptions?.Any() ?? false) return null;
                if (_contents != null) return _contents;

                var contents = BankHelpers.GetEmbeddedBytes(this);

                if (!ContentProcessors?.Any() ?? true)
                {
                    _contents = contents;

                    return _contents;
                }

                _contents = ContentProcessors.Aggregate(contents, (current, processor) => processor.Process(current));

                return _contents;
            }
        }

        /// <summary>
        /// Key/Value pairs of placeholder variables to be injected in text-based resources.
        /// </summary>
        public Dictionary<string, string> Variables { get; } = new();

        public string BaseUrl { get; private set; }
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
                SetLocation();
            }
        }
        public string FileName
        {
            get => _fileName;
            set
            {
                _fileName = value;
                SetLocation();
            }
        }
        public string NameSpace
        {
            get => _nameSpace;
            set
            {
                _nameSpace = value;
                SetLocation();
            }
        }

        public string ResourceKey
        {
            //get => string.IsNullOrWhiteSpace(_resourceKey) ? $"EmbeddedResource-{_instanceId}-({Url})" : _resourceKey;
            get => string.IsNullOrWhiteSpace(_resourceKey) ? $"{_assembly.GetName().Name}.{NameSpace}.{FileName}" : _resourceKey;
            set => _resourceKey = string.IsNullOrWhiteSpace(value.Trim()) ? null : BankAssets.All.All(asset => asset.Value.ResourceKey != value) ? value : throw new Exception("Resource key already exists in BankAssets");
        }
        public string Url { get; private set; }
        public string VirtualPath { get; private set; }

        /// <summary>
        /// Value with which to prepend the Url when using RenderEmbeddedResource
        /// </summary>
        public string UrlPrepend
        {
            get => _urlPrepend;
            set
            {
                _urlPrepend = value;
                SetLocation();
            }
        }

        /// <summary>
        /// Set the Url property based on the value of the other properties
        /// </summary>
        /// <remarks>
        /// We want to set Url on property sets rather than compute it on a
        /// getter as the Url is used in request path comparisons in the middleware.
        /// </remarks>
        private void SetLocation()
        {
            if (Assembly == null || string.IsNullOrWhiteSpace(NameSpace))
            {
                Url = string.Empty;

                return;
            }

            var fileName = string.IsNullOrWhiteSpace(_facadeFileName) ? _fileName : _facadeFileName;
            var prepend = string.IsNullOrWhiteSpace(UrlPrepend) ? string.Empty : UrlPrepend;
            var baseUrl = string.IsNullOrWhiteSpace(fileName) ? string.Empty : $"{Assembly.GetName().Name}/{NameSpace}/{fileName}".Replace("//", "/");

            BaseUrl = $"/{baseUrl}";
            Url = $"{prepend}/{baseUrl}";
            VirtualPath = $"~/{baseUrl}";
        }
    }
}
