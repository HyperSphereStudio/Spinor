#This File Is A Special File That is Run without full Spinor type initialization
#Types are built organically from this File
#Only define abstract types, abstractbuiltin & primitive types in this file

abstract type Number end
abstract type Real <: Number end
abstract type Integer <: Real end

abstractbuiltin type AbstractFloat <: Real end
abstractbuiltin type Signed <: Integer end
abstractbuiltin type Unsigned <: Integer end

primitive type Float16 <: AbstractFloat 16 end
primitive type Float32 <: AbstractFloat 32 end
primitive type Float64 <: AbstractFloat 64 end
primitive type Float128 <: AbstractFloat 128 end

primitive type Bool <: Signed 8 end
primitive type Char <: Unsigned 32 end

primitive type Int8    <: Signed   8 end
primitive type UInt8   <: Unsigned 8 end
primitive type Int16   <: Signed   16 end
primitive type UInt16  <: Unsigned 16 end
primitive type Int32   <: Signed   32 end
primitive type UInt32  <: Unsigned 32 end
primitive type Int64   <: Signed   64 end
primitive type UInt64  <: Unsigned 64 end
primitive type Int128  <: Signed   128 end
primitive type UInt128 <: Unsigned 128 end

#After Running the following will be added: Int = Native Int, UInt = Native UInt, Float = Native Float

abstractbuiltin type Exception end

abstract type IO end

struct s 
    x
end