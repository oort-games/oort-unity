using System;
using System.Collections.Generic;
using System.IO;

namespace OortUnity.Utilities
{
    public static class PathUtility
    {
        #region File Name

        /// <summary>
        /// 파일명으로 사용할 수 없는 문자를 지정된 문자로 대체합니다.
        /// </summary>
        /// <param name="fileName">정리할 파일명입니다.</param>
        /// <param name="replacement">사용할 수 없는 문자를 대체할 문자입니다.</param>
        /// <returns>사용할 수 없는 문자가 대체된 파일명을 반환합니다.</returns>
        public static string SanitizeFileName(string fileName, char replacement = '_')
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return fileName;
            }

            foreach (char c in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, replacement);
            }

            return fileName;
        }

        /// <summary>
        /// 확장자가 점(.)으로 시작하도록 정규화합니다.
        /// </summary>
        /// <param name="extension">정규화할 확장자입니다.</param>
        /// <returns>점(.)으로 시작하는 확장자를 반환합니다. 값이 없으면 빈 문자열을 반환합니다.</returns>
        public static string NormalizeExtension(string extension)
        {
            if (string.IsNullOrEmpty(extension))
            {
                return string.Empty;
            }

            return extension.StartsWith(".")
                ? extension
                : "." + extension;
        }

        #endregion

        #region Path

        /// <summary>
        /// 경로 구분자를 슬래시(/)로 통일합니다.
        /// </summary>
        /// <param name="path">정규화할 경로입니다.</param>
        /// <returns>경로 구분자가 슬래시(/)로 통일된 경로를 반환합니다.</returns>
        public static string NormalizePath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }

            return path.Replace('\\', '/');
        }

        /// <summary>
        /// 경로 끝에 디렉토리 구분자가 없으면 슬래시(/)를 추가합니다.
        /// </summary>
        /// <param name="path">디렉토리 경로입니다.</param>
        /// <returns>슬래시(/)로 끝나는 디렉토리 경로를 반환합니다.</returns>
        private static string EnsureTrailingSeparator(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }

            path = NormalizePath(path);

            return path.EndsWith("/")
                ? path
                : path + "/";
        }

        /// <summary>
        /// 루트 경로를 제외하고 경로 끝의 디렉토리 구분자를 제거합니다.
        /// </summary>
        /// <param name="path">디렉토리 경로입니다.</param>
        /// <returns>끝의 디렉토리 구분자가 제거된 경로를 반환합니다.</returns>
        private static string RemoveTrailingSeparator(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }

            path = NormalizePath(path);

            string rootPath = NormalizePath(Path.GetPathRoot(path));
            int minimumLength = string.IsNullOrEmpty(rootPath)
                ? 0
                : rootPath.Length;

            while (path.Length > minimumLength && path.EndsWith("/"))
            {
                path = path.Substring(0, path.Length - 1);
            }

            return path;
        }

        #endregion

        #region Unique

        /// <summary>
        /// 지정된 폴더에서 중복되지 않는 파일명을 반환합니다.
        /// 실제 파일이나 디렉토리는 생성하지 않습니다.
        /// </summary>
        /// <param name="folderPath">파일이 위치할 폴더 경로입니다.</param>
        /// <param name="baseName">기본 파일명입니다.</param>
        /// <param name="extension">파일 확장자입니다. 점(.)은 생략할 수 있습니다.</param>
        /// <param name="reservedPaths">이미 예약되어 사용할 수 없는 전체 파일 경로 목록입니다.</param>
        /// <returns>중복되지 않는 파일명을 반환합니다.</returns>
        public static string GetUniqueFileName(
            string folderPath,
            string baseName,
            string extension,
            IEnumerable<string> reservedPaths = null)
        {
            extension = NormalizeExtension(extension);

            string safeBaseName = SanitizeFileName(baseName);

            int counter = 0;

            while (true)
            {
                string suffix = counter == 0
                    ? string.Empty
                    : $"_{counter}";

                string fileName = $"{safeBaseName}{suffix}{extension}";
                string fullPath = Path.Combine(folderPath, fileName);

                if (!File.Exists(fullPath) &&
                    !Directory.Exists(fullPath) &&
                    !ContainsPath(reservedPaths, fullPath))
                {
                    return fileName;
                }

                counter++;
            }
        }

        /// <summary>
        /// 지정된 폴더에서 중복되지 않는 전체 파일 경로를 반환합니다.
        /// 실제 파일이나 디렉토리는 생성하지 않습니다.
        /// </summary>
        /// <param name="folderPath">파일이 위치할 폴더 경로입니다.</param>
        /// <param name="baseName">기본 파일명입니다.</param>
        /// <param name="extension">파일 확장자입니다. 점(.)은 생략할 수 있습니다.</param>
        /// <param name="reservedPaths">이미 예약되어 사용할 수 없는 전체 파일 경로 목록입니다.</param>
        /// <returns>중복되지 않는 전체 파일 경로를 반환합니다.</returns>
        public static string GetUniqueFilePath(
            string folderPath,
            string baseName,
            string extension,
            IEnumerable<string> reservedPaths = null)
        {
            string fileName = GetUniqueFileName(
                folderPath,
                baseName,
                extension,
                reservedPaths);

            return NormalizePath(Path.Combine(folderPath, fileName));
        }

        #endregion

        #region Comparison

        /// <summary>
        /// 두 경로가 동일한 위치를 가리키는지 확인합니다.
        /// 상대 경로와 경로 구분자 차이를 정규화한 뒤 비교합니다.
        /// </summary>
        /// <param name="pathA">비교할 첫 번째 경로입니다.</param>
        /// <param name="pathB">비교할 두 번째 경로입니다.</param>
        /// <returns>두 경로가 동일하면 true, 그렇지 않으면 false를 반환합니다.</returns>
        public static bool IsSamePath(string pathA, string pathB)
        {
            if (string.IsNullOrEmpty(pathA) ||
                string.IsNullOrEmpty(pathB))
            {
                return false;
            }

            string fullPathA = NormalizePath(Path.GetFullPath(pathA));
            string fullPathB = NormalizePath(Path.GetFullPath(pathB));

            fullPathA = RemoveTrailingSeparator(fullPathA);
            fullPathB = RemoveTrailingSeparator(fullPathB);

            return string.Equals(
                fullPathA,
                fullPathB,
                GetPathComparison());
        }

        /// <summary>
        /// 지정된 경로가 부모 경로의 하위에 위치하는지 확인합니다.
        /// 동일한 경로는 하위 경로로 판단하지 않습니다.
        /// </summary>
        /// <param name="path">확인할 경로입니다.</param>
        /// <param name="parentPath">부모 경로입니다.</param>
        /// <returns>지정된 경로가 부모 경로의 하위에 있으면 true, 그렇지 않으면 false를 반환합니다.</returns>
        public static bool IsSubPathOf(string path, string parentPath)
        {
            if (string.IsNullOrEmpty(path) ||
                string.IsNullOrEmpty(parentPath))
            {
                return false;
            }

            string fullPath = NormalizePath(Path.GetFullPath(path));
            string fullParentPath = NormalizePath(Path.GetFullPath(parentPath));

            fullPath = RemoveTrailingSeparator(fullPath);
            fullParentPath = RemoveTrailingSeparator(fullParentPath);

            if (string.Equals(
                fullPath,
                fullParentPath,
                GetPathComparison()))
            {
                return false;
            }

            fullParentPath = EnsureTrailingSeparator(fullParentPath);

            return fullPath.StartsWith(
                fullParentPath,
                GetPathComparison());
        }

        /// <summary>
        /// 지정된 경로 목록에 대상 경로가 포함되어 있는지 확인합니다.
        /// </summary>
        /// <param name="paths">확인할 경로 목록입니다.</param>
        /// <param name="targetPath">찾을 대상 경로입니다.</param>
        /// <returns>대상 경로가 포함되어 있으면 true, 그렇지 않으면 false를 반환합니다.</returns>
        private static bool ContainsPath(
            IEnumerable<string> paths,
            string targetPath)
        {
            if (paths == null)
            {
                return false;
            }

            foreach (string path in paths)
            {
                if (string.IsNullOrEmpty(path))
                {
                    continue;
                }

                if (IsSamePath(path, targetPath))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Windows에서는 대소문자를 무시하고,
        /// 그 외 플랫폼에서는 대소문자를 구분하는 경로 비교 방식을 반환합니다.
        /// </summary>
        /// <returns>경로 비교에 사용할 문자열 비교 방식을 반환합니다.</returns>
        private static StringComparison GetPathComparison()
        {
            return Path.DirectorySeparatorChar == '\\'
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;
        }

        #endregion
    }
}