from PySide6.QtWidgets import QApplication, QMainWindow, QWidget
from PySide6.QtGui import QPainter, QPen, QColor, QPainterPath
import sys
def GetMatrixResult(a0, a1, a2, a3, b0,b1,b2,b3):
    matrix =[]
    t=0
    while (t<=1): 
        x = a0+a1*t+a2*t*t+a3*t*t*t
        y = b0+b1*t+b2*t*t+b3*t*t*t
        tempMatrix =[x, y]
        matrix.append(tempMatrix)
        t+=0.1
    return matrix
    
def GetPolinomKoef(x1,x2,x3,x4):
    a3 = (-x1+3*x2-3*x3+x4)/6
    a2 = (x1-2*x2+x3)/2
    a1 = (x3-x1)/2
    a0 = (x1+4*x2+x3)/6
    return (a0,a1,a2,a3)    
    
matrixBoss = [[1,6],[3,5],[6,7],[8,5],[11,5],[15,3],[9,1],[6,2],[5,4],[2,5],[1,6]]
#matrixBoss = [[4,1],[6,1],[7,3],[7,4],[6,5],[4,6],[2,5],[2,4],[2.5,3],[4,2],[4,1]]

matrixLineBoss = []
percentDevider=8
numPoints = 10
scale = 50
def scale_matrix(matrix, scale_factor):
    return [[element * scale_factor for element in row] for row in matrix]

matrixBoss = scale_matrix(matrixBoss, scale)

def FullMatrixLineBoss():
    matrixLineBoss.clear()  # Очистка перед заполнением
    for i in range(0, numPoints):         
        koef_x = GetPolinomKoef(matrixBoss[i%percentDevider][0], matrixBoss[(i+1)%percentDevider][0], matrixBoss[(i+2)%percentDevider][0], matrixBoss[(i+3)%percentDevider][0])
        koef_y = GetPolinomKoef(matrixBoss[i%percentDevider][1], matrixBoss[(i+1)%percentDevider][1], matrixBoss[(i+2)%percentDevider][1], matrixBoss[(i+3)%percentDevider][1])       
        matrixLineBoss.append(GetMatrixResult(koef_x[0], koef_x[1], koef_x[2], koef_x[3],
                        koef_y[0], koef_y[1], koef_y[2], koef_y[3]))


class DrawingWidget(QWidget):
    def paintEvent(self, event):
        painter = QPainter(self)
        painter.setRenderHint(QPainter.Antialiasing)  # Сглаживание
        
        # Данные точек заполняем
        FullMatrixLineBoss()

        # Рисуем линии
        painter.setPen(QPen(QColor(0, 0, 255), 2))
        for line in matrixLineBoss:
            path = QPainterPath()

            start_x, start_y = line[0][0], line[0][1]
            path.moveTo(start_x, start_y)
            for point in line[1:]:

                x, y = point[0], point[1]
                path.lineTo(x, y)
            painter.drawPath(path)
        
        # Рисуем красные линии
        painter.setPen(QPen(QColor(255, 0, 0), 3))
        if len(matrixBoss) >= 2:
            path = QPainterPath()
            start_x, start_y = matrixBoss[0][0], matrixBoss[0][1]
            path.moveTo(start_x, start_y)
            for point in matrixBoss[1:]:
                x, y = point[0], point[1]
                path.lineTo(x, y)
            painter.drawPath(path)        


# Запуск приложения
app = QApplication(sys.argv)
window = QMainWindow()
window.setCentralWidget(DrawingWidget())
window.resize(800, 400)
window.show()
sys.exit(app.exec())