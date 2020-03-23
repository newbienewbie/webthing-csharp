using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using Mozilla.IoT.WebThing.Actions;

namespace Mozilla.IoT.WebThing.Extensions
{
    internal static class ILGeneratorExtensions
    {
        private static readonly MethodInfo s_getService = typeof(IServiceProvider).GetMethod(nameof(IServiceProvider.GetService))!;
        private static readonly MethodInfo s_getTypeFromHandle = typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle))!;

        private static readonly PropertyInfo s_getSource = typeof(ActionInfo).GetProperty("Source", BindingFlags.NonPublic | BindingFlags.Instance)!;
        private static readonly PropertyInfo s_getToken = typeof(CancellationTokenSource).GetProperty(nameof(CancellationTokenSource.Token), BindingFlags.Public | BindingFlags.Instance)!;

        private static readonly MethodInfo s_getItem = typeof(Dictionary<string, object>).GetMethod("get_Item")!;
        
        private static readonly Type s_stringArray = typeof(string[]);
        private static readonly Type s_link = typeof(Link);
        private static readonly ConstructorInfo s_linkerConstructor = typeof(Link).GetConstructors()[1];
        
        #region Return
        public static void Return(this ILGenerator generator, string? value)
        {
            if (value == null)
            {
                generator.Emit(OpCodes.Ldnull);
            }
            else
            {
                generator.Emit(OpCodes.Ldstr, value);
            }
            
            generator.Emit(OpCodes.Ret);
        }
        
        public static void Return(this ILGenerator generator, FieldBuilder field)
        {
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldfld, field);
            generator.Emit(OpCodes.Ret);
        }

        public static void Return(this ILGenerator generator, LocalBuilder local)
        {
            generator.Emit(OpCodes.Ldloca_S, local.LocalIndex);
            generator.Emit(OpCodes.Initobj, local.LocalType);
            generator.Emit(OpCodes.Ldloc_0);
            generator.Emit(OpCodes.Ret);
        }
        
        public static void Return(this ILGenerator generator,  ConstructorInfo constructor)
        {
            generator.Emit(OpCodes.Newobj, constructor);
            generator.Emit(OpCodes.Ret);
        }
        #endregion

        #region Set

        public static void Set(this ILGenerator generator, FieldBuilder field)
        {
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldarg_1);
            generator.Emit(OpCodes.Stfld, field);
            generator.Emit(OpCodes.Ret);
        }

        public static void SetArgToLocal(this ILGenerator generator, LocalBuilder local)
        {
            generator.Emit(OpCodes.Ldarg_1);
            generator.Emit(OpCodes.Stloc_S, local.LocalIndex);
        }

        public static void SetProperty(this ILGenerator generator, PropertyInfo property)
        {
            generator.Emit(OpCodes.Dup);
            generator.Emit(OpCodes.Ldarg_1);
            generator.Emit(OpCodes.Ldstr, property.Name);
            generator.EmitCall(OpCodes.Callvirt, s_getItem, null);
            if (property.PropertyType.IsClass)
            {
                generator.Emit(OpCodes.Castclass, property.PropertyType);   
            }
            else
            {
                generator.Emit(OpCodes.Unbox_Any, property.PropertyType);
            }
            generator.EmitCall(OpCodes.Callvirt, property.SetMethod!, null);
        }
        
        
        #endregion

        #region Cast

        public static void CastFirstArg(this ILGenerator generator, Type type)
        {
            generator.Emit(OpCodes.Ldarg_1);
            generator.Emit(OpCodes.Castclass, type);
        }

        #endregion

        #region Load

        public static void LoadFromService(this ILGenerator generator, Type parameterType)
        {
            generator.Emit(OpCodes.Ldarg_2);
            generator.Emit(OpCodes.Ldtoken, parameterType);
            generator.EmitCall(OpCodes.Call, s_getTypeFromHandle, null);
            generator.EmitCall(OpCodes.Callvirt, s_getService, null);
            generator.Emit(OpCodes.Castclass, parameterType);
        }

        public static void LoadCancellationToken(this ILGenerator generator)
        {
            generator.Emit(OpCodes.Ldarg_0);
            generator.EmitCall(OpCodes.Call, s_getSource.GetMethod!, null);
            generator.EmitCall(OpCodes.Callvirt, s_getToken.GetMethod!, null);
        }
        
        public static void LoadFromInput(this ILGenerator generator, MethodInfo getInput, MethodInfo getValue)
        {
            generator.Emit(OpCodes.Ldarg_0);
            generator.EmitCall(OpCodes.Call, getInput, null);
            generator.EmitCall(OpCodes.Callvirt, getValue, null);
        }

        #endregion

        #region Call

        public static void Call(this ILGenerator generator, MethodInfo method)
        {
            if (method.DeclaringType!.IsClass)
            {
                generator.EmitCall(OpCodes.Callvirt, method, null);
            }
            else
            {
                generator.EmitCall(OpCodes.Call, method, null);
            }
        }

        #endregion

        #region NewObj

        public static void NewObj(this ILGenerator generator, ConstructorInfo constructor, bool addDup = false)
        {
            if (addDup)
            {
                generator.Emit(OpCodes.Dup);
            }
            
            generator.Emit(OpCodes.Newobj, constructor);
        }


        public static void NewObj(this ILGenerator generator, FieldBuilder field, ConstructorInfo constructor)
        {
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Newobj, constructor);
            generator.Emit(OpCodes.Stfld, field);
        }
        #endregion

        #region Array

        public static void NewLinkArray(this ILGenerator generator, FieldBuilder field, string href, string rel)
        {
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldc_I4_1);
            generator.Emit(OpCodes.Newarr, s_link);
            generator.Emit(OpCodes.Dup);
            generator.Emit(OpCodes.Ldc_I4_0);
            generator.Emit(OpCodes.Ldstr,href);
            generator.Emit(OpCodes.Ldstr, rel);
            generator.Emit(OpCodes.Newobj,s_linkerConstructor);
            generator.Emit(OpCodes.Stelem,s_link);
            generator.Emit(OpCodes.Stfld, field);
        }

        public static void NewStringArray(this ILGenerator generator, FieldBuilder field, string[] values)
        {
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldc_I4_S, values.Length);
            generator.Emit(OpCodes.Newarr, s_stringArray);
                
            for (var i = 0; i < values.Length; i++)
            {
                generator.Emit(OpCodes.Dup);
                generator.Emit(OpCodes.Ldc_I4_S, i);
                generator.Emit(OpCodes.Ldstr, values[i]);
                generator.Emit(OpCodes.Stelem_Ref);
            }
                
            generator.Emit(OpCodes.Stfld, field);
        }

        #endregion
    }
}
