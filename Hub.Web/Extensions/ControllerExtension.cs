using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc;

namespace Hub.Web.Extensions
{
    public static class ControllerExtension
    {
        /// <summary>
        /// Efetua a conversão de uma view ou partial para string
        /// </summary>
        public static string RenderPartialToString(this Controller controller, string partialViewName = "Index", object model = null)
        {
            if (string.IsNullOrEmpty(partialViewName))
            {
                partialViewName = controller.ControllerContext.ActionDescriptor.ActionName;
            }

            controller.ViewData.Model = model;

            using (var writer = new StringWriter())
            {
                IViewEngine viewEngine = controller.HttpContext.RequestServices.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;

                ViewEngineResult viewResult = null;

                if (partialViewName.EndsWith(".cshtml"))
                    viewResult = viewEngine.GetView(partialViewName, partialViewName, false);
                else
                    viewResult = viewEngine.FindView(controller.ControllerContext, partialViewName, false);


                if (viewResult.Success == false)
                {
                    return $"A view with the name {partialViewName} could not be found";
                }

                ViewContext viewContext = new ViewContext(
                    controller.ControllerContext,
                    viewResult.View,
                    controller.ViewData,
                    controller.TempData,
                    writer,
                    new HtmlHelperOptions()
                );

                viewResult.View.RenderAsync(viewContext).Wait();

                return writer.GetStringBuilder().ToString();
            }
        }
    }
}
