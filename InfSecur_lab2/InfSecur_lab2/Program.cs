using System;
using System.Collections;
using System.Linq;
using System.Text;

class Program
{
    static BitArray RightShift(BitArray array, int n)
    {
        int length = array.Length;
        BitArray res = new BitArray(length);
        for (int i = 0; i < length; i++)
        {
            res[i] = false; // Initialize with zeros
        }
        if (n < length)
        {
            for (int i = n; i < length; i++)
            {
                res[i] = array[i - n];
            }
        }
        return res;
    }

    static BitArray LeftShift(BitArray array, int n)
    {
        int length = array.Length;
        BitArray res = new BitArray(length);
        for (int i = 0; i < length; i++)
        {
            res[i] = false;
        }
        if (n < length)
        {
            for (int i = 0; i < length - n; i++)
            {
                res[i] = array[i + n];
            }
        }
        return res;
    }

    static BitArray Transformation(BitArray left, BitArray key)
    {
        BitArray shiftedLeft = LeftShift(left, 3); // Shift left by 3 bits
        BitArray shiftedRightKey = RightShift(key, 2); // Shift right by 2 bits
        BitArray result = shiftedLeft.Xor(shiftedRightKey); // Apply XOR operation
        return result;
    }

    static BitArray EncryptBlock(BitArray block, BitArray key, int numRounds = 2)
    {
        int blockSize = block.Length;
        for (int roundNumber = 0; roundNumber < numRounds; roundNumber++)
        {
            BitArray keySegment = RightShift(key, roundNumber * 8);
            keySegment.Length = 32;
            BitArray left = block.Cast<bool>().Take(blockSize / 2).ToArray().ToBitArray();
            BitArray right = block.Cast<bool>().Skip(blockSize / 2).ToArray().ToBitArray();

            BitArray transformed = Transformation(left, keySegment).Xor(right);

            BitArray newBlock;
            if (roundNumber == numRounds - 1)
            {
                newBlock = left.Cast<bool>().Concat(transformed.Cast<bool>()).ToArray().ToBitArray();
            }
            else
            {
                newBlock = transformed.Cast<bool>().Concat(left.Cast<bool>()).ToArray().ToBitArray();
            }

            block = newBlock;
        }
        return block;
    }

    static BitArray DecryptBlock(BitArray block, BitArray key, int numRounds = 2)
    {
        int blockSize = block.Length;
        for (int roundNumber = 0; roundNumber < numRounds; roundNumber++)
        {
            BitArray keySegment = RightShift(key, (numRounds - roundNumber - 1) * 8);
            keySegment.Length = 32;
            BitArray left = block.Cast<bool>().Take(blockSize / 2).ToArray().ToBitArray();
            BitArray right = block.Cast<bool>().Skip(blockSize / 2).ToArray().ToBitArray();

            BitArray transformed = Transformation(right, keySegment).Xor(left);

            BitArray newBlock;
            if (roundNumber == numRounds - 1)
            {
                newBlock = right.Cast<bool>().Concat(transformed.Cast<bool>()).ToArray().ToBitArray();
            }
            else
            {
                newBlock = transformed.Cast<bool>().Concat(right.Cast<bool>()).ToArray().ToBitArray();
            }

            block = newBlock;
        }
        return block;
    }

    static BitArray EncryptCBC(BitArray plaintext, BitArray key, BitArray IV, int numRounds = 2, int blockSize = 64)
    {
        BitArray encryptedResult = new BitArray(0);
        for (int index = 0; index < plaintext.Length; index += blockSize)
        {
            BitArray block = new BitArray(blockSize);
            for (int i = 0; i < blockSize && index + i < plaintext.Length; i++)
            {
                block[i] = plaintext[index + i];
            }

            if (index == 0)
            {
                block.Xor(IV); // XOR with IV for the first block
            }
            else
            {
                block.Xor(encryptedResult.SubArray(index - blockSize, blockSize).Cast<bool>().ToArray().ToBitArray());
            }

            BitArray encryptedBlock = EncryptBlock(block, key, numRounds);
            encryptedResult = encryptedResult.Cast<bool>().Concat(encryptedBlock.Cast<bool>()).ToArray().ToBitArray();
        }
        return encryptedResult;
    }

    static BitArray DecryptCBC(BitArray ciphertext, BitArray key, BitArray IV, int numRounds = 2, int blockSize = 64)
    {
        BitArray decryptedResult = new BitArray(0);
        for (int index = 0; index < ciphertext.Length; index += blockSize)
        {
            BitArray block = new BitArray(blockSize);
            for (int i = 0; i < blockSize && index + i < ciphertext.Length; i++)
            {
                block[i] = ciphertext[index + i];
            }

            BitArray decryptedBlock = DecryptBlock(block, key, numRounds);

            if (index == 0)
            {
                decryptedBlock.Xor(IV); // XOR with IV for the first block
            }
            else
            {
                decryptedBlock.Xor(ciphertext.SubArray(index - blockSize, blockSize).Cast<bool>().ToArray().ToBitArray());
            }

            decryptedResult = decryptedResult.Cast<bool>().Concat(decryptedBlock.Cast<bool>()).ToArray().ToBitArray();
        }
        return decryptedResult;
    }

    static void Main(string[] args)
    {
        string inputText = "Top Secret Data";
        byte[] inputBytes = Encoding.UTF8.GetBytes(inputText);

        int blockSize = 64;
        byte[] paddedInput = new byte[inputBytes.Length + blockSize - (inputBytes.Length % blockSize)];
        Array.Copy(inputBytes, paddedInput, inputBytes.Length);

        BitArray inputBitArray = new BitArray(paddedInput);

        byte[] keyBytes = new byte[8];
        Random rnd = new Random();
        rnd.NextBytes(keyBytes);
        BitArray keyBitArray = new BitArray(keyBytes);

        byte[] IVBytes = new byte[8];
        rnd.NextBytes(IVBytes);
        BitArray IVBitArray = new BitArray(IVBytes);

        BitArray cfbEncrypted = EncryptCBC(inputBitArray, keyBitArray, IVBitArray, 4);

        BitArray cfbDecrypted = DecryptCBC(cfbEncrypted, keyBitArray, IVBitArray, 4);

        byte[] cfbDecryptedBytes = new byte[cfbDecrypted.Length / 8];
        cfbDecrypted.CopyTo(cfbDecryptedBytes, 0);

        string cfbDecryptedText = Encoding.UTF8.GetString(cfbDecryptedBytes).TrimEnd('\0');

        Console.WriteLine("CFB Encrypted: " + cfbEncrypted.ToString());
        Console.WriteLine("CFB Decrypted: " + cfbDecryptedText);
    }
}

public static class ExtensionMethods
{
    public static BitArray ToBitArray(this bool[] array)
    {
        return new BitArray(array);
    }

    public static BitArray SubArray(this BitArray array, int index, int length)
    {
        bool[] subArray = new bool[length];
        for (int i = 0; i < length; i++)
        {
            subArray[i] = array[i + index];
        }
        return subArray.ToBitArray();
    }
}
