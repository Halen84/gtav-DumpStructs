﻿using DumpFormatter.Json;
using System.Runtime.InteropServices;

using System;
using System.Text.Json.Serialization;
using System.Data;

namespace DumpFormatter.Model;

[JsonConverter(typeof(EnumConverter<ParMemberType>))]
internal enum ParMemberType
{
    BOOL,
    CHAR,
    UCHAR,
    SHORT,
    USHORT,
    INT,
    UINT,
    FLOAT,
    VECTOR2,
    VECTOR3,
    VECTOR4,
    STRING,
    STRUCT,
    ARRAY,
    ENUM,
    BITSET,
    MAP,
    MATRIX34,
    MATRIX44,
    VEC2V,
    VEC3V,
    VEC4V,
    MAT33V,
    MAT34V,
    MAT44V,
    SCALARV,
    BOOLV,
    VECBOOLV,
    PTRDIFFT,
    SIZET,
    FLOAT16,
    INT64,
    UINT64,
    DOUBLE,
    GUID,
    VEC2F,
    QUATV,
}

[JsonConverter(typeof(EnumConverter<ParMemberSubtype>))]
internal enum ParMemberSubtype
{
    NONE = 0,

    // parMemberCommonSubtype
    COLOR,
    ANGLE,

    // parMemberArraySubtype
    ATARRAY,
    ATFIXEDARRAY,
    ATRANGEARRAY,
    POINTER,
    MEMBER,
    _0x2087BB00,
    POINTER_WITH_COUNT,
    POINTER_WITH_COUNT_8BIT_IDX,
    POINTER_WITH_COUNT_16BIT_IDX,
    VIRTUAL,

    // parMemberEnumSubtype
    _64BIT,
    _32BIT,
    _16BIT,
    _8BIT,

    // parMemberBitsetSubtype
    // _64BIT,
    // _32BIT,
    // _16BIT,
    // _8BIT,
    ATBITSET,

    // parMemberMapSubtype
    ATMAP,
    ATBINARYMAP,

    // parMemberStringSubtype
    // MEMBER,
    // POINTER,
    CONST_STRING,
    ATSTRING,
    WIDE_MEMBER,
    WIDE_POINTER,
    ATWIDESTRING,
    ATNONFINALHASHSTRING,
    ATFINALHASHSTRING,
    ATHASHVALUE,
    ATPARTIALHASHVALUE,
    ATNSHASHSTRING,
    ATNSHASHVALUE,
    ATHASHVALUE16U,

    // parMemberStructSubtype
    STRUCTURE,
    EXTERNAL_NAMED,
    EXTERNAL_NAMED_USERNULL,
    //POINTER,
    SIMPLE_POINTER,

    // parMemberGuidSubtype
    _0xDF7EBE85,
}

[JsonConverter(typeof(ParMemberConverter))]
internal record ParMember(
    Name Name,
    ulong Offset,
    ulong Size,
    ulong Align,
    [property: JsonConverter(typeof(HexConverter))] ulong Flags1,
    [property: JsonConverter(typeof(HexConverter))] ulong Flags2,
    [property: JsonConverter(typeof(HexConverter))] ulong ExtraData,
    ParMemberType Type,
    ParMemberSubtype Subtype,
    ParAttributeList? Attributes);

internal record ParMemberSimple(
    Name Name, ulong Offset, ulong Size, ulong Align, ulong Flags1, ulong Flags2, ulong ExtraData, ParMemberType Type, ParMemberSubtype Subtype, ParAttributeList? Attributes,
    double InitValue)
    : ParMember(Name, Offset, Size, Align, Flags1, Flags2, ExtraData, Type, Subtype, Attributes);

internal record ParMemberVector(
    Name Name, ulong Offset, ulong Size, ulong Align, ulong Flags1, ulong Flags2, ulong ExtraData, ParMemberType Type, ParMemberSubtype Subtype, ParAttributeList? Attributes,
    double[] InitValues)
    : ParMember(Name, Offset, Size, Align, Flags1, Flags2, ExtraData, Type, Subtype, Attributes)
{
    public uint NumComponents => Type switch
    {
        ParMemberType.VEC2V or
        ParMemberType.VECTOR2 or
        ParMemberType.VEC2F => 2,
        ParMemberType.VEC3V or
        ParMemberType.VECTOR3 => 3,
        ParMemberType.VEC4V or
        ParMemberType.VECTOR4 or
        ParMemberType.QUATV or
        ParMemberType.VECBOOLV => 4,
        _ => throw new InvalidOperationException("Not a vector type"),
    };
}

internal record ParMemberMatrix(
    Name Name, ulong Offset, ulong Size, ulong Align, ulong Flags1, ulong Flags2, ulong ExtraData, ParMemberType Type, ParMemberSubtype Subtype, ParAttributeList? Attributes,
    double[] InitValues)
    : ParMember(Name, Offset, Size, Align, Flags1, Flags2, ExtraData, Type, Subtype, Attributes);

internal record ParMemberString(
    Name Name, ulong Offset, ulong Size, ulong Align, ulong Flags1, ulong Flags2, ulong ExtraData, ParMemberType Type, ParMemberSubtype Subtype, ParAttributeList? Attributes,
    ulong MemberSize,
    byte NamespaceIndex)
    : ParMember(Name, Offset, Size, Align, Flags1, Flags2, ExtraData, Type, Subtype, Attributes);

internal record ParMemberEnum(
    Name Name, ulong Offset, ulong Size, ulong Align, ulong Flags1, ulong Flags2, ulong ExtraData, ParMemberType Type, ParMemberSubtype Subtype, ParAttributeList? Attributes,
    Name EnumName,
    long InitValue)
    : ParMember(Name, Offset, Size, Align, Flags1, Flags2, ExtraData, Type, Subtype, Attributes);

[Flags, JsonConverter(typeof(EnumConverter<ParMemberArrayAllocFlags>))]
internal enum ParMemberArrayAllocFlags : ushort
{
    USE_PHYSICAL_ALLOCATOR = 1 << 0,
}

internal record ParMemberArray(
    Name Name, ulong Offset, ulong Size, ulong Align, ulong Flags1, ulong Flags2, ulong ExtraData, ParMemberType Type, ParMemberSubtype Subtype, ParAttributeList? Attributes,
    ParMember Item,
    ParMemberArrayAllocFlags AllocFlags,
    ulong? ArraySize,
    ulong? CountOffset)
    : ParMember(Name, Offset, Size, Align, Flags1, Flags2, ExtraData, Type, Subtype, Attributes);

internal record ParMemberMap(
    Name Name, ulong Offset, ulong Size, ulong Align, ulong Flags1, ulong Flags2, ulong ExtraData, ParMemberType Type, ParMemberSubtype Subtype, ParAttributeList? Attributes,
    ParMember Key,
    ParMember Value,
    Pointer? CreateIteratorFunc,
    Pointer? CreateInterfaceFunc)
    : ParMember(Name, Offset, Size, Align, Flags1, Flags2, ExtraData, Type, Subtype, Attributes);

internal record ParMemberStruct(
    Name Name, ulong Offset, ulong Size, ulong Align, ulong Flags1, ulong Flags2, ulong ExtraData, ParMemberType Type, ParMemberSubtype Subtype, ParAttributeList? Attributes,
    Name? StructName,
    Pointer? ExternalNamedResolveFunc,
    Pointer? ExternalNamedGetNameFunc,
    Pointer? AllocateStructFunc)
    : ParMember(Name, Offset, Size, Align, Flags1, Flags2, ExtraData, Type, Subtype, Attributes);
