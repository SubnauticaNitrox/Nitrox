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
        }

        public ValidatedCodeInstruction(OpCode opcode, object operand) : base(opcode, operand)
        {
            Validate.NotNull(operand);
        }

        public ValidatedCodeInstruction(OpCode opcode, object operand, string errorMessage) : base(opcode, operand)
        {
            Validate.NotNull(operand, "operand : " + errorMessage);
        }
    }
}
