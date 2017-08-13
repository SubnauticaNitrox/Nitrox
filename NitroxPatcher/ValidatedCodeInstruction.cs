using Harmony;
using NitroxModel.Helper;
using System;
using System.Reflection.Emit;

namespace NitroxPatcher
{
    class ValidatedCodeInstruction : CodeInstruction
    {
        public ValidatedCodeInstruction(OpCode opcode) : base(opcode)
        {
            Validate.NotNull(opcode);
        }

        public ValidatedCodeInstruction(OpCode opcode, object operand) : base(opcode, operand)
        {
            Validate.NotNull(opcode);
            Validate.NotNull(operand);
        }

        public ValidatedCodeInstruction(OpCode opcode, object operand, String errorMessage) : base(opcode, operand)
        {
            Validate.NotNull(opcode, "opcode : " + errorMessage);
            Validate.NotNull(operand, "operand : " + errorMessage);
        }
    }
}
