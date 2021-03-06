﻿using MemTrick.RawMemory;
using MemTrick.RumtimeSpecific;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace MemTrick
{
    // MEMO:
    // In some environment, GC may mark the method tables to pMT|1,
    // and may not recover this because these objects will not be sweeped.
    // Need to track down and handle these cases.
    // @Harnel
    public static unsafe class ClassAllocator
    {
        /// <summary>
        /// Similar with FormatterServices.GetUninitializedObject. Allocate and return zeroed T-typed object.
        /// </summary>
        public static ClassDisposeHandle UninitializedAllocation<T>(out T result) where T : class
        {
            ClassDisposeHandle handle = UninitializedAllocation(typeof(T), out Object obj);
            result = obj as T;

            return handle;
        }

        /// <summary>
        /// Similar with FormatterServices.GetUninitializedObject. Allocate and return zeroed T-typed object.
        /// </summary>
        public static ClassDisposeHandle UninitializedAllocation(Type t, out Object result)
        {
            MethodTable* mt = MethodTable.GetMethodTable(t);
            int size = mt->BaseSize;

            ObjectHeader* objHeader = (ObjectHeader*)RawMemoryManager.Allocate(size);

            objHeader->SyncBlock = 0;
            objHeader->MethodTable = mt;
            RawMemoryManager.FillMemory(objHeader + 1, 0, size - sizeof(ObjectHeader));

            result = TypedReferenceHelper.PointerToObject<Object>(objHeader);
            return new ClassDisposeHandle(size, objHeader);
        }

        private static ConstructorInfo GetConstructorInfo<T>(params Type[] types)
        {
            return typeof(T).GetConstructor(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.CreateInstance,
                null,
                CallingConventions.Any,
                types,
                null);
        }


        private static Dictionary<Type, IntPtr> cache = new Dictionary<Type, IntPtr>();

        public static void PreCacheConstructor<T>()
        {
            ConstructorInfo ci = GetConstructorInfo<T>(Type.EmptyTypes);
            cache.Add(typeof(T), ci.MethodHandle.GetFunctionPointer());
        }
        private static IntPtr GetConstructorLocation<T>()
        {
            return cache[typeof(T)];
        }

        #region Allocate<T, ...> series
        /// <summary>
        /// Similar with new T();
        /// </summary>
        public static ClassDisposeHandle Allocate<T>(out T result) where T : class
        {
            ClassDisposeHandle handle = UninitializedAllocation<T>(out result);

            ArbitaryMethodInvoker.InvokeAction(GetConstructorLocation<T>(), result);

            return handle;
        }

        /// <summary>
        /// Similar with new T(arg0);
        /// </summary>
        public static ClassDisposeHandle Allocate<T, TArg0>(
            out T result,
            TArg0 arg0)
            where T : class
        {
            ClassDisposeHandle handle = UninitializedAllocation<T>(out result);

            ConstructorInfo ci = GetConstructorInfo<T>(typeof(TArg0));
            ArbitaryMethodInvoker.InvokeAction(ci.MethodHandle.GetFunctionPointer(), result, arg0);

            return handle;
        }

        /// <summary>
        /// Similar with new T(arg0, arg1);
        /// </summary>
        public static ClassDisposeHandle Allocate<T, TArg0, TArg1>(
            out T result,
            TArg0 arg0, TArg1 arg1)
            where T : class
        {
            ClassDisposeHandle handle = UninitializedAllocation<T>(out result);

            ConstructorInfo ci = GetConstructorInfo<T>(typeof(TArg0), typeof(TArg1));
            ArbitaryMethodInvoker.InvokeAction(ci.MethodHandle.GetFunctionPointer(), result, arg0, arg1);

            return handle;
        }

        /// <summary>
        /// Similar with new T(arg0, arg1, arg2);
        /// </summary>
        public static ClassDisposeHandle Allocate<T, TArg0, TArg1, TArg2>(
            out T result,
            TArg0 arg0, TArg1 arg1, TArg2 arg2)
            where T : class
        {
            ClassDisposeHandle handle = UninitializedAllocation<T>(out result);

            ConstructorInfo ci = GetConstructorInfo<T>(typeof(TArg0), typeof(TArg1), typeof(TArg2));
            ArbitaryMethodInvoker.InvokeAction(ci.MethodHandle.GetFunctionPointer(), result, arg0, arg1, arg2);

            return handle;
        }

        /// <summary>
        /// Similar with new T(arg0, arg1, arg2, arg3);
        /// </summary>
        public static ClassDisposeHandle Allocate<T, TArg0, TArg1, TArg2, TArg3>(
            out T result,
            TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3)
            where T : class
        {
            ClassDisposeHandle handle = UninitializedAllocation<T>(out result);

            ConstructorInfo ci = GetConstructorInfo<T>(typeof(TArg0), typeof(TArg1), typeof(TArg2), typeof(TArg3));
            ArbitaryMethodInvoker.InvokeAction(ci.MethodHandle.GetFunctionPointer(), result, arg0, arg1, arg2, arg3);

            return handle;
        }

        /// <summary>
        /// Similar with new T(arg0, arg1, arg2, arg3, arg4);
        /// </summary>
        public static ClassDisposeHandle Allocate<T, TArg0, TArg1, TArg2, TArg3, TArg4>(
            out T result,
            TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
            where T : class
        {
            ClassDisposeHandle handle = UninitializedAllocation<T>(out result);

            ConstructorInfo ci = GetConstructorInfo<T>(typeof(TArg0), typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4));
            ArbitaryMethodInvoker.InvokeAction(ci.MethodHandle.GetFunctionPointer(), result, arg0, arg1, arg2, arg3, arg4);

            return handle;
        }

        /// <summary>
        /// Similar with new T(arg0, arg1, arg2, arg3, arg4, arg5);
        /// </summary>
        public static ClassDisposeHandle Allocate<T, TArg0, TArg1, TArg2, TArg3, TArg4, TArg5>(
            out T result,
            TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5)
            where T : class
        {
            ClassDisposeHandle handle = UninitializedAllocation<T>(out result);

            ConstructorInfo ci = GetConstructorInfo<T>(typeof(TArg0), typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5));
            ArbitaryMethodInvoker.InvokeAction(ci.MethodHandle.GetFunctionPointer(), result, arg0, arg1, arg2, arg3, arg4, arg5);

            return handle;
        }
        #endregion

        public static ClassDisposeHandle AllocateSZArray<T>(int size, out T[] array)
        {
            MethodTable* pElementMT = MethodTable.GetMethodTable<T>();
            MethodTable* pArrayMT = MethodTable.GetMethodTable<T[]>();
            int elementSize = pElementMT->IsClass ? sizeof(IntPtr) : pElementMT->DataSize;

            int memSize = sizeof(ObjectHeader) + sizeof(SZArrayHeader) + elementSize * size;
            int align = sizeof(IntPtr) - 1;
            memSize = (memSize + align) & (~align);

            void* addr = RawMemoryManager.Allocate(memSize);
            ObjectHeader* objHeader = (ObjectHeader*)addr;
            SZArrayHeader* szArrayHeader = (SZArrayHeader*)(objHeader + 1);

            objHeader->MethodTable = pArrayMT;
            szArrayHeader->NumComponents = size;

            array = TypedReferenceHelper.PointerToObject<T[]>(objHeader);
            return new ClassDisposeHandle(memSize, objHeader);
        }

        public static ClassDisposeHandle Box<T>(T val, out Object boxed) where T : struct
        {
            MethodTable* mt = MethodTable.GetMethodTable<T>();
            int size = mt->BaseSize;

            ObjectHeader* objHeader = (ObjectHeader*)RawMemoryManager.Allocate(size);

            void* src = TypedReferenceHelper.StructToPointer(ref val);

            objHeader->SyncBlock = 0;
            objHeader->MethodTable = mt;
            RawMemoryManager.MemCpy(objHeader + 1, src, mt->DataSize);

            boxed = TypedReferenceHelper.PointerToObject<Object>(objHeader);
            return new ClassDisposeHandle(size, objHeader);
        }
    }
}
