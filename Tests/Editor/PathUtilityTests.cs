using System;
using System.IO;
using NUnit.Framework;
using OortUnity.Utilities;

namespace OortUnity.Tests
{
    public class PathUtilityTests
    {
        private string _testDirectory;

        #region Setup

        [SetUp]
        public void SetUp()
        {
            _testDirectory = Path.Combine(
                Path.GetTempPath(),
                "OortUnityTests",
                Guid.NewGuid().ToString());

            Directory.CreateDirectory(_testDirectory);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, true);
            }
        }

        #endregion

        #region NormalizeExtension

        [TestCase("png", ".png")]
        [TestCase(".png", ".png")]
        [TestCase("json", ".json")]
        [TestCase("", "")]
        [TestCase(null, "")]
        public void NormalizeExtension_ReturnsNormalizedExtension(
            string extension,
            string expected)
        {
            string result = PathUtility.NormalizeExtension(extension);

            Assert.AreEqual(expected, result);
        }

        #endregion

        #region NormalizePath

        [Test]
        public void NormalizePath_ReplacesBackslashes()
        {
            string result = PathUtility.NormalizePath(
                @"Assets\Editor\Tools\Screenshot");

            Assert.AreEqual(
                "Assets/Editor/Tools/Screenshot",
                result);
        }

        #endregion

        #region IsSamePath

        [Test]
        public void IsSamePath_ReturnsTrue_ForSamePath()
        {
            string pathA = Path.Combine(_testDirectory, "Folder");
            string pathB = Path.Combine(_testDirectory, "Folder") + "/";

            bool result = PathUtility.IsSamePath(pathA, pathB);

            Assert.IsTrue(result);
        }

        [Test]
        public void IsSamePath_ReturnsFalse_ForDifferentPaths()
        {
            string pathA = Path.Combine(_testDirectory, "FolderA");
            string pathB = Path.Combine(_testDirectory, "FolderB");

            bool result = PathUtility.IsSamePath(pathA, pathB);

            Assert.IsFalse(result);
        }

        #endregion

        #region IsSubPathOf

        [Test]
        public void IsSubPathOf_ReturnsTrue_ForChildPath()
        {
            string parentPath = Path.Combine(_testDirectory, "Parent");
            string childPath = Path.Combine(parentPath, "Child");

            bool result = PathUtility.IsSubPathOf(
                childPath,
                parentPath);

            Assert.IsTrue(result);
        }

        [Test]
        public void IsSubPathOf_ReturnsFalse_ForSamePath()
        {
            string path = Path.Combine(_testDirectory, "Folder");

            bool result = PathUtility.IsSubPathOf(
                path,
                path);

            Assert.IsFalse(result);
        }

        #endregion

        #region Unique File Name

        [Test]
        public void GetUniqueFileName_ReturnsBaseName_WhenFileDoesNotExist()
        {
            string result = PathUtility.GetUniqueFileName(
                _testDirectory,
                "Screenshot",
                "png");

            Assert.AreEqual(
                "Screenshot.png",
                result);
        }

        [Test]
        public void GetUniqueFileName_AddsSuffix_WhenFileExists()
        {
            string existingPath = Path.Combine(
                _testDirectory,
                "Screenshot.png");

            File.WriteAllText(existingPath, string.Empty);

            string result = PathUtility.GetUniqueFileName(
                _testDirectory,
                "Screenshot",
                "png");

            Assert.AreEqual(
                "Screenshot_1.png",
                result);
        }

        [Test]
        public void GetUniqueFileName_IncrementsSuffix_WhenMultipleFilesExist()
        {
            File.WriteAllText(
                Path.Combine(_testDirectory, "Screenshot.png"),
                string.Empty);

            File.WriteAllText(
                Path.Combine(_testDirectory, "Screenshot_1.png"),
                string.Empty);

            File.WriteAllText(
                Path.Combine(_testDirectory, "Screenshot_2.png"),
                string.Empty);

            string result = PathUtility.GetUniqueFileName(
                _testDirectory,
                "Screenshot",
                "png");

            Assert.AreEqual(
                "Screenshot_3.png",
                result);
        }

        #endregion
    }
}