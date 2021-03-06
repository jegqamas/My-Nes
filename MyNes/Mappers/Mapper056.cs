// This file is part of My Nes
//
// A Nintendo Entertainment System / Family Computer (Nes/Famicom)
// Emulator written in C#.
// 
// Copyright © Alaa Ibrahim Hadid 2009 - 2022
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with this program.If not, see<http://www.gnu.org/licenses/>.
// 
// Author email: mailto:alaahadidfreeware@gmail.com
//
namespace MyNes.Core
{
    [BoardInfo("Pirate SMB3", 56)]
    // TODO: Mapper 56 not work ?
    class Mapper056 : Board
    {
        private int irqCounter = 0;
        private int irqLatch = 0;
        private bool irqEnabled = false;
        private int irqControl = 0;
        private int switchControl = 0;
        internal override string Issues { get { return MNInterfaceLanguage.IssueMapper56; } }
        internal override void HardReset()
        {
            base.HardReset();
            Switch08KPRG(PRG_ROM_08KB_Mask, PRGArea.AreaE000);
            irqLatch = 0;
            irqCounter = 0;
            irqControl = 0;
            irqEnabled = false;
        }
        internal override void WritePRG(ref ushort address, ref byte data)
        {
            if (address < 0xF000)
            {
                switch (address & 0xE000)
                {
                    case 0x8000: irqLatch = (irqLatch & 0xFFF0) | (data & 0xF) << 00; break;
                    case 0x9000: irqLatch = (irqLatch & 0xFF0F) | (data & 0xF) << 04; break;
                    case 0xA000: irqLatch = (irqLatch & 0xF0FF) | (data & 0xF) << 08; break;
                    case 0xB000: irqLatch = (irqLatch & 0x0FFF) | (data & 0xF) << 12; break;

                    case 0xC000:
                        irqControl = data & 0x5;
                        irqEnabled = (data & 0x2) == 0x2;
                        if (irqEnabled)
                            irqCounter = irqLatch;
                        NesEmu.IRQFlags &= ~NesEmu.IRQ_BOARD;
                        break;

                    case 0xD000: irqEnabled = (irqControl & 0x1) == 0x1; NesEmu.IRQFlags &= ~NesEmu.IRQ_BOARD; break;
                    case 0xE000: switchControl = data; break;
                }
            }
            else//0xF000 - 0xFFFF
            {
                //switch prg
                int offset = (switchControl & 0xF) - 1;

                if (offset < 3)
                {
                    Switch08KPRG((data & 0x0F) | (PRG_AREA_BLK_INDEX[(offset >> 13) + 1] & 0x10), (PRGArea)offset);
                }
                switch (address & 0xC00)
                {
                    case 0x000:

                        address &= 0x3;

                        if (address < 3)
                        {
                            Switch08KPRG((data & 0x0F) | (PRG_AREA_BLK_INDEX[(offset >> 13) + 1] & 0x10), (PRGArea)address);
                        }
                        break;
                    case 0x800: Switch01KNMTFromMirroring((data & 0x1) == 0x1 ? Mirroring.Vert : Mirroring.Horz); break;
                    case 0xC00: Switch01KCHR(data, (CHRArea)(address & 0x7)); break;
                }
            }
        }
        internal override void OnCPUClock()
        {
            if (irqEnabled)
            {
                if (irqCounter++ == 0xFFFF)
                {
                    irqCounter = irqLatch;
                    NesEmu.IRQFlags |= NesEmu.IRQ_BOARD;
                }
            }
        }
    }
}
