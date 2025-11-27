import sys
import os
import secrets
from PySide6.QtWidgets import (QApplication, QMainWindow, QVBoxLayout, 
                               QHBoxLayout, QWidget, QPushButton, 
                               QLabel, QFileDialog, QMessageBox, 
                               QTextEdit, QProgressBar, QLineEdit)
from PySide6.QtCore import QThread, Signal

class CryptoWorker(QThread):
    progress = Signal(int)
    finished = Signal(str)
    error = Signal(str)

    def __init__(self, input_file, key, mode):
        super().__init__()
        self.input_file = input_file
        self.key = key
        self.mode = mode  # 'encrypt' or 'decrypt'

    def run(self):
        try:
            if self.mode == 'encrypt':
                output_file = self.input_file + '.encrypted'
                self.encrypt_file(output_file)
                self.finished.emit(f"Файл зашифрован: {output_file}\nКлюч: {self.key.hex()}")
            else:
                if self.input_file.endswith('.encrypted'):
                    output_file = self.input_file[:-10] + '.decrypted'
                else:
                    output_file = self.input_file + '.decrypted'
                self.decrypt_file(output_file)
                self.finished.emit(f"Файл расшифрован: {output_file}")
        except Exception as e:
            self.error.emit(str(e))

    def process_block(self, block, gamma_block):
        """Обработка одного блока (32 бита = 4 байта)"""
        if len(block) < 4:
            # Дополнение последнего блока нулями
            block = block.ljust(4, b'\x00')
        
        # Преобразуем блоки в целые числа
        block_int = int.from_bytes(block, byteorder='big')
        gamma_int = int.from_bytes(gamma_block, byteorder='big')
        
        # Выполняем сложение по модулю 2^32
        result_int = (block_int + gamma_int) % (2**32)
        
        # Преобразуем обратно в байты
        return result_int.to_bytes(4, byteorder='big')

    def encrypt_file(self, output_file):
        """Шифрование файла"""
        with open(self.input_file, 'rb') as f:
            data = f.read()

        # Шифруем данные
        encrypted_data = bytearray()
        total_blocks = (len(data) + 3) // 4  # Округление вверх

        for i in range(total_blocks):
            start_idx = i * 4
            end_idx = start_idx + 4
            block = data[start_idx:end_idx]
            
            # Берем соответствующий блок гаммы из ключа
            gamma_block = self.key[start_idx:end_idx]
            if len(gamma_block) < 4:
                gamma_block = gamma_block.ljust(4, b'\x00')
            
            encrypted_block = self.process_block(block, gamma_block)
            encrypted_data.extend(encrypted_block)
            
            # Обновляем прогресс
            progress = int((i + 1) / total_blocks * 100)
            self.progress.emit(progress)

        # Сохраняем зашифрованные данные
        with open(output_file, 'wb') as f:
            f.write(encrypted_data)

    def decrypt_file(self, output_file):
        """Дешифрование файла"""
        with open(self.input_file, 'rb') as f:
            encrypted_data = f.read()

        # Дешифруем данные
        decrypted_data = bytearray()
        total_blocks = (len(encrypted_data) + 3) // 4

        for i in range(total_blocks):
            start_idx = i * 4
            end_idx = start_idx + 4
            block = encrypted_data[start_idx:end_idx]
            
            # Берем соответствующий блок гаммы из ключа
            gamma_block = self.key[start_idx:end_idx]
            if len(gamma_block) < 4:
                gamma_block = gamma_block.ljust(4, b'\x00')
            
            # Для дешифрования используем вычитание по модулю 2^32
            block_int = int.from_bytes(block, byteorder='big')
            gamma_int = int.from_bytes(gamma_block, byteorder='big')
            
            # Вычитание по модулю 2^32
            decrypted_int = (block_int - gamma_int) % (2**32)
            decrypted_block = decrypted_int.to_bytes(4, byteorder='big')
            
            decrypted_data.extend(decrypted_block)
            
            # Обновляем прогресс
            progress = int((i + 1) / total_blocks * 100)
            self.progress.emit(progress)

        # Убираем дополняющие нули (только для дешифрования)
        if self.mode == 'decrypt':
            original_length = len(decrypted_data)
            while original_length > 0 and decrypted_data[original_length - 1] == 0:
                original_length -= 1
            decrypted_data = decrypted_data[:original_length]

        # Сохраняем расшифрованные данные
        with open(output_file, 'wb') as f:
            f.write(decrypted_data)


class CryptoApp(QMainWindow):
    def __init__(self):
        super().__init__()
        self.initUI()
        self.current_input_file = ""
        self.current_key = b""

    def initUI(self):
        self.setWindowTitle("Шифрование методом однократного гаммирования")
        self.setGeometry(100, 100, 600, 500)

        # Центральный виджет
        central_widget = QWidget()
        self.setCentralWidget(central_widget)

        # Основной layout
        layout = QVBoxLayout()

        # Поле для информации
        self.info_text = QTextEdit()
        self.info_text.setReadOnly(True)
        self.info_text.setPlaceholderText("Информация о операциях будет отображаться здесь...")
        layout.addWidget(self.info_text)

        # Progress bar
        self.progress_bar = QProgressBar()
        self.progress_bar.setVisible(False)
        layout.addWidget(self.progress_bar)

        # Кнопка выбора файла
        file_layout = QHBoxLayout()
        
        self.input_btn = QPushButton("Выбрать файл для шифрования/дешифрования")
        self.input_btn.clicked.connect(self.select_input_file)
        file_layout.addWidget(self.input_btn)

        layout.addLayout(file_layout)

        # Метка с путем файла
        self.input_label = QLabel("Файл: не выбран")
        layout.addWidget(self.input_label)

        # Поле для ввода/отображения ключа
        key_layout = QVBoxLayout()
        key_layout.addWidget(QLabel("Ключ шифрования (hex):"))
        
        self.key_edit = QLineEdit()
        self.key_edit.setPlaceholderText("Автоматически генерируется при шифровании...")
        key_layout.addWidget(self.key_edit)
        
        layout.addLayout(key_layout)

        # Кнопки операций
        op_layout = QHBoxLayout()
        
        self.encrypt_btn = QPushButton("Зашифровать")
        self.encrypt_btn.clicked.connect(self.encrypt)
        self.encrypt_btn.setEnabled(False)
        op_layout.addWidget(self.encrypt_btn)

        self.decrypt_btn = QPushButton("Расшифровать")
        self.decrypt_btn.clicked.connect(self.decrypt)
        self.decrypt_btn.setEnabled(False)
        op_layout.addWidget(self.decrypt_btn)

        layout.addLayout(op_layout)

        # Кнопка генерации ключа
        self.generate_key_btn = QPushButton("Сгенерировать новый ключ")
        self.generate_key_btn.clicked.connect(self.generate_key)
        layout.addWidget(self.generate_key_btn)

        central_widget.setLayout(layout)

    def log_message(self, message):
        """Добавление сообщения в лог"""
        self.info_text.append(message)

    def select_input_file(self):
        file_path, _ = QFileDialog.getOpenFileName(self, "Выберите файл")
        if file_path:
            self.current_input_file = file_path
            self.input_label.setText(f"Файл: {os.path.basename(file_path)}")
            self.check_buttons_state()
            
            # Автоматически определяем режим по расширению
            if file_path.endswith('.encrypted'):
                self.log_message("Обнаружен зашифрованный файл. Готов к дешифрованию.")
                self.decrypt_btn.setEnabled(True)
            else:
                self.log_message("Обнаружен обычный файл. Готов к шифрованию.")
                self.encrypt_btn.setEnabled(True)

    def generate_key(self):
        """Генерация случайного ключа"""
        if self.current_input_file:
            # Определяем размер ключа на основе размера файла
            file_size = os.path.getsize(self.current_input_file)
            # Ключ должен быть не меньше размера файла
            key_size = max(file_size, 1024)  # Минимум 1KB для маленьких файлов
            self.current_key = secrets.token_bytes(key_size)
            self.key_edit.setText(self.current_key.hex())
            self.log_message(f"Сгенерирован новый ключ длиной {key_size} байт")
        else:
            QMessageBox.warning(self, "Ошибка", "Сначала выберите файл!")

    def check_buttons_state(self):
        """Проверка состояния кнопок"""
        file_ok = bool(self.current_input_file)
        key_ok = bool(self.key_edit.text().strip())
        
        if file_ok and self.current_input_file.endswith('.encrypted'):
            self.decrypt_btn.setEnabled(key_ok)
            self.encrypt_btn.setEnabled(False)
        elif file_ok:
            self.encrypt_btn.setEnabled(key_ok)
            self.decrypt_btn.setEnabled(False)

    def encrypt(self):
        if not self.current_input_file:
            QMessageBox.warning(self, "Ошибка", "Выберите файл для шифрования!")
            return

        if not self.key_edit.text().strip():
            QMessageBox.warning(self, "Ошибка", "Сгенерируйте ключ шифрования!")
            return

        try:
            # Преобразуем ключ из hex строки в байты
            key_hex = self.key_edit.text().strip()
            self.current_key = bytes.fromhex(key_hex)
        except ValueError:
            QMessageBox.warning(self, "Ошибка", "Неверный формат ключа! Используйте hex-строку.")
            return

        self.log_message("Начато шифрование...")
        self.progress_bar.setVisible(True)
        
        self.worker = CryptoWorker(
            self.current_input_file,
            self.current_key,
            'encrypt'
        )
        self.worker.progress.connect(self.progress_bar.setValue)
        self.worker.finished.connect(self.operation_finished)
        self.worker.error.connect(self.operation_error)
        self.worker.start()

    def decrypt(self):
        if not self.current_input_file:
            QMessageBox.warning(self, "Ошибка", "Выберите файл для дешифрования!")
            return

        if not self.key_edit.text().strip():
            QMessageBox.warning(self, "Ошибка", "Введите ключ дешифрования!")
            return

        try:
            # Преобразуем ключ из hex строки в байты
            key_hex = self.key_edit.text().strip()
            self.current_key = bytes.fromhex(key_hex)
        except ValueError:
            QMessageBox.warning(self, "Ошибка", "Неверный формат ключа! Используйте hex-строку.")
            return

        self.log_message("Начато дешифрование...")
        self.progress_bar.setVisible(True)
        
        self.worker = CryptoWorker(
            self.current_input_file,
            self.current_key,
            'decrypt'
        )
        self.worker.progress.connect(self.progress_bar.setValue)
        self.worker.finished.connect(self.operation_finished)
        self.worker.error.connect(self.operation_error)
        self.worker.start()

    def operation_finished(self, message):
        self.log_message(message)
        self.progress_bar.setVisible(False)
        QMessageBox.information(self, "Успех", "Операция завершена успешно!")

    def operation_error(self, error_message):
        self.log_message(f"Ошибка: {error_message}")
        self.progress_bar.setVisible(False)
        QMessageBox.critical(self, "Ошибка", f"Произошла ошибка: {error_message}")


if __name__ == "__main__":
    app = QApplication(sys.argv)
    
    window = CryptoApp()
    window.show()
    
    sys.exit(app.exec())