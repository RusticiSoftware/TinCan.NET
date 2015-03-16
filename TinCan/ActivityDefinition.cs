/*
    Copyright 2014 Rustici Software

    Licensed under the Apache License, Version 2.0 (the "License");
    you may not use this file except in compliance with the License.
    You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

    Unless required by applicable law or agreed to in writing, software
    distributed under the License is distributed on an "AS IS" BASIS,
    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
    See the License for the specific language governing permissions and
    limitations under the License.
*/

/*
 *  Modified by Paul Carpenter 2015
 *  
 *  Includes support for Activity Interactions
 *  
 */

using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using TinCan.Json;

namespace TinCan
{
    public class ActivityDefinition : JsonModel
    {
        public Uri type { get; set; }
        public Uri moreInfo { get; set; }
        public LanguageMap name { get; set; }
        public LanguageMap description { get; set; }
        public Extensions extensions { get; set; }
        public String interactionType { get; set; }
        public List<String> correctResponsesPattern { get; set; }
        public List<InteractionComponent> choices { get; set; }
        public List<InteractionComponent> scale { get; set; }
        public List<InteractionComponent> source { get; set; }
        public List<InteractionComponent> target { get; set; }
        public List<InteractionComponent> steps { get; set; }

        public ActivityDefinition() {}

        public ActivityDefinition(StringOfJSON json): this(json.toJObject()) {}

        public ActivityDefinition(JObject jobj)
        {
            if (jobj["type"] != null)
            {
                type = new Uri(jobj.Value<String>("type"));
            }
            if (jobj["moreInfo"] != null)
            {
                moreInfo = new Uri(jobj.Value<String>("moreInfo"));
            }
            if (jobj["name"] != null)
            {
                name = (LanguageMap)jobj.Value<JObject>("name");
            }
            if (jobj["description"] != null)
            {
                description = (LanguageMap)jobj.Value<JObject>("description");
            }
            if (jobj["extensions"] != null)
            {
                extensions = (Extensions)jobj.Value<JObject>("extensions");
            }
            if (jobj["interactionType"] != null)
            {
                interactionType = jobj.Value<String>("interactionType");
            }
            if (jobj["correctResponsesPattern"] != null)
            {
                correctResponsesPattern = new List<String>();
                foreach (JValue jcorrectResponsesPattern in jobj["correctResponsesPattern"])
                {
                    correctResponsesPattern.Add(jcorrectResponsesPattern.ToString());
                }
            }
            if (jobj["choices"] != null)
            {
                choices = new List<InteractionComponent>();
                foreach (JObject jchoices in jobj["choices"])
                {
                    choices.Add((InteractionComponent)jchoices);
                }
            }
            if (jobj["scale"] != null)
            {
                scale = new List<InteractionComponent>();
                foreach (JObject jscale in jobj["scale"])
                {
                    scale.Add((InteractionComponent)jscale);
                }
            }
            if (jobj["source"] != null)
            {
                source = new List<InteractionComponent>();
                foreach (JObject jsource in jobj["source"])
                {
                    source.Add((InteractionComponent)jsource);
                }
            }
            if (jobj["target"] != null)
            {
                target = new List<InteractionComponent>();
                foreach (JObject jtarget in jobj["target"])
                {
                    target.Add((InteractionComponent)jtarget);
                }
            }
            if (jobj["steps"] != null)
            {
                steps = new List<InteractionComponent>();
                foreach (JObject jsteps in jobj["steps"])
                {
                    steps.Add((InteractionComponent)jsteps);
                }
            }
        }

        public override JObject ToJObject(TCAPIVersion version) {
            JObject result = new JObject();

            if (type != null)
            {
                result.Add("type", type.ToString());
            }
            if (moreInfo != null)
            {
                result.Add("moreInfo", moreInfo.ToString());
            }
            if (name != null && ! name.isEmpty())
            {
                result.Add("name", name.ToJObject(version));
            }
            if (description != null && ! description.isEmpty())
            {
                result.Add("description", description.ToJObject(version));
            }
            if (extensions != null && ! extensions.isEmpty())
            {
                result.Add("extensions", extensions.ToJObject(version));
            }
            if (interactionType != null)
            {
                result.Add("interactionType", interactionType.ToString());
            }
            if (correctResponsesPattern != null && correctResponsesPattern.Count > 0)
            {
                var jcorrectResponsesPattern = new JArray();
                result.Add("correctResponsesPattern", jcorrectResponsesPattern);

                foreach (String correctReponse in correctResponsesPattern)
                {
                    jcorrectResponsesPattern.Add(correctReponse.ToString());
                }
            }
            if (choices != null && choices.Count > 0)
            {
                var jchoices = new JArray();
                result.Add("choices", jchoices);

                foreach (InteractionComponent c in choices)
                {
                    jchoices.Add(c.ToJObject(version));
                }
            }
            if (scale != null && scale.Count > 0)
            {
                var jscale = new JArray();
                result.Add("scale", jscale);

                foreach (InteractionComponent s in scale)
                {
                    jscale.Add(s.ToJObject(version));
                }
            }
            if (source != null && source.Count > 0)
            {
                var jsource = new JArray();
                result.Add("source", jsource);

                foreach (InteractionComponent s in source)
                {
                    jsource.Add(s.ToJObject(version));
                }
            }
            if (target != null && target.Count > 0)
            {
                var jtarget = new JArray();
                result.Add("target", jtarget);

                foreach (InteractionComponent t in target)
                {
                    jtarget.Add(t.ToJObject(version));
                }
            }
            if (steps != null && steps.Count > 0)
            {
                var jsteps = new JArray();
                result.Add("steps", jsteps);

                foreach (InteractionComponent s in steps)
                {
                    jsteps.Add(s.ToJObject(version));
                }
            }

            return result;
        }

        public static explicit operator ActivityDefinition(JObject jobj)
        {
            return new ActivityDefinition(jobj);
        }
    }
}
