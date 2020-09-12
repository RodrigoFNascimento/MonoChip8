using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Chip8.Chip8
{
    public class CPU
    {
        /// <summary>
        /// 16-key hexadecimal keypad.
        /// </summary>
        private Keyboard _keyboard { get; set; }
        /// <summary>
        /// Monochrome display.
        /// </summary>
        private Renderer _renderer { get; set; }
        private Speaker _speaker { get; set; }
        /// <summary>
        /// The Chip-8 language is capable of accessing up to 4KB (4,096 bytes)
        /// of RAM, from location 0x000 (0) to 0xFFF (4095).
        /// </summary>
        /// <remarks>
        /// The first 512 bytes, from 0x000 to 0x1FF, are where the original
        /// interpreter was located, and should not be used by programs.
        /// Most Chip-8 programs start at location 0x200 (512), but some begin at
        /// 0x600 (1536). Programs beginning at 0x600 are intended for the ETI 660 computer.
        /// </remarks>
        private byte[] _memory { get; set; }
        /// <summary>
        /// 16 general purpose 8-bit registers, usually referred to as Vx,
        /// where x is a hexadecimal digit (0 through F)
        /// </summary>
        /// <remarks>
        /// The VF register should not be used by any program,
        /// as it is used as a flag by some instructions.
        /// </remarks>
        private byte[] _v { get; set; }
        /// <summary>
        /// 16-bit register generally used to store memory addresses,
        /// so only the lowest (rightmost) 12 bits are usually used.
        /// </summary>
        private ushort _i { get; set; }
        /// <summary>
        /// Special purpose 8-bit register for the delay timer.
        /// The delay timer is active whenever the delay timer register (DT) is non-zero.
        /// This timer does nothing more than subtract 1 from the value of DT at a rate of 60Hz.
        /// When DT reaches 0, it deactivates.
        /// </summary>
        private byte _delayTimer { get; set; }
        /// <summary>
        /// The sound timer is active whenever the sound timer register (ST) is non-zero.
        /// This timer also decrements at a rate of 60Hz, however,
        /// as long as ST's value is greater than zero, the Chip-8 buzzer will sound.
        /// When ST reaches zero, the sound timer deactivates.
        /// </summary>
        private int _soundTimer { get; set; }
        /// <summary>
        /// The program counter (PC) should be 16-bit,
        /// and is used to store the currently executing address.
        /// </summary>
        private ushort _pc { get; set; }
        /// <summary>
        /// The stack is an array of 16 16-bit values, used to store the address
        /// that the interpreter shoud return to when finished with a subroutine.
        /// Chip-8 allows for up to 16 levels of nested subroutines.
        /// </summary>
        private ushort[] _stack { get; set; }
        /// <summary>
        /// The stack pointer (SP) can be 8-bit,
        /// it is used to point to the topmost level of the stack.
        /// </summary>
        private byte _sp { get; set; }
        private bool _paused { get; set; }
        private byte _speed { get; set; }

        public CPU(Keyboard keyboard, Renderer renderer, Speaker speaker)
        {
            _keyboard = keyboard;
            _renderer = renderer;
            _speaker = speaker;

            // 4KB (4096 bytes) of memory
            _memory = new byte[4096];

            // 16 8-bit registers
            _v = new byte[16];

            // Stores memory addresses. Set this to 0 since we aren't storing anything at initialization.
            _i = 0;

            // Timers
            _delayTimer = 0;
            _soundTimer = 0;

            // Program counter. Stores the currently executing address.
            _pc = 0x200;
            _stack = new ushort[16];
            _sp = 0;

            _speed = 10;
        }

        /// <summary>
        /// Loops through each byte in the sprites array and stores it in memory starting at hex 0x000.
        /// </summary>
        public void LoadSpritesIntoMemory()
        {
            // Array of hex values for each sprite. Each sprite is 5 bytes.
            // The technical reference provides us with each one of these values.
            byte[] sprites = new byte[]
            {
                0xF0, 0x90, 0x90, 0x90, 0xF0, // 0
                0x20, 0x60, 0x20, 0x20, 0x70, // 1
                0xF0, 0x10, 0xF0, 0x80, 0xF0, // 2
                0xF0, 0x10, 0xF0, 0x10, 0xF0, // 3
                0x90, 0x90, 0xF0, 0x10, 0x10, // 4
                0xF0, 0x80, 0xF0, 0x10, 0xF0, // 5
                0xF0, 0x80, 0xF0, 0x90, 0xF0, // 6
                0xF0, 0x10, 0x20, 0x40, 0x40, // 7
                0xF0, 0x90, 0xF0, 0x90, 0xF0, // 8
                0xF0, 0x90, 0xF0, 0x10, 0xF0, // 9
                0xF0, 0x90, 0xF0, 0x90, 0x90, // A
                0xE0, 0x90, 0xE0, 0x90, 0xE0, // B
                0xF0, 0x80, 0x80, 0x80, 0xF0, // C
                0xE0, 0x90, 0x90, 0x90, 0xE0, // D
                0xF0, 0x80, 0xF0, 0x80, 0xF0, // E
                0xF0, 0x80, 0xF0, 0x80, 0x80  // F
            };

            // According to the technical reference, sprites are stored in the interpreter section of memory starting at hex 0x000
            for (int i = 0; i < sprites.Length; i++)
            {
                _memory[i] = sprites[i];
            }
        }

        /// <summary>
        /// Loops through the contents of the ROM/program and store it in memory
        /// </summary>
        /// <param name="program"></param>
        public void LoadProgramIntoMemory(byte[] program)
        {
            for (int loc = 0; loc < program.Length; loc++)
            {
                // The technical reference specifically tells us that "most Chip-8 programs start at location 0x200".
                // So when we load the ROM into memory, we start at 0x200 and increment from there.
                _memory[0x200 + loc] = program[loc];
            }
        }

        /// <summary>
        /// Load a ROM stored in the local memory.
        /// </summary>
        /// <param name="path">Local path to the ROM file.</param>
        public void LoadROM(string path)
        {
            using var sr = new StreamReader(path);
            string romContent = sr.ReadToEnd();
            byte[] rom = Encoding.ASCII.GetBytes(romContent);
            LoadProgramIntoMemory(rom);
        }

        public void Cycle()
        {
            // grab that opcode from memory and pass that along to another function
            // that'll handle the execution of that instruction
            if (!_paused)
            {
                /*
                 * First of all, each instruction is 16 bits (2 bytes) long (3.0),
                 * but our memory is made up of 8 bit (1 byte) pieces.
                 * This means we have to combine two pieces of memory in order to
                 * get the full opcode. That's why we have _pc and _pc + 1
                 * in the line of code above. We're simply grabbing both halves of the opcode.
                 * 
                 * But you can't just combine two, 1-byte values to get a 2-byte value.
                 * To properly do this, we need to shift the first piece of memory,
                 * _memory[_pc], 8 bits left to make it 2 bytes long.
                 * In the most basic of terms, this will add two zeros, or more accurately
                 * hex value 0x00 onto the right-hand side of our 1-byte value, making it 2 bytes.
                 * 
                 * For example, shifting hex 0x11 8 bits left will give us hex 0x1100.
                 * From there, we bitwise OR (|) it with the second piece of memory, _memory[_pc + 1]).
                 * 
                 * _memory[_pc] = PC = 0x10
                 * _memory[_pc + 1] = PC + 1 = 0xF0
                 * 
                 * Shift PC 8 bits (1 byte) left to make it 2 bytes:
                 * 
                 * PC = 0x1000
                 * 
                 * Bitwise OR PC and PC + 1:
                 * 
                 * PC | PC + 1 = 0x10F0
                 * 
                 * or
                 * 
                 * 0x1000 | 0xF0 = 0x10F0
                */

                ushort opcode = (ushort)((_memory[_pc] << 8) | _memory[_pc + 1]);
                Debug.WriteLine($"Executing opcode 0x{Convert.ToString(opcode, 16)}" +
                    $" (memory[{_pc}] = 0x{Convert.ToString(_memory[_pc] << 8, 16)} | memory[{_pc+1}] = 0x{Convert.ToString(_memory[_pc + 1], 16)})");
                ExecuteInstruction(opcode);
                _pc += 2;
            }

            if (!_paused)
                UpdateTimers();

            PlaySound();
            _renderer.Render();
        }

        private void UpdateTimers()
        {
            // Each timer, delay and sound, decrement by 1 at a rate of 60Hz.
            // In other words, every 60 frames our timers will decrement by 1.
            if (_delayTimer > 0)
            {
                _delayTimer -= 1;
            }

            if (_soundTimer > 0)
            {
                _soundTimer -= 1;
            }
        }

        /// <summary>
        /// Plays a sound as long as the sound timer > 0.
        /// </summary>
        private void PlaySound()
        {
            if (_soundTimer > 0)
            {
                _speaker.Play(440);
            }
            else
            {
                _speaker.Stop();
            }
        }

        private ushort Pop(ushort[] stack, byte sp)
        {
            ushort address = stack[sp];
            return address;
        }

        private void Push(ushort[] stack, byte sp, ushort instruction)
        {
            stack[sp] = instruction;
        }

        private void ExecuteInstruction(ushort opcode)
        {
            /*
             * The first piece of information to be aware of is that all instructions are 2 bytes long.
             * So every time we execute an instruction, or run this function,
             * we have to increment the program counter (_pc) by 2 so the CPU knows where the next instruction is.
             */

            // Increment the program counter to prepare it for the next instruction.
            // Each instruction is 2 bytes long, so increment it by 2.
            _pc += 2;

            /*
             * nnn or addr  - A 12-bit value, the lowest 12 bits of the instruction
             * n or nibble  - A 4-bit value, the lowest 4 bits of the instruction
             * x            - A 4-bit value, the lower 4 bits of the high byte of the instruction
             * y            - A 4-bit value, the upper 4 bits of the low byte of the instruction
             * kk or byte   - An 8-bit value, the lowest 8 bits of the instruction
             */

            var x = (byte)((byte)(opcode >> 8) & 0x000F); // the lower 4 bits of the high byte
            var y = (byte)((byte)(opcode >> 4) & 0x000F); // the upper 4 bits of the low byte
            var n = (byte)(opcode & 0x000F); // the lowest 4 bits
            var kk = (byte)(opcode & 0x00FF); // the lowest 8 bits
            var nnn = (ushort)(opcode & 0x0FFF); // the lowest 12 bits

            /*
             * To explain this, let's once again assume we have an instruction 0x5460.
             * If we & (bitwise AND) that instruction with hex value 0x0F00 we'll end up with 0x0400.
             * Shift that 8 bits right and we end up with 0x04 or 0x4. Same thing with y.
             * We & the instruction with hex value 0x00F0 and get 0x0060.
             * Shift that 4 bits right and we end up with 0x006 or 0x6.
             */

            switch ((ushort)(opcode & 0xF000))  // grabs the upper 4 bits of the most significant byte of the opcode
            {
                case 0x0000:
                    // 0nnn - SYS addr
                    // This opcode can be ignored.
                    switch (opcode)
                    {
                        case 0x00E0:
                            // 00E0 - CLS
                            // Clear the display.
                            _renderer.Clear();
                            break;
                        case 0x00EE:
                            // 00EE - RET
                            // The interpreter sets the program counter to the address
                            // at the top of the stack, then subtracts 1 from the stack pointer.
                            // This will return us from a subroutine.
                            _pc = Pop(_stack, _sp);
                            _sp--;
                            break;
                        default:
                            throw new Exception($"Unknown opcode 0x{Convert.ToString(opcode, 16)}");
                    }
                    break;
                case 0x1000:
                    // 1nnn - JP addr
                    // Jump to location nnn.
                    // The interpreter sets the program counter to nnn.
                    _pc = nnn;
                    break;
                case 0x2000:
                    // 2nnn - CALL addr
                    // Call subroutine at nnn.
                    // The interpreter increments the stack pointer,
                    // then puts the current PC on the top of the stack. The PC is then set to nnn.
                    //_sp++;
                    //Push(_stack, _sp, _pc);
                    _stack[++_sp] = _pc;
                    _pc = nnn;
                    break;
                case 0x3000:
                    // 3xkk - SE Vx, byte
                    // Skip next instruction if Vx = kk.
                    // The interpreter compares register Vx to kk,
                    // and if they are equal, increments the program counter by 2.
                    if (_v[x] == kk)
                        _pc += 2;
                    break;
                case 0x4000:
                    // 4xkk - SNE Vx, byte
                    // Skip next instruction if Vx != kk.
                    // The interpreter compares register Vx to kk,
                    // and if they are not equal, increments the program counter by 2.
                    if (_v[x] != kk)
                        _pc += 2;
                    break;
                case 0x5000:
                    // 5xy0 - SE Vx, Vy
                    // Skip next instruction if Vx = Vy.
                    // The interpreter compares register Vx to register Vy,
                    // and if they are equal, increments the program counter by 2.
                    if (_v[x] == _v[y])
                        _pc += 2;
                    break;
                case 0x6000:
                    // 6xkk - LD Vx, byte
                    // Set Vx = kk.
                    // The interpreter puts the value kk into register Vx.
                    _v[x] = kk;
                    break;
                case 0x7000:
                    // 7xkk - ADD Vx, byte
                    // Set Vx = Vx + kk.
                    // Adds the value kk to the value of register Vx,
                    // then stores the result in Vx.
                    _v[x] += kk;
                    break;
                case 0x8000:
                    switch (opcode & 0xF)
                    {
                        case 0x0:
                            // 8xy0 - LD Vx, Vy
                            // Set Vx = Vy.
                            // Stores the value of register Vy in register Vx.
                            _v[x] = _v[y];
                            break;
                        case 0x1:
                            // 8xy1 - OR Vx, Vy
                            // Set Vx = Vx OR Vy.
                            // Performs a bitwise OR on the values of Vx and Vy,
                            // then stores the result in Vx. A bitwise OR compares
                            // the corrseponding bits from two values, and if either
                            // bit is 1, then the same bit in the result is also 1.
                            // Otherwise, it is 0.
                            _v[x] |= _v[y];
                            break;
                        case 0x2:
                            // 8xy2 - AND Vx, Vy
                            // Set Vx = Vx AND Vy.
                            // Performs a bitwise AND on the values of Vx and Vy,
                            // then stores the result in Vx. A bitwise AND compares
                            // the corrseponding bits from two values, and if both bits are 1,
                            // then the same bit in the result is also 1. Otherwise, it is 0.
                            _v[x] &= _v[y];
                            break;
                        case 0x3:
                            // 8xy3 - XOR Vx, Vy
                            // Set Vx = Vx XOR Vy.
                            // Performs a bitwise exclusive OR on the values of Vx and Vy,
                            // then stores the result in Vx. An exclusive OR compares the
                            // corrseponding bits from two values, and if the bits are not
                            // both the same, then the corresponding bit in the result is set to 1.
                            // Otherwise, it is 0.
                            _v[x] ^= _v[y];
                            break;
                        case 0x4:
                            // 8xy4 - ADD Vx, Vy
                            // Set Vx = Vx + Vy, set VF = carry.
                            // The values of Vx and Vy are added together.
                            // If the result is greater than 8 bits (i.e., > 255,)
                            // VF is set to 1, otherwise 0.
                            // Only the lowest 8 bits of the result are kept, and stored in Vx.
                            var sum = _v[x] + _v[y];
                            _v[0xF] = 0;
                            if (sum > 0xFF)  // 0xFF == 255
                                _v[0xF] = 1;
                            _v[x] = (byte)(sum & 0xFF);
                            break;
                        case 0x5:
                            // 8xy5 - SUB Vx, Vy
                            // Set Vx = Vx - Vy, set VF = NOT borrow.
                            // If Vx > Vy, then VF is set to 1, otherwise 0.
                            // Then Vy is subtracted from Vx, and the results stored in Vx.
                            _v[0xF] = 0;
                            if (_v[x] > _v[y])
                                _v[0xF] = 1;
                            _v[x] -= _v[y];
                            break;
                        case 0x6:
                            // 8xy6 - SHR Vx {, Vy}
                            // Set Vx = Vx SHR 1.
                            // If the least-significant bit of Vx is 1,
                            // then VF is set to 1, otherwise 0. Then Vx is divided by 2.
                            _v[0xF] = (byte)(_v[x] & 0x1);  // This line is going to determine the least-significant bit and set VF accordingly.
                            _v[x] >>= 1;
                            break;
                        case 0x7:
                            // 8xy7 - SUBN Vx, Vy
                            // Set Vx = Vy - Vx, set VF = NOT borrow.
                            // If Vy > Vx, then VF is set to 1, otherwise 0.
                            // Then Vx is subtracted from Vy, and the results stored in Vx.
                            _v[0xF] = 0;
                            if (_v[y] > _v[x])
                                _v[0xF] = 1;
                            _v[x] = (byte)(_v[y] - _v[x]);
                            break;
                        case 0xE:
                            // 8xyE - SHL Vx {, Vy}
                            // Set Vx = Vx SHL 1.
                            // If the most-significant bit of Vx is 1, then VF is set to 1,
                            // otherwise to 0. Then Vx is multiplied by 2.
                            _v[0xF] = (byte)(_v[x] & 0x10);
                            _v[x] <<= 1;  // Multiply Vx by 2 by shifting it left 1.
                            break;
                        default:
                            throw new Exception($"Unknown opcode 0x{Convert.ToString(opcode, 16)}");
                    }
                    break;
                case 0x9000:
                    // 9xy0 - SNE Vx, Vy
                    // Skip next instruction if Vx != Vy.
                    // The values of Vx and Vy are compared, and if they are not equal,
                    // the program counter is increased by 2.
                    if (_v[x] != _v[y])
                        _pc += 2;
                    break;
                case 0xA000:
                    // Annn - LD I, addr
                    // Set I = nnn.
                    // The value of register I is set to nnn.
                    _i = nnn;
                    break;
                case 0xB000:
                    // Bnnn - JP V0, addr
                    // Jump to location nnn + V0.
                    // The program counter is set to nnn plus the value of V0.
                    _pc = (ushort)(nnn + _v[0]);
                    break;
                case 0xC000:
                    // Cxkk - RND Vx, byte
                    // Set Vx = random byte AND kk.
                    // The interpreter generates a random number from 0 to 255,
                    // which is then ANDed with the value kk. The results are stored in Vx.
                    var randomValue = new Random().Next(0xFF);
                    _v[x] = (byte)(randomValue & kk);
                    break;
                case 0xD000:
                    // Dxyn - DRW Vx, Vy, nibble
                    // Display n-byte sprite starting at memory location I at (Vx, Vy), set VF = collision.

                    // Each sprite is 8 pixels wide, so it's safe to hardcode that value in
                    byte width = 8;

                    // Set height to the value of the last nibble (n) of the opcode.
                    // If our opcode is 0xD235, height will be set to 5
                    byte height = n;

                    // Set VF to 0, which if necessary, will be set to 1 later on if pixels are erased.
                    _v[0xF] = 0;

                    for (int row = 0; row < height; row++)
                    {
                        // Grabs 8-bits of memory, or a single row of a sprite, that's stored at _i + row.
                        // The technical reference states we start at the address stored in I,
                        // or _i in our case, when we read sprites from memory.
                        var sprite = _memory[_i + row];

                        for (ushort col = 0; col < width; col++)
                        {
                            // Grabs the leftmost bit and checks to see if it's greater than 0.
                            // If the bit (sprite) is not 0, render/erase the pixel.
                            // A value of 0 indicates that the sprite does not have a pixel at that location,
                            // so we don't need to worry about drawing or erasing it.
                            // If the value is 1, we move on to another if statement that checks the return value of SetPixel
                            if ((sprite & 0x80) > 0)
                            {
                                // If setPixel returns 1, which means a pixel was erased, set VF to 1
                                if (_renderer.SetPixel((byte)(_v[x] + col), (byte)(_v[y] + row)))
                                {
                                    _v[0xF] = 1;
                                }
                            }

                            // Shift the sprite left 1. This will move the next next col/bit of the sprite into the first position.
                            // Ex. 10010000 << 1 will become 0010000
                            sprite <<= 1;
                        }
                    }
                    break;
                case 0xE000:
                    switch (opcode & 0xFF)
                    {
                        case 0x9E:
                            // Ex9E - SKP Vx
                            // Skip next instruction if key with the value of Vx is pressed.
                            // Checks the keyboard, and if the key corresponding to the value
                            // of Vx is currently in the down position, PC is increased by 2.
                            if (_keyboard.IsKeyPressed(_v[x]))
                                _pc += 2;
                            break;
                        case 0xA1:
                            // ExA1 - SKNP Vx
                            // Skip next instruction if key with the value of Vx is not pressed.
                            if (!_keyboard.IsKeyPressed(_v[x]))
                                _pc += 2;
                            break;
                        default:
                            throw new Exception($"Unknown opcode {Convert.ToString(opcode, 16)}");
                    }
                    break;
                case 0xF000:
                    switch (opcode & 0xFF)
                    {
                        case 0x07:
                            // Fx07 - LD Vx, DT
                            // Set Vx = delay timer value.
                            _v[x] = _delayTimer;
                            break;
                        case 0x0A:
                            // Fx0A - LD Vx, K
                            // Wait for a key press, store the value of the key in Vx.
                            // All execution stops until a key is pressed, then the value of that key is stored in Vx.
                            // TODO: implement properly
                            throw new NotImplementedException();
                        case 0x15:
                            // Fx15 - LD DT, Vx
                            // Set delay timer = Vx.
                            _delayTimer = _v[x];
                            break;
                        case 0x18:
                            // Fx18 - LD ST, Vx
                            // Set sound timer = Vx.
                            _soundTimer = _v[x];
                            break;
                        case 0x1E:
                            // Fx1E - ADD I, Vx
                            // Set I = I + Vx.
                            _i += _v[x];
                            break;
                        case 0x29:
                            // Fx29 - LD F, Vx - ADD I, Vx
                            // Set I = location of sprite for digit Vx.
                            // It's multiplied by 5 because each sprite is 5 bytes long.
                            _i += (ushort)(_v[x] * 5);
                            break;
                        case 0x33:
                            // Fx33 - LD B, Vx
                            // Store BCD representation of Vx in memory locations I, I+1, and I+2.

                            // Get the hundreds digit and place it in I.
                            _memory[_i] = (byte)(_v[x] / 100);

                            // Get tens digit and place it in I+1. Gets a value between 0 and 99,
                            // then divides by 10 to give us a value between 0 and 9.
                            _memory[_i + 1] = (byte)((_v[x] % 100) / 10);

                            // Get the value of the ones (last) digit and place it in I+2.
                            _memory[_i + 2] = (byte)(_v[x] % 10);
                            break;
                        case 0x55:
                            // Fx55 - LD [I], Vx
                            // Store registers V0 through Vx in memory starting at location I.
                            // The interpreter copies the values of registers V0 through Vx into memory,
                            // starting at the address in I.
                            for (int registerIndex = 0; registerIndex <= x; registerIndex++)
                                _memory[_i + registerIndex] = _v[registerIndex];
                            break;
                        case 0x65:
                            // Fx65 - LD Vx, [I]
                            // The interpreter reads values from memory starting
                            // at location I into registers V0 through Vx.
                            for (int registerIndex = 0; registerIndex <= x; registerIndex++)
                                _memory[registerIndex] = _v[_i + registerIndex];
                            break;
                        default:
                            throw new Exception($"Unknown opcode 0x{Convert.ToString(opcode, 16)}");
                    }
                    break;

                default:
                    throw new Exception($"Unknown opcode 0x{Convert.ToString(opcode, 16)}");
            }
        }
    }
}
