using Harmony;
using NitroxModel.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

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
    }
}
