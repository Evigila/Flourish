param(
    [string]$Source = (Join-Path $PSScriptRoot '../src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/icons/material/MaterialSymbolsOutlined.woff2'),
    [string]$Destination = (Join-Path $PSScriptRoot '../src/Flourish.WPF/Flourish.WPF.Framework/Icons/MaterialSymbolsOutlined.ttf'),
    [string]$Codepoints = (Join-Path $PSScriptRoot '../src/Flourish.WPF/Flourish.WPF.Framework/Icons/MaterialSymbolsOutlined.codepoints'),
    [string]$FilledDestination = (Join-Path $PSScriptRoot '../src/Flourish.WPF/Flourish.WPF.Framework/Icons/MaterialSymbolsFilled.json.br')
)

$ErrorActionPreference = 'Stop'
# Windows supplies the WOFF2 decoder; no downloaded converter or runtime package is required.
# IDWriteFactory5::UnpackFontFile: https://learn.microsoft.com/windows/win32/api/dwrite_3/nf-dwrite_3-idwritefactory5-unpackfontfile
# IDWriteFactory6::CreateFontFaceReference: https://learn.microsoft.com/windows/win32/api/dwrite_3/nf-dwrite_3-idwritefactory6-createfontfacereference
$nativeSource = @'
using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

public static class NativeFontContainer
{
    [DllImport("dwrite.dll", ExactSpelling = true)]
    private static extern int DWriteCreateFactory(uint type, ref Guid iid, out IntPtr factory);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int Unpack(IntPtr self, uint container, IntPtr bytes, uint size, out IntPtr stream);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int GetSize(IntPtr self, out ulong size);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int Read(IntPtr self, out IntPtr fragment, ulong offset, ulong size, out IntPtr context);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate void ReleaseFragment(IntPtr self, IntPtr context);

    private static T Method<T>(IntPtr instance, int slot) where T : Delegate
        => Marshal.GetDelegateForFunctionPointer<T>(Marshal.ReadIntPtr(Marshal.ReadIntPtr(instance), slot * IntPtr.Size));

    public static void Convert(string source, string destination)
    {
        byte[] bytes = File.ReadAllBytes(source);
        if (bytes.Length < 4 || bytes[0] != 0x77 || bytes[1] != 0x4f || bytes[2] != 0x46 || bytes[3] != 0x32)
            throw new InvalidDataException("The source is not a WOFF2 container.");
        Guid iid = new Guid("958DB99A-BE2A-4F09-AF7D-65189803D1D3");
        IntPtr factory = IntPtr.Zero, stream = IntPtr.Zero, context = IntPtr.Zero;
        GCHandle pin = default(GCHandle);
        bool read = false;
        try
        {
            Marshal.ThrowExceptionForHR(DWriteCreateFactory(0, ref iid, out factory));
            pin = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            // IUnknown(3), Factory(21), Factory1(2), Factory2(5), Factory3(9), Factory4(3), Factory5(5).
            Marshal.ThrowExceptionForHR(Method<Unpack>(factory, 47)(factory, 2, pin.AddrOfPinnedObject(), checked((uint)bytes.Length), out stream));
            ulong length;
            Marshal.ThrowExceptionForHR(Method<GetSize>(stream, 5)(stream, out length));
            IntPtr fragment;
            Marshal.ThrowExceptionForHR(Method<Read>(stream, 3)(stream, out fragment, 0, length, out context));
            read = true;
            byte[] unpacked = new byte[checked((int)length)];
            Marshal.Copy(fragment, unpacked, 0, unpacked.Length);
            if (unpacked.Length < 12 || !((unpacked[0] == 0 && unpacked[1] == 1 && unpacked[2] == 0 && unpacked[3] == 0)
                || (unpacked[0] == 0x4f && unpacked[1] == 0x54 && unpacked[2] == 0x54 && unpacked[3] == 0x4f)))
                throw new InvalidDataException("DirectWrite did not return an sfnt font.");
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(destination)));
            File.WriteAllBytes(destination, unpacked);
        }
        finally
        {
            if (read && stream != IntPtr.Zero) Method<ReleaseFragment>(stream, 4)(stream, context);
            if (stream != IntPtr.Zero) Marshal.Release(stream);
            if (factory != IntPtr.Zero) Marshal.Release(factory);
            if (pin.IsAllocated) pin.Free();
        }
    }
}

[StructLayout(LayoutKind.Sequential)]
public struct NativePoint { public float X; public float Y; }
[StructLayout(LayoutKind.Sequential)]
public struct NativeBezier { public NativePoint P1; public NativePoint P2; public NativePoint P3; }
[StructLayout(LayoutKind.Sequential)]
public struct FontAxis { public uint Tag; public float Value; }

[ComVisible(true), Guid("2cd9069e-12e2-11dc-9fed-001143a055f9"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface INativeOutlineSink
{
    [PreserveSig] void SetFillMode(uint mode);
    [PreserveSig] void SetSegmentFlags(uint flags);
    [PreserveSig] void BeginFigure(NativePoint point, uint begin);
    [PreserveSig] void AddLines(IntPtr points, uint count);
    [PreserveSig] void AddBeziers(IntPtr segments, uint count);
    [PreserveSig] void EndFigure(uint end);
    [PreserveSig] int Close();
}

[ComVisible(true), ClassInterface(ClassInterfaceType.None)]
public sealed class NativeOutlineSink : INativeOutlineSink
{
    private readonly StringBuilder path = new StringBuilder();
    private uint fillMode;
    private static string Number(float value) => value.ToString("R", CultureInfo.InvariantCulture);
    private void Point(NativePoint point) => path.Append(Number(point.X)).Append(',').Append(Number(point.Y)).Append(' ');
    public void SetFillMode(uint mode) { fillMode = mode; }
    public void SetSegmentFlags(uint flags) { }
    public void BeginFigure(NativePoint point, uint begin) { path.Append("M "); Point(point); }
    public void AddLines(IntPtr points, uint count)
    {
        for (uint index = 0; index < count; index++) { path.Append("L "); Point(Marshal.PtrToStructure<NativePoint>(IntPtr.Add(points, checked((int)index * 8)))); }
    }
    public void AddBeziers(IntPtr segments, uint count)
    {
        for (uint index = 0; index < count; index++)
        {
            var segment = Marshal.PtrToStructure<NativeBezier>(IntPtr.Add(segments, checked((int)index * 24)));
            path.Append("C "); Point(segment.P1); Point(segment.P2); Point(segment.P3);
        }
    }
    public void EndFigure(uint end) { if (end == 1) path.Append("Z "); }
    public int Close() => 0;
    public override string ToString() => (fillMode == 1 ? "F1 " : "F0 ") + path.ToString().TrimEnd();
}

public static class NativeIconOutlines
{
    [DllImport("dwrite.dll", ExactSpelling = true)]
    private static extern int DWriteCreateFactory(uint type, ref Guid iid, out IntPtr factory);
    [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    private delegate int CreateFile(IntPtr self, string path, IntPtr lastWriteTime, out IntPtr file);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int CreateReference(IntPtr self, IntPtr file, uint index, uint simulations, [In] FontAxis[] axes, uint count, out IntPtr reference);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int CreateFace(IntPtr self, out IntPtr face);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int GetGlyphs(IntPtr self, [In] uint[] codepoints, uint count, [Out] ushort[] glyphs);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int GetOutline(IntPtr self, float emSize, [In] ushort[] glyphs, IntPtr advances, IntPtr offsets, uint count, int sideways, int rightToLeft, IntPtr sink);
    private static T Method<T>(IntPtr instance, int slot) where T : Delegate
        => Marshal.GetDelegateForFunctionPointer<T>(Marshal.ReadIntPtr(Marshal.ReadIntPtr(instance), slot * IntPtr.Size));
    private static uint Tag(string value) => (uint)value[0] | ((uint)value[1] << 8) | ((uint)value[2] << 16) | ((uint)value[3] << 24);

    public static Dictionary<string, string> Extract(string fontPath, string codepointsPath)
    {
        Guid iid = new Guid("F3744D80-21F7-42EB-B35D-995BC72FC223");
        IntPtr factory = IntPtr.Zero, file = IntPtr.Zero, reference = IntPtr.Zero, face = IntPtr.Zero;
        try
        {
            Marshal.ThrowExceptionForHR(DWriteCreateFactory(0, ref iid, out factory));
            Marshal.ThrowExceptionForHR(Method<CreateFile>(factory, 7)(factory, fontPath, IntPtr.Zero, out file));
            var axes = new[] { new FontAxis { Tag = Tag("FILL"), Value = 1 }, new FontAxis { Tag = Tag("GRAD"), Value = 0 },
                new FontAxis { Tag = Tag("opsz"), Value = 24 }, new FontAxis { Tag = Tag("wght"), Value = 400 } };
            // Factory6 follows the 48 inherited vtable entries. Reference1 follows 14 base methods plus IUnknown.
            Marshal.ThrowExceptionForHR(Method<CreateReference>(factory, 48)(factory, file, 0, 0, axes, 4, out reference));
            Marshal.ThrowExceptionForHR(Method<CreateFace>(reference, 17)(reference, out face));
            var result = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (string line in File.ReadLines(codepointsPath))
            {
                string[] fields = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                uint codepoint = uint.Parse(fields[1], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                string key = codepoint.ToString(CultureInfo.InvariantCulture);
                if (result.ContainsKey(key)) continue;
                ushort[] glyphs = new ushort[1];
                Marshal.ThrowExceptionForHR(Method<GetGlyphs>(face, 11)(face, new[] { codepoint }, 1, glyphs));
                if (glyphs[0] == 0) throw new InvalidDataException("Official codepoint is absent: " + fields[0]);
                var sink = new NativeOutlineSink();
                IntPtr sinkPointer = Marshal.GetComInterfaceForObject(sink, typeof(INativeOutlineSink));
                try { Marshal.ThrowExceptionForHR(Method<GetOutline>(face, 14)(face, 1, glyphs, IntPtr.Zero, IntPtr.Zero, 1, 0, 0, sinkPointer)); }
                finally { Marshal.Release(sinkPointer); GC.KeepAlive(sink); }
                string outline = sink.ToString();
                if (outline.Length < 6) throw new InvalidDataException("Official filled outline is empty: " + fields[0]);
                result.Add(key, outline);
            }
            return result;
        }
        finally
        {
            if (face != IntPtr.Zero) Marshal.Release(face);
            if (reference != IntPtr.Zero) Marshal.Release(reference);
            if (file != IntPtr.Zero) Marshal.Release(file);
            if (factory != IntPtr.Zero) Marshal.Release(factory);
        }
    }
}
'@

if (-not ('NativeFontContainer' -as [type])) { Add-Type -TypeDefinition $nativeSource }
[NativeFontContainer]::Convert([IO.Path]::GetFullPath($Source), [IO.Path]::GetFullPath($Destination))
# WPF uses the default variable-font instance. Preserve Blazor's FILL=1 instance as native path data as well.
$filled = [NativeIconOutlines]::Extract([IO.Path]::GetFullPath($Destination), [IO.Path]::GetFullPath($Codepoints))
$jsonBytes = [Text.Encoding]::UTF8.GetBytes(($filled | ConvertTo-Json -Depth 3 -Compress))
$filledFile = [IO.File]::Create([IO.Path]::GetFullPath($FilledDestination))
try {
    $compressed = [IO.Compression.BrotliStream]::new($filledFile, [IO.Compression.CompressionLevel]::SmallestSize, $true)
    try { $compressed.Write($jsonBytes, 0, $jsonBytes.Length) } finally { $compressed.Dispose() }
} finally { $filledFile.Dispose() }
Get-Item -LiteralPath $Destination, $FilledDestination | Select-Object FullName, Length
