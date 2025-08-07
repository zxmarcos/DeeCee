namespace DeeCee.SH4;

public enum Opcode
{
    COPY,
    ADD,
    SUB,
    STORE,
    LOAD,
    AND,
    OR,
    XOR,
    NOT,
    CMP_EQ,
    CMP_NE,
    CMP_LT,
    CMP_GT,
    CMP_GT_SIGN,
    CMP_GE,
    CMP_GE_SIGN,
    BRANCH,
    BRANCH_TRUE,
    BRANCH_FALSE
}