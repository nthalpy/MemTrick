# String Size

Following is how before .NET Core 3.0 evaluates `m_baseSize` of `System.String`. Code is from v.2.1.23. Object can be over-allocated at most 8B because `StringObject` contains one `DWORD` member and `Byte[]` member.
```
// src/vm/methodtablebuilder.cpp #9736
if (strcmp(name, g_StringName) == 0 && strcmp(nameSpace, g_SystemNS) == 0)
{
    // Strings are not "normal" objects, so we need to mess with their method table a bit
    // so that the GC can figure out how big each string is...
    DWORD baseSize = ObjSizeOf(StringObject) + sizeof(WCHAR);
    pMT->SetBaseSize(baseSize); // NULL character included

    GetHalfBakedClass()->SetBaseSizePadding(baseSize - bmtFP->NumInstanceFieldBytes);

    pMT->SetComponentSize(2);
}
```

Following is how .NET Core 3.0 and after evaluates `m_baseSize` of `System.String`. Code is from v3.0.0
```
// src/vm/methodtablebuilder.cpp #9763
if (strcmp(name, g_StringName) == 0 && strcmp(nameSpace, g_SystemNS) == 0)
{
    // Strings are not "normal" objects, so we need to mess with their method table a bit
    // so that the GC can figure out how big each string is...
    DWORD baseSize = StringObject::GetBaseSize();
    pMT->SetBaseSize(baseSize);

    GetHalfBakedClass()->SetBaseSizePadding(baseSize - bmtFP->NumInstanceFieldBytes);

    pMT->SetComponentSize(2);
}

// src/vm/object.inl #50
__forceinline /*static*/ DWORD StringObject::GetBaseSize()
{
    LIMITED_METHOD_DAC_CONTRACT;

    return OBJECT_BASESIZE + sizeof(DWORD) /* length */ + sizeof(WCHAR) /* null terminator */;
}
```

Related commit : 37e643696f860094ca3182c87f1375540b9d704e (https://github.com/dotnet/coreclr/commit/)
