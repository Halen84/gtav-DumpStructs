﻿using DumpFormatter.Model;

using System.Text;
using System.Xml.Linq;

namespace DumpFormatter.Formatters;

internal class HtmlFormatter : PlainTextFormatter
{
    public override void Format(TextWriter writer, ParDump dump)
    {
        writer.Write($@"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""utf-8"" />
    <title>{dump.Game.ToUpperInvariant()} (build {dump.Build})</title>
</head>
<body>
    <main>
        <ul>
");
        foreach (var s in dump.Structs.OrderBy(s => s.Name.ToFormattedString()))
        {
            FormatStruct(writer, s);
        }
        foreach (var e in dump.Enums.OrderBy(s => s.Name.ToFormattedString()))
        {
            FormatEnum(writer, e);
        }
        writer.Write($@"
        </ul>
    </main>
</body>
</html>");
    }

    private void BeginCodeBlock(TextWriter w, Name name)
    {
        w.Write($"<li><pre id=\"{name.Hash:X08}\"><code>");
    }

    private void EndCodeBlock(TextWriter w, Name name)
    {
        w.Write($"</code></pre></li>");
    }

    protected override void FormatStruct(TextWriter w, ParStructure s)
    {
        BeginCodeBlock(w, s.Name);
        w.Write($"{Keyword("struct")} {Type(s.Name.ToFormattedString())}<span class=\"c-w\">");
        if (s.Base != null)
        {
            w.Write($" : {TypeLink(s.Base.Value.Name)}");
        }
        w.WriteLine();
        w.WriteLine("{");
        var (paddingBetweenTypeAndName, paddingBetweenNameAndComment) = CalculatePaddingForMembers(s);
        var membersOrdered = s.Members.Select((m, i) => (Member: m, Index: i)).OrderBy(m => m.Member.Offset).ToArray();
        var membersSB = new StringBuilder();
        var tmpSB = new StringBuilder();
        for (int i = 0; i < membersOrdered.Length; i++)
        {
            var member = membersOrdered[i].Member;
            var name = member.Name.ToFormattedString();
            membersSB.Append('\t');
            tmpSB.Clear();
            FormatMemberType(tmpSB, member, out var typeLength);
            FormatMemberTypeHTML(membersSB, member);
            membersSB.Append(' ', paddingBetweenTypeAndName - typeLength);
            membersSB.Append(' ');
            membersSB.Append(name);
            membersSB.Append(';');
            membersSB.Append(' ', paddingBetweenNameAndComment - name.Length - 1);
            FormatMemberCommentHTML(membersSB, member);
            membersSB.AppendLine();
        }
        w.Write(membersSB);
        w.WriteLine("};</span>");
        EndCodeBlock(w, s.Name);
    }

    protected override void FormatEnum(TextWriter w, ParEnum e)
    {
        BeginCodeBlock(w, e.Name);
        w.Write(Keyword("enum"));
        w.Write(' ');
        w.Write(Type(e.Name.ToFormattedString()));
        w.WriteLine("<span class=\"c-w\">");
        w.WriteLine("{");
        for (int i = 0; i < e.Values.Length; i++)
        {
            var value = e.Values[i];
            w.WriteLine($"\t{value.Name.ToFormattedString()} = {value.Value},");
        }
        w.WriteLine("};</span>");
        EndCodeBlock(w, e.Name);
    }

    private void FormatMemberTypeHTML(StringBuilder sb, ParMember m)
    {
        formatRecursive(sb, m);

        static void formatRecursive(StringBuilder sb, ParMember m)
        {
            sb.Append(TypeToString(m.Type));
            switch (m.Type)
            {
                case ParMemberType.STRUCT:
                    sb.Append(' ');
                    var structName = ((ParMemberStruct)m).StructName;
                    sb.Append(structName != null ? TypeLink(structName.Value) : Keyword("void"));
                    break;
                case ParMemberType.ENUM:
                    sb.Append(' ');
                    sb.Append(TypeLink(((ParMemberEnum)m).EnumName));
                    break;
                case ParMemberType.BITSET:
                    sb.Append('<');
                    sb.Append(Keyword("enum"));
                    sb.Append(' ');
                    sb.Append(TypeLink(((ParMemberEnum)m).EnumName));
                    sb.Append('>');
                    break;
                case ParMemberType.ARRAY:
                    sb.Append('<');
                    var arr = (ParMemberArray)m;
                    formatRecursive(sb, arr.Item);
                    if (arr.ArraySize.HasValue)
                    {
                        sb.Append($", {arr.ArraySize.Value}");
                    }
                    sb.Append('>');
                    break;
                case ParMemberType.MAP:
                    sb.Append('<');
                    formatRecursive(sb, ((ParMemberMap)m).Key);
                    sb.Append(", ");
                    formatRecursive(sb, ((ParMemberMap)m).Value);
                    sb.Append('>');
                    break;
            }
        }
    }

    private void FormatMemberCommentHTML(StringBuilder sb, ParMember m)
    {
        sb.Append("<span class=\"c-c\">");
        FormatMemberComment(sb, m);
        sb.Append("</span>");
    }

    private static string TypeToString(ParMemberType type)
        => type switch
        {
            ParMemberType.BOOL => Keyword("bool"),
            ParMemberType.CHAR => Keyword("char"),
            ParMemberType.UCHAR => Keyword("uchar"),
            ParMemberType.SHORT => Keyword("short"),
            ParMemberType.USHORT => Keyword("ushort"),
            ParMemberType.INT => Keyword("int"),
            ParMemberType.UINT => Keyword("uint"),
            ParMemberType.FLOAT => Keyword("float"),
            ParMemberType.VECTOR2 => Type("Vector2"),
            ParMemberType.VECTOR3 => Type("Vector3"),
            ParMemberType.VECTOR4 => Type("Vector4"),
            ParMemberType.STRING => Keyword("string"),
            ParMemberType.STRUCT => Keyword("struct"),
            ParMemberType.ARRAY => Keyword("array"),
            ParMemberType.ENUM => Keyword("enum"),
            ParMemberType.BITSET => Keyword("bitset"),
            ParMemberType.MAP => Keyword("map"),
            ParMemberType.MATRIX34 => Type("Matrix34"),
            ParMemberType.MATRIX44 => Type("Matrix44"),
            ParMemberType.VEC2V => Type("Vec2V"),
            ParMemberType.VEC3V => Type("Vec3V"),
            ParMemberType.VEC4V => Type("Vec4V"),
            ParMemberType.MAT33V => Type("Mat33V"),
            ParMemberType.MAT34V => Type("Mat34V"),
            ParMemberType.MAT44V => Type("Mat44V"),
            ParMemberType.SCALARV => Type("ScalarV"),
            ParMemberType.BOOLV => Type("BoolV"),
            ParMemberType.VECBOOLV => Type("VecBoolV"),
            ParMemberType.PTRDIFFT => Keyword("ptrdiff_t"),
            ParMemberType.SIZET => Keyword("size_t"),
            ParMemberType.FLOAT16 => Keyword("float16"),
            ParMemberType.INT64 => Keyword("int64"),
            ParMemberType.UINT64 => Keyword("uint64"),
            ParMemberType.DOUBLE => Keyword("double"),
            ParMemberType.GUID => Keyword("guid"),
            ParMemberType.VEC2F => Type("Vec2f"),
            ParMemberType.QUATV => Type("QuatV"),
            _ => "UNKNOWN",
        };

    private static string Keyword(string keyword) => $"<span class=\"c-k\">{keyword}</span>";
    private static string Type(string type) => $"<span class=\"c-t\">{type}</span>";
    private static string TypeLink(Name name) => $"<a href=\"#{name.Hash:X08}\" class=\"c-t-l\">{Type(name.ToFormattedString())}</a>";
}
