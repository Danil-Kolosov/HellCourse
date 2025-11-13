import sys
from collections import Counter, defaultdict
from PySide6.QtWidgets import (QApplication, QMainWindow, QWidget, QVBoxLayout, 
                               QHBoxLayout, QTextEdit, QPushButton, QLabel, 
                               QMessageBox, QTabWidget)
from PySide6.QtCore import Qt

class HuffmanNode:
    def __init__(self, char, freq):
        self.char = char
        self.freq = freq
        self.left = None
        self.right = None
    
    def __lt__(self, other):
        return self.freq < other.freq

class CompressionLab(QMainWindow):
    def __init__(self):
        super().__init__()
        self.initUI()
        
    def initUI(self):
        self.setWindowTitle("Лабораторная работа: Сжатие данных")
        self.setGeometry(100, 100, 800, 600)
        
        central_widget = QWidget()
        self.setCentralWidget(central_widget)
        
        layout = QVBoxLayout(central_widget)
        
        # Создаем вкладки
        tabs = QTabWidget()
        layout.addWidget(tabs)
        
        # Вкладка для ввода
        input_tab = QWidget()
        input_layout = QVBoxLayout(input_tab)
        
        input_layout.addWidget(QLabel("Введите текст для сжатия:"))
        self.input_text = QTextEdit()
        self.input_text.setPlaceholderText("Введите текст здесь...")
        input_layout.addWidget(self.input_text)
        
        # Кнопки
        button_layout = QHBoxLayout()
        self.compress_btn = QPushButton("Выполнить сжатие")
        self.compress_btn.clicked.connect(self.compress_data)
        button_layout.addWidget(self.compress_btn)
        
        self.clear_btn = QPushButton("Очистить")
        self.clear_btn.clicked.connect(self.clear_all)
        button_layout.addWidget(self.clear_btn)
        
        input_layout.addLayout(button_layout)
        
        # Вкладка для результатов Хаффмана
        huffman_tab = QWidget()
        huffman_layout = QVBoxLayout(huffman_tab)
        
        huffman_layout.addWidget(QLabel("Результат сжатия Хаффмана:"))
        self.huffman_result = QTextEdit()
        self.huffman_result.setReadOnly(True)
        huffman_layout.addWidget(self.huffman_result)
        
        # Вкладка для результатов LZW
        lzw_tab = QWidget()
        lzw_layout = QVBoxLayout(lzw_tab)
        
        lzw_layout.addWidget(QLabel("Результат сжатия LZW:"))
        self.lzw_result = QTextEdit()
        self.lzw_result.setReadOnly(True)
        lzw_layout.addWidget(self.lzw_result)
        
        # Вкладка для итогового результата
        final_tab = QWidget()
        final_layout = QVBoxLayout(final_tab)
        
        final_layout.addWidget(QLabel("Итоговый результат (двойное сжатие):"))
        self.final_result = QTextEdit()
        self.final_result.setReadOnly(True)
        final_layout.addWidget(self.final_result)
        
        # Добавляем вкладки
        tabs.addTab(input_tab, "Ввод")
        tabs.addTab(huffman_tab, "Хаффман")
        tabs.addTab(lzw_tab, "LZW")
        tabs.addTab(final_tab, "Итог")
    
    def build_huffman_tree(self, text):
        """Построение дерева Хаффмана"""
        if not text:
            return None
            
        # Подсчет частот
        freq = Counter(text)
        
        # Создание узлов
        nodes = [HuffmanNode(char, freq) for char, freq in freq.items()]
        nodes.sort(key=lambda x: x.freq)
        
        while len(nodes) > 1:
            # Берем два узла с наименьшей частотой
            left = nodes.pop(0)
            right = nodes.pop(0)
            
            # Создаем родительский узел
            merged = HuffmanNode(None, left.freq + right.freq)
            merged.left = left
            merged.right = right
            
            # Вставляем обратно в список и сортируем
            nodes.append(merged)
            nodes.sort(key=lambda x: x.freq)
        
        return nodes[0] if nodes else None
    
    def build_huffman_codes(self, node, current_code, codes):
        """Рекурсивное построение кодов Хаффмана"""
        if node is None:
            return
            
        # Если это лист (символ)
        if node.char is not None:
            codes[node.char] = current_code
            return
        
        # Рекурсивно обходим левое и правое поддерево
        self.build_huffman_codes(node.left, current_code + "0", codes)
        self.build_huffman_codes(node.right, current_code + "1", codes)
    
    def huffman_compress(self, text):
        """Сжатие методом Хаффмана"""
        if not text:
            return "", {}
            
        # Строим дерево и коды
        root = self.build_huffman_tree(text)
        huffman_codes = {}
        self.build_huffman_codes(root, "", huffman_codes)
        
        # Кодируем текст
        compressed_text = ''.join(huffman_codes[char] for char in text)
        
        return compressed_text, huffman_codes
    
    def lzw_compress(self, data):
        """Сжатие методом Lempel-Ziv-Welch (LZW)"""
        if not data:
            return []
            
        # Инициализация словаря с ASCII символами
        dictionary = {chr(i): i for i in range(256)}
        dict_size = 256
        
        w = ""
        result = []
        
        for c in data:
            wc = w + c
            if wc in dictionary:
                w = wc
            else:
                # Добавляем код для w
                result.append(dictionary[w])
                # Добавляем wc в словарь
                dictionary[wc] = dict_size
                dict_size += 1
                w = c
        
        # Output the code for w
        if w:
            result.append(dictionary[w])
        
        return result
    
    def compress_data(self):
        """Основная функция сжатия"""
        text = self.input_text.toPlainText().strip()
        
        if not text:
            QMessageBox.warning(self, "Ошибка", "Пожалуйста, введите текст для сжатия")
            return
        
        try:
            # Первое сжатие: Хаффман
            huffman_compressed, huffman_codes = self.huffman_compress(text)
            
            # Второе сжатие: LZW на результате Хаффмана
            lzw_compressed = self.lzw_compress(huffman_compressed)
            
            # Форматируем результаты для отображения
            huffman_output = self.format_huffman_output(text, huffman_compressed, huffman_codes)
            lzw_output = self.format_lzw_output(huffman_compressed, lzw_compressed)
            final_output = self.format_final_output(text, huffman_compressed, lzw_compressed)
            
            # Обновляем интерфейс
            self.huffman_result.setPlainText(huffman_output)
            self.lzw_result.setPlainText(lzw_output)
            self.final_result.setPlainText(final_output)
            
        except Exception as e:
            QMessageBox.critical(self, "Ошибка", f"Произошла ошибка при сжатии: {str(e)}")
    
    def format_huffman_output(self, original_text, compressed, codes):
        """Форматирование вывода для Хаффмана"""
        output = []
        output.append("ИСХОДНЫЙ ТЕКСТ:")
        output.append(original_text)
        output.append("\n" + "="*50 + "\n")
        
        output.append("КОДЫ ХАФФМАНА:")
        for char, code in sorted(codes.items()):
            char_display = char if char.isprintable() else f"\\x{ord(char):02x}"
            output.append(f"'{char_display}': {code}")
        
        output.append("\n" + "="*50 + "\n")
        output.append("СЖАТЫЙ ТЕКСТ (бинарный):")
        output.append(compressed)
        
        output.append("\n" + "="*50 + "\n")
        original_bits = len(original_text) * 8
        compressed_bits = len(compressed)
        compression_ratio = (1 - compressed_bits / original_bits) * 100 if original_bits > 0 else 0
        
        output.append(f"СТАТИСТИКА:")
        output.append(f"Исходный размер: {original_bits} бит")
        output.append(f"Сжатый размер: {compressed_bits} бит")
        output.append(f"Коэффициент сжатия: {compression_ratio:.2f}%")
        
        return "\n".join(output)
    
    def format_lzw_output(self, huffman_output, lzw_compressed):
        """Форматирование вывода для LZW"""
        output = []
        output.append("ВХОДНЫЕ ДАННЫЕ ДЛЯ LZW (результат Хаффмана):")
        output.append(huffman_output)
        
        output.append("\n" + "="*50 + "\n")
        output.append("СЖАТЫЕ ДАННЫЕ LZW (коды):")
        output.append(" ".join(map(str, lzw_compressed)))
        
        output.append("\n" + "="*50 + "\n")
        original_bits = len(huffman_output)
        compressed_bits = len(lzw_compressed) * 12  # предполагаем 12 бит на код
        
        if original_bits > 0:
            compression_ratio = (1 - compressed_bits / original_bits) * 100
            output.append(f"СТАТИСТИКА LZW:")
            output.append(f"Размер входа: {original_bits} бит")
            output.append(f"Размер выхода: {compressed_bits} бит")
            output.append(f"Коэффициент сжатия: {compression_ratio:.2f}%")
        
        return "\n".join(output)
    
    def format_final_output(self, original_text, huffman_compressed, lzw_compressed):
        """Форматирование итогового результата"""
        output = []
        output.append("ПРОЦЕСС ДВОЙНОГО СЖАТИЯ:")
        output.append("\n1. ХАФФМАН -> LZW")
        
        output.append("\n" + "="*50 + "\n")
        output.append("РЕЗУЛЬТАТЫ:")
        
        original_bits = len(original_text) * 8
        huffman_bits = len(huffman_compressed)
        lzw_bits = len(lzw_compressed) * 12  # предполагаем 12 бит на код
        
        huffman_ratio = (1 - huffman_bits / original_bits) * 100 if original_bits > 0 else 0
        lzw_ratio = (1 - lzw_bits / huffman_bits) * 100 if huffman_bits > 0 else 0
        total_ratio = (1 - lzw_bits / original_bits) * 100 if original_bits > 0 else 0
        
        output.append(f"Исходный размер: {original_bits} бит")
        output.append(f"После Хаффмана: {huffman_bits} бит (сжатие: {huffman_ratio:.2f}%)")
        output.append(f"После LZW: {lzw_bits} бит (сжатие: {lzw_ratio:.2f}%)")
        output.append(f"Общее сжатие: {total_ratio:.2f}%")
        
        output.append("\n" + "="*50 + "\n")
        output.append("ФИНАЛЬНЫЕ ДАННЫЕ (LZW коды):")
        output.append(" ".join(map(str, lzw_compressed)))
        
        return "\n".join(output)
    
    def clear_all(self):
        """Очистка всех полей"""
        self.input_text.clear()
        self.huffman_result.clear()
        self.lzw_result.clear()
        self.final_result.clear()

def main():
    app = QApplication(sys.argv)
    window = CompressionLab()
    window.show()
    sys.exit(app.exec())

if __name__ == "__main__":
    main()