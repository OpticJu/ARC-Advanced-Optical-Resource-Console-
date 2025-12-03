using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ARC.FileRouting
{
    public static class TagRouter
    {
        private static readonly Regex TagRegex = new Regex(@"\[([^\]]+)\]", RegexOptions.Compiled);

        private static string? ExtractTag(string filename)
        {
            var m = TagRegex.Match(filename);
            return m.Success ? m.Groups[1].Value.Trim() : null;
        }

        private static string SanitizeName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "_";
            // Normalize unicode
            name = name.Normalize(NormalizationForm.FormKD);
            // Replace invalid filename chars with underscore
            var invalid = Path.GetInvalidFileNameChars();
            var sb = new StringBuilder();
            foreach (var ch in name)
            {
                if (invalid.Contains(ch))
                    sb.Append('_');
                else
                    sb.Append(ch);
            }
            var sanitized = sb.ToString();
            // Collapse multiple underscores/spaces and trim
            sanitized = Regex.Replace(sanitized, @"[_\s]{2,}", " ").Trim();
            if (string.IsNullOrEmpty(sanitized)) return "_";
            return sanitized;
        }

        private static string GetUniqueDestination(string destFilePath)
        {
            if (!File.Exists(destFilePath) && !Directory.Exists(destFilePath)) return destFilePath;

            var dir = Path.GetDirectoryName(destFilePath) ?? "";
            var name = Path.GetFileNameWithoutExtension(destFilePath);
            var ext = Path.GetExtension(destFilePath);
            int counter = 1;
            string candidate;
            do
            {
                candidate = Path.Combine(dir, $"{name} ({counter}){ext}");
                counter++;
            } while (File.Exists(candidate) || Directory.Exists(candidate));
            return candidate;
        }

        /// <summary>
        /// 파일 이름에 포함된 [Tag]를 추출하여 지정된 폴더로 이동(또는 복사)합니다.
        /// </summary>
        /// <param name="srcPath">소스 파일 경로</param>
        /// <param name="destRoot">대상 루트 디렉터리</param>
        /// <param name="tagMap">태그 문자열 -> 폴더명 매핑 (예: "Setup" -> "Setup")</param>
        /// <param name="defaultFolder">태그가 없을 때 사용할 기본 폴더명</param>
        /// <param name="move">true면 이동, false면 복사</param>
        /// <returns>최종 대상 파일 경로</returns>
        public static string RouteFile(
            string srcPath,
            string destRoot,
            IDictionary<string, string>? tagMap = null,
            string defaultFolder = "Unsorted",
            bool move = true)
        {
            if (string.IsNullOrWhiteSpace(srcPath)) throw new ArgumentException("srcPath is required", nameof(srcPath));
            if (string.IsNullOrWhiteSpace(destRoot)) throw new ArgumentException("destRoot is required", nameof(destRoot));

            var src = Path.GetFullPath(srcPath);
            if (!File.Exists(src)) throw new FileNotFoundException("Source file not found", src);

            var filename = Path.GetFileName(src);
            var tag = ExtractTag(filename);
            var folderName = tag != null
                ? (tagMap != null && tagMap.ContainsKey(tag) ? tagMap[tag] : tag)
                : defaultFolder;

            folderName = SanitizeName(folderName);
            var destDir = Path.Combine(Path.GetFullPath(destRoot), folderName);
            Directory.CreateDirectory(destDir);

            var sanitizedFilename = SanitizeName(Path.GetFileNameWithoutExtension(filename)) + Path.GetExtension(filename);
            var destCandidate = Path.Combine(destDir, sanitizedFilename);
            var finalDest = GetUniqueDestination(destCandidate);

            if (move)
            {
                File.Move(src, finalDest);
            }
            else
            {
                File.Copy(src, finalDest);
            }

            return finalDest;
        }
    }
}