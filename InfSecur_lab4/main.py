import random
import math

def gcd(a, b):
    while b != 0:
        a, b = b, a % b
    return a

def generate_prime(bits):
    while True:
        p = random.getrandbits(bits)
        if p % 2 != 0 and is_prime(p):
            return p

def is_prime(n, k=5):
    if n <= 1:
        return False
    if n <= 3:
        return True
    if n % 2 == 0:
        return False

    # Write n as d*2^r + 1
    r = 0
    d = n - 1
    while d % 2 == 0:
        d //= 2
        r += 1

    # Witness loop
    for _ in range(k):
        a = random.randint(2, n - 1)
        x = pow(a, d, n)
        if x == 1 or x == n - 1:
            continue
        for _ in range(r - 1):
            x = pow(x, 2, n)
            if x == n - 1:
                break
        else:
            return False
    return True

def generate_keys(bits):
    p = generate_prime(bits)
    q = generate_prime(bits)
    n = p * q
    phi = (p - 1) * (q - 1)

    e = random.randint(1, phi)
    g = gcd(e, phi)
    while g != 1:
        e = random.randint(1, phi)
        g = gcd(e, phi)

    d = mod_inverse(e, phi)
    return ((n, e), (n, d))

def mod_inverse(a, m):
    m0, x0, x1 = m, 0, 1
    while a > 1:
        q = a // m
        m, a = a % m, m
        x0, x1 = x1 - q * x0, x0
    return x1 + m0 if x1 < 0 else x1

def encrypt(message, public_key):
    n, e = public_key
    cipher = [pow(ord(char), e, n) for char in message]
    return cipher

def decrypt(cipher, private_key):
    n, d = private_key
    message = ''.join([chr(pow(char, d, n)) for char in cipher])
    return message

# Пример использования
message = "Hello, World!"
public_key, private_key = generate_keys(1024)
encrypted_message = encrypt(message, public_key)
decrypted_message = decrypt(encrypted_message, private_key)

print("Original message:", message)
print("Encrypted message:", encrypted_message)
print("Decrypted message:", decrypted_message)
