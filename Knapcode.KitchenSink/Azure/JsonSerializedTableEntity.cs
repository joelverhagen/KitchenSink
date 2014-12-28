using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace Knapcode.KitchenSink.Azure
{
    public class JsonSerializedTableEntity<TContent> : TableEntity
    {
        private const string ContentKey = "Content";

        public JsonSerializedTableEntity()
        {
            JsonSerializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };
        }

        public JsonSerializerSettings JsonSerializerSettings { get; set; }

        public TContent Content { get; set; }

        public override void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
            EntityProperty entityPropertyContent;
            if (!properties.TryGetValue(ContentKey, out entityPropertyContent) ||
                entityPropertyContent.PropertyType != EdmType.String ||
                entityPropertyContent.StringValue == null)
            {
                return;
            }

            Content = JsonConvert.DeserializeObject<TContent>(entityPropertyContent.StringValue, JsonSerializerSettings);
        }

        public override IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {
            string jsonContent = JsonConvert.SerializeObject(Content, JsonSerializerSettings);
            var entityPropertyContent = new EntityProperty(jsonContent);
            return new Dictionary<string, EntityProperty> {{ContentKey, entityPropertyContent}};
        }
    }
}