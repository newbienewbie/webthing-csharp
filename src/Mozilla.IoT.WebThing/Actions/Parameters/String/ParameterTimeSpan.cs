using System;
using System.Linq;
using System.Text.Json;

namespace Mozilla.IoT.WebThing.Actions.Parameters.String
{
    public readonly struct ParameterTimeSpan : IActionParameter
    {
        private readonly TimeSpan[]? _enums;

        public ParameterTimeSpan(bool isNullable, TimeSpan[]? enums)
        {
            CanBeNull = isNullable;
            _enums = enums;
        }

        public bool CanBeNull { get; }

        public bool TryGetValue(JsonElement element, out object? value)
        {
            value = null;
            if (CanBeNull && element.ValueKind == JsonValueKind.Null)
            {
                return true;
            }
            
            if (element.ValueKind != JsonValueKind.String)
            {
                return false;
            }

            if (!TimeSpan.TryParse(element.GetString(), out var jsonValue))
            {
                return false;
            }
            
            if (_enums != null && _enums.Length > 0 &&  !_enums.Contains(jsonValue))
            {
                return false;
            }

            value = jsonValue;
            return true;
        }
    }
}
