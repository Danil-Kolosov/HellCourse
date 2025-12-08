// 11. Пользовательская функция
:primo: calculate :heart: :primo: sum :triangular_ruler: :primo: count :broken_heart: :muscle:
    :dart: :heart: count :snowflake: 0 :broken_heart: :muscle:
        :back: sum :droplet: count :triangular_ruler:
    :bone: :skull: :muscle:
        :back: 0 :triangular_ruler:
    :bone:
:bone:

:crown: :heart: :broken_heart: :muscle:
    // 1. Ввод с клавиатуры
    :primo: size :triangular_ruler:
    :ear: :heart: "Enter matrix size (2-5): " :triangular_ruler: :primo: size :broken_heart: :triangular_ruler:
    
    // 2. Проверка условия (if/else)
    :dart: :heart: size :moon: 2 :link: size :sun: 5 :broken_heart: :muscle:
        :mega: :heart: "Invalid size!\n" :broken_heart: :triangular_ruler:
        :back: 0 :triangular_ruler:
    :bone: :skull: :muscle:
        :mega: :heart: "Creating " :triangular_ruler: :primo: size :triangular_ruler: "x" :triangular_ruler: :primo: size :triangular_ruler: " matrix...\n" :broken_heart: :triangular_ruler:
    :bone:
    
    // 3. ДВУМЕРНЫЙ МАССИВ
    :primo: :locomotive: size :locomotive: size matrix :triangular_ruler:
    
    // 4. Заполнение матрицы
    :primo: i :triangular_ruler:
    :primo: j :triangular_ruler:
    
    :cyclone: :heart: i :in: 0 ..< size :broken_heart: :muscle:
        :cyclone: :heart: j :in: 0 ..< size :broken_heart: :muscle:
            // 5. Математические операции
            :primo: value :zap: :heart: i :boom: 1 :broken_heart: :star: :heart: j :boom: 1 :broken_heart: :triangular_ruler:
            :chak: matrix :railway_track: i :railway_track: :railway_track: j :railway_track: :zap: value :triangular_ruler:
    :bone:
    :bone: //НЕ БЫЛО
    
    // 6. Вывод матрицы
    :mega: :heart: "Matrix elements:\n" :broken_heart: :triangular_ruler:
    
    :cyclone: :heart: i :in: 0 ..< size :broken_heart: :muscle:
        :mega: :heart: "Row " :triangular_ruler: :primo: i :triangular_ruler: ": \n" :broken_heart: :triangular_ruler:
        
        :cyclone: :heart: j :in: 0 ..< size :broken_heart: :muscle:
            :mega: :heart: :primo: matrix :railway_track: i :railway_track: :railway_track: j :railway_track: :triangular_ruler: " " :broken_heart: :triangular_ruler:
        :bone:
        
        :mega: :heart: "\n" :broken_heart: :triangular_ruler:
    :bone:
    
    // 7. Вычисление суммы всех элементов
    :primo: total :zap: 0 :triangular_ruler:
    
    :cyclone: :heart: i :in: 0 ..< size :broken_heart: :muscle:
        :cyclone: :heart: j :in: 0 ..< size :broken_heart: :muscle:
            total :zap: total :boom: matrix :railway_track: i :railway_track: :railway_track: j :railway_track: :triangular_ruler:
        :bone:
    :bone:
    
    // 8. Условный оператор с логическими операторами
    :dart: :heart: total :sun: 50 :bulb: total :moon: 100 :broken_heart: :muscle:
        :mega: :heart: "Sum (" :triangular_ruler: :primo: total :triangular_ruler: ") is between 50 and 100\n" :broken_heart: :triangular_ruler:
    :bone: :skull: :dart: :heart: total :fire: 0 :broken_heart: :muscle:
        :mega: :heart: "Sum (" :triangular_ruler: :primo: total :triangular_ruler: ") is exactly 0!\n" :broken_heart: :triangular_ruler:
    :bone: :skull: :muscle:
        :mega: :heart: "Sum (" :triangular_ruler: :primo: total :triangular_ruler: ") is " :triangular_ruler: :primo: total :broken_heart: :triangular_ruler:
        :mega: :heart: "\n" :broken_heart: :triangular_ruler:
    :bone:
    
    // 9. Математические функции (sin/cos)
    :gem: angle :zap: 45 :star: 3.14159 :droplet: 180 :triangular_ruler:
    :gem: sin_val :zap: :sin: :heart: angle :broken_heart: :triangular_ruler:
    :gem: cos_val :zap: :cos: :heart: angle :broken_heart: :triangular_ruler:
    
    :mega: :heart: "sin(45) = " :triangular_ruler: :gem: sin_val :broken_heart: :triangular_ruler:
    :mega: :heart: "\n" :broken_heart: :triangular_ruler:
    :mega: :heart: "cos(45) = " :triangular_ruler: :gem: cos_val :broken_heart: :triangular_ruler:
    :mega: :heart: "\n" :broken_heart: :triangular_ruler:
    
    // 10. Функция с возвращаемым значением
    :primo: result :zap: calculate :heart: total :triangular_ruler: size :broken_heart: :triangular_ruler:
    :mega: :heart: "Average per element: " :triangular_ruler: :primo: result :broken_heart: :triangular_ruler:
    :mega: :heart: "\n" :broken_heart: :triangular_ruler:
    
:bone: