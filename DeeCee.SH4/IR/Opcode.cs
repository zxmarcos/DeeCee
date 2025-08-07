namespace DeeCee.SH4;

public enum Opcode
{
    COPY,
    ADD,
    STORE,
    LOAD,
    AND,
    OR,
    XOR,
    CMP_EQ,
    CMP_NE,
    CMP_LT,
    CMP_GT,
    CMP_GE,
    CMP_GE_SIGN,
    BRANCH,
    BRANCH_TRUE,
    BRANCH_FALSE,
}