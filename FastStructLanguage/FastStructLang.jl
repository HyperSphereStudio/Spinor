module FastStructLang
    abstract type AbstractType end
    abstract type AbstractStruct <: AbstractType end
    abstract type PtrTy{T <: AbstractType} <: AbstractType end
    abstract type RefTy{T <: AbstractType} <: AbstractType end
    abstract type AbstractContainer <: AbstractStruct end
    abstract type AbstractCompiler end

    mutable struct BasicField
        name::Symbol
        type::AbstractStruct
        size::Int
    
        BasicField(name::Symbol, type::AbstractStruct, size::Int) = BasicField(name, type, size)

        Base.length(f::BasicField) = f.size
    end

    mutable struct BasicType <: AbstractType
        name::Symbol
        mutable::Bool
        inherited::BasicType

        BasicType(name::Symbol, mutable, inherited) = new(name, mutable, inherited)
    end

    mutable struct BasicStruct <: AbstractStruct 
        bt::BasicType
        fields::Array{BasicField}
        
        BasicStruct(bt::BasicType, fields::Array{BasicField}) = new(bt, fields)

        Base.length(t::BasicStruct) = sum(length, t.fields)
    end

    mutable struct BasicTargetLanguage
        name::Symbol
        compiler::AbstractCompiler
        stlTypes::Array{AbstractType}
        baseObject::AbstractType

        BasicTargetLanguage(name::Symbol, compiler::AbstractCompiler, stlTypes::Array{AbstractType}, baseObject) = new(name, compiler, stlTypes, baseObject)
    end

    macro CompileFastStructToTargetLangauage(expr, lang, loc)
        Base.remove_linenums!(expr)

        structDict = Dict{Symbol, BasicStruct}()
        stlTypes = lang.stlTypes
        
        function StructSearch(name)
            haskey(structDict, name) && (return structDict[name])
            return stlTypes[name]
        end        

        for e in expr.args
            if e.head == :struct
                blk = e.args[3]
                sf = Array{BasicField}(undef, length(blk.args))
                sv = BasicStruct(BasicType(e.args[2], e.args[1], false), sf)
                structDict[sv.name] = sv
                for f in eachindex(blk)
                    if f.head == :(::)
                        ty = StructSearch(f.args[2])
                        sf[f.args[1]] = BasicField(f.args[1], ty, length(ty))
                    else
                        sf[f.args[1]] = lang.baseObject
                    end
                end
            end
        end

        compile(lang.compiler, structDict, loc)
    end

    include("CSharpLang.jl")
end

