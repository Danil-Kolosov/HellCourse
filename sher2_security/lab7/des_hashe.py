import sys
from PySide6.QtWidgets import (QApplication, QMainWindow, QVBoxLayout, 
                               QHBoxLayout, QPushButton, QTextEdit, 
                               QLabel, QWidget, QMessageBox,
                               QLineEdit, QGroupBox)
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
            if block & (1 << (15 - pos)):
                result |= (1 << (15 - i))
        return result
    
    def expand(self, block):
        """
        Расширяет 8-битный блок до 16 бит для XOR с ключом
        """
        expansion_table = [3, 0, 1, 2, 1, 2, 3, 0, 3, 0, 1, 2, 1, 2, 3, 0]
        return self.permute(block, expansion_table)
    
    def s_box_substitution(self, block):
        """
        Применяет S-блоки к блоку
        """
        result = 0
        for i in range(4):
            part = (block >> (12 - i * 4)) & 0xF
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
        Функция Фейстеля
        """
        expanded = self.expand(right_half)
        xored = expanded ^ round_key
        substituted = self.s_box_substitution(xored)
        return self.p_box_permutation(substituted)
    
    def generate_round_keys(self):
        """
        Генерирует раундовые ключи
        """
        keys = []
        key = self.key
        for i in range(4):
            key = ((key << 3) | (key >> 13)) & 0xFFFF
            keys.append(key)
        return keys
    
    def encrypt_block(self, block):
        """
        Шифрует один 16-битный блок
        """
        round_keys = self.generate_round_keys()
        
        initial_perm = [1, 5, 9, 13, 2, 6, 10, 14, 3, 7, 11, 15, 4, 8, 12, 0]
        block = self.permute(block, initial_perm)
        
        left = (block >> 8) & 0xFF
        right = block & 0xFF
        
        for i in range(4):
            temp = right
            right = left ^ self.feistel_function(temp, round_keys[i])
            left = temp
        
        result = (left << 8) | right
        final_perm = [15, 11, 7, 3, 14, 10, 6, 2, 13, 9, 5, 1, 12, 8, 4, 0]
        return self.permute(result, final_perm)

class PasswordHasher:
    """
    Класс для хеширования паролей с использованием DES
    """
    def __init__(self, salt=0x1234):
        self.salt = salt  # Соль для усиления хеша
    
    def hash_password(self, password):
        """
        Хеширует пароль с использованием DES в конструкции Davies-Meyer
        H_i = E_{H_{i-1}}(M_i) XOR M_i
        """
        if len(password) < 4:
            raise ValueError("Пароль должен быть не менее 4 символов")
        
        if len(password) > 64:
            raise ValueError("Пароль должен быть не более 64 символов")
        
        # Преобразуем пароль в байты
        password_bytes = password.encode('utf-8')
        
        # Начальное значение хеша (IV - Initialization Vector)
        current_hash = 0xABCD
        
        # Добавляем соль к паролю для усиления безопасности
        salt_bytes = self.salt.to_bytes(2, byteorder='big')
        salted_password = salt_bytes + password_bytes
        
        # Обрабатываем salted_password блоками по 2 байта
        for i in range(0, len(salted_password), 2):
            # Берем блок данных
            block_bytes = salted_password[i:i+2]
            if len(block_bytes) < 2:
                block_bytes += b'\x00'  # Дополнение последнего блока
            
            data_block = int.from_bytes(block_bytes, byteorder='big')
            
            # Davies-Meyer construction: H_i = E_{H_{i-1}}(M_i) XOR M_i
            des = SimpleDES(current_hash)  # Текущий хеш как ключ для DES
            encrypted = des.encrypt_block(data_block)
            current_hash = encrypted ^ data_block  # XOR с исходными данными
        
        return current_hash
    
    def verify_password(self, password, stored_hash):
        """
        Проверяет пароль против сохраненного хеша
        Вычисляет хеш от введенного пароля и сравнивает с stored_hash
        """
        computed_hash = self.hash_password(password)
        return computed_hash == stored_hash

class HashApp(QMainWindow):
    """
    Главное окно приложения для хеширования паролей
    """
    def __init__(self):
        super().__init__()
        
        # Инициализируем хешер
        self.hasher = PasswordHasher()
        
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
        title = QLabel("Хеширование паролей с использованием DES")
        title.setAlignment(Qt.AlignCenter)
        title.setStyleSheet("font-size: 16px; font-weight: bold; margin: 10px;")
        layout.addWidget(title)
        
        # Информация о алгоритме
        info_label = QLabel(
            
        )
        info_label.setAlignment(Qt.AlignCenter)
        info_label.setStyleSheet("margin: 5px;")
        layout.addWidget(info_label)
        
        # Группа для хеширования
        hash_group = QGroupBox("Создание хеша пароля")
        hash_layout = QVBoxLayout()
        hash_group.setLayout(hash_layout)
        
        # Поле для ввода пароля
        hash_layout.addWidget(QLabel("Введите пароль (4-64 символа):"))
        self.password_input = QLineEdit()
        self.password_input.setPlaceholderText("Введите пароль для хеширования...")
        hash_layout.addWidget(self.password_input)
        
        # Кнопка хеширования
        self.hash_btn = QPushButton("Создать хеш")
        self.hash_btn.clicked.connect(self.create_hash)
        hash_layout.addWidget(self.hash_btn)
        
        # Поле для отображения хеша
        hash_layout.addWidget(QLabel("Хеш пароля:"))
        self.hash_output = QLineEdit()
        self.hash_output.setReadOnly(True)
        hash_layout.addWidget(self.hash_output)
        
        layout.addWidget(hash_group)
        
        # Группа для проверки
        verify_group = QGroupBox("Проверка пароля по хешу")
        verify_layout = QVBoxLayout()
        verify_group.setLayout(verify_layout)
        
        # Поле для ввода пароля для проверки
        verify_layout.addWidget(QLabel("Введите пароль для проверки:"))
        self.verify_password_input = QLineEdit()
        self.verify_password_input.setPlaceholderText("Введите пароль...")
        verify_layout.addWidget(self.verify_password_input)
        
        # Поле для ввода хеша для проверки
        verify_layout.addWidget(QLabel("Введите хеш для проверки:"))
        self.verify_hash_input = QLineEdit()
        self.verify_hash_input.setPlaceholderText("Введите хеш в hex-формате (0x...)")
        verify_layout.addWidget(self.verify_hash_input)
        
        # Кнопка проверки
        self.verify_btn = QPushButton("Проверить пароль")
        self.verify_btn.clicked.connect(self.verify_password)
        verify_layout.addWidget(self.verify_btn)
        
        # Результат проверки
        self.verify_result = QLabel("")
        self.verify_result.setAlignment(Qt.AlignCenter)
        verify_layout.addWidget(self.verify_result)
        
        layout.addWidget(verify_group)
        
        # Текстовое поле для вывода информации
        self.text_output = QTextEdit()
        self.text_output.setPlaceholderText("Здесь будет отображаться информация о процессе хеширования...")
        layout.addWidget(self.text_output)
        
        # Настраиваем главное окно
        self.setWindowTitle("DES Password Hasher")
        self.setGeometry(100, 100, 600, 700)
    
    def log_message(self, message):
        """Добавляет сообщение в текстовое поле"""
        self.text_output.append(message)
    
    def create_hash(self):
        """Создает хеш введенного пароля"""
        try:
            password = self.password_input.text()
            
            if len(password) < 4:
                QMessageBox.warning(self, "Ошибка", "Пароль должен содержать не менее 4 символов")
                return
            
            if len(password) > 64:
                QMessageBox.warning(self, "Ошибка", "Пароль должен содержать не более 64 символов")
                return
            
            # Хешируем пароль
            password_hash = self.hasher.hash_password(password)
            
            # Отображаем хеш
            hex_hash = hex(password_hash)
            self.hash_output.setText(hex_hash)
            
            self.log_message("=== СОЗДАНИЕ ХЕША ===")
            self.log_message(f"Пароль: {'*' * len(password)}")
            self.log_message(f"Длина пароля: {len(password)} символов")
            self.log_message(f"Хеш (16-ричный): {hex_hash}")
            self.log_message(f"Хеш (десятичный): {password_hash}")            
            self.log_message("-" * 50)
            
        except Exception as e:
            QMessageBox.critical(self, "Ошибка", f"Ошибка при хешировании: {str(e)}")
    
    def verify_password(self):
        """Проверяет пароль против введенного хеша"""
        try:
            password = self.verify_password_input.text()
            hash_input = self.verify_hash_input.text()
            
            if not password or not hash_input:
                QMessageBox.warning(self, "Ошибка", "Введите пароль и хеш для проверки")
                return
            
            # Преобразуем хеш в число
            try:
                if hash_input.startswith('0x'):
                    stored_hash = int(hash_input, 16)
                else:
                    stored_hash = int(hash_input)
            except ValueError:
                QMessageBox.warning(self, "Ошибка", "Неверный формат хеша. Используйте hex (0x...) или десятичный формат")
                return
            
            # Проверяем пароль
            is_valid = self.hasher.verify_password(password, stored_hash)
            
            if is_valid:
                self.verify_result.setText("✓ Пароль верный - хеши совпадают")
                self.verify_result.setStyleSheet("color: green; font-weight: bold; padding: 5px;")
                self.log_message(f"ПРОВЕРКА: пароль совпадает с хешем {hex(stored_hash)}")
            else:
                self.verify_result.setText("✗ Пароль неверный - хеши не совпадают")
                self.verify_result.setStyleSheet("color: red; font-weight: bold; padding: 5px;")
                self.log_message(f"ПРОВЕРКА: пароль НЕ совпадает с хешем {hex(stored_hash)}")
            
        except Exception as e:
            QMessageBox.critical(self, "Ошибка", f"Ошибка при проверке: {str(e)}")

def main():
    """
    Главная функция, запускает приложение
    """
    app = QApplication(sys.argv)
    window = HashApp()
    window.show()
    sys.exit(app.exec())

if __name__ == "__main__":
    main()