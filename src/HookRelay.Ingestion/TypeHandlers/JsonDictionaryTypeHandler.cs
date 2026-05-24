using Dapper;
using System.Data;
using System.Text.Json;

namespace HookRelay.Ingestion.TypeHandlers
{
    public class JsonDictionaryTypeHandler : SqlMapper.TypeHandler<Dictionary<string, string>>
    {
        public override void SetValue(IDbDataParameter parameter, Dictionary<string, string>? value)
        {
            parameter.Value = JsonSerializer.Serialize(value);
        }

        public override Dictionary<string, string> Parse(object value)
        {
            return JsonSerializer.Deserialize<Dictionary<string, string>>(value.ToString()!)!;
        }
    }
}
