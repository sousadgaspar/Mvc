using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// Reads an object from the request body.
    /// </summary>
    public abstract class InputFormatter : IInputFormatter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InputFormatter"/> class.
        /// </summary>
        protected InputFormatter()
        {
            SupportedEncodings = new List<Encoding>();
            SupportedMediaTypes = new List<MediaTypeHeaderValue>();
        }

        /// <summary>
        /// Gets the mutable collection of character encodings supported by
        /// this <see cref="OutputFormatter"/>. The encodings are
        /// used when writing the data.
        /// </summary>
        public IList<Encoding> SupportedEncodings { get; private set; }

        /// <summary>
        /// Gets the mutable collection of <see cref="MediaTypeHeaderValue"/> elements supported by
        /// this <see cref="OutputFormatter"/>.
        /// </summary>
        public IList<MediaTypeHeaderValue> SupportedMediaTypes { get; private set; }

        protected object GetDefaultValueForType(Type modelType)
        {
            if (modelType.GetTypeInfo().IsValueType)
            {
                return Activator.CreateInstance(modelType);
            }

            return null;
        }

        /// <inheritdoc />
        public virtual bool CanRead(InputFormatterContext context)
        {
            var contentType = context.ActionContext.HttpContext.Request.ContentType;
            MediaTypeHeaderValue requestContentType;
            if (!MediaTypeHeaderValue.TryParse(contentType, out requestContentType))
            {
                return false;
            }

            return SupportedMediaTypes
                            .Any(supportedMediaType => supportedMediaType.IsSubsetOf(requestContentType));
        }

        /// <inheritdoc />
        public async Task<object> ReadAsync(InputFormatterContext context)
        {
            var request = context.ActionContext.HttpContext.Request;
            if (request.ContentLength == 0)
            {
                return GetDefaultValueForType(context.ModelType);
            }

            return await ReadRequestBodyAsync(context);
        }

        /// <summary>
        /// Reads the request body.
        /// </summary>
        /// <param name="context">The <see cref="InputFormatterContext"/> associated with the call.</param>
        /// <returns>A task which can read the request body.</returns>
        public abstract Task<object> ReadRequestBodyAsync(InputFormatterContext context);
    }
}