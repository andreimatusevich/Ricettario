using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Ricettario.Controllers.Abstract
{
    public abstract class BaseController : Controller
    {
        protected new JsonResult Json(object data, JsonRequestBehavior behavior)
        {
            return new JsonNetResult()
            {
                Data = data,
                JsonRequestBehavior = behavior
            };
        }

        protected JsonResult JsonCamelCase(object data)
        {
            return new JsonNetResult()
            {
                Data = data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                CamelCaseProperties = true
            };
        }
    }

    public class JsonNetResult : JsonResult
    {
        public bool CamelCaseProperties { get; set; }

        public JsonNetResult()
        {
            Settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Error,
                NullValueHandling = NullValueHandling.Ignore
            };

            SettingsCamelCase = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Error,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore
            };
        }

        private JsonSerializerSettings Settings { get; set; }
        private JsonSerializerSettings SettingsCamelCase { get; set; }

        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            if (this.JsonRequestBehavior == JsonRequestBehavior.DenyGet && string.Equals(context.HttpContext.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("JSON GET is not allowed");

            HttpResponseBase response = context.HttpContext.Response;
            response.ContentType = string.IsNullOrEmpty(this.ContentType) ? "application/json" : this.ContentType;

            if (this.ContentEncoding != null)
                response.ContentEncoding = this.ContentEncoding;
            if (this.Data == null)
                return;

            var scriptSerializer = JsonSerializer.Create(CamelCaseProperties ? this.SettingsCamelCase : this.Settings);

            using (var sw = new StringWriter())
            {
                scriptSerializer.Serialize(sw, this.Data);
                response.Write(sw.ToString());
            }
        }
    }
}