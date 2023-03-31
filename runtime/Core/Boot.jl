#This File Is A Special File That is Run without full Spinor type initialization
#Types are built organically from this File
#Only define abstract types, abstractbuiltin & primitive types in this file

abstract type Number end
abstract type Real          <: Number end
abstract type AbstractFloat <: Real    end
abstract type Integer       <: Real end
abstract type Signed        <: Integer end
abstract type Unsigned      <: Integer end

system using System

#=
system type System.Half    as Float16  <: AbstractFloat end
system type System.Single  as Float32  <: AbstractFloat end
system type System.Double  as Float64  <: AbstractFloat end
system type System.Decimal as Float128 <: AbstractFloat end

system type System.Boolean as Bool <: Signed   end
system type System.Int32   as Char <: Unsigned end

struct Int8 <: Signed
    value::System{SByte}    
end

system type System.Byte   as UInt8     <: Unsigned end
system type System.Int16   as Int16    <: Signed   end
system type System.UInt16  as UInt16   <: Unsigned end
system type System.Int32   as Int32    <: Signed   end
system type System.UInt32  as UInt32   <: Unsigned end
system type System.Int64   as Int64    <: Signed   end
system type System.UInt64  as UInt64   <: Unsigned end
system type System.Int128  as Int128   <: Signed   end
system type System.UInt128 as UInt128  <: Unsigned end
#After Running the following will be added: Int = Native Int, UInt = Native UInt, Float = Native Float

=#

abstract type Exception end
abstract type IO end

struct Nothing end

struct test 
    x::Int32
end