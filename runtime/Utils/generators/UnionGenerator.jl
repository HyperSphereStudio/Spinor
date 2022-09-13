
function writeUnionStruct(file, size::Integer)
    write(file, "\n\tpublic struct Union<")
    write(file, join(["T$i" for i in 1:size], ","))
    write(file, "> ")
    write(file, join(["where T$i : struct" for i in 1:size], " "))
 
    write(file, "{\n\t\tprivate byte type;")
    for i in 1:size
        write(file, "\n\t\t[FieldOffset(1)] private T$i a$i;")
    end   
    write(file, "\n")
    
    for i in 1:size
        write(file, "\t\tpublic T$i A$i {\n\t\t\tget => a$i;\n\t\t\tset {\n\t\t\t\ta$i = value;\n\t\t\t\ttype = $i;\n\t\t\t}\n\t\t}\n\n")
    end
    
    write(file, """\t\tpublic Union(int _ = 0) {\n\t\t\ttype = 0;\n$(join(["\t\t\ta$i = default;" for i in 1:size], "\n"))\n\t\t}\n""")
   
    for i in 1:size
        write(file, "\t\tpublic Union(T$i a) : this() => A$i = a;\n")
    end                    
    
    write(file, "\t\tpublic bool HasValue => type < 1 || type > $size;\n\t\tpublic bool IsNull => !HasValue;\n")
    for i in 1:size
        write(file, "\t\tpublic bool IsT$i => type == $i;\n")
    end
    write(file, "\t\tpublic Type GetType() {\n\t\t\tswitch (type) {\n")
    
    for i in 1:size
        write(file, "\t\t\t\tcase $i: return typeof(T$i);\n")
    end
                               
    write(file, "\t\t\t\tdefault: return null;\n\t\t\t}\n\t\t}\n\t}")                               
end

function generateUnionFile(file, sizes)
    write(file, """using System;
                 using System.Runtime.InteropServices;
                 
                 namespace runtime.Utils
                 {""")    
    for size in sizes
        writeUnionStruct(file, size)
    end                 
    write(file, "}")
    close(file)
end


#Default Generator
generateUnionFile(open("../generated/Union.cs", "w"), 2:5)