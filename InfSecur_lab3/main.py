import numpy as np
import matplotlib.pyplot as plt

class LinearCongruentialGenerator:
    def __init__(self, multiplier, increment, modulus, initial_seed):
        self.M = multiplier  # Multiplier
        self.C = increment   # Increment
        self.P = modulus     # Modulus
        self.seed = initial_seed  # Initial seed

    # Generate random numbers with LCG
    def generate_random_number(self):
        self.seed = (self.seed * self.M + self.C) % self.P  # LCG formula to generate new seed
        return self.seed / self.P  # Normalize random value between 0 and 1

    # Detect the cycle length in LCG using Floyd's cycle-finding algorithm
    def detect_cycle(self):
        tortoise = self.generate_random_number()  # Move one step
        hare = self.generate_random_number()  # Move two steps

        while tortoise != hare:
            tortoise = self.generate_random_number()
            hare = self.generate_random_number()
            hare = self.generate_random_number()  # Move two steps again

        # At this point, tortoise and hare are at the same position within the cycle
        # Reset hare to the initial seed and move both tortoise and hare one step at a time until they meet again
        mu = 0
        hare = self.seed
        while tortoise != hare:
            tortoise = self.generate_random_number()
            hare = self.generate_random_number()
            mu += 1

        # Now we have the start of the cycle, reset tortoise to hare and move both one step at a time until they meet again
        lam = 1
        hare = self.generate_random_number()
        while tortoise != hare:
            hare = self.generate_random_number()
            lam += 1

        return lam

    # Generate an array of random numbers
    def generate_random_array(self, length):
        random_values = []  # List to store random values

        # Generate the specified number of random values
        for _ in range(length):
            if len(random_values) > 10000:  # Safety limit to prevent infinite loops
                raise Exception("Generated too many random numbers. Check for errors.")

            rand_val = self.generate_random_number()  # Generate random number
            random_values.append(rand_val)  # Store in the list

        return random_values

# Parameters for the Linear Congruential Generator (LCG)
initial_seed = 0  # Starting seed for the LCG
multiplier = 101  # Multiplier
increment = 1  # Increment
modulus = 2**32  # Modulus for the LCG

# Create LCG instance
lcg = LinearCongruentialGenerator(multiplier, increment, modulus, initial_seed)

# Calculate the cycle length
cycle_length = lcg.detect_cycle()
print("Cycle length for LCG:", cycle_length)

# Generate an array of random numbers for visualization
random_array_length = 10000  # Desired length for the random array
random_array = lcg.generate_random_array(random_array_length)

# Visualize the distribution of random numbers using a histogram
plt.figure()
plt.hist(random_array, bins=50, density=True, color='skyblue', edgecolor='black')  # Histogram with 50 bins
plt.title("Distribution of Random Numbers (Histogram)")
plt.xlabel("Random Number")
plt.ylabel("Frequency")
plt.grid(True)
plt.show()

# Visualize the relationship between pairs of random numbers using a 2D histogram
x_vals = random_array[::2]  # Use even indices for x values
y_vals = random_array[1::2]  # Use odd indices for y values

plt.figure()
plt.hist2d(x_vals, y_vals, bins=20, cmap='viridis')  # 20 bins for each axis
plt.title("2D Histogram of Random Numbers")
plt.xlabel("X Values")
plt.ylabel("Y Values")
plt.colorbar(label="Frequency")  # Color bar for frequency
plt.grid(True)
plt.show()
