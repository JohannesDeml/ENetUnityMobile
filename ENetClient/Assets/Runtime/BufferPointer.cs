// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BufferPointer.cs">
//   Copyright (c) 2020 Johannes Deml. All rights reserved.
// </copyright>
// <author>
//   Johannes Deml
//   public@deml.io
// </author>
// --------------------------------------------------------------------------------------------------------------------

namespace Supyrb
{
	public readonly struct BufferPointer
	{
		public readonly int Start;
		public readonly int Length;

		public BufferPointer(int start, int length)
		{
			Start = start;
			Length = length;
		}
	}
}
