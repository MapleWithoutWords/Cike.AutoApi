using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using NET.AutoWebApi.ModelBinding;

namespace NET.AutoApi.ModelBinding
{
    public class AutoApiStreamContentModelBinder<TRemoteStreamContent> : IModelBinder
                                        where TRemoteStreamContent : class, IAutoApiStreamContent
    {
        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            var postedFiles = GetCompatibleCollection<TRemoteStreamContent>(bindingContext);

            // 获取模型绑定的参数名称
            var modelName = bindingContext.IsTopLevelObject
                ? bindingContext.BinderModelName ?? bindingContext.FieldName
                : bindingContext.ModelName;

            await GetFormFilesAsync(modelName, bindingContext, postedFiles);

            if (postedFiles.Count == 0 &&
                bindingContext.OriginalModelName != null &&
                !string.Equals(modelName, bindingContext.OriginalModelName, StringComparison.Ordinal) &&
                !modelName.StartsWith(bindingContext.OriginalModelName + "[", StringComparison.Ordinal) &&
                !modelName.StartsWith(bindingContext.OriginalModelName + ".", StringComparison.Ordinal))
            {
                modelName = ModelNames.CreatePropertyModelName(bindingContext.OriginalModelName, modelName);
                await GetFormFilesAsync(modelName, bindingContext, postedFiles);
            }

            object value;
            if (bindingContext.ModelType == typeof(TRemoteStreamContent))
            {
                if (postedFiles.Count == 0)
                {
                    // 没有上传文件
                    return;
                }

                value = postedFiles.First();
            }
            else
            {
                if (postedFiles.Count == 0 && !bindingContext.IsTopLevelObject)
                {
                    //没有上传文件
                    return;
                }

                // 判断方法参数是否是数组
                var modelType = bindingContext.ModelType;
                if (modelType == typeof(TRemoteStreamContent[]))
                {
                    value = postedFiles.ToArray();
                }
                else
                {
                    value = postedFiles;
                }
            }

            bindingContext.ValidationState.Add(value, new ValidationStateEntry()
            {
                Key = modelName,
            });

            bindingContext.ModelState.SetModelValue(
                modelName,
                rawValue: null,
                attemptedValue: null);

            bindingContext.Result = ModelBindingResult.Success(value);
        }

        /// <summary>
        /// 获取文件流
        /// </summary>
        /// <param name="modelName"></param>
        /// <param name="bindingContext"></param>
        /// <param name="postedFiles"></param>
        /// <returns></returns>
        private async Task GetFormFilesAsync(
            string modelName,
            ModelBindingContext bindingContext,
            ICollection<TRemoteStreamContent> postedFiles)
        {
            var request = bindingContext.HttpContext.Request;
            if (request.HasFormContentType)
            {
                var form = await request.ReadFormAsync();

                foreach (var file in form.Files)
                {
                    // If there is an <input type="file" ... /> in the form and is left blank.
                    if (file.Length == 0 && string.IsNullOrEmpty(file.FileName))
                    {
                        continue;
                    }

                    if (file.Name.Equals(modelName, StringComparison.OrdinalIgnoreCase))
                    {
                        postedFiles.Add(new AutoApiStreamContent(file.OpenReadStream(), file.FileName, file.ContentType, file.Length) as TRemoteStreamContent);
                    }
                }
            }
            else if (bindingContext.IsTopLevelObject)
            {
                postedFiles.Add(new AutoApiStreamContent(request.Body, null, request.ContentType, request.ContentLength) as TRemoteStreamContent);
            }
        }

        private static ICollection<T> GetCompatibleCollection<T>(ModelBindingContext bindingContext)
        {
            var model = bindingContext.Model;
            var modelType = bindingContext.ModelType;

            // There's a limited set of collection types we can create here.
            //
            // For the simple cases: Choose List<T> if the destination type supports it (at least as an intermediary).
            //
            // For more complex cases: If the destination type is a class that implements ICollection<T>, then activate
            // an instance and return that.
            //
            // Otherwise just give up.
            if (typeof(T).IsAssignableFrom(modelType))
            {
                return new List<T>();
            }

            if (modelType == typeof(T[]))
            {
                return new List<T>();
            }

            // Does collection exist and can it be reused?
            if (model is ICollection<T> collection && !collection.IsReadOnly)
            {
                collection.Clear();

                return collection;
            }

            if (modelType.IsAssignableFrom(typeof(List<T>)))
            {
                return new List<T>();
            }

            return (ICollection<T>)Activator.CreateInstance(modelType);
        }
    }
}
