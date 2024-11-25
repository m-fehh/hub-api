using Hub.Infrastructure.Nominator;
using Hub.Infrastructure.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
using System.Net;
using System.Reflection;
using Hub.Domain;
using Hub.Shared.Interfaces;
using Hub.Shared.Enums.Infrastructure;
using Hub.Shared.Interfaces.Logger;
using Hub.Domain.Entities.Logger;

namespace Hub.Infrastructure.Logger.Interfaces
{
    public interface ILogManager
    {
        ILog Audit(IBaseEntity obj, ELogAction action, bool verifyLogableEntity = true, bool deeper = true);
        void Error(Exception ex);
    }

    public class LogManager : ILogManager
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DatabaseContext _dbContext;

        public LogManager(IHttpContextAccessor httpContextAccessor, DatabaseContext dbContext)
        {
            _httpContextAccessor = httpContextAccessor;
            _dbContext = dbContext;
        }

        protected virtual ILog InterceptLog(ILog log)
        {
            return log;
        }

        public ILog Audit(IBaseEntity obj, ELogAction action, bool verifyLogableEntity = true, bool deeper = true)
        {
            bool logsActived = true;

            if (bool.TryParse(ConfigurationManager.AppSettings["LogsActived"], out logsActived))
            {
                if (!logsActived) return null;
            }

            if (obj == null) return null;

            if (verifyLogableEntity)
            {
                if (!typeof(ILogableEntity).IsAssignableFrom(obj.GetType())) return null;
            }

            ILog log = new Log
            {
                Action = action,
                CreateDate = DateTime.Now,
                LogType = ELogType.Audit,
                IpAddress = GetIp(),
                ObjectId = obj.Id,
                CreateUser = Engine.Resolve<ISecurityProvider>().GetCurrent(),
                Message = Engine.Resolve<INominatorManager>().GetName(obj)
            };

            if (deeper)
            {
                log.Fields = GetFieldList(obj, log);

                if (log.Fields.Count == 0) return null;
            }

            if (typeof(ILogableEntityCustomName).IsAssignableFrom(obj.GetType()))
            {
                var customNameEntity = obj as ILogableEntityCustomName;
                log.ObjectName = !string.IsNullOrEmpty(customNameEntity?.CustomLogName)
                    ? customNameEntity.CustomLogName
                    : Engine.Get(obj.GetType().Name.Replace("Proxy", ""));
            }
            else
            {
                log.ObjectName = Engine.Get(obj.GetType().Name.Replace("Proxy", ""));
            }

            // Refresh logic if needed
            if (string.IsNullOrEmpty(log.Message))
            {
                var entityType = obj.GetType();
                var method = typeof(DbContext).GetMethod("Set").MakeGenericMethod(entityType);
                var dbSet = (IQueryable<IBaseEntity>)method.Invoke(_dbContext, null); // Cast para IQueryable<IBaseEntity>

                var loadedObj = dbSet.FirstOrDefault(e => e.Id == obj.Id); // Usando FirstOrDefault após o cast
                log.Message = Engine.Resolve<INominatorManager>().GetName(loadedObj);
            }

            log = InterceptLog(log);

            return log;
        }

        public string GetIp()
        {
            string ip = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
            if (ip == null || ip == "::1")
            {
                ip = Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault()?.ToString();
            }
            return ip;
        }

        public void Error(Exception ex)
        {
            bool logsActived = true;

            if (bool.TryParse(ConfigurationManager.AppSettings["LogsActived"], out logsActived))
            {
                if (!logsActived) return;
            }

            ILog log = new Log
            {
                Action = ELogAction.Insertion,
                CreateDate = DateTime.Now,
                CreateUser = Engine.Resolve<ISecurityProvider>().GetCurrent(),
                LogType = ELogType.Error,
                ObjectId = ex.HResult,
                ObjectName = ex.Message,
                Message = ex.StackTrace
            };

            log = InterceptLog(log);

            _dbContext.Set<ILog>().Add(log);
            _dbContext.SaveChanges();
        }

        private ISet<ILogField> GetFieldList<T>(T obj, ILog logFather) where T : class, IBaseEntity // Adicionado restrição para T ser classe
        {
            var nominator = Engine.Resolve<INominatorManager>();
            T oldObj = default(T);

            if (logFather.Action == ELogAction.Update)
            {
                oldObj = _dbContext.Set<T>().FirstOrDefault(e => e.Id == obj.Id);
            }

            var ret = new HashSet<ILogField>();
            var propertyList = obj.GetType().GetProperties().Where(p => p.Name != "Id" && !p.GetCustomAttributes(true).Any(a => a is IgnoreLog)).ToList();

            foreach (var prop in propertyList)
            {
                var newValue = prop.GetValue(obj);
                var oldComparator = GetPropertyComparator(oldObj, prop);
                var newComparator = GetPropertyComparator(obj, prop);

                if (newComparator != oldComparator || logFather.Action == ELogAction.Insertion)
                {
                    var field = Engine.Resolve<ILogField>();
                    field.OldValue = nominator.GetPropertyDescritor(prop, oldObj, false);
                    field.NewValue = nominator.GetPropertyDescritor(prop, obj);
                    field.PropertyName = Engine.Get(prop.Name);
                    field.Log = logFather;

                    ret.Add(field);
                }
            }

            return ret;
        }

        private string GetPropertyComparator(IBaseEntity obj, PropertyInfo prop)
        {
            if (typeof(IBaseEntity).IsAssignableFrom(prop.PropertyType))
            {
                return ((IBaseEntity)prop.GetValue(obj))?.Id.ToString();
            }
            return prop.GetValue(obj)?.ToString();
        }
    }
}
