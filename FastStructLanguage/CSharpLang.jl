mutable struct CSharpCompiler <: AbstractCompiler

end

function _createSharpTypes()
    
end

const CSharpLang = BasicTargetLanguage(:sharp, CSharpCompiler(), _createSharpTypes(), Basic)