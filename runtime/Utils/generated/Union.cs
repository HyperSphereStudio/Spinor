using System;
using System.Runtime.InteropServices;

namespace runtime.Utils
{
	public struct Union<T1,T2> where T1 : struct where T2 : struct{
		private byte type;
		[FieldOffset(1)] private T1 a1;
		[FieldOffset(1)] private T2 a2;
		public T1 A1 {
			get => a1;
			set {
				a1 = value;
				type = 1;
			}
		}

		public T2 A2 {
			get => a2;
			set {
				a2 = value;
				type = 2;
			}
		}

		public Union(int _ = 0) {
			type = 0;
			a1 = default;
			a2 = default;
		}
		public Union(T1 a) : this() => A1 = a;
		public Union(T2 a) : this() => A2 = a;
		public bool HasValue => type < 1 || type > 2;
		public bool IsNull => !HasValue;
		public bool IsT1 => type == 1;
		public bool IsT2 => type == 2;
		public Type GetType() {
			switch (type) {
				case 1: return typeof(T1);
				case 2: return typeof(T2);
				default: return null;
			}
		}
	}
	public struct Union<T1,T2,T3> where T1 : struct where T2 : struct where T3 : struct{
		private byte type;
		[FieldOffset(1)] private T1 a1;
		[FieldOffset(1)] private T2 a2;
		[FieldOffset(1)] private T3 a3;
		public T1 A1 {
			get => a1;
			set {
				a1 = value;
				type = 1;
			}
		}

		public T2 A2 {
			get => a2;
			set {
				a2 = value;
				type = 2;
			}
		}

		public T3 A3 {
			get => a3;
			set {
				a3 = value;
				type = 3;
			}
		}

		public Union(int _ = 0) {
			type = 0;
			a1 = default;
			a2 = default;
			a3 = default;
		}
		public Union(T1 a) : this() => A1 = a;
		public Union(T2 a) : this() => A2 = a;
		public Union(T3 a) : this() => A3 = a;
		public bool HasValue => type < 1 || type > 3;
		public bool IsNull => !HasValue;
		public bool IsT1 => type == 1;
		public bool IsT2 => type == 2;
		public bool IsT3 => type == 3;
		public Type GetType() {
			switch (type) {
				case 1: return typeof(T1);
				case 2: return typeof(T2);
				case 3: return typeof(T3);
				default: return null;
			}
		}
	}
	public struct Union<T1,T2,T3,T4> where T1 : struct where T2 : struct where T3 : struct where T4 : struct{
		private byte type;
		[FieldOffset(1)] private T1 a1;
		[FieldOffset(1)] private T2 a2;
		[FieldOffset(1)] private T3 a3;
		[FieldOffset(1)] private T4 a4;
		public T1 A1 {
			get => a1;
			set {
				a1 = value;
				type = 1;
			}
		}

		public T2 A2 {
			get => a2;
			set {
				a2 = value;
				type = 2;
			}
		}

		public T3 A3 {
			get => a3;
			set {
				a3 = value;
				type = 3;
			}
		}

		public T4 A4 {
			get => a4;
			set {
				a4 = value;
				type = 4;
			}
		}

		public Union(int _ = 0) {
			type = 0;
			a1 = default;
			a2 = default;
			a3 = default;
			a4 = default;
		}
		public Union(T1 a) : this() => A1 = a;
		public Union(T2 a) : this() => A2 = a;
		public Union(T3 a) : this() => A3 = a;
		public Union(T4 a) : this() => A4 = a;
		public bool HasValue => type < 1 || type > 4;
		public bool IsNull => !HasValue;
		public bool IsT1 => type == 1;
		public bool IsT2 => type == 2;
		public bool IsT3 => type == 3;
		public bool IsT4 => type == 4;
		public Type GetType() {
			switch (type) {
				case 1: return typeof(T1);
				case 2: return typeof(T2);
				case 3: return typeof(T3);
				case 4: return typeof(T4);
				default: return null;
			}
		}
	}
	public struct Union<T1,T2,T3,T4,T5> where T1 : struct where T2 : struct where T3 : struct where T4 : struct where T5 : struct{
		private byte type;
		[FieldOffset(1)] private T1 a1;
		[FieldOffset(1)] private T2 a2;
		[FieldOffset(1)] private T3 a3;
		[FieldOffset(1)] private T4 a4;
		[FieldOffset(1)] private T5 a5;
		public T1 A1 {
			get => a1;
			set {
				a1 = value;
				type = 1;
			}
		}

		public T2 A2 {
			get => a2;
			set {
				a2 = value;
				type = 2;
			}
		}

		public T3 A3 {
			get => a3;
			set {
				a3 = value;
				type = 3;
			}
		}

		public T4 A4 {
			get => a4;
			set {
				a4 = value;
				type = 4;
			}
		}

		public T5 A5 {
			get => a5;
			set {
				a5 = value;
				type = 5;
			}
		}

		public Union(int _ = 0) {
			type = 0;
			a1 = default;
			a2 = default;
			a3 = default;
			a4 = default;
			a5 = default;
		}
		public Union(T1 a) : this() => A1 = a;
		public Union(T2 a) : this() => A2 = a;
		public Union(T3 a) : this() => A3 = a;
		public Union(T4 a) : this() => A4 = a;
		public Union(T5 a) : this() => A5 = a;
		public bool HasValue => type < 1 || type > 5;
		public bool IsNull => !HasValue;
		public bool IsT1 => type == 1;
		public bool IsT2 => type == 2;
		public bool IsT3 => type == 3;
		public bool IsT4 => type == 4;
		public bool IsT5 => type == 5;
		public Type GetType() {
			switch (type) {
				case 1: return typeof(T1);
				case 2: return typeof(T2);
				case 3: return typeof(T3);
				case 4: return typeof(T4);
				case 5: return typeof(T5);
				default: return null;
			}
		}
	}}