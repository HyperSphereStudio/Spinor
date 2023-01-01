create_string_array_from_string(x) = println("new[]{\"$(join(split(x, " "), "\", \""))\"}")

function generate_parse(path)
    println("Generating Antlr Parser...")
    
    if !success(run(Cmd(Cmd(["java", "-jar", "antlr.jar", 
                                    "-o", abspath(srcpath(path)), 
                                    "-package", "HyperSphere",
                                    "-encoding", "utf8",
                                    abspath(srcpath(path, "SpinorLexer.g4"))]), dir=srcpath(path)), wait=true))
            throw(error("Antlr Lexer Generation Failed!"))
        end
    
    if !success(run(Cmd(Cmd(["java", "-jar", "antlr.jar", 
                                "-o", abspath(genpath(path)),
                                "-visitor", "-package", "HyperSphere",
                                "-encoding", "utf8",
                                abspath(srcpath(path, "SpinorParser.g4"))]), dir=srcpath(path)), wait=true))
        throw(error("Antlr Parser Generation Failed!"))
    end
    println("Finished Generating Antlr Parser")
end