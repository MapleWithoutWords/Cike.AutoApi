using Microsoft.AspNetCore.Mvc.ModelBinding;
using NET.AutoWebApi.ModelBinding;

namespace NET.AutoApi.ModelBinding
{
    public class AutoApiStreamContentModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.Metadata.ModelType == typeof(AutoApiStreamContent) ||
                typeof(IEnumerable<AutoApiStreamContent>).IsAssignableFrom(context.Metadata.ModelType))
            {
                return new AutoApiStreamContentModelBinder<AutoApiStreamContent>();
            }

            if (context.Metadata.ModelType == typeof(IAutoApiStreamContent) ||
                typeof(IEnumerable<IAutoApiStreamContent>).IsAssignableFrom(context.Metadata.ModelType))
            {
                return new AutoApiStreamContentModelBinder<IAutoApiStreamContent>();
            }

            return null;
        }
    }
}
