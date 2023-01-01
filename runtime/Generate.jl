const SrcRootDir = "src"
const GenRootDir = "gen"

srcpath(paths...) = joinpath(SrcRootDir, paths...)
genpath(paths...) = begin
    path = joinpath(GenRootDir, paths...)
    mkpath(joinpath(GenRootDir, paths[1:(end-1)]...))
    return path
end

function generate(path, name)
    path = joinpath(path, name)
    println("Generating $path")
    include(srcpath(path, "Generate.jl"))
    function_name = Symbol("generate_$name")
    eval(:($function_name($path)))
    println("Finished Generating $path")
end

clean_generated() = rm(GenRootDir, recursive=true)

generate("", "core")

