using System;
using System.Linq;
using System.Reflection;

namespace AveTranslatorM.Models
{
    [AttributeUsage(AttributeTargets.Field)]
    public class GptModelValueAttribute : Attribute
    {
        public string Value { get; }
        public GptModelValueAttribute(string value)
        {
            Value = value;
        }
    }

    public enum GPTModel
    {
        [GptModelValue("gpt-3.5-turbo")]
        Gpt35Turbo,
        
        [GptModelValue("gpt-4")]
        Gpt4,
        
        [GptModelValue("gpt-4o")]
        Gpt4o,
        
        [GptModelValue("gpt-4o-mini")]
        Gpt4o_mini,
        
        [GptModelValue("o4-mini")]
        O4_mini,
        
        [GptModelValue("gpt-4.1")]
        Gpt41,
        
        [GptModelValue("gpt-4.1-nano")]
        Gpt41_nano,
        
        [GptModelValue("chatgpt-4o-latest")]
        СhatGpt40
    }

    public static class GPTModelExtensions
    {
        public static string ToModelString(this GPTModel model)
        {
            var memberInfo = model.GetType().GetMember(model.ToString()).FirstOrDefault();
            var attribute = memberInfo?.GetCustomAttribute<GptModelValueAttribute>();
            return attribute?.Value ?? "gpt-3.5-turbo";
        }

        public static GPTModel FromModelString(string modelString)
        {
            foreach (GPTModel model in Enum.GetValues(typeof(GPTModel)))
            {
                var memberInfo = typeof(GPTModel).GetMember(model.ToString()).FirstOrDefault();
                var attribute = memberInfo?.GetCustomAttribute<GptModelValueAttribute>();
                
                if (attribute?.Value == modelString)
                {
                    return model;
                }
            }
            
            return GPTModel.Gpt35Turbo; // значення за замовчуванням
        }

        public static bool IsKnownModel(string modelString)
        {
            return Enum.GetValues<GPTModel>()
                .Any(m => m.ToModelString() == modelString);
        }
    }
}
