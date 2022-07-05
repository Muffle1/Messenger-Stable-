using Newtonsoft.Json;
using System;
using System.Text.Json;

namespace Messenger
{
    public class Package
    {
        public Command Command { get; private set; }
        public object Data { get; set; }

        public Package(Command command, object data)
        {
            Command = command;
            Data = data;
        }

        public T GetValue<T>()
        {
            if (Data is JsonElement jsonElement)
                Data = JsonConvert.DeserializeObject<T>(jsonElement.GetRawText()); 
            return (T)Data;
        }
    }
}
