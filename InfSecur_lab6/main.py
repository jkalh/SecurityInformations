from PIL import Image

def hide_message(container_image_path, message, output_path):
    container_image = Image.open(container_image_path)
    message_bits = ''.join(format(ord(char), '08b') for char in message)

    # Add termination marker "00000000" to the message
    message_bits += "00000000"

    container_pixels = list(container_image.getdata())
    new_pixels = []

    message_index = 0
    message_length = len(message_bits)

    for pixel in container_pixels:
        # Extract RGB values from the pixel
        r, g, b = pixel

        # Embed message bits into the least significant bit of each color channel
        if message_index < message_length:
            r &= 0xFE  # Clear the least significant bit
            if message_index < message_length:
                r |= int(message_bits[message_index])  # Set the least significant bit to the message bit
                message_index += 1

        new_pixels.append((r, g, b))

    # Create a new image with the modified pixel data
    new_image = Image.new(container_image.mode, container_image.size)
    new_image.putdata(new_pixels)
    new_image.save(output_path)

def extract_message(container_image_path):
    container_image = Image.open(container_image_path)
    container_pixels = list(container_image.getdata())

    message_bits = ''
    for pixel in container_pixels:
        # Extract the least significant bit of the red channel
        r, _, _ = pixel
        message_bits += str(r & 1)

    # Find the index of the termination marker "00000000"
    end_index = message_bits.find("00000000")
    if end_index != -1:
        # Extract only the message bits before the termination marker
        message_bits = message_bits[:end_index]

        # Convert message bits to characters
        decoded_message = ""
        for i in range(0, len(message_bits), 8):
            byte = message_bits[i:i+8]
            decoded_message += chr(int(byte, 2))

        return decoded_message
    else:
        # If termination marker is not found, return an error message
        return "Termination marker not found. Message extraction unsuccessful."

# Пример использования
container_image_path = "image.bmp"
output_path = "output_image.bmp"
message = "This is a secret message"

# Скрыть сообщение в контейнерном изображении
hide_message(container_image_path, message, output_path)
print("Сообщение успешно скрыто в контейнерном изображении.")

# Извлечь сообщение из контейнерного изображения
extracted_message = extract_message(output_path)
print("Извлеченное сообщение:", extracted_message)
