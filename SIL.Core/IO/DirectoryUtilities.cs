﻿// Copyright (c) 2017 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using SIL.PlatformUtilities;
using SIL.Reporting;

namespace SIL.IO
{
	public static class DirectoryUtilities
	{

		/// <summary>
		/// Makes a full copy of the specified directory in the system's temporary directory.
		/// If the copy fails at any point in the process, the user is notified of the
		/// problem and an attempt is made to remove the destination directory if the failure
		/// happened part way through the process.
		/// </summary>
		/// <param name="srcDirectory">Directory to copy</param>
		/// <returns>Null if the copy was unsuccessful, otherwise the path to the copied directory</returns>

		public static string CopyDirectoryToTempDirectory(string srcDirectory)
		{
			string dstDirectory;
			return (CopyDirectory(srcDirectory, Path.GetTempPath(), out dstDirectory) ? dstDirectory : null);
		}


		/// <summary>
		/// Makes a copy of the specifed source directory and its contents in the specified
		/// destination directory. The copy has the same directory name as the source, but ends up
		/// as a sub directory of the specified destination directory. The destination directory must
		/// already exist. If the copy fails at any point in the process, the user is notified
		/// of the problem and an attempt is made to remove the destination directory if the
		/// failure happened part way through the process.
		/// </summary>
		/// <param name="srcDirectory">Directory being copied</param>
		/// <param name="dstDirectoryParent">Destination directory where source directory and its contents are copied</param>
		/// <returns>true if successful, otherwise, false.</returns>

		public static bool CopyDirectory(string srcDirectory, string dstDirectoryParent)
		{
			string dstDirectory;
			return CopyDirectory(srcDirectory, dstDirectoryParent, out dstDirectory);
		}


		private static bool CopyDirectory(string srcDirectory, string dstDirectoryParent, out string dstDirectory)
		{
			dstDirectory = Path.Combine(dstDirectoryParent, Path.GetFileName(srcDirectory));

			if (!Directory.Exists(dstDirectoryParent))
			{
				ErrorReport.NotifyUserOfProblem(new DirectoryNotFoundException(dstDirectoryParent + " not found."),
					"{0} was unable to copy the directory {1} to {2}", EntryAssembly.ProductName, srcDirectory, dstDirectoryParent);
				return false;
			}
			if (AreDirectoriesEquivalent(srcDirectory, dstDirectory))
			{
				ErrorReport.NotifyUserOfProblem(new Exception("Cannot copy directory to itself."),
					"{0} was unable to copy the directory {1} to {2}", EntryAssembly.ProductName, srcDirectory, dstDirectoryParent);
				return false;
			}

			return CopyDirectoryContents(srcDirectory, dstDirectory);
		}


		/// <summary>
		/// Copies the specified source directory's contents to the specified destination directory.
		/// If the destination directory does not exist, it will be created first. If the source
		/// directory contains sub directorys, those and their content will also be copied. If the
		/// copy fails at any point in the process, the user is notified of the problem and
		/// an attempt is made to remove the destination directory if the failure happened part
		/// way through the process.
		/// </summary>
		/// <param name="sourcePath">Directory whose contents will be copied</param>
		/// <param name="destinationPath">Destination directory receiving the content of the source directory</param>
		/// <returns>true if successful, otherwise, false.</returns>
		///
		public static bool CopyDirectoryContents(string sourcePath, string destinationPath)
		{
			try
			{
				CopyDirectoryWithException(sourcePath,destinationPath);
			}
			catch (Exception e)
			{
				//Review: generally, it's better if Palaso doesn't undertake to make these kind of UI decisions.
				//I've extracted CopyDirectoryWithException, so as not to mess up whatever client is using this version
				ReportFailedCopyAndCleanUp(e, sourcePath, destinationPath);
				return false;
			}

			return true;
		}

		public static void CopyDirectoryWithException(string sourcePath, string destinationPath, bool overwrite = false)
		{
			if (!Directory.Exists(destinationPath))
				Directory.CreateDirectory(destinationPath);

			// Copy all the files.
			foreach (var filepath in Directory.GetFiles(sourcePath))
			{
				var filename = Path.GetFileName(filepath);
				File.Copy(filepath, Path.Combine(destinationPath, filename), overwrite);
			}

			// Copy all the sub directories.
			foreach (var directorypath in Directory.GetDirectories(sourcePath))
			{
				var directoryname = Path.GetFileName(directorypath);
				CopyDirectoryWithException(directorypath, Path.Combine(destinationPath, directoryname), overwrite);
			}
		}

		public static bool AreDirectoriesEquivalent(string dir1, string dir2)
		{
			return AreDirectoriesEquivalent(new DirectoryInfo(dir1), new DirectoryInfo(dir2));
		}

		// Gleaned from http://stackoverflow.com/questions/2281531/how-can-i-compare-directory-paths-in-c
		public static bool AreDirectoriesEquivalent(DirectoryInfo dirInfo1, DirectoryInfo dirInfo2)
		{
			var comparison = Platform.IsWindows ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture;
			var backslash = new char[] { '\\' }; // added this step because mono does not implicitly convert from char to char[]
			return string.Compare(dirInfo1.FullName.TrimEnd(backslash), dirInfo2.FullName.TrimEnd(backslash), comparison) == 0;
		}

		/// <summary>
		/// Move <paramref name="sourcePath"/> to <paramref name="destinationPath"/>. If src
		/// and dest are on different partitions (e.g., temp and documents are on different
		/// drives) then this method will do a copy followed by a delete. This is in contrast
		/// to Directory.Move which fails if src and dest are on different partitions.
		/// </summary>
		/// <param name="sourcePath">The source directory or file, similar to Directory.Move</param>
		/// <param name="destinationPath">The destination directory or file. If <paramref name="sourcePath"/>
		/// is a file then <paramref name="destinationPath"/> also needs to be a file.</param>
		public static void MoveDirectorySafely(string sourcePath, string destinationPath)
		{
			if (PathUtilities.PathsAreOnSameVolume(destinationPath, sourcePath))
			{
				Directory.Move(sourcePath, destinationPath);
				return;
			}
			if (Directory.Exists(sourcePath))
			{
				CopyDirectoryWithException(sourcePath, destinationPath);
				Directory.Delete(sourcePath, true);
			}
			else if (File.Exists(sourcePath))
			{
				if (File.Exists(destinationPath) || Directory.Exists(destinationPath))
					throw new IOException("Cannot create a file when that file already exists.");

				File.Copy(sourcePath, destinationPath);
				File.Delete(sourcePath);
			}
			else
			{
				throw new DirectoryNotFoundException(
					string.Format("Could not find a part of the path '{0}'", sourcePath));
			}
		}

		private static void ReportFailedCopyAndCleanUp(Exception error, string srcDirectory, string dstDirectory)
		{
			ErrorReport.NotifyUserOfProblem(error, "{0} was unable to copy the directory {1} to {2}",
				EntryAssembly.ProductName, srcDirectory, dstDirectory);

			try
			{
				if (!Directory.Exists(dstDirectory))
					return;

				// Clean up by removing the partially copied directory.
				Directory.Delete(dstDirectory, true);
			}
			catch { }
		}

		/// <summary>
		/// Return subdirectories of <paramref name="path"/> that are not system or hidden.
		/// There are some cases where our call to Directory.GetDirectories() throws.
		/// For example, when the access permissions on a folder are set so that it can't be read.
		/// Another possible example may be Windows Backup files, which apparently look like directories.
		/// </summary>
		/// <param name="path">Directory path to look in.</param>
		/// <returns>Zero or more directory names that are not system or hidden.</returns>
		/// <exception cref="System.UnauthorizedAccessException">E.g. when the user does not have
		/// read permission.</exception>
		public static string[] GetSafeDirectories(string path)
		{
				return (from directoryName in Directory.GetDirectories(path)
						let dirInfo = new DirectoryInfo(directoryName)
						where (dirInfo.Attributes & FileAttributes.System) != FileAttributes.System
						where (dirInfo.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden
						select directoryName).ToArray();
		}

		/// <summary>
		/// There are various things which can prevent a simple directory deletion, mostly timing related things which are hard to debug.
		/// This method uses all the tricks to do its best.
		/// </summary>
		/// <returns>returns true if the directory is fully deleted</returns>
		public static bool DeleteDirectoryRobust(string path, bool overrideReadOnly=true)
		{
			// ReSharper disable EmptyGeneralCatchClause

			if (!Platform.IsWindows)
			{
				// The Mono runtime deletes readonly files and directories that contain readonly files.
				// This violates the MSDN specification of Directory.Delete and File.Delete.
				if (!overrideReadOnly && DirectoryContainsReadOnly(path))
					return false;
			}

			for (int i = 0; i < 40; i++) // each time, we sleep a little. This will try for up to 2 seconds (40*50ms)
			{
				if (!Directory.Exists(path))
					break;

				try
				{
					Directory.Delete(path, true);
				}
				catch (Exception)
				{
				}

				if (!Directory.Exists(path))
					break;

				try
				{
					//try to clear it out a bit
					string[] dirs = Directory.GetDirectories(path);
					string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
					foreach (string filePath in files)
					{
						try
						{
							if(overrideReadOnly)
							{
								File.SetAttributes(filePath, FileAttributes.Normal);
							}
							File.Delete(filePath);
						}
						catch (Exception)
						{
						}
					}
					foreach (var dir in dirs)
					{
						DeleteDirectoryRobust(dir);
					}

				}
				catch (Exception)//yes, even these simple queries can throw exceptions, as stuff suddenly is deleted based on our prior request
				{
				}
				//sleep and let some OS things catch up
				Thread.Sleep(50);
			}

			return !Directory.Exists(path);
			// ReSharper restore EmptyGeneralCatchClause
		}

		/// <summary>
		/// If necessary, append a number to make the folder path unique.
		/// </summary>
		/// <param name="folderPath">Source folder pathname.</param>
		/// <returns>A unique folder pathname at the same level as <paramref name="folderPath"/>. It may have a number apended to <paramref name="folderPath"/>, or it may be <paramref name="folderPath"/>.</returns>
		public static string GetUniqueFolderPath(string folderPath)
		{
			var i = 0;
			var suffix = "";
			// Remove ending path separator, if it exists.
			folderPath = folderPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			var parent = Directory.GetParent(folderPath).FullName;
			var name = Path.GetFileName(folderPath);
			while (Directory.Exists(Path.Combine(parent, name + suffix)))
			{
				++i;
				suffix = i.ToString();
			}
			return Path.Combine(parent, name + suffix);
		}

		/// <summary>
		/// Check whether the given directory is readonly, or contains files or subdirectories that are readonly.
		/// </summary>
		/// <remarks>
		/// Using this check could be considered a workaround for a bug in the Mono runtime, but that bug goes so
		/// deep that it's safer and easier to work around it here.
		/// </remarks>
		static bool DirectoryContainsReadOnly(string path)
		{
			var dirInfo = new DirectoryInfo(path);
			if ((dirInfo.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
				return true;
			foreach (var file in Directory.GetFiles(path))
			{
				var fileInfo = new FileInfo(file);
				if ((fileInfo.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
					return true;
			}
			foreach (var dir in Directory.GetDirectories(path))
			{
				if (DirectoryContainsReadOnly(dir))
					return true;
			}
			return false;
		}

		/// <summary>
		/// Checks if there are any entries in a directory
		/// </summary>
		/// <param name="path">Path to the directory to check</param>
		/// <param name="onlyCheckForFiles">if this is TRUE, a directory that contains subdirectories but no files will be considered empty.
		/// Subdirectories are not checked, so if onlyCheckForFiles is TRUE and there is a subdirectory that contains a file, the directory
		/// will still be considered empty.</param>
		/// <returns></returns>
		public static bool DirectoryIsEmpty(string path, bool onlyCheckForFiles = false)
		{
			if (onlyCheckForFiles)
				return !Directory.EnumerateFiles(path).Any();

			return !Directory.EnumerateFileSystemEntries(path).Any();
		}

		/// <summary>
		/// Sets the permissions for this directory so that everyone has full control
		/// </summary>
		/// <param name="fullDirectoryPath"></param>
		/// <param name="showErrorMessage"></param>
		/// <returns>True if able to set access, False otherwise</returns>
		public static bool SetFullControl(string fullDirectoryPath, bool showErrorMessage = true)
		{
			// get current settings
			var everyone = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
			var security = Directory.GetAccessControl(fullDirectoryPath, AccessControlSections.Access);
			var currentRules = security.GetAccessRules(true, true, typeof(SecurityIdentifier));

			// if everyone already has full control, return now
			if (currentRules.Cast<FileSystemAccessRule>()
				.Where(rule => rule.IdentityReference.Value == everyone.Value)
				.Any(rule => rule.FileSystemRights == FileSystemRights.FullControl))
			{
				return true;
			}

			// initialize
			var returnVal = false;

			try
			{
				// set the permissions so everyone can read and write to this directory
				var fullControl = new FileSystemAccessRule(everyone,
															FileSystemRights.FullControl,
															InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
															PropagationFlags.None,
															AccessControlType.Allow);
				security.AddAccessRule(fullControl);
				Directory.SetAccessControl(fullDirectoryPath, security);

				returnVal = true;
			}
			catch (Exception ex)
			{
				if (showErrorMessage)
				{
					ErrorReport.NotifyUserOfProblem(ex, "{0} was not able to set directory security for '{1}' to 'full control' for everyone.",
						EntryAssembly.ProductName, fullDirectoryPath);
				}
			}

			return returnVal;
		}
	}
}
