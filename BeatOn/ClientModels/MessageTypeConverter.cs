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

namespace BeatOn.ClientModels
{
    public class MessageTypeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(MessageBase);
        }

        public override bool CanWrite { get { return false; } }

        private static Dictionary<MessageType, Type> _classMap = new Dictionary<MessageType, Type>()
        { {MessageType.AddOrUpdatePlaylist, typeof(ClientAddOrUpdatePlaylist)},
            {MessageType.DeletePlaylist, typeof(ClientDeletePlaylist) },
            {MessageType.DeleteSong, typeof(ClientDeleteSong) },
            {MessageType.MoveSongToPlaylist, typeof(ClientMoveSongToPlaylist) },
            {MessageType.GetOps, typeof(ClientGetOps) },
            {MessageType.SortPlaylist, typeof(ClientSortPlaylist) },
            {MessageType.AutoCreatePlaylists, typeof(ClientAutoCreatePlaylists) },
            {MessageType.SetModStatus, typeof(ClientSetModStatus) },
            {MessageType.MovePlaylist, typeof(ClientMovePlaylist) },
            {MessageType.MoveSongInPlaylist, typeof(ClientMoveSongInPlaylist) },
            {MessageType.DeleteMod, typeof(ClientDeleteMod) }
        };

        //todo: make this class map stuff dynamic, the type is on the base type, I can figure it out with reflection and cache it
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
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