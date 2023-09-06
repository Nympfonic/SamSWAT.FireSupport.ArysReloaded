using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace SamSWAT.FireSupport.Utils
{
    internal static class AccessToolsUtil
    {
        //stolen from kmyuhkyuk
        public static DelegateType MethodDelegate<DelegateType>(MethodInfo method, bool virtualCall = true) where DelegateType : Delegate
        {
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }

            var delegateType = typeof(DelegateType);

            var declaringType = method.DeclaringType;

            var delegateMethod = delegateType.GetMethod("Invoke");
            var delegateParameters = delegateMethod.GetParameters();
            var delegateParameterTypes = delegateParameters.Select(x => x.ParameterType).ToArray();

            Type returnType;
            bool needBox;

            if (delegateMethod.ReturnType == typeof(object) && method.ReturnType.IsValueType)
            {
                returnType = typeof(object);

                needBox = true;
            }
            else
            {
                returnType = method.ReturnType;

                needBox = false;
            }

            var dmd = new DynamicMethod("OpenInstanceDelegate_" + method.Name, returnType, delegateParameterTypes);

            var ilGen = dmd.GetILGenerator();

            Type[] parameterTypes;
            int num;

            if (!method.IsStatic)
            {
                var parameters = method.GetParameters();
                var numParameters = parameters.Length;
                parameterTypes = new Type[numParameters + 1];
                parameterTypes[0] = typeof(object);

                for (int i = 0; i < numParameters; i++)
                {
                    parameterTypes[i + 1] = parameters[i].ParameterType;
                }

                if (declaringType != null && declaringType.IsValueType)
                {
                    ilGen.Emit(OpCodes.Ldarga_S, 0);
                }
                else
                {
                    ilGen.Emit(OpCodes.Ldarg_0);
                }

                ilGen.Emit(OpCodes.Castclass, declaringType);

                num = 1;
            }
            else
            {
                parameterTypes = method.GetParameters().Select(x => x.ParameterType).ToArray();

                num = 0;
            }

            for (int i = num; i < parameterTypes.Length; i++)
            {
                ilGen.Emit(OpCodes.Ldarg, i);
                var parameterType = parameterTypes[i];
                var isValueType = parameterType.IsValueType;
                if (!isValueType)
                {
                    ilGen.Emit(OpCodes.Castclass, parameterType);
                }
                else if (delegateParameterTypes[i] == typeof(object))
                {
                    ilGen.Emit(OpCodes.Unbox_Any, parameterType);
                }
            }

            if (method.IsStatic || !virtualCall)
            {
                ilGen.Emit(OpCodes.Call, method);
            }
            else
            {
                ilGen.Emit(OpCodes.Callvirt, method);
            }

            if (needBox)
            {
                ilGen.Emit(OpCodes.Box, method.ReturnType);
            }

            ilGen.Emit(OpCodes.Ret);

            return (DelegateType)dmd.CreateDelegate(delegateType);
        }
    }
}