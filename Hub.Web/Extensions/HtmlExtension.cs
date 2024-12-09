using Hub.Infrastructure.Autofac;
using Hub.Infrastructure.Exceptions;
using Hub.Infrastructure.Extensions.Generate;
using Hub.Infrastructure.Extensions;
using Hub.Infrastructure;
using MailKit.Search;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text;
using System.Xml.Linq;
using Hub.Infrastructure.Search;

namespace Hub.Web.Extensions
{
    public static class HtmlExtension
    {
        public static string ToHtmlString(this IHtmlContent content)
        {
            var writer = new System.IO.StringWriter();
            content.WriteTo(writer, HtmlEncoder.Default);
            return writer.ToString();
        }

        /// <summary>
        /// Helper para label de aplicações do tipo portal
        /// </summary>
        public static IHtmlContent MLabelFor<TModel, TValue>(this IHtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression, IDictionary<string, object> htmlAttributes = null, string labelTitle = "")
        {
            StringBuilder classe = new StringBuilder("control-label");

            var attributes = new Dictionary<string, object>();

            if (expression.IsRequired()) classe.Append(" required");

            attributes.Add("class", classe);

            if (htmlAttributes != null)
            {
                htmlAttributes.ToList().ForEach(dic =>
                {
                    if (attributes.ContainsKey(dic.Key))
                    {
                        attributes.Remove(dic.Key);
                    }
                });

                attributes = attributes.Union(htmlAttributes).ToDictionary(g => g.Key, g => g.Value);
            }

            if (!string.IsNullOrEmpty(labelTitle))
            {
                return helper.LabelFor(expression, labelTitle, attributes);
            }
            else
            {
                return helper.LabelFor(expression, attributes);
            }
        }

        /// <summary>
        /// Helper para label de aplicações do tipo portal
        /// </summary>
        public static IHtmlContent MLabel(this IHtmlHelper helper, string name, IDictionary<string, object> htmlAttributes = null)
        {
            StringBuilder classe = new StringBuilder("control-label");

            var attributes = new Dictionary<string, object>();

            attributes.Add("class", classe);

            if (htmlAttributes != null)
            {
                htmlAttributes.ToList().ForEach(dic =>
                {
                    if (attributes.ContainsKey(dic.Key))
                    {
                        attributes.Remove(dic.Key);
                    }
                });

                attributes = attributes.Union(htmlAttributes).ToDictionary(g => g.Key, g => g.Value);
            }

            return helper.Label("", name, attributes);
        }

        public static IHtmlContent MNumericBox(this IHtmlHelper helper, string name, IDictionary<string, object> htmlAttributes = null)
        {
            var attributes = new Dictionary<string, object>();

            attributes.Add("class", "form-control auto-numeric");

            if (htmlAttributes != null)
            {
                htmlAttributes.ToList().ForEach(dic =>
                {
                    if (attributes.ContainsKey(dic.Key))
                    {
                        attributes.Remove(dic.Key);
                    }
                });

                attributes = attributes.Union(htmlAttributes).ToDictionary(g => g.Key, g => g.Value);
            }

            if (htmlAttributes == null)
            {
                htmlAttributes = new Dictionary<string, object>();
            }

            return helper.MTextBox(name, attributes);
        }

        public static IHtmlContent MTextBox(this IHtmlHelper helper, string name, IDictionary<string, object> htmlAttributes = null)
        {
            var classe = new StringBuilder("form-control");

            var attributes = new Dictionary<string, object>();

            var toupper = Engine.AppSettings["User Upper Case?"];

            if (!string.IsNullOrEmpty(toupper))
            {
                if (Boolean.Parse(toupper))
                {
                    classe.Append(" toupper");
                }
            }

            attributes.Add("class", classe);

            attributes.Add("autocomplete", "off");

            if (htmlAttributes != null)
            {
                htmlAttributes.ToList().ForEach(dic =>
                {
                    if (attributes.ContainsKey(dic.Key))
                    {
                        attributes.Remove(dic.Key);
                    }
                });

                attributes = attributes.Union(htmlAttributes).ToDictionary(g => g.Key, g => g.Value);
            }

            return helper.TextBox(name, null, attributes);
        }

        public static Type GetObjectType<TModel, TValue>(Expression<Func<TModel, TValue>> expr)
        {
            if ((expr.Body.NodeType == ExpressionType.Convert) ||
                (expr.Body.NodeType == ExpressionType.ConvertChecked))
            {
                var unary = expr.Body as UnaryExpression;
                if (unary != null)
                    return unary.Operand.Type;
            }
            return expr.Body.Type;
        }

        /// <summary>
        /// Helper para textbox de aplicações do tipo portal
        /// </summary>
        public static IHtmlContent MTextBoxFor<TModel, TValue>(this IHtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression, IDictionary<string, object> htmlAttributes = null, bool? upperCase = true)
        {
            StringBuilder classe = new StringBuilder("form-control");

            var attributes = new Dictionary<string, object>();

            if (upperCase == true)
            {
                var toupper = Engine.AppSettings["User Upper Case?"];

                if (!string.IsNullOrEmpty(toupper))
                {
                    if (Boolean.Parse(toupper) && (htmlAttributes == null || htmlAttributes.ContainsKey("data-ignore-ucase") == false || (string)htmlAttributes["data-ignore-ucase"] == "false"))
                    {
                        classe.Append(" toupper");
                    }
                }
            }

            var returnType = GetObjectType(expression);

            if (returnType.IsNumericType())
            {
                classe.Append(" form-numeric");
            }

            attributes.Add("class", classe);

            attributes.Add("autocomplete", "off");

            if (htmlAttributes != null)
            {
                htmlAttributes.ToList().ForEach(dic =>
                {
                    if (attributes.ContainsKey(dic.Key))
                    {
                        attributes.Remove(dic.Key);
                    }
                });

                attributes = attributes.Union(htmlAttributes).ToDictionary(g => g.Key, g => g.Value);
            }

            return helper.TextBoxFor(expression, attributes);
        }

        /// <summary>
        /// Helper para password de aplicações do tipo portal
        /// </summary>
        public static IHtmlContent MPasswordFor<TModel, TValue>(this IHtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression, IDictionary<string, object> htmlAttributes = null)
        {
            StringBuilder classe = new StringBuilder("form-control col-md-12");

            var attributes = new Dictionary<string, object>();

            attributes.Add("class", classe);


            if (htmlAttributes != null)
            {
                htmlAttributes.ToList().ForEach(dic =>
                {
                    if (attributes.ContainsKey(dic.Key))
                    {
                        attributes.Remove(dic.Key);
                    }
                });

                attributes = attributes.Union(htmlAttributes).ToDictionary(g => g.Key, g => g.Value);
            }

            return helper.PasswordFor(expression, attributes);
        }

        /// <summary>
        /// Helper para textarea de aplicações do tipo portal
        /// </summary>
        public static IHtmlContent MTextArea(this IHtmlHelper helper, string name, IDictionary<string, object> htmlAttributes = null)
        {
            StringBuilder classe = new StringBuilder("form-control");

            var attributes = new Dictionary<string, object>();

            var toupper = Engine.AppSettings["User Upper Case?"];

            if (!string.IsNullOrEmpty(toupper))
            {
                if (Boolean.Parse(toupper))
                {
                    classe.Append(" toupper");
                }
            }

            attributes.Add("class", classe);

            if (htmlAttributes != null)
            {
                htmlAttributes.ToList().ForEach(dic =>
                {
                    if (attributes.ContainsKey(dic.Key))
                    {
                        attributes.Remove(dic.Key);
                    }
                });

                attributes = attributes.Union(htmlAttributes).ToDictionary(g => g.Key, g => g.Value);
            }

            return helper.TextArea(name, null, attributes);
        }


        /// <summary>
        /// Helper para textarea de aplicações do tipo portal
        /// </summary>
        public static IHtmlContent MTextAreaFor<TModel, TValue>(this IHtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression, IDictionary<string, object> htmlAttributes = null)
        {
            StringBuilder classe = new StringBuilder("form-control");

            var attributes = new Dictionary<string, object>();

            var toupper = Engine.AppSettings["User Upper Case?"];

            if (!string.IsNullOrEmpty(toupper))
            {
                if (Boolean.Parse(toupper) && (htmlAttributes == null || htmlAttributes.ContainsKey("data-ignore-ucase") == false || (string)htmlAttributes["data-ignore-ucase"] == "false"))
                {
                    classe.Append(" toupper");
                }
            }

            attributes.Add("class", classe);

            if (htmlAttributes != null)
            {
                htmlAttributes.ToList().ForEach(dic =>
                {
                    if (attributes.ContainsKey(dic.Key))
                    {
                        attributes.Remove(dic.Key);
                    }
                });

                attributes = attributes.Union(htmlAttributes).ToDictionary(g => g.Key, g => g.Value);
            }

            return helper.TextAreaFor(expression, attributes);
        }


        /// <summary>
        /// Helper para textarea do tipo ckeditor de aplicações do tipo portal
        /// </summary>
        public static IHtmlContent MCKEditorFor<TModel, TValue>(this IHtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression, IDictionary<string, object> htmlAttributes = null)
        {
            StringBuilder classe = new StringBuilder("ckeditor");

            var attributes = new Dictionary<string, object>();

            attributes.Add("class", classe);

            if (htmlAttributes != null)
            {
                htmlAttributes.ToList().ForEach(dic =>
                {
                    if (attributes.ContainsKey(dic.Key))
                    {
                        attributes.Remove(dic.Key);
                    }
                });

                attributes = attributes.Union(htmlAttributes).ToDictionary(g => g.Key, g => g.Value);
            }

            return helper.TextAreaFor(expression, attributes);
        }
        /// <summary>
        /// Helper para datepickers de aplicações do tipo portal
        /// </summary>
        public static IHtmlContent MDatePicker(this IHtmlHelper helper, string name, string format = "{0:dd/MM/yyyy}")
        {
            StringBuilder html = new StringBuilder("<div class=\"input-icon right date date-picker\" data-date-format=\"dd/mm/yyyy\" data-date-viewmode=\"years\">");

            html.Append("<span class=\"add-on\"></span>");

            html.Append("<i class=\"icon-calendar\"></i>");

            html.Append(helper.TextBox(name, null, format, new { @class = "form-control date date-picker", autocomplete = "off" }).ToHtmlString());

            html.Append("</div>");

            return new HtmlString(html.ToString());
        }

        /// <summary>
        /// Helper para datepickers de aplicações do tipo portal
        /// Obs.: somente um dentre os paramêtros (futureOnly, pastOnly ou futureWithOutCurrentDate) deve possuir valor igual a true
        /// para que sejam aplicados corretamente os bloqueios na datas conforme necessidade.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="helper"></param>
        /// <param name="expression"></param>
        /// <param name="format">Permite informar em qual formato a data deve ser apresentada</param>
        /// <param name="futureOnly">Booleano para permitir que selecionem apenas a data atual ou futuras</param>
        /// <param name="pastOnly">Booleano para permitir que selecionem apenas a data atual e as datas anteriores à data de execução da aplicação</param>
        /// <param name="futureWithOutCurrentDate">Booleano para permitir que selecionem apenas datas posteriores à data de execução da aplicação</param>
        /// <returns></returns>
        public static IHtmlContent MDatePickerFor<TModel, TValue>(this IHtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression, string format = "{0:dd/MM/yyyy}", bool futureOnly = false, bool pastOnly = false, bool futureWithOutCurrentDate = false)
        {
            StringBuilder html = new StringBuilder("<div class=\"input-icon right date date-picker"
                                                    + (futureOnly ? " future-only" : pastOnly ? " past-only" : futureWithOutCurrentDate ? " future-without-current-date" : "")
                                                    + "\" data-date-format=\"dd/mm/yyyy\" data-date-viewmode=\"years\">");

            html.Append("<span class=\"add-on\"></span>");

            html.Append("<i class=\"icon-calendar\"></i>");

            html.Append(helper.TextBoxFor(expression, format, new { @class = "form-control date date-picker", autocomplete = "off" }).ToHtmlString());

            html.Append("</div>");

            return new HtmlString(html.ToString());
        }

        /// <summary>
        /// Helper para monthpickers de aplicações do tipo portal
        /// </summary>
        public static IHtmlContent MMonthPicker(this IHtmlHelper helper, string name, bool futureOnly = false, bool pastOnly = false)
        {
            StringBuilder html = new StringBuilder("<div class=\"input-icon right date date-month-picker" + (futureOnly ? " future-only" : "") + (pastOnly ? " past-only" : "") + "\" data-date-format=\"mm/yyyy\" data-date-viewmode=\"months\">");

            html.Append("<span class=\"add-on\"></span>");

            html.Append("<i class=\"icon-calendar\"></i>");

            html.Append(helper.TextBox(name, "", new { @class = "form-control date date-month-picker", autocomplete = "off" }).ToHtmlString());

            html.Append("</div>");

            return new HtmlString(html.ToString());
        }


        /// <summary>
        /// Helper para monthpickers de aplicações do tipo portal
        /// </summary>
        public static IHtmlContent MMonthPickerFor<TModel, TValue>(this IHtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression, bool futureOnly = false, bool pastOnly = false)
        {
            StringBuilder html = new StringBuilder("<div class=\"input-icon right date date-month-picker" + (futureOnly ? " future-only" : "") + (pastOnly ? " past-only" : "") + "\" data-date-format=\"mm/yyyy\" data-date-viewmode=\"months\">");

            html.Append("<span class=\"add-on\"></span>");

            html.Append("<i class=\"icon-calendar\"></i>");

            html.Append(helper.TextBoxFor(expression, "{0:MM/yyyy}", new { @class = "form-control date date-month-picker", autocomplete = "off" }).ToHtmlString());

            html.Append("</div>");

            return new HtmlString(html.ToString());
        }

        /// <summary>
        /// Helper para datetimepickers de aplicações do tipo portal
        /// </summary>
        public static IHtmlContent MDateTimePickerFor<TModel, TValue>(this IHtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression, string format = "{0:dd/MM/yyyy HH:mm}")
        {
            StringBuilder html = new StringBuilder("<div class=\"input-icon right\">");

            html.Append("<span class=\"add-on\"></span>");

            html.Append("<i class=\"icon-clock\"></i>");

            html.Append(helper.TextBoxFor(expression, format, new { @class = "form-control date datetime-picker", autocomplete = "off" }).ToHtmlString());

            html.Append("</div>");

            return new HtmlString(html.ToString());
        }

        /// <summary>
        /// Helper para datetimepickers de aplicações do tipo portal
        /// </summary>
        public static IHtmlContent MTimePickerFor<TModel, TValue>(this IHtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression)
        {
            StringBuilder html = new StringBuilder("<div class=\"input-icon right\">");

            html.Append("<span class=\"add-on\"></span>");

            html.Append("<i class=\"icon-clock\"></i>");

            html.Append(helper.TextBoxFor(expression, new { @class = "form-control time-picker", autocomplete = "off" }).ToHtmlString());

            html.Append("</div>");

            return new HtmlString(html.ToString());
        }

        /// <summary>
        /// Helper para checkbox de aplicações do tipo portal
        /// </summary>
        public static IHtmlContent MCheckBox(this IHtmlHelper helper, string name, bool isChecked = false, IDictionary<string, object> htmlAttributes = null)
        {
            StringBuilder html = new StringBuilder();

            var attributes = new Dictionary<string, object>();

            StringBuilder classe = new StringBuilder("switch");

            attributes.Add("class", classe);

            if (isChecked)
            {
                attributes.Add("checked", isChecked);
            }

            if (htmlAttributes != null)
            {
                htmlAttributes.ToList().ForEach(dic =>
                {
                    if (attributes.ContainsKey(dic.Key))
                    {
                        attributes.Remove(dic.Key);
                    }
                });

                attributes = attributes.Union(htmlAttributes).ToDictionary(g => g.Key, g => g.Value);
            }

            html.Append(helper.CheckBox(name, attributes).ToHtmlString());

            return new HtmlString(html.ToString());
        }

        public static IHtmlContent MLegendIcon(this IHtmlHelper helper, string legendText, string placement = "top")
        {
            StringBuilder html = new StringBuilder($"<span style=\"white-space: normal\"><i class=\"fa fa-info-circle legend-tooltip\" data-html=\"true\" data-placement=\"{placement.Replace("\"", "")}\" data-original-title=\"{legendText.Replace("\"", "")}\"></i></span>");

            return new HtmlString(html.ToString());
        }

        /// <summary>
        /// Helper para checkbox de aplicações do tipo portal
        /// </summary>
        public static IHtmlContent MCheckBoxFor<TModel>(this IHtmlHelper<TModel> helper, Expression<Func<TModel, bool>> expression)
        {
            StringBuilder html = new StringBuilder(helper.CheckBoxFor(expression, new { @class = "switch" }).ToHtmlString());

            return new HtmlString(html.ToString());
        }

        /// <summary>
        /// Helper para buscadores de aplicações do tipo portal
        /// </summary>
        public static IHtmlContent MSearch(this IHtmlHelper helper, string fieldName, string searchName, string extraCondition = null, string referenceNameInput = null, bool registration = true, IDictionary<string, object> htmlAttributes = null)
        {
            var item = FindInSearchCollection(searchName);

            var classes = "form-control col-md-12 Voeit-search";

            var attributes = new Dictionary<string, object>();

            attributes.Add("searchName", searchName);
            attributes.Add("extraCondition", extraCondition);
            attributes.Add("referenceNameInput", referenceNameInput);

            var noFreeSearch = item.GetType().GetCustomAttributes(typeof(NoFreeSearchAttribute)).Count() > 0;

            if (noFreeSearch)
            {
                classes += " noFreeSearch";
            }

            if (item is ISearchItemRegistration && registration == true)
            {
                classes += " Voeit-search-register";

                attributes.Add("registerName", (item as ISearchItemRegistration).RegistrationName);
                attributes.Add("registerAddress", (item as ISearchItemRegistration).RegistrationAddress);
            }

            attributes.Add("class", classes);

            if (htmlAttributes != null)
            {
                htmlAttributes.ToList().ForEach(dic =>
                {
                    if (attributes.ContainsKey(dic.Key))
                    {
                        attributes.Remove(dic.Key);
                    }
                });

                attributes = attributes.Union(htmlAttributes).ToDictionary(g => g.Key, g => g.Value);
            }

            return helper.Hidden(fieldName, null, attributes);
        }

        /// <summary>
        /// Helper para buscadores de aplicações do tipo portal
        /// </summary>
        public static IHtmlContent MSearchFor<TModel, TValue>(this IHtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression, string searchName, string extraCondition = null, string referenceNameInput = null)
        {
            var item = FindInSearchCollection(searchName);

            var classes = "form-control col-md-12 Voeit-search";

            var attributes = new Dictionary<string, object>();

            attributes.Add("searchName", searchName);
            attributes.Add("extraCondition", extraCondition);
            attributes.Add("referenceNameInput", referenceNameInput);

            var noFreeSearch = item.GetType().GetCustomAttributes(typeof(NoFreeSearchAttribute)).Count() > 0;

            if (noFreeSearch)
            {
                classes += " noFreeSearch";
            }

            if (item is ISearchItemRegistration)
            {
                classes += " Voeit-search-register";

                attributes.Add("registerName", (item as ISearchItemRegistration).RegistrationName);
                attributes.Add("registerAddress", (item as ISearchItemRegistration).RegistrationAddress);
            }

            attributes.Add("class", classes);

            return helper.HiddenFor(expression, attributes);
        }

        /// <summary>
        /// Helper para combobox com valores estáticos
        /// </summary>
        public static IHtmlContent MCombo(this IHtmlHelper helper, string name, IEnumerable<SelectListItem> values, bool multiple = false, IDictionary<string, object> htmlAttributes = null)
        {
            StringBuilder classe = new StringBuilder("form-control col-md-12 voeit-combobox");

            var attributes = new Dictionary<string, object>();

            attributes.Add("class", classe);

            if (htmlAttributes != null)
            {
                htmlAttributes.ToList().ForEach(dic =>
                {
                    if (attributes.ContainsKey(dic.Key))
                    {
                        attributes.Remove(dic.Key);
                    }
                });

                attributes = attributes.Union(htmlAttributes).ToDictionary(g => g.Key, g => g.Value);
            }

            if (multiple)
            {
                attributes.Add("multiple", "");

                return helper.ListBox(name, values, attributes);
            }
            else
            {
                return helper.DropDownList(name, values, attributes);
            }
        }

        public static IHtmlContent MEnumComboFor<TModel, TValue>(this IHtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression, string optionLabel = null)
            where TModel : struct
        {
            StringBuilder classe = new StringBuilder("form-control col-md-12 voeit-combobox");

            var attributes = new Dictionary<string, object>();

            attributes.Add("class", classe);

            if (!string.IsNullOrEmpty(optionLabel))
            {
                return helper.DropDownListFor(expression, helper.GetEnumSelectList<TModel>(), optionLabel, attributes);
            }
            else
            {
                return helper.DropDownListFor(expression, helper.GetEnumSelectList<TModel>(), attributes);
            }
        }


        /// <summary>
        /// Helper para combobox com valores estáticos
        /// </summary>
        public static IHtmlContent MComboFor<TModel, TValue>(this IHtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression, IEnumerable<SelectListItem> values, bool multiple = false, string optionLabel = null, IDictionary<string, object> htmlAttributes = null)
        {
            StringBuilder classe = new StringBuilder("form-control col-md-12 voeit-combobox");

            var attributes = new Dictionary<string, object>();

            attributes.Add("class", classe);

            if (htmlAttributes != null)
            {
                htmlAttributes.ToList().ForEach(dic =>
                {
                    if (attributes.ContainsKey(dic.Key))
                    {
                        attributes.Remove(dic.Key);
                    }
                });

                attributes = attributes.Union(htmlAttributes).ToDictionary(g => g.Key, g => g.Value);
            }

            if (multiple)
            {
                attributes.Add("multiple", "");

                return helper.ListBoxFor(expression, values, attributes);
            }
            else if (!string.IsNullOrEmpty(optionLabel))
            {
                return helper.DropDownListFor(expression, values, optionLabel, attributes);
            }
            else
            {
                return helper.DropDownListFor(expression, values, attributes);
            }
        }

        public static IHtmlContent MComboFor<TModel, TValue>(this IHtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression, string searchName, bool multiple = false, string extraCondition = null)
        {
            var item = FindInSearchCollection(searchName);

            var list = item.Get("", int.MaxValue, 1, extraCondition).Cast<SearchResult>().Select(i => new SelectListItem() { Text = i.text, Value = i.id.ToString() }).ToList();

            list.Insert(0, new SelectListItem());

            return MComboFor(helper, expression, list, multiple);
        }

        private static ISearchItem FindInSearchCollection(string searchName)
        {
            ISearchItem item = Singleton<SearchCollection>.Instance.Itens.FirstOrDefault(i => i.SearchName == searchName);

            if (item == null) throw new BusinessException(Engine.Get("SearchRuleNotImplemented", searchName));

            return item;
        }

        public static IHtmlContent MFileUploadFor<TModel, TValue>(this IHtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression, string url, string customFileUploadDoneEvent = null, IDictionary<string, object> htmlAttributes = null, string allowedFileTypes = null, bool allowMultiple = false)
        {
            string caseInsensitiveFileTypes = string.Join(",", allowedFileTypes?.ToLower(), allowedFileTypes?.ToUpper());
            StringBuilder html = new StringBuilder();

            html.Append("<div class=\"fileupload\" data-url=\"" + url + "\" " + (customFileUploadDoneEvent != null ? "data-fileuploaddone=\"" + customFileUploadDoneEvent + "\"" : ""));
            html.Append((allowedFileTypes != null ? "data-allowedtypes=\"" + caseInsensitiveFileTypes + "\"" : "") + ">");

            html.Append(helper.HiddenFor(expression, htmlAttributes).ToHtmlString());

            html.Append("<div class=\"row form-group\">");
            html.Append("   <div class=\"col-md-12\">");
            html.Append("       <span class=\"btn btn-block green fileinput-button\">");
            html.Append("       <i class=\"fa fa-upload\"></i>");
            html.Append("       <span>");
            html.Append(Engine.Get("Selecione..."));
            html.Append("       </span>");
            html.Append("       <input type=\"file\" name=\"files\" " + (allowMultiple ? "multiple=\"\"" : "") + "/>");
            html.Append("       </span>");
            html.Append("   </div>");
            html.Append("</div>");

            html.Append("<div class=\"row form-group\">");
            html.Append("<div class=\"col-md-12\">");
            html.Append("   <button type=\"button\" class=\"btn btn-block red cancel\" style=\"display: none\">");
            html.Append("      <i class=\"fa fa-ban\"></i>");
            html.Append("      <span>");
            html.Append(Engine.Get("Cancelar Upload"));
            html.Append("      </span>");
            html.Append("   </button>");
            html.Append("</div>");
            html.Append("</div>");

            html.Append("<div class=\"row form-group\">");
            html.Append("<div class=\"col-md-12 fileupload-progress\" style=\"display: none\">");
            html.Append("   <div class=\"progress progress-striped active\" role=\"progressbar\" aria-valuemin=\"0\" aria-valuemax=\"100\">");
            html.Append("       <div class=\"progress-bar progress-bar-success\" style=\"width:0%;\"></div>");
            html.Append("   </div>");
            html.Append("   <div class=\"progress-extended\">&nbsp;</div>");
            html.Append("</div>");
            html.Append("</div>");
            html.Append("</div>");

            return new HtmlString(html.ToString());
        }

        /// <summary>
        /// Cria um validation summary customizado para as aplicações que usam o modelo-portal.
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="excludePropertyErrors"></param>
        /// <returns></returns>
        public static IHtmlContent MValidationSummary(this IHtmlHelper helper, bool excludePropertyErrors)
        {
            var element = helper.ValidationSummary(excludePropertyErrors, (string)null, new { @class = "note note-warning" });

            var htmlString = element.ToHtmlString();

            if (!string.IsNullOrEmpty(htmlString))
            {
                XElement xEl = XElement.Parse(htmlString);

                var lis = xEl.Element("ul").Elements("li");

                if (lis.Count() == 1 && lis.First().Value == "")
                    return null;
            }

            return element;
        }

        public static IHtmlContent MMultiSelect(this IHtmlHelper helper, string name, string dataSourceUrl, bool startWithSelectedOnly = false)
        {
            var id = $"{name}{Engine.Resolve<IRandomGeneration>().Generate()}";

            return helper.Hidden(name, new { @id = id, @Name = name, @class = "evup-multiSelect", data_Source_Url = dataSourceUrl, data_start_selected_only = startWithSelectedOnly });
        }

        /// <summary>
        /// Helper para criação de campo para seleção de domínios virtuais
        /// </summary>
        public static IHtmlContent MMultiSelectFor<TModel, TValue>(this IHtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression, string dataSourceUrl, bool startWithSelectedOnly = false)
        {
            var name = ((expression.Body as MemberExpression).Member as PropertyInfo).Name;

            var id = $"{name}{Engine.Resolve<IRandomGeneration>().Generate()}";

            return helper.HiddenFor(expression, new { @id = id, @Name = name, @class = "evup-multiSelect", data_Source_Url = dataSourceUrl, data_start_selected_only = startWithSelectedOnly });
        }

        #region KENDO 

        ///// <summary>
        ///// Extensão da Column da KendoGrid para campos do tipo boolean
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="builder"></param>
        ///// <returns></returns>
        //public static GridBoundColumnBuilder<T> MBooleanColumn<T>(this GridBoundColumnBuilder<T> builder, bool forTemplate = false) where T : class
        //{
        //    var comment = forTemplate ? "\\" : "";

        //    return builder
        //        .ClientGroupHeaderTemplate(Engine.Get(builder.Column.Member) + ": " + comment + "#= (value ? '" + Engine.Get("Yes") + "' : '" + Engine.Get("No") + "')" + comment + "#")
        //        .ClientTemplate("<i " + comment + "#= " + builder.Column.Member + " ? 'class=" + comment + "\\\"fa fa-check-square-o" + comment + "\\\"' : 'class=" + comment + "\\\"fa fa-square-o" + comment + "\\\"' " + comment + "# ></i>");
        //}

        ///// <summary>
        ///// Extensão da Column da KendoGrid para campos que devem ser formatados como moeda
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="builder"></param>
        ///// <returns></returns>
        //public static GridBoundColumnBuilder<T> MCurrency<T>(this GridBoundColumnBuilder<T> builder, bool forTemplate = false, bool stylizeSign = false) where T : class
        //{
        //    var comment = forTemplate ? "\\" : "";

        //    var template = new StringBuilder();
        //    if (stylizeSign)
        //    {
        //        template.Append(comment + "# if (" + builder.Column.Member + " != null && " + builder.Column.Member + " < 0) {");
        //        template.Append(comment + "# <span class='negative-sign'>" + comment + "#= " + builder.Column.Member + " != null ? kendo.toString(" + builder.Column.Member + ", 'c2') : ' ' " + comment + "#</span>");
        //        template.Append(comment + "# } else if (" + builder.Column.Member + " != null && " + builder.Column.Member + " > 0) {");
        //        template.Append(comment + "# <span class='positive-sign'>" + comment + "#= " + builder.Column.Member + " != null ? kendo.toString(" + builder.Column.Member + ", 'c2') : ' ' " + comment + "#</span>");
        //        template.Append(comment + "# } else { ");
        //        template.Append(comment + "# " + comment + "#= " + builder.Column.Member + " != null ? kendo.toString(" + builder.Column.Member + ", 'c2') : ' ' " + comment + "# " + comment + "# } " + comment + "#");
        //        return builder.ClientTemplate(template.ToString());
        //    }
        //    else
        //    {
        //        return builder.ClientTemplate(comment + "#= " + builder.Column.Member + " != null ? kendo.toString(" + builder.Column.Member + ", 'c2') : ' ' " + comment + "#");
        //    }
        //}

        ///// <summary>
        ///// Extensão da Column da KendoGrid para campos que devem ser formatados como moeda
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="builder"></param>
        ///// <returns></returns>
        //public static GridBoundColumnBuilder<T> MDecimal<T>(this GridBoundColumnBuilder<T> builder, bool forTemplate = false, bool stylizeSign = false) where T : class
        //{
        //    var comment = forTemplate ? "\\" : "";

        //    var template = new StringBuilder();
        //    if (stylizeSign)
        //    {
        //        template.Append(comment + "# if (" + builder.Column.Member + " != null && " + builder.Column.Member + " < 0) {");
        //        template.Append(comment + "# <span class='negative-sign'>" + comment + "#= " + builder.Column.Member + " != null ? kendo.toString(" + builder.Column.Member + ", 'N2') : ' ' " + comment + "#</span>");
        //        template.Append(comment + "# } else if (" + builder.Column.Member + " != null && " + builder.Column.Member + " > 0) {");
        //        template.Append(comment + "# <span class='positive-sign'>" + comment + "#= " + builder.Column.Member + " != null ? kendo.toString(" + builder.Column.Member + ", 'N2') : ' ' " + comment + "#</span>");
        //        template.Append(comment + "# } else { ");
        //        template.Append(comment + "# " + comment + "#= " + builder.Column.Member + " != null ? kendo.toString(" + builder.Column.Member + ", 'N2') : ' ' " + comment + "# " + comment + "# } " + comment + "#");
        //        return builder.ClientTemplate(template.ToString());
        //    }
        //    else
        //    {
        //        return builder.ClientTemplate(comment + "#= " + builder.Column.Member + " != null ? kendo.toString(" + builder.Column.Member + ", 'N2') : ' ' " + comment + "#");
        //    }
        //}

        ///// <summary>
        ///// Extensão da Column da KendoGrid para campos do tipo Date
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="builder"></param>
        ///// <returns></returns>
        //public static GridBoundColumnBuilder<T> MDateFormat<T>(this GridBoundColumnBuilder<T> builder, bool forTemplate = false, string format = "DD/MM/YYYY", bool displayInServerTimezone = true, bool isUtcDate = false) where T : class
        //{
        //    var comment = forTemplate ? "\\" : "";

        //    var ret = new StringBuilder();

        //    ret.Append(comment);
        //    ret.Append("#= ");
        //    ret.Append(builder.Column.Member);
        //    ret.Append($" != null ? moment.tz(");

        //    ret.Append(builder.Column.Member);



        //    if (isUtcDate)
        //    {
        //        ret.Append(", 'UTC').tz(");
        //    }
        //    else
        //    {
        //        ret.Append(", ");
        //    }

        //    if (displayInServerTimezone)
        //    {
        //        ret.Append("serverTimezone");
        //    }
        //    else
        //    {
        //        ret.Append("localTimezone");
        //    }

        //    ret.Append(").format('");

        //    ret.Append(format);

        //    ret.Append("') : ' '");
        //    ret.Append(comment);
        //    ret.Append("#");

        //    format = format.Replace("DD", "dd").Replace("YYYY", "yyyy");


        //    return builder

        //        .ClientTemplate(ret.ToString()).Filterable(cnf => cnf.UI(Kendo.Mvc.UI.GridFilterUIRole.DatePicker))
        //        .ClientGroupHeaderTemplate(Engine.Get(builder.Column.Member) + ": " + comment + $"#= kendo.toString(kendo.parseDate(value), '{format}')" + comment + "#");
        //}

        //public static GridBoundColumnBuilder<T> MFilter<T>(this GridBoundColumnBuilder<T> builder) where T : class
        //{
        //    return builder.Filterable(f => f.Extra(false).Operators(o =>
        //    o.ForString(c =>
        //        c.Clear()
        //        .StartsWith(Engine.Get("StartsWith"))
        //        .Contains(Engine.Get("Contains"))
        //        .IsEqualTo(Engine.Get("IsEqualTo"))
        //        .IsNotEqualTo(Engine.Get("IsNotEqualTo"))
        //        .IsEmpty(Engine.Get("IsEmpty"))
        //        .IsNull(Engine.Get("IsNull"))
        //        .IsNotNull(Engine.Get("IsNotNull"))
        //        )));
        //}

        ///// <summary>
        ///// Extensão da Column da KendoGrid para campos do tipo datetime
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="builder"></param>
        ///// <returns></returns> 
        //public static GridBoundColumnBuilder<T> MDateTimeFormat<T>(this GridBoundColumnBuilder<T> builder, bool forTemplate = false, bool displayInServerTimezone = false, bool isUtcDate = false) where T : class
        //{
        //    return MDateFormat(builder, forTemplate, "DD/MM/YYYY HH:mm", displayInServerTimezone, isUtcDate);
        //}

        ///// <summary>
        ///// Extensão da Column da KendoGrid para campos do tipo Time
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="builder"></param>
        ///// <returns></returns> 
        //public static GridBoundColumnBuilder<T> MTimeFormat<T>(this GridBoundColumnBuilder<T> builder, bool forTemplate = false, bool displayInServerTimezone = false, bool isUtcDate = false) where T : class
        //{
        //    return MDateFormat(builder, forTemplate, "HH:mm", displayInServerTimezone, isUtcDate);
        //}

        ///// <summary>
        ///// Extensão da Column da KendoGrid para campos do Date
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="builder"></param>
        ///// <returns></returns> 
        //public static GridBoundColumnBuilder<T> MMonthFormat<T>(this GridBoundColumnBuilder<T> builder, bool forTemplate = false, bool displayInServerTimezone = false, bool isUtcDate = false) where T : class
        //{
        //    return MDateFormat(builder, forTemplate, "MM/YYYY", displayInServerTimezone, isUtcDate);
        //} 

        #endregion

        /// <summary>
        /// Helper para criar uma div-container de controles de aplicações do tipo portal
        /// </summary>
        public static IDisposable MControlGroup<TModel, TValue>(this IHtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression, bool createLabel = true, string extra_class = "")
        {
            return new MControlGroup<TModel, TValue>(helper, expression, createLabel, extra_class);
        }

        /// <summary>
        /// Helper para criar uma div-container de controles de aplicações do tipo portal
        /// </summary>
        public static IDisposable MControlGroup(this IHtmlHelper helper, string name, bool createLabel = true, string extra_class = "")
        {
            return new MControlGroup(helper, name, createLabel, extra_class);
        }

        /// <summary>
        /// retorna o htmlstring utilizado para criação do cabeçalho de um controle de grupo
        /// </summary>
        public static IHtmlContent MControlGroupHeader<TModel, TValue>(this IHtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression, bool createLabel = true, string labelTitle = "", string extra_class = "")
        {
            var name = helper.NameFor(expression).ToString();

            var errorsCount = helper.ViewData.ModelState[name]?.Errors?.Count ?? 0;

            StringBuilder html = new StringBuilder("<div class=\"form-group " + extra_class + (errorsCount == 0 ? null : " has-error") + "\">");

            if (createLabel)
            {
                if (string.IsNullOrEmpty(labelTitle))
                {
                    html.Append(helper.MLabelFor(expression).ToHtmlString());
                }
                else
                {
                    html.Append(helper.MLabelFor(expression, null, labelTitle).ToHtmlString());
                }
            }

            return new HtmlString(html.ToString());
        }

        public static IHtmlContent MControlGroupHeader(this IHtmlHelper helper, string name, bool createLabel = true, string labelTitle = "", string extra_class = "")
        {
            StringBuilder html = new StringBuilder("<div class=\"form-group " + extra_class + "\">");

            if (createLabel)
            {
                if (string.IsNullOrEmpty(labelTitle))
                    html.Append(helper.MLabel(name).ToHtmlString());
            }

            return new HtmlString(html.ToString());
        }

        /// <summary>
        /// retorna o htmlstring utilizado para criação do rodapé de um controle de grupo
        /// </summary>
        public static IHtmlContent MControlGroupFooter<TModel, TValue>(this IHtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression)
        {
            StringBuilder html = new StringBuilder(helper.ValidationMessageFor(expression, null, new { @class = "help-block" }).ToHtmlString());

            html.Append("</div>");

            return new HtmlString(html.ToString());
        }

        /// <summary>
        /// retorna o htmlstring utilizado para criação do rodapé de um controle de grupo
        /// </summary>
        public static IHtmlContent MControlGroupFooter(this IHtmlHelper helper)
        {
            return new HtmlString("</div>");
        }

        /// <summary>
        /// Helper para radiobuttons de aplicações do tipo portal
        /// </summary>
        public static IHtmlContent MRadioButtonFor<TModel, TValue>(this IHtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression, object value)
        {
            StringBuilder html = new StringBuilder();

            html.Append(helper.RadioButtonFor(expression, value).ToHtmlString());

            return new HtmlString(html.ToString());
        }

        public static IHtmlContent MSearchTree(this IHtmlHelper helper, string treeName, string placeholder)
        {
            StringBuilder builder = new StringBuilder();

            var div = string.Format("<div class='box-col1 evup-searchTree'data-treeName='{0}'>", treeName);

            builder.Append(div);
            builder.Append("<h4>");
            builder.Append(Engine.Get("SearchTreeFind"));
            builder.Append("</h4>");
            builder.Append("<div>");
            builder.Append("<input id='searchTreefilterText' class='form-control uppercase' placeholder='");
            builder.Append(placeholder);
            builder.Append("'/>");
            builder.Append("</div>");
            builder.Append("</div>");
            builder.Append("<br/>");
            builder.Append("<div>");
            builder.Append("<a href='#' id='searchTreeOnlySelected'>");
            builder.Append(Engine.Get("SelectOnlyChecked"));
            builder.Append("</a> | <a href='#' id='searchTreeAll'>");
            builder.Append(Engine.Get("ShowAll"));
            builder.Append("</a>");
            builder.Append("</div>");

            return new HtmlString(builder.ToString());
        }

        #region KENDO 

        /// <summary>
        /// Helper para Telefone 
        /// </summary>
        //public static GridBoundColumnBuilder<T> MPhone<T>(this GridBoundColumnBuilder<T> builder, bool forTemplate = false, string preferencePhone = null, string preferenceWhatsapp = null,
        //    bool allowInstantMessaging = false, bool validate = false) where T : class
        //{
        //    var comment = forTemplate ? "\\" : "";

        //    var template = new StringBuilder();

        //    template.Append("# if((" + builder.Column.Member + " ?? '') != '') {#");

        //    if (validate)
        //    {
        //        template.Append("<div class='hide-phone-column' style='width: 150px;' data-validate='true' data-phone=" + comment + "#=" + builder.Column.Member + " != null ? kendo.toString(" + builder.Column.Member + ") : '' " + comment + "#" + ">");
        //    }
        //    else
        //    {
        //        template.Append("<div class='hide-phone-column' style='width: 150px;' data-phone=" + comment + "#=" + builder.Column.Member + " != null ? kendo.toString(" + builder.Column.Member + ") : '' " + comment + "#" + ">");
        //    }

        //    if (preferencePhone != null)
        //    {
        //        template.Append("<div style='display: none;'>" + comment + "#= kendo.toString(" + preferencePhone + ") #</div>");
        //    }

        //    if (preferenceWhatsapp != null)
        //    {
        //        template.Append("<div style='display: none;'>" + comment + "#= kendo.toString(" + preferenceWhatsapp + ") #</div>");
        //    }

        //    template.Append("<span class='hide-phone-column-span' style='display: none'></span>");

        //    if (allowInstantMessaging)
        //    {
        //        template.Append("<a class='btn-phone-column-whatsapp btn btn-xs green' style='display: none'><i class='fa fa-envelope-o'></i><span></span></a>");
        //    }

        //    template.Append("<a class='btn-phone-column-hide btn btn-xs' style='display: none'><i class='fa fa-eye-slash'></i><span></span></a>");
        //    template.Append("<a class='btn-phone-column-show btn btn-xs'><i class='fa fa-eye'></i><span>" + Engine.Get("View") + "</span></a>");
        //    template.Append("</div>");
        //    template.Append("#} else {# <span></span> #}#");

        //    return builder.ClientTemplate(template.ToString());
        //}

        /// <summary>
        /// Helper para Email 
        /// </summary>
        //public static GridBoundColumnBuilder<T> MEmail<T>(this GridBoundColumnBuilder<T> builder, bool forTemplate = false, string preferenceEmail = null, bool validate = false) where T : class
        //{
        //    var comment = forTemplate ? "\\" : "";

        //    var template = new StringBuilder();

        //    template.Append("# if((" + builder.Column.Member + " ?? '') != '') {#");

        //    if (validate)
        //    {
        //        template.Append("<div class='hide-email-column' style='width:400px;' data-validate='true' data-email=" + comment + "#= " + builder.Column.Member + " != null ? kendo.toString(" + builder.Column.Member + ") : '' " + comment + "#" + ">");
        //    }
        //    else
        //    {
        //        template.Append("<div class='hide-email-column' style='width:400px;' data-email=" + comment + "#= " + builder.Column.Member + " != null ? kendo.toString(" + builder.Column.Member + ") : '' " + comment + "#" + ">");
        //    }

        //    if (preferenceEmail != null)
        //    {
        //        template.Append("<div style='display: none;'>" + comment + "#= kendo.toString(" + preferenceEmail + ") #</div>");
        //    }

        //    template.Append("<span class='hide-email-column-span' style='display: none'></span>");
        //    template.Append("<a class='btn-email-column-hide btn btn-xs' style='display: none'><i class='fa fa-eye-slash'></i><span></span></a>");
        //    template.Append("<a class='btn-email-column-show btn btn-xs'><i class='fa fa-eye'></i><span>" + Engine.Get("View") + "</span></a>");
        //    template.Append("</div>");
        //    template.Append("#} else {# <span></span> #}#");

        //    return builder.ClientTemplate(template.ToString());
        //} 

        #endregion
    }

    public class MControlGroup<TModel, TValue> : IDisposable
    {
        private readonly IHtmlHelper<TModel> _helper;
        private readonly Expression<Func<TModel, TValue>> _expression;

        public MControlGroup(IHtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression, bool createLabel = true, string extra_class = "")
        {
            _helper = helper;

            _expression = expression;

            _helper.ViewContext.Writer.WriteLine(_helper.MControlGroupHeader(expression, createLabel, "", extra_class));
        }

        public void Dispose()
        {
            _helper.ViewContext.Writer.WriteLine(_helper.MControlGroupFooter(_expression));
        }
    }

    public class MControlGroup : IDisposable
    {
        private readonly IHtmlHelper _helper;

        public MControlGroup(IHtmlHelper helper, string name, bool createLabel = true, string extra_class = "")
        {
            _helper = helper;

            _helper.ViewContext.Writer.WriteLine(_helper.MControlGroupHeader(name, createLabel, "", extra_class));
        }

        public void Dispose()
        {
            _helper.ViewContext.Writer.WriteLine(_helper.MControlGroupFooter());
        }
    }
}
