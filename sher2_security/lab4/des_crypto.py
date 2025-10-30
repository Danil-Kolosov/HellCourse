import sys
import os
from PySide6.QtWidgets import (QApplication, QMainWindow, QVBoxLayout, 
                               QHBoxLayout, QPushButton, QTextEdit, 
                               QLabel, QFileDialog, QWidget, QMessageBox)
from PySide6.QtCore import Qt

class SimpleDES:
    """
    Упрощенная реализация DES для 16-битных блоков
    """
    def __init__(self, key):
        # Преобразуем ключ в 16-битное число
        self.key = key & 0xFFFF
        
        # Упрощенные S-блоки (для учебных целей)
        self.s_boxes = [
            # S-box 1
            [
                [14, 4, 13, 1, 2, 15, 11, 8, 3, 10, 6, 12, 5, 9, 0, 7],
                [0, 15, 7, 4, 14, 2, 13, 1, 10, 6, 12, 11, 9, 5, 3, 8],
                [4, 1, 14, 8, 13, 6, 2, 11, 15, 12, 9, 7, 3, 10, 5, 0],
                [15, 12, 8, 2, 4, 9, 1, 7, 5, 11, 3, 14, 10, 0, 6, 13]
            ]
        ]
    
    def permute(self, block, permutation):
        """
        Выполняет перестановку битов в блоке согласно таблице permutation
        """
        result = 0
        for i, pos in enumerate(permutation):
            # Если бит в позиции pos установлен, устанавливаем соответствующий бит в результате
            if block & (1 << (15 - pos)):
                result |= (1 << (15 - i))
                """
                Переставляет биты согласно таблице permutation
                permutation = [2, 0, 1]  # таблица перестановки
                block = 0b110 (6 в десятичной)

                i=0, pos=2: проверяем 2-й бит исходного блока (1), ставим в 0-ю позицию результата
                i=1, pos=0: проверяем 0-й бит исходного блока (0), ставим в 1-ю позицию результата  
                i=2, pos=1: проверяем 1-й бит исходного блока (1), ставим в 2-ю позицию результата

                Результат: 0b101 (5 в десятичной)"""
        return result
    
    def expand(self, block):
        """
        Расширяет 8-битный блок до 16 бит для XOR с ключом
        """
        # Простая расширяющая перестановка
        expansion_table = [3, 0, 1, 2, 1, 2, 3, 0, 3, 0, 1, 2, 1, 2, 3, 0]
        return self.permute(block, expansion_table)
    
    def s_box_substitution(self, block):
        """
        Применяет S-блоки к блоку
        """
        result = 0
        # Разбиваем 16-битный блок на 4 части по 4 бита
        for i in range(4):
            # Извлекаем 4 бита
            part = (block >> (12 - i * 4)) & 0xF
            # Применяем S-бокс
            row = ((part >> 3) & 0x1) * 2 + ((part >> 2) & 0x1)
            col = part & 0x3
            sbox_value = self.s_boxes[0][row][col]
            result = (result << 4) | sbox_value
        return result
    
    def p_box_permutation(self, block):
        """
        P-бокс перестановка
        """
        p_box = [1, 5, 9, 13, 2, 6, 10, 14, 3, 7, 11, 15, 4, 8, 12, 0]
        return self.permute(block, p_box)
    
    def feistel_function(self, right_half, round_key):
        """
        Функция Фейстеля - сердце алгоритма DES
        """
        # 1. Расширяем правую половину (8 бит -> 16 бит)
        expanded = self.expand(right_half)
        
        # 2. XOR с раундовым ключом
        xored = expanded ^ round_key
        
        # 3. S-бокс подстановка
        substituted = self.s_box_substitution(xored)
        
        # 4. P-бокс перестановка
        return self.p_box_permutation(substituted)
    
    def generate_round_keys(self):
        """
        Генерирует раундовые ключи
        """
        keys = []
        key = self.key
        for i in range(4):  # 4 раунда
            # Циклический сдвиг влево на 3 бита
            key = ((key << 3) | (key >> 13)) & 0xFFFF
            """
            key << 3 - сдвигаем все биты влево на 3 позиции

            key >> 13 - сдвигаем все биты вправо на 13 позиций

            | - побитовое ИЛИ (объединяем результаты)

            & 0xFFFF - обрезаем до 16 бит (маска)"""
            keys.append(key)
        return keys
    
    def encrypt_block(self, block):
        """
        Шифрует один 16-битный блок
        """
        # Генерируем раундовые ключи
        round_keys = self.generate_round_keys()
        
        # Начальная перестановка
        initial_perm = [1, 5, 9, 13, 2, 6, 10, 14, 3, 7, 11, 15, 4, 8, 12, 0]
        block = self.permute(block, initial_perm)
        
        # Разделяем блок на левую и правую части (по 8 бит)
        left = (block >> 8) & 0xFF
        right = block & 0xFF
        
        # 4 раунда Фейстеля
        for i in range(4):
            # Сохраняем правую часть
            temp = right
            
            # Правая часть = левая часть XOR feistel(правая часть, ключ)
            right = left ^ self.feistel_function(temp, round_keys[i])
            
            # Левая часть = старая правая часть
            left = temp
        
        # Объединяем части (после последнего раунда не меняем местами)
        result = (left << 8) | right
        
        # Конечная перестановка (обратная начальной)
        final_perm = [15, 11, 7, 3, 14, 10, 6, 2, 13, 9, 5, 1, 12, 8, 4, 0]
        return self.permute(result, final_perm)
    
    def decrypt_block(self, block):
        """
        Дешифрует один 16-битный блок
        """
        # Генерируем раундовые ключи
        round_keys = self.generate_round_keys()
        
        # Начальная перестановка (такая же как при шифровании)
        initial_perm = [1, 5, 9, 13, 2, 6, 10, 14, 3, 7, 11, 15, 4, 8, 12, 0]
        block = self.permute(block, initial_perm)
        
        # Разделяем блок на левую и правую части
        left = (block >> 8) & 0xFF
        right = block & 0xFF
        
        # 4 раунда Фейстеля в обратном порядке
        for i in range(3, -1, -1):  # от 3 до 0
            # Сохраняем левую часть
            temp = left
            
            # Левая часть = правая часть XOR feistel(левая часть, ключ)
            left = right ^ self.feistel_function(temp, round_keys[i])
            
            # Правая часть = старая левая часть
            right = temp
        
        # Объединяем части
        result = (left << 8) | right
        
        # Конечная перестановка
        final_perm = [15, 11, 7, 3, 14, 10, 6, 2, 13, 9, 5, 1, 12, 8, 4, 0]
        return self.permute(result, final_perm)

class CryptoApp(QMainWindow):
    """
    Главное окно приложения для шифрования/дешифрования файлов
    """
    def __init__(self):
        super().__init__()
        
        # Устанавливаем ключ шифрования (16 бит)
        self.key = 0b1100110011001100  # Пример ключа
        
        # Инициализируем DES
        self.des = SimpleDES(self.key)
        
        # Настраиваем интерфейс
        self.init_ui()
    
    def init_ui(self):
        """Инициализация пользовательского интерфейса"""
        # Создаем центральный виджет
        central_widget = QWidget()
        self.setCentralWidget(central_widget)
        
        # Создаем основной layout
        layout = QVBoxLayout()
        central_widget.setLayout(layout)
        
        # Заголовок
        title = QLabel("Шифрование файлов методом DES (16-бит)")
        title.setAlignment(Qt.AlignCenter)
        layout.addWidget(title)
        
        # Отображение ключа
        key_label = QLabel(f"Используемый ключ: {self.key} ({bin(self.key)[2:]})") # cо 2 чтоб 0b не было видно - призник битовой записи числа
        layout.addWidget(key_label)
        
        # Кнопки для выбора файлов
        file_buttons_layout = QHBoxLayout()
        
        self.encrypt_btn = QPushButton("Выбрать файл для шифрования")
        self.encrypt_btn.clicked.connect(self.encrypt_file)
        file_buttons_layout.addWidget(self.encrypt_btn)
        
        self.decrypt_btn = QPushButton("Выбрать файл для дешифрования")
        self.decrypt_btn.clicked.connect(self.decrypt_file)
        file_buttons_layout.addWidget(self.decrypt_btn)
        
        layout.addLayout(file_buttons_layout)
        
        # Текстовое поле для вывода информации
        self.text_output = QTextEdit()
        self.text_output.setPlaceholderText("Здесь будет отображаться информация о процессе шифрования/дешифрования...")
        layout.addWidget(self.text_output)
        
        # Настраиваем главное окно
        self.setWindowTitle("DES File Crypto")
        self.setGeometry(100, 100, 800, 600)
    
    def log_message(self, message):
        """Добавляет сообщение в текстовое поле"""
        self.text_output.append(message)
    
    
    def encrypt_file(self):
        """Шифрует выбранный файл"""
        try:
            # Открываем диалог выбора файла
            file_path, _ = QFileDialog.getOpenFileName(self, "Выберите файл для шифрования")
            
            if not file_path:
                return
            
            self.log_message(f"Шифрование файла: {file_path}")
            
            # Генерируем имя для зашифрованного файла
            encrypted_path = file_path + ".encrypted"
            
            # Получаем размер исходного файла
            original_size = os.path.getsize(file_path)
            
            # Читаем и шифруем файл
            with open(file_path, 'rb') as input_file:
                with open(encrypted_path, 'wb') as output_file:
                    # Записываем размер исходного файла в начало
                    output_file.write(original_size.to_bytes(4, byteorder='big'))
                    
                    while True:
                        # Читаем блок данных (2 байта = 16 бит)
                        chunk = input_file.read(2)
                        if not chunk:
                            break
                        
                        # Если блок меньше 2 байт, дополняем нулями
                        if len(chunk) < 2:
                            chunk = chunk + b'\x00' * (2 - len(chunk))
                        
                        # Преобразуем байты в 16-битное число
                        block = int.from_bytes(chunk, byteorder='big')
                        
                        # Шифруем блок
                        encrypted_block = self.des.encrypt_block(block)
                        
                        # Записываем зашифрованный блок
                        output_file.write(encrypted_block.to_bytes(2, byteorder='big'))
            
            encrypted_size = os.path.getsize(encrypted_path)
            self.log_message(f"Файл успешно зашифрован: {encrypted_path}")
            self.log_message(f"Размер исходного файла: {original_size} байт")
            self.log_message(f"Размер зашифрованного файла: {encrypted_size} байт")
            self.log_message("=" * 50)
            
        except Exception as e:
            QMessageBox.critical(self, "Ошибка", f"Ошибка при шифровании: {str(e)}")
    
    def decrypt_file(self):
        """Дешифрует выбранный файл"""
        try:
            # Открываем диалог выбора файла
            file_path, _ = QFileDialog.getOpenFileName(self, "Выберите файл для дешифрования")
            
            if not file_path:
                return
            
            self.log_message(f"Дешифрование файла: {file_path}")
            
            # Генерируем имя для расшифрованного файла
            if file_path.endswith('.encrypted'):
                decrypted_path = file_path[:-10] + ".decrypted"
            else:
                decrypted_path = file_path + ".decrypted"
            
            # Читаем и дешифруем файл
            with open(file_path, 'rb') as input_file:
                # Читаем размер исходного файла
                original_size_bytes = input_file.read(4)
                if len(original_size_bytes) < 4:
                    raise ValueError("Файл слишком короткий")
                
                original_size = int.from_bytes(original_size_bytes, byteorder='big')
                
                with open(decrypted_path, 'wb') as output_file:
                    bytes_written = 0
                    
                    while True:
                        # Читаем блок данных (2 байта = 16 бит)
                        chunk = input_file.read(2)
                        if not chunk:
                            break
                        
                        # Если блок меньше 2 байт, дополняем нулями
                        if len(chunk) < 2:
                            chunk = chunk + b'\x00' * (2 - len(chunk))
                        
                        # Преобразуем байты в 16-битное число
                        block = int.from_bytes(chunk, byteorder='big')
                        
                        # Дешифруем блок
                        decrypted_block = self.des.decrypt_block(block)
                        
                        # Преобразуем обратно в байты
                        decrypted_bytes = decrypted_block.to_bytes(2, byteorder='big')
                        
                        # Убираем дополняющие нули в конце файла
                        if bytes_written + 2 > original_size:
                            bytes_to_write = original_size - bytes_written
                            decrypted_bytes = decrypted_bytes[:bytes_to_write]
                        
                        output_file.write(decrypted_bytes)
                        bytes_written += len(decrypted_bytes)
                        
                        if bytes_written >= original_size:
                            break
            
            self.log_message(f"Файл успешно расшифрован: {decrypted_path}")
            
            # Показываем содержимое если это текстовый файл
            self.show_file_content(decrypted_path)
            self.log_message("=" * 50)
            
        except Exception as e:
            QMessageBox.critical(self, "Ошибка", f"Ошибка при дешифровании: {str(e)}")
    
    def show_file_content(self, file_path):
        """Показывает содержимое файла если это текст"""
        try:
            with open(file_path, 'rb') as file:
                content = file.read()
                
                # Проверяем, является ли файл текстовым
                try:
                    text_content = content.decode('utf-8')
                    self.log_message("Содержимое файла:")
                    self.log_message(text_content[:1000] + "..." if len(text_content) > 1000 else text_content)
                except UnicodeDecodeError:
                    self.log_message("Файл содержит бинарные данные (не текст)")
                    self.log_message(f"Размер файла: {len(content)} байт")
                    
        except Exception as e:
            self.log_message(f"Не удалось прочитать содержимое файла: {str(e)}")

def main():
    """
    Главная функция, запускает приложение
    """
    # Создаем экземпляр приложения
    app = QApplication(sys.argv)
    
    # Создаем и показываем главное окно
    window = CryptoApp()
    window.show()
    
    # Запускаем главный цикл приложения
    sys.exit(app.exec())

# Точка входа в программу
if __name__ == "__main__":
    main()