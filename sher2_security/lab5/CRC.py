import sys
import numpy as np
from PySide6.QtWidgets import (QApplication, QMainWindow, QVBoxLayout, 
                               QHBoxLayout, QWidget, QTextEdit, QPushButton, 
                               QLabel, QMessageBox, QSplitter, QComboBox)
from PySide6.QtCore import Qt

class CyclicCode:
    def __init__(self, generator_poly=None):
        # Доступные порождающие полиномы 7-й степени
        self.available_polys = {
            "x⁷ + x⁶ + x⁵ + x⁴ + 1": [1, 1, 1, 1, 0, 0, 0, 1],
            "x⁷ + x³ + 1": [1, 0, 0, 0, 1, 0, 0, 1],
            "x⁷ + x + 1": [1, 0, 0, 0, 0, 0, 1, 1],
            "x⁷ + x⁶ + 1": [1, 1, 0, 0, 0, 0, 0, 1]
        }
        
        if generator_poly is None:
            generator_poly = self.available_polys["x⁷ + x⁶ + x⁵ + x⁴ + 1"]
        
        self.set_generator_poly(generator_poly)
    
    def set_generator_poly(self, generator_poly):
        """Установка порождающего полинома и пересчет параметров"""
        self.generator_poly = generator_poly
        self.r = len(generator_poly) - 1  # степень полинома (7)
        # Убираем фиксированную длину - работаем с блоками
        self.block_size = 8  # размер блока данных (можно менять)
    
    def text_to_binary(self, text):
        """Преобразование текста в двоичную строку"""
        binary_str = ""
        for char in text:
            # Преобразуем символ в ASCII код (0-255) и затем в 8 бит
            ascii_code = ord(char)
            binary_char = format(ascii_code, '08b')
            binary_str += binary_char
        return binary_str
    
    def binary_to_text(self, binary_str):
        """Преобразование двоичной строки в текст"""
        text = ""
        # Обрабатываем по 8 бит
        for i in range(0, len(binary_str), 8):
            byte = binary_str[i:i+8]
            if len(byte) == 8:
                ascii_code = int(byte, 2)
                # Пропускаем непечатаемые символы кроме пробела
                if 32 <= ascii_code <= 126 or ascii_code == 10:
                    text += chr(ascii_code)
                else:
                    text += f"\\x{ascii_code:02x}"
        return text
    
    def poly_to_str(self, poly):
        """Преобразование полинома в строку фиксированной длины"""
        poly_str = ''.join(str(bit) for bit in poly)
        # Дополняем нулями слева до длины r (7)
        return poly_str.zfill(self.r)
    
    def poly_to_binary(self, poly):
        """Преобразование полинома в двоичную строку"""
        return ''.join(str(x) for x in poly)
    
    def binary_to_poly(self, binary_str):
        """Преобразование двоичной строки в полином"""
        return [int(bit) for bit in binary_str]
    
    def poly_mod(self, dividend, divisor):
        """Деление полиномов по модулю 2"""
        dividend = dividend.copy()
        divisor_len = len(divisor)
        
        while len(dividend) >= divisor_len:
            if dividend[0] == 1:
                for i in range(divisor_len):
                    dividend[i] ^= divisor[i]
            dividend = dividend[1:]
        
        return dividend
    
    def calculate_crc_for_block(self, data_block):
        """Вычисление CRC для одного блока данных"""
        # Сдвигаем данные на r позиций влево
        shifted_data = data_block + [0] * self.r
        
        # Вычисляем остаток от деления (CRC)
        remainder = self.poly_mod(shifted_data, self.generator_poly)
        
        # Формируем блок с контрольной суммой
        block_with_crc = data_block + remainder
        
        return block_with_crc
    
    def calculate_crc_for_binary(self, binary_str):
        """Вычисление CRC для всей двоичной строки (разбивая на блоки)"""
        data_bits = self.binary_to_poly(binary_str)
        result_bits = []
        
        # Обрабатываем данные блоками по block_size бит
        for i in range(0, len(data_bits), self.block_size):
            block = data_bits[i:i + self.block_size]
            
            # Дополняем последний блок нулями если нужно
            if len(block) < self.block_size:
                block = block + [0] * (self.block_size - len(block))
            
            # Вычисляем CRC для блока
            block_with_crc = self.calculate_crc_for_block(block)
            result_bits.extend(block_with_crc)
        
        return self.poly_to_binary(result_bits)
    
    def verify_and_correct_block(self, received_block):
        """Проверка и исправление ошибок для одного блока"""
        n = len(received_block)  # длина блока с CRC
        
        # Вычисляем синдром
        syndrome = self.poly_mod(received_block.copy(), self.generator_poly)
        syndrome_str = self.poly_to_str(syndrome)
        
        # Если синдром нулевой - ошибок нет
        if all(bit == 0 for bit in syndrome):
            return received_block[:self.block_size], "✓ Ошибок не обнаружено", syndrome_str
        
        # Пытаемся найти и исправить одну ошибку
        # Для этого генерируем таблицу синдромов для текущего блока
        syndrome_table = {}
        for error_pos in range(n):
            error_vector = [0] * n
            error_vector[error_pos] = 1
            error_syndrome = self.poly_mod(error_vector, self.generator_poly)
            syndrome_table[self.poly_to_str(error_syndrome)] = error_pos
        
        if syndrome_str in syndrome_table:
            error_pos = syndrome_table[syndrome_str]
            corrected = received_block.copy()
            corrected[error_pos] ^= 1
            return corrected[:self.block_size], f"✓ Исправлена ошибка в позиции {error_pos}", syndrome_str
        
        # Множественные ошибки
        return received_block[:self.block_size], "✗ Обнаружены ошибки. Исправление невозможно", syndrome_str
    
    def verify_and_correct_binary(self, received_binary):
        """Проверка и исправление ошибок для всей двоичной строки"""
        received_bits = self.binary_to_poly(received_binary)
        block_length = self.block_size + self.r  # данные + CRC
        result_bits = []
        messages = []
        
        # Обрабатываем блоки
        for i in range(0, len(received_bits), block_length):
            block = received_bits[i:i + block_length]
            
            if len(block) == block_length:
                corrected_block, message, syndrome = self.verify_and_correct_block(block)
                result_bits.extend(corrected_block)
                messages.append(f"Блок {i//block_length}: {message}")
            else:
                # Неполный блок - оставляем как есть
                result_bits.extend(block)
                messages.append(f"Блок {i//block_length}: Неполный блок")
        
        result_binary = self.poly_to_binary(result_bits)
        return result_binary, "\n".join(messages)

class MainWindow(QMainWindow):
    def __init__(self):
        super().__init__()
        self.cyclic_code = CyclicCode()
        self.initUI()
    
    def initUI(self):
        self.setWindowTitle("CRC - Контроль ошибок")
        self.setGeometry(100, 100, 900, 700)
        
        # Центральный виджет
        central_widget = QWidget()
        self.setCentralWidget(central_widget)
        
        # Основной layout
        layout = QVBoxLayout(central_widget)
        
        # Заголовок
        title = QLabel("Алгоритм CRC (Cyclic Redundancy Check)")
        title.setAlignment(Qt.AlignCenter)
        title.setStyleSheet("font-size: 16pt; font-weight: bold; margin: 10px;")
        layout.addWidget(title)
        
        # Выбор полинома
        poly_layout = QHBoxLayout()
        poly_layout.addWidget(QLabel("Порождающий полином 7-й степени:"))
        self.poly_combo = QComboBox()
        for poly_name in self.cyclic_code.available_polys.keys():
            self.poly_combo.addItem(poly_name)
        self.poly_combo.currentTextChanged.connect(self.change_polynomial)
        poly_layout.addWidget(self.poly_combo)
        poly_layout.addStretch()
        layout.addLayout(poly_layout)
        
        # Разделитель
        splitter = QSplitter(Qt.Vertical)
        layout.addWidget(splitter)
        
        # Верхняя часть - отправитель
        sender_widget = self.create_sender_widget()
        # Нижняя часть - получатель
        receiver_widget = self.create_receiver_widget()
        
        splitter.addWidget(sender_widget)
        splitter.addWidget(receiver_widget)
        splitter.setSizes([300, 400])
    
    def create_sender_widget(self):
        widget = QWidget()
        layout = QVBoxLayout(widget)
        
        sender_label = QLabel("ОТПРАВИТЕЛЬ:")
        sender_label.setStyleSheet("font-size: 12pt; font-weight: bold; color: blue;")
        layout.addWidget(sender_label)
        
        # Поле ввода исходного сообщения
        input_layout = QVBoxLayout()
        input_layout.addWidget(QLabel("Исходное сообщение (текст или биты):"))
        self.input_text = QTextEdit()
        self.input_text.setMaximumHeight(80)
        self.input_text.setPlaceholderText("Введите текст (например: Hello!) или биты (например: 11001010)...")
        input_layout.addWidget(self.input_text)
        layout.addLayout(input_layout)
        
        # Кнопка вычисления контрольной суммы
        self.calculate_btn = QPushButton("Вычислить контрольную сумму и отправить")
        self.calculate_btn.clicked.connect(self.calculate_crc)
        layout.addWidget(self.calculate_btn)
        
        # Поле вывода сообщения с контрольной суммой
        output_layout = QVBoxLayout()
        output_layout.addWidget(QLabel("Сообщение с контрольной суммой (биты):"))
        self.output_text = QTextEdit()
        self.output_text.setMaximumHeight(80)
        self.output_text.setReadOnly(True)
        output_layout.addWidget(self.output_text)
        layout.addLayout(output_layout)
        
        return widget
    
    def create_receiver_widget(self):
        widget = QWidget()
        layout = QVBoxLayout(widget)
        
        receiver_label = QLabel("ПОЛУЧАТЕЛЬ:")
        receiver_label.setStyleSheet("font-size: 12pt; font-weight: bold; color: green;")
        layout.addWidget(receiver_label)
        
        # Поле ввода принятого сообщения
        received_layout = QVBoxLayout()
        received_layout.addWidget(QLabel("Принятое сообщение (биты):"))
        self.received_text = QTextEdit()
        self.received_text.setMaximumHeight(80)
        self.received_text.setPlaceholderText("Можно изменить биты для имитации ошибки...")
        received_layout.addWidget(self.received_text)
        layout.addLayout(received_layout)
        
        # Кнопка проверки
        self.verify_btn = QPushButton("Проверить и получить сообщение")
        self.verify_btn.clicked.connect(self.verify_message)
        layout.addWidget(self.verify_btn)
        
        # Поле вывода результата проверки
        result_layout = QVBoxLayout()
        result_layout.addWidget(QLabel("Результат проверки:"))
        
        self.result_text = QTextEdit()
        self.result_text.setMaximumHeight(120)
        self.result_text.setReadOnly(True)
        result_layout.addWidget(self.result_text)
        
        layout.addLayout(result_layout)
        
        # Информация о системе
        self.info_label = QLabel()
        self.update_info_label()
        layout.addWidget(self.info_label)
        
        return widget
    
    def update_info_label(self):
        info_text = (
            f"Система контроля ошибок CRC\n"
            f"Порождающий полином: {self.poly_combo.currentText()}\n"
            f"Текст → ASCII → двоичный код → CRC → передача\n"
            f"Размер блока: {self.cyclic_code.block_size} бит данных + {self.cyclic_code.r} бит CRC\n"
        )
        self.info_label.setText(info_text)
        self.info_label.setAlignment(Qt.AlignCenter)
        self.info_label.setStyleSheet("font-size: 10pt; margin: 10px; padding: 10px; background-color: #f0f0f0;")
    
    def change_polynomial(self, poly_name):
        """Изменение порождающего полинома"""
        generator_poly = self.cyclic_code.available_polys[poly_name]
        self.cyclic_code.set_generator_poly(generator_poly)
        self.update_info_label()
        
        # Очищаем поля
        self.output_text.clear()
        self.received_text.clear()
        self.result_text.clear()
    
    def calculate_crc(self):
        """Вычисление контрольной суммы и формирование сообщения"""
        try:
            input_str = self.input_text.toPlainText().strip()
            
            if not input_str:
                QMessageBox.warning(self, "Ошибка", "Введите сообщение для отправки")
                return
            
            # Определяем тип ввода: текст или биты
            if all(c in '01' for c in input_str):
                # Ввод в двоичном формате
                binary_input = input_str
                input_type = "биты"
            else:
                # Ввод в текстовом формате - преобразуем в ASCII
                binary_input = self.cyclic_code.text_to_binary(input_str)
                input_type = "текст"
            
            # Вычисляем контрольную сумму
            message_with_crc = self.cyclic_code.calculate_crc_for_binary(binary_input)
            
            # Выводим результат
            self.output_text.setPlainText(message_with_crc)
            
            # Автоматически заполняем поле принятого сообщения
            self.received_text.setPlainText(message_with_crc)
            
            # Очищаем результат предыдущей проверки
            self.result_text.clear()
            
            # Показываем информацию о преобразовании
            info = f"Входные данные: {input_type}\nДлина исходных бит: {len(binary_input)}\nДлина с CRC: {len(message_with_crc)}"
            self.result_text.setText(info)
            
        except Exception as e:
            QMessageBox.critical(self, "Ошибка", f"Ошибка при вычислении контрольной суммы: {str(e)}")
    
    def verify_message(self):
        """Проверка принятого сообщения"""
        try:
            received_str = self.received_text.toPlainText().strip()
            
            if not received_str:
                QMessageBox.warning(self, "Ошибка", "Введите принятое сообщение")
                return
            
            if not all(c in '01' for c in received_str):
                QMessageBox.warning(self, "Ошибка", "Принятое сообщение должно содержать только биты (0 и 1)")
                return
            
            # Проверяем сообщение
            original_binary, message = self.cyclic_code.verify_and_correct_binary(received_str)
            
            # Пытаемся преобразовать обратно в текст
            try:
                original_text = self.cyclic_code.binary_to_text(original_binary)
                result_text = f"Текст: {original_text}\n"
                result_text += f"Биты: {original_binary}\n"
                result_text += f"Статус:\n{message}"
            except:
                result_text = f"Биты: {original_binary}\n"
                result_text += f"Статус:\n{message}"
            
            self.result_text.setPlainText(result_text)
            
        except Exception as e:
            QMessageBox.critical(self, "Ошибка", f"Ошибка при проверке сообщения: {str(e)}")

def main():
    app = QApplication(sys.argv)
    
    window = MainWindow()
    window.show()
    
    sys.exit(app.exec())

if __name__ == "__main__":
    main()