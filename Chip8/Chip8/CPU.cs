using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;

namespace Chip8.Chip8
{
    public class CPU
    {
        /// <summary>
        /// The location from which most programs start.
        /// </summary>
        /// <remarks>
        /// The first 512 bytes, from 0x000 to 0x1FF, are where the original
        /// interpreter was located, and should not be used by programs.
        /// Most Chip-8 programs start at location 0x200 (512),
        /// but some begin at 0x600 (1536). Programs beginning at 0x600
        /// are intended for the ETI 660 computer.
        /// </remarks>
        private const ushort MemoryStartLocation = 0x200;
        /// <summary>
        /// Width of the display.
        /// </summary>
        public const int DisplayWidth = 512;  // 64 pixels * 8 bits
        /// <summary>
        /// Height of the display.
        /// </summary>
        public const int DisplayHeight = 256;  // 32 pixels * 8 bits
        /// <summary>
        /// 16-key hexadecimal keypad.
        /// </summary>
        private Keyboard _keyboard { get; set; }
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
        private int _delayTimer { get; set; }
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
        /// <summary>
        /// Whether or not the execution is paused.
        /// </summary>
        private bool _paused { get; set; }
        /// <summary>
        /// Table of coordinates that represents the display.
        /// </summary>
        public bool[,] Display { get; set; }

        public CPU(Keyboard keyboard, Speaker speaker)
        {
            _keyboard = keyboard;
            _speaker = speaker;

            _memory = new byte[4096];

            _v = new byte[16];

            _i = 0;

            _delayTimer = 0;
            _soundTimer = 0;

            _pc = MemoryStartLocation;
            _stack = new ushort[16];
            _sp = 0;

            Display = new bool[DisplayWidth, DisplayHeight];
        }

        /// <summary>
        /// Loads a sound effect.
        /// </summary>
        /// <param name="sound">Sound effect.</param>
        public void LoadSoundEffect(SoundEffect sound)
        {
            _speaker.LoadSoundEffect(sound);
        }

        /// <summary>
        /// Loops through each byte in the sprites array and stores it in memory starting at hex 0x000.
        /// </summary>
        public void LoadSpritesIntoMemory()
        {
            // Array of hex values for each sprite. Each sprite is 5 bytes.
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

            // Sprites are stored in the interpreter section of memory starting at 0x000
            for (int i = 0; i < sprites.Length; i++)
                _memory[i] = sprites[i];
        }

        /// <summary>
        /// Stores the contents of the program in the chip8 memory.
        /// </summary>
        /// <param name="program">Set of instructions.</param>
        public void LoadProgramIntoMemory(byte[] program)
        {
            Debug.WriteLine("Reading program instructions");
            for (int loc = 0; loc < program.Length; loc++)
            {
                _memory[0x200 + loc] = program[loc];

                if (loc + 1 < program.Length)
                {
                    if (loc % 2 == 0)
                        Debug.Write($"0x{Convert.ToString(program[loc], 16).PadLeft(2, '0')}");
                    else
                        Debug.WriteLine($"{Convert.ToString(program[loc], 16).PadLeft(2, '0')}");
                }
            }
            Debug.WriteLine("End of program instructions");
        }

        /// <summary>
        /// Loads a ROM stored in the local memory.
        /// </summary>
        /// <param name="path">Local path to the ROM file.</param>
        public void LoadROM(string path)
        {
            using BinaryWriter reader = new BinaryWriter(File.Open(path, FileMode.Open));
            var rom = new byte[reader.BaseStream.Length];

            for (int i = 0; i < reader.BaseStream.Length; i++)
                rom[i] = (byte)reader.BaseStream.ReadByte();

            LoadProgramIntoMemory(rom);
        }

        /// <summary>
        /// Executes CPU cycle.
        /// </summary>
        public void Cycle()
        {
            ushort opcode = (ushort)((_memory[_pc] << 8) | _memory[_pc + 1]);

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
                
                Debug.WriteLine($"Executing opcode 0x{Convert.ToString(opcode, 16).PadLeft(4, '0')}" +
                    $" (memory[{_pc}] = 0x{Convert.ToString(_memory[_pc] << 8, 16).PadLeft(2, '0')} |" +
                    $" memory[{_pc + 1}] = 0x{Convert.ToString(_memory[_pc + 1], 16).PadLeft(2, '0')})");
                ExecuteInstruction(opcode);
                UpdateTimers();
                PlaySound();
            }
            else
            {
                byte? keyPressed = _keyboard.GetPressedKey();
                if (keyPressed != null)
                {
                    byte x = (byte)((opcode & 0x0F00) >> 8);
                    _v[x] = (byte)keyPressed;
                    _paused = false;
                    Cycle();
                }
            }
        }

        /// <summary>
        /// Updates the values of the timers.
        /// </summary>
        private void UpdateTimers()
        {
            // Each timer, delay and sound, decrement by 1 at a rate of 60Hz.
            // In other words, every 60 frames our timers will decrement by 1.
            if (_delayTimer > 0)
            {
                _delayTimer--;
            }

            if (_soundTimer > 0)
            {
                _soundTimer--;
            }
        }

        /// <summary>
        /// Plays a sound as long as the sound timer > 0.
        /// </summary>
        private void PlaySound()
        {
            if (_soundTimer > 0)
            {
                _speaker.Play();
            }
            else
            {
                _speaker.Stop();
            }
        }

        /// <summary>
        /// Executes an instruction.
        /// </summary>
        /// <param name="opcode">Hexadecimal instruction.</param>
        private void ExecuteInstruction(ushort opcode)
        {
             // All instructions are 2 bytes long.
             // So every time we execute an instruction, or run this function,
             // we have to increment the program counter (_pc)
             // by 2 so the CPU knows where the next instruction is.
            _pc += 2;

            var x   =   (byte)((opcode & 0x0F00) >> 8);   // the lower 4 bits of the high byte
            var y   =   (byte)((opcode & 0x00F0) >> 4);   // the upper 4 bits of the low byte
            var n   =   (byte)(opcode & 0x000F);          // the lowest 4 bits
            var kk  =   (byte)(opcode & 0x00FF);          // the lowest 8 bits
            var nnn =   (ushort)(opcode & 0x0FFF);        // the lowest 12 bits

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
                            Display = new bool[DisplayWidth, DisplayHeight];
                            break;
                        case 0x00EE:
                            // 00EE - RET
                            // The interpreter sets the program counter to the address
                            // at the top of the stack, then subtracts 1 from the stack pointer.
                            // This will return us from a subroutine.
                            _pc = _stack[_sp];
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
                    _sp++;
                    _stack[_sp] = _pc;
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

                    /*
                     * The interpreter reads n bytes from memory, starting at the address stored in I.
                     * These bytes are then displayed as sprites on screen at coordinates (Vx, Vy).
                     * Sprites are XORed onto the existing screen. If this causes any pixels to be erased,
                     * VF is set to 1, otherwise it is set to 0. If the sprite is positioned so part of
                     * it is outside the coordinates of the display, it wraps around to the opposite side of the screen.
                     */

                    _v[0xF] = 0;
                    for (int i = 0; i < n; i++)
                    {
                        // Bits are added to the BitArray in the reverse order,
                        // so 1000 0000 is stored in the array as 0000 0001.
                        // To get around that, the byte is reversed before being
                        // added to the array.
                        BitArray memoryBits = new BitArray(new byte[] { ReverseByte(_memory[_i + i]) });

                        for (int j = 0; j < memoryBits.Length; j++)
                        {
                            int coordinateX = _v[x] + j;
                            int coordinateY = _v[y] + i;

                            if (coordinateX > DisplayWidth)
                                coordinateX -= DisplayWidth;
                            if (coordinateX < 0)
                                coordinateX += DisplayWidth;
                            if (coordinateY > DisplayHeight)
                                coordinateY -= DisplayHeight;
                            if (coordinateY < 0)
                                coordinateY += DisplayHeight;

                            bool oldBit = Display[coordinateX, coordinateY];

                            Display[coordinateX, coordinateY] ^= memoryBits[j];

                            if (oldBit && !Display[coordinateX, coordinateY])
                                _v[0xF] = 1;
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
                            _v[x] = (byte)_delayTimer;
                            break;
                        case 0x0A:
                            // Fx0A - LD Vx, K
                            // Wait for a key press, store the value of the key in Vx.
                            // All execution stops until a key is pressed, then the value of that key is stored in Vx.
                            _paused = true;
                            break;
                        case 0x15:
                            // Fx15 - LD DT, Vx
                            // Set delay timer = Vx.
                            _delayTimer = _v[x] * 60;  // decrements at a rate of 60Hz
                            break;
                        case 0x18:
                            // Fx18 - LD ST, Vx
                            // Set sound timer = Vx.
                            _soundTimer = _v[x] * 60;  // decrements at a rate of 60Hz
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
                            _i = (ushort)(_v[x] * 5);
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
                                _v[registerIndex] = _memory[_i + registerIndex];
                            break;
                        default:
                            throw new Exception($"Unknown opcode 0x{Convert.ToString(opcode, 16)}");
                    }
                    break;

                default:
                    throw new Exception($"Unknown opcode 0x{Convert.ToString(opcode, 16)}");
            }
        }

        /// <summary>
        /// Reverses the bits of a byte.
        /// </summary>
        /// <param name="b">Byte to be reversed.</param>
        /// <returns>Reversed byte.</returns>
        public byte ReverseByte(byte b)
        {
            b = (byte)((b & 0xF0) >> 4 | (b & 0x0F) << 4);
            b = (byte)((b & 0xCC) >> 2 | (b & 0x33) << 2);
            b = (byte)((b & 0xAA) >> 1 | (b & 0x55) << 1);
            return b;
        }
    }
}
