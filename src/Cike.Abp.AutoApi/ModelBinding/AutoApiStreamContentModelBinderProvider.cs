namespace Abp.AutoApi.ModelBinding;

public class RemoteStreamContentModelBinderProvider : IModelBinderProvider
{
    public IModelBinder GetBinder(ModelBinderProviderContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (context.Metadata.ModelType == typeof(RemoteStreamContent) ||
            typeof(IEnumerable<RemoteStreamContent>).IsAssignableFrom(context.Metadata.ModelType))
        {
            return new RemoteStreamContentModelBinder<RemoteStreamContent>();
        }

        if (context.Metadata.ModelType == typeof(IRemoteStreamContent) ||
            typeof(IEnumerable<IRemoteStreamContent>).IsAssignableFrom(context.Metadata.ModelType))
        {
            return new RemoteStreamContentModelBinder<IRemoteStreamContent>();
        }

        return null;
    }
}
