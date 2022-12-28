
function generate_parse(path)
    println("Expanding JuliaSyntaxGrammar")
    open(genpath(path, "Julia.g4"), "w") do io
        write(io, replace(read(srcpath(path, "JuliaSyntax.g4"), String), r"\$\[([.\S])+\]\$" => m -> eval(Meta.parse(m[1]))))
    end

    println("Generating Antlr Parser...")
    if !success(run(Cmd(["java", "-jar", "antlr.jar", 
                                "-lib", genpath(path), "-visitor",
                                "-package", "HyperSphere",
                                "Julia.g4", "-Dlanguage=CSharp"], dir="lib"), wait=true))
        throw(error("Antlr Parser Generation Failed!"))
    end
    println("Finished Generating Antlr Parser")
end