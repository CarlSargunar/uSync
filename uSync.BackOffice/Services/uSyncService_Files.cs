﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;

using uSync.Core;

namespace uSync.BackOffice;

public partial class uSyncService
{

    public MemoryStream CompressFolder(string folder)
    {
        if (!Directory.Exists(folder))
            throw new DirectoryNotFoundException(folder);

        var directoryInfo = new DirectoryInfo(folder);
        var files = directoryInfo.GetFiles("*.*", SearchOption.AllDirectories);

        var stream = new MemoryStream();
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
        {
            foreach (var file in files)
            {
                var relativePath = GetRelativePath(folder, file.FullName);
                archive.CreateEntryFromFile(file.FullName, relativePath);
            }
        }

        stream.Seek(0, SeekOrigin.Begin);
        return stream;
    }

    public void DeCompressFile(string zipArchive, string target)
    {
        if (string.IsNullOrWhiteSpace(target))
            throw new ArgumentException("No path");

        if (zipArchive == null || !File.Exists(zipArchive))
            throw new ArgumentException("missing zip");

        var resolvedTarget = _syncFileService.GetAbsPath(target);

        using (var zip = ZipFile.OpenRead(zipArchive))
        {
            if (!zip.Entries.Any(x => x.FullName.EndsWith(_uSyncConfig.Settings.DefaultExtension)))
                throw new Exception("contains no uSync files");

            foreach (var entry in zip.Entries)
            {
                // things that might be folders. 
                if (entry.Length == 0) continue;

                var filepath = GetOSDependentPath(entry.FullName);
                var destination = Path.Combine(resolvedTarget, filepath);

                var destinationFolder = Path.GetDirectoryName(destination);

                if (!Directory.Exists(destinationFolder))
                    Directory.CreateDirectory(destinationFolder);

                entry.ExtractToFile(destination, true);
            }
        }
    }

    public void ReplaceFiles(string source, string target, bool clean)
    {
        if (clean)
            _syncFileService.DeleteFolder(target);

        _syncFileService.CopyFolder(source, target);
    }


    private string GetRelativePath(string root, string file)
    {

        var cleanRoot = CleanPathForZip(root);
        var cleanFile = CleanPathForZip(file);

        if (cleanFile.Length <= cleanRoot.Length || !cleanFile.StartsWith(cleanRoot))
            throw new ArgumentException($"Mismatch {root} is not parent of {file}");

        return cleanFile.Substring(cleanRoot.Length).TrimStart(Path.DirectorySeparatorChar);
    }

    private string GetOSDependentPath(string file)
        => file.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar)
            .Trim(Path.DirectorySeparatorChar);
    
    private string CleanPathForZip(string path)
        => Path.GetFullPath(
            path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar))
            .TrimEnd(Path.DirectorySeparatorChar);


}