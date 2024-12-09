using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hub.Web.Extensions
{
    public static class AjaxExtensions
    {
        /// <summary>
        /// Cria um formulário ajax padrão para aplicações que usam portal
        /// </summary>
        public static IDisposable MBeginForm<TModel>(
            this IHtmlHelper<TModel> helper,
            string actionName,
            string controllerName,
            object routeValues = null,
            AjaxOptions ajaxOptions = null,
            object formFile = null)
        {
            if (ajaxOptions == null)
            {
                ajaxOptions = new AjaxOptions()
                {
                    OnBegin = "FormComponents.CrudSubmitFormBeginAjax",
                    OnSuccess = "FormComponents.CrudSubmitFormSuccessAjax",
                    OnFailure = "FormComponents.DefaultErrorHandlerAjax"
                };
            }

            var httpAttributes =
                new
                {
                    @class = "form-horizontal",
                    id = "form" + new Random().Next(1, int.MaxValue),
                    data_ajax = "true",
                    data_ajax_begin = ajaxOptions.OnBegin,
                    data_ajax_success = ajaxOptions.OnSuccess,
                    data_ajax_failure = ajaxOptions.OnFailure,
                    data_ajax_method = "POST",
                    enctype = formFile != null ? "multipart/form-data" : ""
                };


            return helper.BeginForm(
                actionName,
                controllerName,
                routeValues,
                FormMethod.Post,
                antiforgery: true,
                httpAttributes);
        }
    }


    /// <summary>
    /// opções passadas para um form ajax
    /// </summary>
    public class AjaxOptions
    {
        public string OnBegin { get; set; }
        public string OnSuccess { get; set; }
        public string OnFailure { get; set; }
    }
}
