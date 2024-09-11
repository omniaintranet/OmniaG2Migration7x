using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omnia.Migration.Models.LegacyWCM
{
    internal class PageDataJsonConverter : JsonConverter
    {

        public override bool CanConvert(Type objectType)
        {
            // Not needed, as we register our converter directly on OmniaJsonBase
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            reader.MaxDepth = null;
            JToken jToken = JToken.ReadFrom(reader);
            string propertyName = $"{nameof(PageData.Type)}";
            int? type = jToken.Value<int?>(propertyName);

            if (type == null)
            {
                char[] typeName = nameof(PageData.Type).ToCharArray();
                typeName[0] = Char.ToLower(typeName[0]);
                propertyName = new string(typeName);
                type = jToken.Value<int?>(propertyName);
            }


            PageData result = null;

            //Must take inheritance into account, highest order first, like pagecollection inherit plainpage, check pagecollection first
            if (type == null)
                throw new ArgumentOutOfRangeException($"{nameof(PageDataJsonConverter)} can't convert to {nameof(PageData)}, property {propertyName} does not have expected value");
            /*else if (BitwiseHelper.IsSet((int)type, PageData.WCMPageTypes.PageType))
            {
                result = new PageTypeData();
            }
            else if (BitwiseHelper.IsSet((int)type, PageData.WCMPageTypes.PageCollection))
            {
                result = new PageCollectionData();
            }
            else if (BitwiseHelper.IsSet((int)type, PageData.WCMPageTypes.PlainPage))
            {
                result = new PlainPageData();
            }
            else
            {
                result = new PageData();
            }*/
            result = new Omnia.Migration.Models.LegacyWCM.PageData();


            serializer.Populate(jToken.CreateReader(), result);

            //Migration.LayoutMigration.EnsureMigrate(result);

            return result;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }


    }
}
