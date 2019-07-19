using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QuestomAssets;

namespace BeatOn.ClientModels
{
    public class MessageTypeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(MessageBase);
        }

        public override bool CanWrite { get { return false; } }
        private object _classMapLock = new object();
        private static Dictionary<MessageType, Type> _classMap = null;

        //todo: make this class map stuff dynamic, the type is on the base type, I can figure it out with reflection and cache it
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            lock (_classMapLock)
            {
                if (_classMap == null)
                {
                    try
                    {
                        var map = new Dictionary<MessageType, Type>();
                        foreach (var msgType in System.Reflection.Assembly.GetExecutingAssembly().GetTypes().Where(x => x.CustomAttributes.Any(y => y.AttributeType == typeof(MessageAttribute))))
                        {
                            var attr = Attribute.GetCustomAttribute(msgType, typeof(MessageAttribute)) as MessageAttribute;
                            if (map.ContainsKey(attr.MessageType))
                            {
                                Log.LogErr($"WARNING: class {msgType.Name} will not be parsed by messagetypeconverter because MessageType {attr.MessageType} is already registered to another class!");
                            }
                            else
                            {
                                map.Add(attr.MessageType, msgType);
                            }
                        }
                        _classMap = map;
                    }
                    catch (Exception ex)
                    {
                        Log.LogErr("Exception loading class map in the message type converter!", ex);
                        throw;
                    }
                }
            }
            var token = JToken.Load(reader);
            var typeToken = token["Type"];
            if (typeToken == null)
                throw new InvalidOperationException("Bad message, has no type.");
            var actualType = _classMap[typeToken.ToObject<MessageType>(serializer)];
            if (existingValue == null || existingValue.GetType() != actualType)
            {
                var contract = serializer.ContractResolver.ResolveContract(actualType);
                existingValue = contract.DefaultCreator();
            }
            using (var subReader = token.CreateReader())
            {
                serializer.Populate(subReader, existingValue);
            }
            return existingValue;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}