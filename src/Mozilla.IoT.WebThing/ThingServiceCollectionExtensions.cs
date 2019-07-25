﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks.Dataflow;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.WebSockets;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Mozilla.IoT.WebThing;
using Mozilla.IoT.WebThing.Background;
using Mozilla.IoT.WebThing.Collections;
using Mozilla.IoT.WebThing.Description;
using Mozilla.IoT.WebThing.Json;
using Mozilla.IoT.WebThing.WebSockets;
using Action = Mozilla.IoT.WebThing.Action;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ThingServiceCollectionExtensions
    {
        #region Singles

        public static void AddThing<T>(this IServiceCollection services)
            where T : Thing
        {
            AddThing(services, options => options.AddThing<T>());
        }

        #endregion

        #region Multi

        public static void AddThing(this IServiceCollection services, Action<ThingBindingOption> thingOptions)
        {
            if (thingOptions == null)
            {
                throw new ArgumentNullException(nameof(thingOptions));
            }
            
            RegisterCommon(services);
            
            var option = new ThingBindingOption();

            thingOptions(option);

            foreach (Type thing in option.ThingsType)
            {
                services.TryAddSingleton(thing);
            }
            
            foreach (Type action in Thing.ActionsTypes)
            {
                services.TryAddTransient(action);
            }

            services.TryAddSingleton<IReadOnlyList<Thing>>(provider =>
            {
                var things = option.Things.ToList();
                things.AddRange(option.ThingsType.Select(thing => (Thing)provider.GetService(thing)));

                if (things.Count == 1 && !option.IsMultiThing)
                {
                    return new SingleThingCollection(things[0]);
                }
                else
                {
                    return new MultipleThingsCollections(things);
                }
            });
        }

        #endregion

        private static void RegisterCommon(IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddRouting();
            services.AddWebSockets(options => { });
            services.AddCors();

            services.TryAddSingleton<IJsonSerializerSettings>(service => new DefaultJsonSerializerSettings(
                new JsonSerializerOptions
                {
                    WriteIndented = false,
                    IgnoreNullValues = true,
                    DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                }));

            services.TryAddSingleton<IJsonConvert, DefaultJsonConvert>();
            services.TryAddSingleton<IJsonSchemaValidator, DefaultJsonSchemaValidator>();
            
            services.TryAddScoped<IActionFactory, ActionFactory>();
            services.TryAddScoped<IDescription<Action>, ActionDescription>();
            services.TryAddScoped<IDescription<Event>, EventDescription>();
            services.TryAddScoped<IDescription<Property>, PropertyDescription>();
            services.TryAddScoped<IDescription<Thing>, ThingDescription>();

            services.AddHostedService<ActionExecutorHostedService>();

            var block = new BufferBlock<Mozilla.IoT.WebThing.Action>();
            services.AddSingleton<ISourceBlock<Mozilla.IoT.WebThing.Action>>(block);
            services.AddSingleton<ITargetBlock<Mozilla.IoT.WebThing.Action>>(block);

            services.AddTransient<WebSocketProcessor>();

            services.TryAddEnumerable(ServiceDescriptor.Transient<IWebSocketAction, AddEventSubscription>());
            services.TryAddEnumerable(ServiceDescriptor.Transient<IWebSocketAction, RequestAction>());
            services.TryAddEnumerable(ServiceDescriptor.Transient<IWebSocketAction, SetThingProperty>());
            services.TryAddEnumerable(ServiceDescriptor.Transient<IWebSocketAction, GetThingProperty>());
        }
    }
}
