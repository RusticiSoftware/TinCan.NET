/*
 *  Created by Paul Carpenter 2015
 *  
 *  Includes support for Activity Interaction Components
 *  
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using TinCan.Json;

namespace TinCan
{
    public class InteractionComponent : JsonModel
    {
        public String id { get; set; }
        public LanguageMap description { get; set; }
        
        public InteractionComponent() { }

        public InteractionComponent(StringOfJSON json): this(json.toJObject()) {}

        public InteractionComponent(JObject jobj)
        {
            if (jobj["id"] != null)
            {
                id = jobj.Value<String>("id");
            }
            if (jobj["description"] != null)
            {
                description = (LanguageMap)jobj.Value<JObject>("description");
            }
        }

        public override JObject ToJObject(TCAPIVersion version)
        {
            JObject result = new JObject();

            if (id != null)
            {
                result.Add("id", id);
            }
            if (description != null && !description.isEmpty())
            {
                result.Add("description", description.ToJObject(version));
            }

            return result;
        }

        public static explicit operator InteractionComponent(JObject jobj)
        {
            return new InteractionComponent(jobj);
        }
    }
}
