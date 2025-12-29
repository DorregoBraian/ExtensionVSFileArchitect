using FileArchitectLibrary.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileArchitectLibrary.Models.UI
{
    public class GenerationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string GeneratedPath { get; set; } = string.Empty;
        public List<GeneratedFile> GeneratedFiles { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public List<string> Errors { get; set; } = new();
        public TimeSpan GenerationTime { get; set; }
    }

    public class GeneratedFile
    {
        public string Path { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public long SizeInBytes { get; set; }
        public DateTime CreatedAt { get; set; }
        public FileType Type { get; set; }
    }
}
