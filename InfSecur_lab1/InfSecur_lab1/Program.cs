using System;
using System.Collections;
using System.Text;

public class CustomEncryption
{
    // Shift functions
    private BitArray RightShift(BitArray array, int n)
    {
        BitArray res = new BitArray(array.Length);
        for (int i = 0; i < array.Length; i++)
        {
            int newIndex = (i + n) % array.Length;
            res[newIndex] = array[i];
        }
        return res;
    }

    private BitArray LeftShift(BitArray array, int n)
    {
        BitArray res = new BitArray(array.Length);
        for (int i = 0; i < array.Length; i++)
        {
            int newIndex = (i - n + array.Length) % array.Length;
            res[newIndex] = array[i];
        }
        return res;
    }

    // Transformation function for Feistel-like structure
    private BitArray F(BitArray left, BitArray key)
    {
        BitArray shiftedLeft = LeftShift(left, 3);  // Left shift
        BitArray shiftedRightKey = RightShift(key, 2);  // Right shift key
        BitArray transformed = shiftedLeft.Xor(shiftedRightKey);  // XOR with shifted key
        return transformed;
    }

    // Custom encryption and decryption functions (with block size checks)
    public BitArray Encrypt(BitArray plaintext, BitArray key, int numRounds = 2, int blockSize = 64)
    {
        BitArray ciphertext = new BitArray(0);  // Initialize ciphertext
        BitArray newBlock = new BitArray(0);
        for (int index = 0; index < plaintext.Length; index += blockSize)
        {
            BitArray block = new BitArray(blockSize);
            for (int i = 0; i < blockSize; i++)
            {
                if (index + i < plaintext.Length)
                    block[i] = plaintext[index + i];
            }

            int halfSize = blockSize / 2;
            BitArray left = new BitArray(halfSize);
            BitArray right = new BitArray(halfSize);
            for (int i = 0; i < halfSize; i++)
            {
                left[i] = block[i];
                right[i] = block[i + halfSize];
            }

            // Perform multiple rounds of encryption
            for (int roundNumber = 0; roundNumber < numRounds; roundNumber++)
            {
                BitArray keySegment = RightShift(key, roundNumber * 8);
                BitArray key32 = new BitArray(32);
                for (int i = 0; i < 32; i++)
                {
                    key32[i] = keySegment[i];
                }
                BitArray transformed = F(left, key32);  // Apply transformation function
                BitArray newRight = transformed.Xor(right);

                if (roundNumber == numRounds - 1)
                {
                    newBlock = Concat(left, newRight);  // Final round maintains order
                }
                else
                {
                    newBlock = Concat(newRight, left);  // Swap halves in intermediate rounds
                }

                left = newRight;
                right = left;
            }

            ciphertext = Concat(ciphertext, newBlock);  // Append to ciphertext
        }

        return ciphertext;
    }

    public BitArray Decrypt(BitArray ciphertext, BitArray key, int numRounds = 2, int blockSize = 64)
    {
        BitArray plaintext = new BitArray(0);  // Initialize plaintext
        BitArray newBlock = new BitArray(0);
        for (int index = 0; index < ciphertext.Length; index += blockSize)
        {
            BitArray block = new BitArray(blockSize);
            for (int i = 0; i < blockSize; i++)
            {
                if (index + i < ciphertext.Length)
                    block[i] = ciphertext[index + i];
            }

            int halfSize = blockSize / 2;
            BitArray left = new BitArray(halfSize);
            BitArray right = new BitArray(halfSize);
            for (int i = 0; i < halfSize; i++)
            {
                left[i] = block[i];
                right[i] = block[i + halfSize];
            }

            // Perform multiple rounds of decryption in reverse order
            for (int roundNumber = numRounds - 1; roundNumber >= 0; roundNumber--)
            {
                BitArray keySegment = RightShift(key, roundNumber * 8);
                BitArray key32 = new BitArray(32);
                for (int i = 0; i < 32; i++)
                {
                    key32[i] = keySegment[i];
                }
                BitArray transformed = F(right, key32);  // Apply transformation
                BitArray newLeft = transformed.Xor(left);

                if (roundNumber == 0)
                {
                    newBlock = Concat(right, newLeft);  // Final round maintains order
                }
                else
                {
                    newBlock = Concat(newLeft, right);  // Swap halves in intermediate rounds
                }

                right = newLeft;
                left = right;
            }

            plaintext = Concat(plaintext, newBlock);  // Append to plaintext
        }

        return plaintext;
    }

    // Concatenate two bit arrays
    private BitArray Concat(BitArray array1, BitArray array2)
    {
        BitArray res = new BitArray(array1.Length + array2.Length);
        for (int i = 0; i < array1.Length; i++)
        {
            res[i] = array1[i];
        }
        for (int i = 0; i < array2.Length; i++)
        {
            res[array1.Length + i] = array2[i];
        }
        return res;
    }

    // Test
    public static void Main(string[] args)
    {
        CustomEncryption customEncryption = new CustomEncryption();
        string originalText = "My secret message";
        byte[] originalBytes = Encoding.UTF8.GetBytes(originalText);

        // Ensure even block size with proper padding
        byte[] paddedBytes = new byte[originalBytes.Length + (8 - originalBytes.Length % 8)];
        Array.Copy(originalBytes, paddedBytes, originalBytes.Length);

        // Convert to bit array
        BitArray originalBitArray = new BitArray(paddedBytes);

        // Generate a random key
        Random random = new Random();
        byte[] keyBytes = new byte[8];
        random.NextBytes(keyBytes);
        BitArray keyBitArray = new BitArray(keyBytes);

        // Encrypt
        int numRounds = 4;
        BitArray encrypted = customEncryption.Encrypt(originalBitArray, keyBitArray, numRounds);

        // Decrypt
        BitArray decrypted = customEncryption.Decrypt(encrypted, keyBitArray, numRounds);

        // Convert decrypted BitArray to bytes
        byte[] decryptedBytes = new byte[decrypted.Length / 8];
        decrypted.CopyTo(decryptedBytes, 0);

        // Convert bytes to string
        string decryptedText = Encoding.UTF8.GetString(decryptedBytes).TrimEnd('\0');

        // Display results
        Console.WriteLine("Original Text: " + originalText);
        Console.WriteLine("Encrypted: " + BitArrayToString(encrypted));
        Console.WriteLine("Decrypted: " + BitArrayToString(decrypted));
        Console.WriteLine("Decrypted Text: " + decryptedText);
    }

    // Helper function to convert BitArray to string
    private static string BitArrayToString(BitArray bits)
    {
        StringBuilder sb = new StringBuilder(bits.Length);
        for (int i = 0; i < bits.Length; i++)
        {
            sb.Append(bits[i] ? '1' : '0');
        }
        return sb.ToString();
    }
}
