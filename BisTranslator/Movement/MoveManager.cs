using Dalamud.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatTwo.Movement
{
    public class MoveManager
    {
        public static readonly int[] BlockedKeys = new int[] { 321, 322, 323, 324, 325, 326 };
        private bool MovingDisabled { get; set; } = false;
        private static MoveMemory _memory { get; set; }

        public bool IsActive { get { return MovingDisabled; } }

        public MoveManager(MoveMemory mem)
        {
            _memory = mem;
        }

        public unsafe void EnableMoving()
        {
            if (MovingDisabled)
            {
                PluginLog.Debug($"Enabling moving, cnt {_memory.ForceDisableMovement}");
                _memory.DisableHooks();
                if (_memory.ForceDisableMovement > 0)
                {
                    _memory.ForceDisableMovement--;
                }
                MovingDisabled = false;
            }
        }

        public void DisableMoving()
        {
            if (!MovingDisabled)
            {
                PluginLog.Debug($"Disabling moving, cnt {_memory.ForceDisableMovement}");
                _memory.EnableHooks();
                _memory.ForceDisableMovement++;
                MovingDisabled = true;
            }
        }
    }
}
