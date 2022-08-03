namespace SaveFix
{
    public class SaveFix
    {
        private const int FILESIZE = 0x8000;

        // SRAM offsets
        private const int SM_OFFSET = 0x2000;
        private const int SM_START = 0x10;

        private const int SM_CHECKSUM = SM_OFFSET + 0x0;
        private const int SM_CHECKSUM_XOR = SM_OFFSET + 0x8;
        private const int SM_CHECKSUM_ALT = SM_OFFSET + 0x1FF0;
        private const int SM_CHECKSUM_XOR_ALT = SM_OFFSET + 0x1FF8;

        private const int SAVE_POINT = SM_OFFSET + 0x166;
        private const int SAVE_AREA = SM_OFFSET + 0x168;

        private const int GAME_LENGTH = 0x65C;

        private static string filePath = "";

        public static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: savefix <path/to/sram>");
                WaitForInput();
                return;
            }

            filePath = args[0];
            var fileInfo = new FileInfo(filePath);
            if (fileInfo.Length != FILESIZE || fileInfo.Extension != ".srm")
            {
                Console.WriteLine("This doesn't appear to be a valid SMZ3 SRAM (.srm) file");
                WaitForInput();
                return;
            }

            CreateBackup();

            using (var fs = File.Open(filePath, FileMode.Open))
            {
                ApplyFix(fs);
                CalculateChecksum(fs);
            }

            Console.WriteLine("SRAM patched!");
            WaitForInput();
        }

        private static void WaitForInput()
        {
            Console.WriteLine("Press any key to close...");
            Console.ReadKey();
        }

        private static void CreateBackup()
        {
            Console.WriteLine("Creating backup...");

            if (File.Exists(filePath + ".bkp"))
            {
                string? input;

                do
                {
                    Console.WriteLine("An existing backup already exists. Overwrite? (Y/N): ");
                    input = Console.ReadLine();

                    if (input == null)
                    {
                        Environment.Exit(1);
                    }

                    if (input.ToLower().Equals("y"))
                    {
                        File.Delete(filePath + ".bkp");
                        File.Copy(filePath, filePath + ".bkp");
                    }
                } while (input.ToLower() != "y" && input.ToLower() != "n");
            }
            else
            {
                File.Copy(filePath, filePath + ".bkp");
            }
        }

        private static void ApplyFix(FileStream sram)
        {
            Console.WriteLine("Applying fix...");
            sram.Position = SAVE_POINT;
            sram.WriteByte(0);
            sram.Position = SAVE_AREA;
            sram.WriteByte(0);
        }

        /// <summary>
        /// The algorithm to calculate the checksum is as follows:
        /// 
        /// For each game file:
        ///     Start at byte 0x10 relative to the beginning of the game file
        ///     Add byte to hi counter
        ///     If hi > 255
        ///         hi &= 255
        ///         lo++
        ///     Take the next byte and add it to the lo counter
        ///     If lo > 255
        ///         lo &= 255
        ///         Do NOT increment hi
        ///     Repeat until 0x65C have been read
        ///     XOR both bytes with 0xFF and store as a complement
        ///     Write hi to 0x0 and 0x1FF0, lo to 0x1 and 0x1FF1, complement hi to 0x8 and 0x1FF8, complement lo to 0x9 and 0x1FF9
        ///     
        /// Games 2 and 3 are written to the bytes following game 1
        /// This ignores the second and third games as they no longer exist in SMZ3
        /// </summary>
        /// <param name="sram"></param>
        private static void CalculateChecksum(FileStream sram)
        {
            Console.WriteLine("Calculating checksum...");

            short hi, lo;
            byte hb, lb, hc, lc;
            hi = lo = 0;

            sram.Position = SM_OFFSET + SM_START;

            for (var i = 0; i < GAME_LENGTH; i++)
            {
                var b = (byte) sram.ReadByte();
                if (i % 2 == 0)
                {
                    if ((hi += b) > 0xFF)
                    {
                        hi &= 0xFF;
                        lo++;
                    }
                }
                else
                {
                    if ((lo += b) > 0xFF)
                    {
                        lo &= 0xFF;
                    }
                }
            }

            hb = (byte)hi;
            lb = (byte)lo;
            hc = (byte)(hb ^ 0xFF);
            lc = (byte)(lb ^ 0xFF);

            sram.Position = SM_CHECKSUM;
            sram.WriteByte(hb);
            sram.WriteByte(lb);
            sram.Position = SM_CHECKSUM_XOR;
            sram.WriteByte(hc);
            sram.WriteByte(lc);

            sram.Position = SM_CHECKSUM_ALT;
            sram.WriteByte(hb);
            sram.WriteByte(lb);
            sram.Position = SM_CHECKSUM_XOR_ALT;
            sram.WriteByte(hc);
            sram.WriteByte(lc);
        }
    }
}