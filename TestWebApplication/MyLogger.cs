using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace TestWebApplication
{
    public class MyLogger<T> : ILogger<T> where T : class
    {
        private const string CategoryNameForDefault = "Default";
        private const string LoggingPath = "Logging";
        private const string LogLevelPath = "LogLevel";
        private readonly string _providerName;
        private readonly string _includeScopePath;

        //Реализация Microsoft.Extensions.Logging.LoggerExternalScopeProvider
        private readonly IExternalScopeProvider _scopeProvider;

        private readonly IConfiguration _configuration;

        public MyLogger(string providerName, IConfiguration configuration, IExternalScopeProvider scopeProvider)
        {
            _providerName = providerName;
            _configuration = configuration;
            _scopeProvider = scopeProvider;
            _includeScopePath = "IncludeScopes";
        }

        public MyLogger(IConfiguration configuration, IExternalScopeProvider scopeProvider) 
            : this(null, configuration, scopeProvider) { }

        public IDisposable BeginScope<TState>(TState state) => _scopeProvider?.Push(state) ?? NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel != LogLevel.None;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            BaseMessage<LogModel> message = null;

            //Нету Logging в appSettings и logLevel < Debug => не пишем лог
            if (!IsPathExist(LoggingPath) && logLevel < LogLevel.Debug)
            {
                return;
            }

            if (!IsPathExist(LoggingPath) && logLevel >= LogLevel.Debug)
            {
                message = CreateMessage(logLevel, state, exception, formatter, null);
            }

            if (IsPathExist(LoggingPath))
            {
                //Если есть имя провайдера и провайдер найден в конфигах
                if (!string.IsNullOrEmpty(_providerName) && IsPathExist($"{LoggingPath}:{_providerName}"))
                {
                    message = MessageByDefaultConfig(_providerName, logLevel, state, exception, formatter);
                }
                else
                {
                    message = MessageByDefaultConfig(null, logLevel, state, exception, formatter);
                }
            }

            if (message != null)
            {
                //Запись переменной message в шину
                Console.WriteLine($"{JsonConvert.SerializeObject(message)}");
            }
        }

        private BaseMessage<LogModel> MessageByDefaultConfig<TState>(string providerName, LogLevel logLevel, TState state, Exception exception, 
            Func<TState, Exception, string> formatter)
        {
            var logLevelPath = GetLogLevelPath(providerName);

            var scopePath = GetScopePath(providerName);

            if (!IsPathExist(logLevelPath))
            {
                throw new ApplicationException("LogLevel isn't exist");
            }

            IEnumerable<LogLevelModel> configs = GetParsedCategoriesLevels(logLevelPath);

            var message = CreateMessageByCategory(configs, logLevel, state, exception, formatter, scopePath);

            return message;
        }

        private string GetLogLevelPath(string providerName)
        {
            if (string.IsNullOrEmpty(providerName))
            {
                return $"{LoggingPath}:{LogLevelPath}";
            }

            return $"{LoggingPath}:{providerName}:{LogLevelPath}";
        }

        private string GetScopePath(string providerName)
        {
            if (string.IsNullOrEmpty(providerName))
            {
                return $"{LoggingPath}:{_includeScopePath}";
            }

            return $"{LoggingPath}:{providerName}:{_includeScopePath}";
        }

        private BaseMessage<LogModel> CreateMessageByCategory<TState>(IEnumerable<LogLevelModel> configs, LogLevel logLevel, 
            TState state, Exception exception, Func<TState, Exception, string> formatter, string scopePath)
        {
            var categoryConfigLevel = configs.FirstOrDefault(logLevelModel => logLevelModel.Category == typeof(T).FullName);

            //Категория задана 
            if (categoryConfigLevel != null && categoryConfigLevel.LogLevel <= logLevel)
            {
                return CreateMessage(logLevel, state, exception, formatter, scopePath);
            }

            var defaultConfigLevel = configs.FirstOrDefault(logLevelModel => logLevelModel.Category == CategoryNameForDefault);

            //Категория не задана, используем значение по дефолту
            if (categoryConfigLevel == null && defaultConfigLevel != null && defaultConfigLevel.LogLevel <= logLevel)
            {
                return CreateMessage(logLevel, state, exception, formatter, scopePath);
            }

            if (categoryConfigLevel == null && defaultConfigLevel == null && LogLevel.Debug <= logLevel)
            {
                return CreateMessage(logLevel, state, exception, formatter, scopePath);
            }

            return null;
        }

        private bool IsPathExist(string path)
        {
            return _configuration.GetSection(path).Exists();
        }

        private IEnumerable<LogLevelModel> GetParsedCategoriesLevels(string configPath)
        {
            return _configuration
                .GetSection(configPath).GetChildren()
                .Select(categoryLevel =>
                    new LogLevelModel
                    {
                        Category = categoryLevel.Key,
                        LogLevel = Enum.Parse<LogLevel>(categoryLevel.Value)
                    });
        }

        private BaseMessage<LogModel> CreateMessage<TState>(LogLevel logLevel, TState state, Exception exception,
            Func<TState, Exception, string> formatter, string scopePath)
        {
            StringBuilder logBuilder = new StringBuilder();

            if (!string.IsNullOrEmpty(scopePath) && IsPathExist(scopePath))
            {
                GetScopeInformation(logBuilder, scopePath, true);
            }
            
            var messageString = formatter?.Invoke(state, exception);

            if (!string.IsNullOrEmpty(messageString))
            {
                logBuilder.Append($" {messageString}");
            }

            return new BaseMessage<LogModel>
            {
                Content = new LogModel
                {
                    Data = new LogData
                    {
                        Message = logBuilder.Length != 0 ? logBuilder.ToString() : null,

                        Exception = exception
                    }
                }
            };
        }

        private void GetScopeInformation(StringBuilder stringBuilder, string sectionFromConfig, bool multiLine)
        {
            bool includeScope = bool.Parse(_configuration.GetSection(sectionFromConfig).Value);

            var scopeProvider = _scopeProvider;
            if (includeScope && scopeProvider != null)
            {
                var initialLength = stringBuilder.Length;

                scopeProvider.ForEachScope((scope, state) =>
                {
                    var (builder, paddAt) = state;
                    var padd = paddAt == builder.Length;
                    if (padd)
                    {
                        builder.Append("=> ");
                    }
                    else
                    {
                        builder.Append(" => ");
                    }
                    builder.Append(scope);
                }, (stringBuilder, multiLine ? initialLength : -1));

                if (stringBuilder.Length > initialLength && multiLine)
                {
                    stringBuilder.AppendLine();
                }
            }
        }
    }
}
