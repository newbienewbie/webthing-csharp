﻿using System;
using System.Linq;
using System.Text.Json;

namespace Mozilla.IoT.WebThing.Properties.Number
{
    public readonly struct PropertyDouble : IProperty
    {
        private readonly Thing _thing;
        private readonly Func<Thing, object> _getter;
        private readonly Action<Thing, object> _setter;

        private readonly bool _isNullable;
        private readonly double? _minimum;
        private readonly double? _maximum;
        private readonly double? _multipleOf;
        private readonly double[]? _enums;

        public PropertyDouble(Thing thing, Func<Thing, object> getter, Action<Thing, object> setter, 
             bool isNullable, double? minimum, double? maximum, double? multipleOf, double[]? enums)
        {
            _thing = thing ?? throw new ArgumentNullException(nameof(thing));
            _getter = getter ?? throw new ArgumentNullException(nameof(getter));
            _setter = setter ?? throw new ArgumentNullException(nameof(setter));
            _isNullable = isNullable;
            _minimum = minimum;
            _maximum = maximum;
            _multipleOf = multipleOf;
            _enums = enums;
        }

        public object GetValue() 
            => _getter(_thing);

        public SetPropertyResult SetValue(JsonElement element)
        {
            if (_isNullable && element.ValueKind == JsonValueKind.Null)
            {
                _setter(_thing, null);
                return SetPropertyResult.Ok;
            }
            
            if (element.ValueKind != JsonValueKind.Number)
            {
                return SetPropertyResult.InvalidValue;
            }
            
            if(!element.TryGetDouble(out var value))
            {
                return SetPropertyResult.InvalidValue;
            }

            if (_minimum.HasValue && value < _minimum.Value)
            {
                return SetPropertyResult.InvalidValue;
            }
            
            if (_maximum.HasValue && value > _maximum.Value)
            {
                return SetPropertyResult.InvalidValue;
            }

            if (_multipleOf.HasValue && value % _multipleOf.Value != 0)
            {
                return SetPropertyResult.InvalidValue;
            }

            if (_enums != null && _enums.Length > 0 && !_enums.Contains(value))
            {
                return SetPropertyResult.InvalidValue;
            }

            _setter(_thing, value);
            return SetPropertyResult.Ok;
        }
    }
}
