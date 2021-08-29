﻿using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;
using uTinyRipper.Classes.Misc;

namespace uTinyRipper.Classes
{
	public static class ObjectUtils
	{
		public static long GenerateExportID(Object asset, Func<long, bool> duplicateChecker)
		{
			if (asset == null)
			{
				throw new ArgumentNullException(nameof(asset));
			}

			ThreadSafeRandom random = new ThreadSafeRandom();
			uint classID = (uint)asset.ClassID;
#if DEBUG
			int length = BitConverterExtensions.GetDigitsCount(classID);
			if (length > 4)
			{
				throw new NotSupportedException($"Class ID {classID} with more that 4 digits isn't supported");
			}
#endif
			long prefix = classID * 1000000000000000L;
			ulong persistentValue = 0;
//#warning TODO: depending on the export version exportID should has random or ordered value
			long exportID = 0;
			do
			{
				ulong value = unchecked((ulong)GenerateInternalID());
				persistentValue = unchecked(persistentValue + value);
				exportID = prefix + (long)(persistentValue % 1000000000000000L);
			}
			while (duplicateChecker(exportID));
			return exportID;
		}

		public static long GenerateInternalID()
		{
			s_random.NextBytes(s_idBuffer.Value);
			return BitConverter.ToInt64(s_idBuffer.Value, 0);
		}

		public const char DirectorySeparatorChar = '/';
		public const string DirectorySeparator = "/";

		private static readonly ThreadSafeRandom s_random = new ThreadSafeRandom();
		private static readonly ThreadLocal<byte[]> s_idBuffer = new ThreadLocal<byte[]>(() => new byte[8]);
	}
}
