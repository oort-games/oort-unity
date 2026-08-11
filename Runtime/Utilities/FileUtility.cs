using System;
using System.IO;

namespace OortUnity.Utilities
{
    public static class FileUtility
    {
        #region Directory

        /// <summary>
        /// 디렉토리가 존재하지 않으면 생성합니다.
        /// </summary>
        /// <param name="directoryPath">생성할 디렉토리 경로입니다.</param>
        public static void CreateDirectory(string directoryPath)
        {
            if (string.IsNullOrEmpty(directoryPath))
            {
                return;
            }

            Directory.CreateDirectory(directoryPath);
        }

        /// <summary>
        /// 디렉토리 생성을 시도하고 성공 여부를 반환합니다.
        /// </summary>
        /// <param name="directoryPath">생성할 디렉토리 경로입니다.</param>
        /// <returns>디렉토리가 존재하거나 정상적으로 생성되었으면 true, 그렇지 않으면 false를 반환합니다.</returns>
        public static bool TryCreateDirectory(string directoryPath)
        {
            if (string.IsNullOrEmpty(directoryPath))
            {
                return false;
            }

            try
            {
                CreateDirectory(directoryPath);
                return true;
            }
            catch (IOException)
            {
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }

        /// <summary>
        /// 파일 경로의 상위 디렉토리가 존재하지 않으면 생성합니다.
        /// </summary>
        /// <param name="filePath">상위 디렉토리를 생성할 파일 경로입니다.</param>
        public static void CreateParentDirectory(string filePath)
        {
            string directoryPath = Path.GetDirectoryName(filePath);

            if (!string.IsNullOrEmpty(directoryPath))
            {
                CreateDirectory(directoryPath);
            }
        }

        /// <summary>
        /// 파일 경로의 상위 디렉토리 생성을 시도하고 성공 여부를 반환합니다.
        /// </summary>
        /// <param name="filePath">상위 디렉토리를 생성할 파일 경로입니다.</param>
        /// <returns>상위 디렉토리가 필요하지 않거나 정상적으로 생성되었으면 true, 그렇지 않으면 false를 반환합니다.</returns>
        public static bool TryCreateParentDirectory(string filePath)
        {
            try
            {
                CreateParentDirectory(filePath);
                return true;
            }
            catch (IOException)
            {
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }

        #endregion

        #region Read

        /// <summary>
        /// 텍스트 파일의 모든 내용을 읽습니다.
        /// </summary>
        /// <param name="filePath">읽을 파일 경로입니다.</param>
        /// <returns>파일의 전체 텍스트 내용입니다.</returns>
        public static string ReadAllText(string filePath)
        {
            return File.ReadAllText(filePath);
        }

        /// <summary>
        /// 텍스트 파일 읽기를 시도하고 성공 여부를 반환합니다.
        /// </summary>
        /// <param name="filePath">읽을 파일 경로입니다.</param>
        /// <param name="contents">읽기에 성공한 경우 파일의 전체 텍스트 내용입니다.</param>
        /// <returns>파일을 정상적으로 읽었으면 true, 그렇지 않으면 false를 반환합니다.</returns>
        public static bool TryReadAllText(string filePath, out string contents)
        {
            try
            {
                contents = ReadAllText(filePath);
                return true;
            }
            catch (IOException)
            {
                contents = null;
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                contents = null;
                return false;
            }
        }

        /// <summary>
        /// 바이너리 파일의 모든 내용을 읽습니다.
        /// </summary>
        /// <param name="filePath">읽을 파일 경로입니다.</param>
        /// <returns>파일의 전체 바이트 배열입니다.</returns>
        public static byte[] ReadAllBytes(string filePath)
        {
            return File.ReadAllBytes(filePath);
        }

        /// <summary>
        /// 바이너리 파일 읽기를 시도하고 성공 여부를 반환합니다.
        /// </summary>
        /// <param name="filePath">읽을 파일 경로입니다.</param>
        /// <param name="bytes">읽기에 성공한 경우 파일의 전체 바이트 배열입니다.</param>
        /// <returns>파일을 정상적으로 읽었으면 true, 그렇지 않으면 false를 반환합니다.</returns>
        public static bool TryReadAllBytes(string filePath, out byte[] bytes)
        {
            try
            {
                bytes = ReadAllBytes(filePath);
                return true;
            }
            catch (IOException)
            {
                bytes = null;
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                bytes = null;
                return false;
            }
        }

        #endregion

        #region Write

        /// <summary>
        /// 필요한 디렉토리를 생성한 뒤 텍스트 파일을 저장합니다.
        /// </summary>
        /// <param name="filePath">저장할 파일 경로입니다.</param>
        /// <param name="contents">파일에 저장할 텍스트 내용입니다.</param>
        public static void WriteAllText(string filePath, string contents)
        {
            CreateParentDirectory(filePath);
            File.WriteAllText(filePath, contents);
        }

        /// <summary>
        /// 필요한 디렉토리를 생성한 뒤 텍스트 파일 저장을 시도하고 성공 여부를 반환합니다.
        /// </summary>
        /// <param name="filePath">저장할 파일 경로입니다.</param>
        /// <param name="contents">파일에 저장할 텍스트 내용입니다.</param>
        /// <returns>파일을 정상적으로 저장했으면 true, 그렇지 않으면 false를 반환합니다.</returns>
        public static bool TryWriteAllText(string filePath, string contents)
        {
            try
            {
                WriteAllText(filePath, contents);
                return true;
            }
            catch (IOException)
            {
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }

        /// <summary>
        /// 필요한 디렉토리를 생성한 뒤 바이너리 파일을 저장합니다.
        /// </summary>
        /// <param name="filePath">저장할 파일 경로입니다.</param>
        /// <param name="bytes">파일에 저장할 바이트 배열입니다.</param>
        public static void WriteAllBytes(string filePath, byte[] bytes)
        {
            CreateParentDirectory(filePath);
            File.WriteAllBytes(filePath, bytes);
        }

        /// <summary>
        /// 필요한 디렉토리를 생성한 뒤 바이너리 파일 저장을 시도하고 성공 여부를 반환합니다.
        /// </summary>
        /// <param name="filePath">저장할 파일 경로입니다.</param>
        /// <param name="bytes">파일에 저장할 바이트 배열입니다.</param>
        /// <returns>파일을 정상적으로 저장했으면 true, 그렇지 않으면 false를 반환합니다.</returns>
        public static bool TryWriteAllBytes(string filePath, byte[] bytes)
        {
            try
            {
                WriteAllBytes(filePath, bytes);
                return true;
            }
            catch (IOException)
            {
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }

        /// <summary>
        /// 필요한 디렉토리를 생성한 뒤 파일 끝에 텍스트를 추가합니다.
        /// </summary>
        /// <param name="filePath">텍스트를 추가할 파일 경로입니다.</param>
        /// <param name="contents">파일 끝에 추가할 텍스트 내용입니다.</param>
        public static void AppendAllText(string filePath, string contents)
        {
            CreateParentDirectory(filePath);
            File.AppendAllText(filePath, contents);
        }

        /// <summary>
        /// 필요한 디렉토리를 생성한 뒤 파일 끝에 텍스트 추가를 시도하고 성공 여부를 반환합니다.
        /// </summary>
        /// <param name="filePath">텍스트를 추가할 파일 경로입니다.</param>
        /// <param name="contents">파일 끝에 추가할 텍스트 내용입니다.</param>
        /// <returns>텍스트를 정상적으로 추가했으면 true, 그렇지 않으면 false를 반환합니다.</returns>
        public static bool TryAppendAllText(string filePath, string contents)
        {
            try
            {
                AppendAllText(filePath, contents);
                return true;
            }
            catch (IOException)
            {
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }

        #endregion

        #region Copy

        /// <summary>
        /// 필요한 디렉토리를 생성한 뒤 파일을 복사합니다.
        /// </summary>
        /// <param name="sourceFilePath">복사할 원본 파일 경로입니다.</param>
        /// <param name="destinationFilePath">복사할 대상 파일 경로입니다.</param>
        /// <param name="overwrite">대상 파일이 이미 존재할 경우 덮어쓸지 여부입니다.</param>
        public static void Copy(
            string sourceFilePath,
            string destinationFilePath,
            bool overwrite = false)
        {
            CreateParentDirectory(destinationFilePath);
            File.Copy(sourceFilePath, destinationFilePath, overwrite);
        }

        /// <summary>
        /// 파일 복사를 시도하고 성공 여부를 반환합니다.
        /// </summary>
        /// <param name="sourceFilePath">복사할 원본 파일 경로입니다.</param>
        /// <param name="destinationFilePath">복사할 대상 파일 경로입니다.</param>
        /// <param name="overwrite">대상 파일이 이미 존재할 경우 덮어쓸지 여부입니다.</param>
        /// <returns>파일을 정상적으로 복사했으면 true, 그렇지 않으면 false를 반환합니다.</returns>
        public static bool TryCopy(
            string sourceFilePath,
            string destinationFilePath,
            bool overwrite = false)
        {
            try
            {
                Copy(sourceFilePath, destinationFilePath, overwrite);
                return true;
            }
            catch (IOException)
            {
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }

        #endregion

        #region Move

        /// <summary>
        /// 필요한 디렉토리를 생성한 뒤 파일을 이동합니다.
        /// </summary>
        /// <param name="sourceFilePath">이동할 원본 파일 경로입니다.</param>
        /// <param name="destinationFilePath">이동할 대상 파일 경로입니다.</param>
        public static void Move(string sourceFilePath, string destinationFilePath)
        {
            CreateParentDirectory(destinationFilePath);
            File.Move(sourceFilePath, destinationFilePath);
        }

        /// <summary>
        /// 파일 이동을 시도하고 성공 여부를 반환합니다.
        /// </summary>
        /// <param name="sourceFilePath">이동할 원본 파일 경로입니다.</param>
        /// <param name="destinationFilePath">이동할 대상 파일 경로입니다.</param>
        /// <returns>파일을 정상적으로 이동했으면 true, 그렇지 않으면 false를 반환합니다.</returns>
        public static bool TryMove(string sourceFilePath, string destinationFilePath)
        {
            try
            {
                Move(sourceFilePath, destinationFilePath);
                return true;
            }
            catch (IOException)
            {
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }

        #endregion

        #region Delete

        /// <summary>
        /// 파일을 삭제합니다.
        /// </summary>
        /// <param name="filePath">삭제할 파일 경로입니다.</param>
        public static void Delete(string filePath)
        {
            File.Delete(filePath);
        }

        /// <summary>
        /// 파일이 존재하면 삭제를 시도하고 성공 여부를 반환합니다.
        /// </summary>
        /// <param name="filePath">삭제할 파일 경로입니다.</param>
        /// <returns>파일을 정상적으로 삭제했으면 true, 그렇지 않으면 false를 반환합니다.</returns>
        public static bool TryDelete(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return false;
            }

            try
            {
                Delete(filePath);
                return true;
            }
            catch (IOException)
            {
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }

        #endregion

        #region Info

        /// <summary>
        /// 파일의 존재 여부를 반환합니다.
        /// </summary>
        /// <param name="filePath">확인할 파일 경로입니다.</param>
        /// <returns>파일이 존재하면 true, 그렇지 않으면 false를 반환합니다.</returns>
        public static bool Exists(string filePath)
        {
            return File.Exists(filePath);
        }

        /// <summary>
        /// 파일 크기를 바이트 단위로 반환합니다.
        /// </summary>
        /// <param name="filePath">크기를 확인할 파일 경로입니다.</param>
        /// <returns>파일 크기를 바이트 단위로 반환합니다.</returns>
        public static long GetSize(string filePath)
        {
            return new FileInfo(filePath).Length;
        }

        /// <summary>
        /// 파일 크기 조회를 시도하고 성공 여부를 반환합니다.
        /// </summary>
        /// <param name="filePath">크기를 확인할 파일 경로입니다.</param>
        /// <param name="size">조회에 성공한 경우 파일 크기를 바이트 단위로 반환합니다.</param>
        /// <returns>파일 크기를 정상적으로 조회했으면 true, 그렇지 않으면 false를 반환합니다.</returns>
        public static bool TryGetSize(string filePath, out long size)
        {
            try
            {
                size = GetSize(filePath);
                return true;
            }
            catch (IOException)
            {
                size = 0;
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                size = 0;
                return false;
            }
        }

        /// <summary>
        /// 파일의 마지막 수정 시간을 UTC 기준으로 반환합니다.
        /// </summary>
        /// <param name="filePath">마지막 수정 시간을 확인할 파일 경로입니다.</param>
        /// <returns>파일의 마지막 수정 시간을 UTC 기준으로 반환합니다.</returns>
        public static DateTime GetLastWriteTimeUtc(string filePath)
        {
            return File.GetLastWriteTimeUtc(filePath);
        }

        /// <summary>
        /// 파일의 마지막 수정 시간 조회를 시도하고 성공 여부를 반환합니다.
        /// </summary>
        /// <param name="filePath">마지막 수정 시간을 확인할 파일 경로입니다.</param>
        /// <param name="lastWriteTimeUtc">조회에 성공한 경우 파일의 마지막 수정 시간을 UTC 기준으로 반환합니다.</param>
        /// <returns>마지막 수정 시간을 정상적으로 조회했으면 true, 그렇지 않으면 false를 반환합니다.</returns>
        public static bool TryGetLastWriteTimeUtc(
            string filePath,
            out DateTime lastWriteTimeUtc)
        {
            if (!File.Exists(filePath))
            {
                lastWriteTimeUtc = default;
                return false;
            }

            try
            {
                lastWriteTimeUtc = GetLastWriteTimeUtc(filePath);
                return true;
            }
            catch (IOException)
            {
                lastWriteTimeUtc = default;
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                lastWriteTimeUtc = default;
                return false;
            }
        }

        #endregion
    }
}