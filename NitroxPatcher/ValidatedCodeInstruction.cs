using System.Collections.Generic;
using System.Reflection.Emit;
using Harmony;
using NitroxModel.Helper;

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

        public ValidatedCodeInstruction(OpCode opcode, object operand, List<Label> labels) : base(opcode, operand)
        {
            Validate.NotNull(operand);
            this.labels = labels;
        }

        public ValidatedCodeInstruction(OpCode opcode, object operand, string errorMessage) : base(opcode, operand)
        {
            Validate.NotNull(operand, "operand : " + errorMessage);
        }
    }
}
