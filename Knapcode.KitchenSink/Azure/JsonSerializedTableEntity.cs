using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace Knapcode.KitchenSink.Azure
{
    public class JsonSerializedTableEntity<TContent> : TableEntity
    {
        private const string ContentKey = "Content";

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

            Content = JsonConvert.DeserializeObject<TContent>(entityPropertyContent.StringValue);
        }

        public override IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {
            string jsonContent = JsonConvert.SerializeObject(Content);
            var entityPropertyContent = new EntityProperty(jsonContent);
            return new Dictionary<string, EntityProperty> {{ContentKey, entityPropertyContent}};
        }
    }
}