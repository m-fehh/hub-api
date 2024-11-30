﻿using Hub.Infrastructure;
using Hub.Infrastructure.Autofac;

using Hub.Infrastructure.MultiTenant;
using Microsoft.AspNetCore.Http;

namespace Hub.API.Configuration
{
    public class HubProvider : SchemaNameProvider
    {
        private static readonly HttpContextAccessor _httpContextAccessor = new HttpContextAccessor();

        public string TenantName()
        {
            // Retorna o TenantName se já estiver configurado
            string currentTenantName = Engine.Resolve<TenantLifeTimeScope>().CurrentTenantName;
            if (!string.IsNullOrEmpty(currentTenantName))
            {
                return currentTenantName;
            }

            // Verifica se HttpContext está disponível
            if (_httpContextAccessor.HttpContext == null || _httpContextAccessor.HttpContext.Request == null)
            {
                return "trainly";
            }

            // Recupera RouteValues do HttpContext
            var routeValues = _httpContextAccessor.HttpContext.GetRouteData()?.Values;

            // Busca tenantName nos RouteValues
            if (routeValues != null && routeValues.TryGetValue("tenantName", out var tenantName) && tenantName != null)
            {
                return tenantName.ToString();
            }


            return "trainly";
        }

        //// Método para pegar o nome do tenant a partir do subdomínio
        //public string TenantByUrl(string a)
        //{
        //    var context = _httpContextAccessor.HttpContext;
        //    if (context?.Request?.Host != null)
        //    {
        //        var host = context.Request.Host.Value;
        //        var subdomain = host.Split('.')[0]; // Assume que o primeiro subdomínio é o nome do tenant
        //        return subdomain; // Pode retornar o subdomínio, ou manipular conforme necessário
        //    }

        //    return null;
        //}
    }
}
