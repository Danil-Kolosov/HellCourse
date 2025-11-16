using GrafRedactor;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Security.Cryptography;
using System.Xml.Linq;
using static System.Windows.Forms.AxHost;

namespace GrafRedactor
{
    public class GroupManager
    {
        private Dictionary<string, List<FigureElement>> groups = new Dictionary<string, List<FigureElement>>();
        private int groupCounter = 1;

        public string CreateGroup(List<FigureElement> elements)
        {
            if (elements == null || elements.Count < 2)
                return null;

            // Убираем из групп элементы, которые уже состоят в других группах
            var elementsToGroup = new List<FigureElement>();
            string groupId = $"Group_{groupCounter++}";

            foreach (var element in elements)
            {
                if (element.IsGrouped)
                {
                    groupId = element.GroupId;
                    // Если элемент уже в группе, разгруппируем его сначала******************
                    //UngroupElementFromAllGroups(element);
                }
                elementsToGroup.Add(element);
            }

            
            foreach (var element in elementsToGroup)
            {
                element.IsGrouped = true;
                element.GroupId = groupId;
            }

            groups[groupId] = elementsToGroup;
            return groupId;
        }

        public void RemoveItem(string groupId, FigureElement element) 
        {
            groups[groupId].Remove(element);
            //Dictionary<string, List<FigureElement>> newGroups = new Dictionary<string, List<FigureElement>>();
            //foreach (var group in groups)
            //{
            //    if (group.Value.Contains(element))
            //    {
            //        List<FigureElement> newGroup = new List<FigureElement>();
            //        foreach (var item in group.Value)
            //        {
            //            if (item != element)
            //                newGroup.Add(item);
            //        }
            //        newGroups.Add(group.Key, newGroup);
            //    }
            //    else
            //        newGroups.Add(group.Key, group.Value);
            //}
            //groups = newGroups;
        }

        private void UngroupElementFromAllGroups(FigureElement element)
        {
            var groupsToRemove = new List<string>();
            foreach (var group in groups)
            {
                if (group.Value.Contains(element))
                {
                    group.Value.Remove(element);
                    if (group.Value.Count == 0)
                    {
                        groupsToRemove.Add(group.Key);
                    }
                }
            }

            foreach (var groupId in groupsToRemove)
            {
                groups.Remove(groupId);
            }

            element.IsGrouped = false;
            element.GroupId = null;
        }

        public void Ungroup(string groupId)
        {
            if (groups.ContainsKey(groupId))
            {
                foreach (var element in groups[groupId])
                {
                    element.IsGrouped = false;
                    element.GroupId = null;
                }
                groups.Remove(groupId);
            }
        }

        public void UngroupSelected(List<FigureElement> selectedElements)
        {
            if (selectedElements == null) return;

            var groupsToUngroup = new HashSet<string>();
            foreach (var element in selectedElements)
            {
                if (element.IsGrouped && !string.IsNullOrEmpty(element.GroupId))
                {
                    groupsToUngroup.Add(element.GroupId);
                }
            }

            foreach (var groupId in groupsToUngroup)
            {
                Ungroup(groupId);
            }
        }

        public List<FigureElement> GetGroupElements(string groupId)
        {
            return groups.ContainsKey(groupId) ? new List<FigureElement>(groups[groupId]) : new List<FigureElement>();
        }

        public string GetSelectedGroupId(List<FigureElement> selectedElements)
        {
            if (selectedElements == null || selectedElements.Count == 0)
                return null;

            // Проверяем, все ли выделенные элементы принадлежат одной группе
            var groupIds = selectedElements
                .Where(e => e.IsGrouped)
                .Select(e => e.GroupId)
                .Distinct()
                .ToList();

            return groupIds.Count == 1 ? groupIds[0] : null;
        }

        public bool IsGroupSelected(List<FigureElement> selectedElements)
        {
            if (selectedElements == null || selectedElements.Count == 0)
                return false;

            // Группа считается выделенной, если выделены все ее элементы
            var groupId = GetSelectedGroupId(selectedElements);
            if (groupId == null) return false;

            var groupElements = GetGroupElements(groupId);
            return selectedElements.Count == groupElements.Count &&
                   groupElements.All(ge => selectedElements.Contains(ge));
        }

        // Остальные методы остаются без изменений...
        public void MoveGroup(string groupId, PointF delta, float height, float weight, float deltaZ, 
            string axeName,
            float resetAngleValueX = 0, float resetAngleValueY = 0, float resetAngleValueZ = 0,
            float totalRotationX = 0, float totalRotationY = 0, float totalRotationZ = 0
            )
        {
            if (groups.ContainsKey(groupId))
            {
                    // Сначала проверяем, можно ли переместить всю группу
                if (!CanMoveGroup(groupId, delta, height, weight, deltaZ))
                {
                    return;
                }

                // Если можно - перемещаем все элементы
                foreach (var element in groups[groupId])
                {
                    if (element is Cube3D cube) 
                    {
                        cube.Move3D(delta, height, weight, deltaZ, axeName);
                    }
                    else
                        element.Move(delta, height, weight, deltaZ, axeName);
                }       
            }
        }
        public bool CanMoveGroup(string groupId, PointF delta, float height, float width, float deltaZ)
        {
            if (!groups.ContainsKey(groupId)) return false;

            var groupElements = groups[groupId];

            foreach (var element in groupElements)
            {
                // Создаем временную копию для проверки
                if (element is LineElement line)
                {
                    var testLine = new LineElement(line.StartPoint, line.EndPoint, line.Color, line.Thickness);

                    // Пробуем применить перемещение к копии
                    testLine.Move(delta, height, width, deltaZ, "group");

                    // Проверяем bounding box после перемещения
                    var testBbox = testLine.GetBoundingBox();

                    // Если хотя бы один элемент выходит за границы - перемещение невозможно
                    if (testBbox.Left < -width/2 || testBbox.Right > width/2 ||
                        testBbox.Top < -height/2 || testBbox.Bottom > height/2)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public bool RotateGroup(string groupId, float angleX, float angleY, float angleZ, Point3D rotationCenter, 
            Rectangle drawingArea, float zc)
        {
            if (groups.ContainsKey(groupId))
            {
                var groupElements = groups[groupId];

                if (CanPerformRotation3D(angleX, angleY, angleZ, rotationCenter, drawingArea, groupId, zc))
                {
                    // Вращаем всю группу как жесткое тело
                    foreach (var element in groupElements)
                    {
                        if (element is LineElement3D line3D)
                        {
                            // Вращаем обе точки линии
                            Point3D newStart = RotatePoint3D(line3D.ZeroRatatedStartPoint, rotationCenter, angleX, angleY, angleZ);
                            Point3D newEnd = RotatePoint3D(line3D.ZeroRatatedEndPoint, rotationCenter, angleX, angleY, angleZ);

                            // Обновляем реальные координаты
                            line3D.ZeroRatatedStartPoint = newStart;
                            line3D.ZeroRatatedEndPoint = newEnd;
                            line3D.StartPoint3D = newStart;
                            line3D.EndPoint3D = newEnd;
                        }
                        else 
                        {
                            if (element is Cube3D cube)
                            {
                                // Для куба используем его метод вращения
                                cube.Rotate3D(angleX, angleY, angleZ, rotationCenter, drawingArea, zc);
                            }
                            else
                            {
                                if (element is LineElement line)
                                {
                                    line.Rotate(angleZ, rotationCenter.ToPoint2D());
                                }
                            }
                        }
                    }
                    return true;
                }
            }
            return false;
            //старое - группа разваливается
            //if (groups.ContainsKey(groupId))
            //{
            //    var groupElements = groups[groupId];
            //    PointF center = GetGroupCenter(groupElements).ToPoint2D();
            //    if (CanPerformRotation(angle, center, drawingArea, groupId))
            //    {
            //        foreach (var element in groupElements)
            //        {
            //            if (element is LineElement line)
            //            {
            //                RotateLineAroundPoint(line, center, angle);
            //            }
            //        }
            //        return true;
            //    }
            //}
            //return false;
        }

        private Point3D RotatePoint3D(Point3D point, Point3D center, float angleX, float angleY, float angleZ)
        {
            // Перенос в систему координат с центром в center
            float x = point.X - center.X;
            float y = point.Y - center.Y;
            float z = point.Z - center.Z;

            // Преобразуем углы в радианы
            float radX = angleX * (float)Math.PI / 180f;
            float radY = angleY * (float)Math.PI / 180f;
            float radZ = angleZ * (float)Math.PI / 180f;

            // Вычисляем синусы и косинусы
            float cosX = (float)Math.Cos(radX), sinX = (float)Math.Sin(radX);
            float cosY = (float)Math.Cos(radY), sinY = (float)Math.Sin(radY);
            float cosZ = (float)Math.Cos(radZ), sinZ = (float)Math.Sin(radZ);

            // Матрица вращения (комбинированная: Z * Y * X)
            float x1 = x * cosY * cosZ + y * (sinX * sinY * cosZ - cosX * sinZ) + z * (cosX * sinY * cosZ + sinX * sinZ);
            float y1 = x * cosY * sinZ + y * (sinX * sinY * sinZ + cosX * cosZ) + z * (cosX * sinY * sinZ - sinX * cosZ);
            float z1 = x * -sinY + y * sinX * cosY + z * cosX * cosY;

            if ((Math.Abs(angleZ) > 0.0001f))
            {
                double angleRad = angleZ * Math.PI / 180.0;
                double cos = Math.Cos(angleRad);
                double sin = Math.Sin(angleRad);

                double newX = x * cos - y * sin;
                double newY = x * sin + y * cos;

                x = (float)newX;
                y = (float)newY;
            }

            if ((Math.Abs(angleX) > 0.0001f))
            {
                double angleRad = angleX * Math.PI / 180.0;
                double cos = Math.Cos(angleRad);
                double sin = Math.Sin(angleRad);

                double newY = y * cos - z * sin;
                double newZ = y * sin + z * cos;

                y = (float)newY;
                z = (float)newZ;
            }

            if ((Math.Abs(angleY) > 0.0001f))
            {
                double angleRad = angleY * Math.PI / 180.0;
                double cos = Math.Cos(angleRad);
                double sin = Math.Sin(angleRad);

                double newX = x * cos + z * sin;
                double newZ = -x * sin + z * cos;

                x = (float)newX;
                z = (float)newZ;
            }

            // Возврат в исходную систему координат - разницы нет, что первым способом вертеть (x1,y1,z1) что так)
            return new Point3D(
                x + center.X,
                y + center.Y,
                z + center.Z
            );
            return new Point3D(
                x1 + center.X,
                y1 + center.Y,
                z1 + center.Z
            );
        }

        private bool CanPerformRotation3D(float angleX, float angleY, float angleZ, Point3D rotationCenter, 
            Rectangle drawingArea, string groupId, float zc)
        {
            return true;
            var groupElements = groups[groupId];

            foreach (var figure in groupElements)
            {
                if (figure is LineElement3D line3D)
                {
                    // Создаем копию линии для проверки
                    var testLine = new LineElement3D(
                        line3D.ZeroRatatedStartPoint,
                        line3D.ZeroRatatedEndPoint,
                        line3D.Color,
                        line3D.Thickness
                    );

                    // Вращаем копию вокруг указанного центра
                    Point3D newStart = RotatePoint3D(testLine.ZeroRatatedStartPoint, rotationCenter, angleX, angleY, angleZ);
                    Point3D newEnd = RotatePoint3D(testLine.ZeroRatatedEndPoint, rotationCenter, angleX, angleY, angleZ);

                    // Обновляем тестовую линию
                    testLine.ZeroRatatedStartPoint = newStart;
                    testLine.ZeroRatatedEndPoint = newEnd;
                    testLine.StartPoint3D = newStart;
                    testLine.EndPoint3D = newEnd;                    

                    // Проверяем 2D проекцию после вращения
                    var bbox = testLine.GetBoundingBox();
                    if (bbox.Left < 0 || bbox.Right > drawingArea.Width ||
                        bbox.Top < 0 || bbox.Bottom > drawingArea.Height)
                    {
                        return false;
                    }
                }
                else if (figure is Cube3D cube)
                {
                    // Для куба создаем тестовую копию
                    throw new NotImplementedException();
                    var testCube = new Cube3D(cube.Center, cube.Size, cube.CubeColor,"", 0);

                    // Вращаем тестовый куб
                    testCube.Rotate3D(angleX, angleY, angleZ, rotationCenter, drawingArea, zc);

                    // Проверяем bounding box
                    var bbox = testCube.GetBoundingBox();
                    if (bbox.Left < 0 || bbox.Right > drawingArea.Width ||
                        bbox.Top < 0 || bbox.Bottom > drawingArea.Height)
                    {
                        return false;
                    }
                }
                else if (figure is LineElement line2D)
                {
                    // Для 2D линий используем старую проверку
                    var testLine = new LineElement(line2D.StartPoint, line2D.EndPoint, line2D.Color, line2D.Thickness);

                    // Вращаем вокруг центра вращения (преобразованного в 2D)
                    testLine.Rotate(angleZ, rotationCenter.ToPoint2D());

                    if (testLine.EndPoint.X < 0 || testLine.EndPoint.X > drawingArea.Width ||
                        testLine.EndPoint.Y < 0 || testLine.EndPoint.Y > drawingArea.Height ||
                        testLine.StartPoint.X < 0 || testLine.StartPoint.X > drawingArea.Width ||
                        testLine.StartPoint.Y < 0 || testLine.StartPoint.Y > drawingArea.Height)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        //старое линия разваливается
        private bool CanPerformRotation(float angle, PointF center, Rectangle drawingArea, string groupId)
        {
            return true;
            throw new NotImplementedException();
            var groupElements = groups[groupId];
            foreach (var figure in groupElements)
            {
                if (figure is LineElement line)
                {
                    // Создаем копию линии для проверки
                    var testLine = new LineElement(line.StartPoint, line.EndPoint, line.Color, line.Thickness);
                    testLine.Rotate(angle);

                    if (testLine.EndPoint.X < 0 || testLine.EndPoint.X > drawingArea.Width
                        || testLine.EndPoint.Y < 0 || testLine.EndPoint.Y > drawingArea.Height
                        || testLine.StartPoint.X < 0 || testLine.StartPoint.X > drawingArea.Width
                        || testLine.StartPoint.Y < 0 || testLine.StartPoint.Y > drawingArea.Width)
                        return false;
                }
            }
            return true;
        }

        private bool CanPerformScaling(float scaleX, float scaleY, Rectangle drawingArea, string groupId, float sz = 1)
        {
            return true;
            throw new NotImplementedException();
            var groupElements = groups[groupId];
            Point3D center = GetGroupCenter3D(groupId);
            foreach (var figure in groupElements)
            {
                if (figure is LineElement3D line3d)
                {
                    // Создаем копию линии для проверки
                    var testLine3d = new LineElement3D(line3d.ZeroRatatedStartPoint, line3d.ZeroRatatedEndPoint, line3d.Color, line3d.Thickness);

                    //Point3D center = new Point3D(
                    //    (testLine3d.ZeroRatatedStartPoint.X + testLine3d.ZeroRatatedEndPoint.X) / 2,
                    //    (testLine3d.ZeroRatatedStartPoint.Y + testLine3d.ZeroRatatedEndPoint.Y) / 2,
                    //    (testLine3d.ZeroRatatedStartPoint.Z + testLine3d.ZeroRatatedEndPoint.Z) / 2
                    //);

                    testLine3d.Scale(center, scaleX, scaleY, sz);

                    if (testLine3d.EndPoint.X < -drawingArea.Width / 2 || testLine3d.EndPoint.X > drawingArea.Width/2
                        || testLine3d.EndPoint.Y < -drawingArea.Height / 2 || testLine3d.EndPoint.Y > drawingArea.Height/2
                        || testLine3d.StartPoint.X < -drawingArea.Width / 2 || testLine3d.StartPoint.X > drawingArea.Width/2
                        || testLine3d.StartPoint.Y < -drawingArea.Height / 2 || testLine3d.StartPoint.Y > drawingArea.Height/2)
                        return false;
                }
                else 
                {
                    if(figure is Cube3D cube) 
                    {
                        var testCube = new Cube3D(cube.Center, cube.Size, cube.Color, "", 0);
                        // Зеркалируем точки линии для проверки
                        testCube.Scale(center, scaleX, scaleY, sz);

                        // Проверяем, остаются ли точки в пределах области рисования
                        if (cube.Center.X < -drawingArea.Width / 2 || cube.Center.X > drawingArea.Width / 2
                        || cube.Center.Y < -drawingArea.Height / 2 || cube.Center.Y > drawingArea.Height / 2)
                            return false;
                    }
                    else 
                    {
                        if (figure is LineElement line)
                        {
                            // Создаем копию линии для проверки
                            var testLine = new LineElement(line.StartPoint, line.EndPoint, line.Color, line.Thickness);

                            //PointF center = new PointF(
                            //    (testLine.StartPoint.X + testLine.EndPoint.X) / 2,
                            //    (testLine.StartPoint.Y + testLine.EndPoint.Y) / 2
                            //);

                            testLine.Scale(center.ToPoint2D(), scaleX, scaleY);

                            if (testLine.EndPoint.X < -drawingArea.Width / 2 || testLine.EndPoint.X > drawingArea.Width / 2
                                || testLine.EndPoint.Y < -drawingArea.Height / 2 || testLine.EndPoint.Y > drawingArea.Height / 2
                                || testLine.StartPoint.X < -drawingArea.Width / 2 || testLine.StartPoint.X > drawingArea.Width / 2
                                || testLine.StartPoint.Y < -drawingArea.Height / 2 || testLine.StartPoint.Y > drawingArea.Height / 2)
                                return false;
                        }
                    }
                }
            }
            return true;
        }

        public bool ScaleGroupAverage(string groupId, float scaleFactor, Rectangle drawingArea, float zc, string currentAxiName, Point3D sceneCenter = null, 
            float resetAngleValueX = 0, float resetAngleValueY = 0, float resetAngleValueZ = 0, 
            float totalRotationX = 0, float totalRotationY = 0, float totalRotationZ = 0)
        {
            if (groups.ContainsKey(groupId))
            {
                return ScaleGroup(groupId, scaleFactor, scaleFactor, drawingArea, zc, currentAxiName, scaleFactor, 
                    sceneCenter, resetAngleValueX, resetAngleValueY, resetAngleValueZ, 
                    totalRotationX, totalRotationY, totalRotationZ);
                //if(CanPerformScaling(scaleFactor, scaleFactor, drawingArea, groupId))
                //{
                //    var groupElements = groups[groupId];
                //    PointF center = GetGroupCenter(groupElements).ToPoint2D();

                //    foreach (var element in groupElements)
                //    {
                //        if (element is LineElement line)
                //        {
                //            ScaleLineAroundPoint(line, center, scaleFactor);
                //        }
                //        if (element is LineElement3D line3d)
                //        {
                //            line3d.ScaleAverage(scaleFactor);
                //            line3d.Rotate3DWithScene(sceneCenter, resetAngleValueX, resetAngleValueY, resetAngleValueZ);
                //        }
                //        if (element is Cube3D cube)
                //        {
                //            cube.ScaleAverage(scaleFactor);
                //            cube.Rotate3DWithScene(resetAngleValueX, resetAngleValueY, resetAngleValueZ, sceneCenter);
                //        }
                //    }
                //    return true;
                //}
            }
            return false;
        }


        public bool ScaleGroup(string groupId, float sx, float sy, Rectangle drawingArea, float zc, string currentAxiName, float sz = 1, Point3D sceneCenter = null, 
            float resetAngleValueX = 0, float resetAngleValueY = 0, float resetAngleValueZ = 0, 
            float totalRotationX = 0, float totalRotationY = 0, float totalRotationZ = 0)
        {
            if (groups.ContainsKey(groupId))
            {
                if(CanPerformScaling(sx, sy, drawingArea, groupId, sz))
                {
                    var groupElements = groups[groupId];
                    Point3D center = GetGroupCenter(groupElements);

                    foreach (var element in groupElements)
                    {
                        if (element is LineElement3D line3d)
                        {
                            line3d.Scale(center, sx, sy, sz);
                            line3d.Rotate3DWithScene(sceneCenter, resetAngleValueX, resetAngleValueY, resetAngleValueZ, zc, currentAxiName);
                            if (totalRotationX != 0 || totalRotationY != 0 || totalRotationZ != 0)
                                line3d.Rotate3DWithScene(sceneCenter, totalRotationX, totalRotationY, totalRotationZ, zc, currentAxiName);
                        }
                        else
                        {
                            if (element is LineElement line)
                            {
                                line.Scale(center.ToPoint2D(), sx, sy);
                            }
                            else 
                            {
                                if (element is Cube3D cube)
                                {
                                    cube.Scale(center, sx, sy, sz);
                                    cube.Rotate3DWithScene( resetAngleValueX, resetAngleValueY, resetAngleValueZ, sceneCenter, zc, currentAxiName);
                                    if (totalRotationX != 0 || totalRotationY != 0 || totalRotationZ != 0)
                                        cube.Rotate3DWithScene(totalRotationX, totalRotationY, totalRotationZ, sceneCenter, zc, currentAxiName);
                                }
                            }
                        }
                    }
                    return true;
                }
            }
            return false;
        }
        public void MirrorGroup(string groupId, bool horizontal)
        {
            if (groups.ContainsKey(groupId))
            {
                var groupElements = groups[groupId];
                PointF center = GetGroupCenter(groupElements).ToPoint2D();

                foreach (var element in groupElements)
                {
                    if (element is LineElement line)
                    {
                        MirrorLineAroundPoint(line, center, horizontal);
                    }
                }
            }
        }

        public bool MirrorGroup(string groupId, LineElement mirrorLine, Rectangle drawingArea, float zc, string currentAxiName, Point3D sceneCenter = null,
            float resetAngleValueX = 0, float resetAngleValueY = 0, float resetAngleValueZ = 0,
            float totalRotationX = 0, float totalRotationY = 0, float totalRotationZ = 0) 
        {
            if (groups.ContainsKey(groupId))
            {
                (float A, float B, float C, float tempZ) = mirrorLine.GetEquation();
                if (CanPerformMirror(groupId, drawingArea, A, B, C))
                {
                    var groupElements = groups[groupId];

                    foreach (var element in groupElements)
                    {
                        
                        if (element is LineElement3D line3d)
                        {                            
                            line3d.Mirror3DRelativeToLine(mirrorLine);
                            line3d.Rotate3DWithScene(sceneCenter, resetAngleValueX, resetAngleValueY, resetAngleValueZ, zc, currentAxiName);
                            if (totalRotationX != 0 || totalRotationY != 0 || totalRotationZ != 0)
                                line3d.Rotate3DWithScene(sceneCenter, totalRotationX, totalRotationY, totalRotationZ, zc, currentAxiName);
                        }
                        else 
                        {
                            if (element is LineElement line)
                            {
                                line.Mirror(A, B, C);
                            }
                            else 
                            {
                                if (element is Cube3D cube)
                                {
                                    cube.Mirror3DRelativeToLine(mirrorLine);
                                    cube.Rotate3DWithScene(resetAngleValueX, resetAngleValueY, resetAngleValueZ, sceneCenter, zc, currentAxiName);
                                    if (totalRotationX != 0 || totalRotationY != 0 || totalRotationZ != 0)
                                        cube.Rotate3DWithScene(totalRotationX, totalRotationY, totalRotationZ, sceneCenter, zc, currentAxiName);
                                }
                            }
                        }
                        

                    }


                    return true;
                }
            }
            return false;
        } 

        private bool CanPerformMirror(string groupId, Rectangle drawingArea, float A, float B, float C)
        {
            return true;
            throw new NotImplementedException();
            var groupElements = groups[groupId];
            foreach (var figure in groupElements)
            {
                if (figure is LineElement line)
                {
                    var testLine = new LineElement(line.StartPoint, line.EndPoint, line.Color, line.Thickness);
                    // Зеркалируем точки линии для проверки
                    testLine.Mirror(A, B, C);

                    // Проверяем, остаются ли точки в пределах области рисования
                    if (testLine.EndPoint.X < -drawingArea.Width / 2 || testLine.EndPoint.X > drawingArea.Width / 2
                       || testLine.EndPoint.Y < -drawingArea.Height / 2 || testLine.EndPoint.Y > drawingArea.Height / 2
                       || testLine.StartPoint.X < -drawingArea.Width / 2 || testLine.StartPoint.X > drawingArea.Width / 2
                       || testLine.StartPoint.Y < -drawingArea.Height / 2 || testLine.StartPoint.Y > drawingArea.Height / 2)
                        return false;
                }
                else 
                {
                    if (figure is Cube3D cube)
                    {
                        var testCube = new Cube3D(cube.Center, cube.Size, cube.Color, "", 0);
                        // Зеркалируем точки линии для проверки
                        testCube.Mirror(A, B, C);

                        // Проверяем, остаются ли точки в пределах области рисования
                        if (testCube.Center.X < -drawingArea.Width / 2 || testCube.Center.X > drawingArea.Width / 2
                        || testCube.Center.Y < -drawingArea.Height / 2 || testCube.Center.Y > drawingArea.Height / 2)
                            return false;
                    }
                }
            }
            return true;
        }

        private Point3D GetGroupCenter(List<FigureElement> elements)
        {
            if (elements.Count == 0)
                return null;

            float minX = elements.Min(e => e.GetBoundingBox().Left);
            float minY = elements.Min(e => e.GetBoundingBox().Top);
            float maxX = elements.Max(e => e.GetBoundingBox().Right);
            float maxY = elements.Max(e => e.GetBoundingBox().Bottom);
            float maxZ = elements.Max(e => 
            { 
                if (e is LineElement3D line3d)
                {
                    return (int)Math.Max(line3d.StartPoint3D.Z, line3d.EndPoint3D.Z);
                }
                if (e is Cube3D cube)
                {
                    return (int)cube.Center.Z;
                }
                return 0;
            } );

            float minZ = elements.Min(e => {
                if (e is LineElement3D line3d)
                {
                    return (int)Math.Min(line3d.StartPoint3D.Z, line3d.EndPoint3D.Z);
                }
                if (e is Cube3D cube)
                {
                    return (int)cube.Center.Z;
                }
                return 0;
            });

            return new Point3D((minX + maxX) / 2, (minY + maxY) / 2, (minZ + maxZ) / 2);
        }

        public PointF GetGroupCenter(string groupId)
        {            
            return GetGroupCenter(GetGroupElements(groupId)).ToPoint2D();
        }

        public Point3D GetGroupCenter3D(List<FigureElement> elements)
        {
            if (elements.Count == 0)
                return new Point3D(0, 0, 0);

            float minX = float.MaxValue, minY = float.MaxValue, minZ = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue, maxZ = float.MinValue;

            foreach (var element in elements)
            {
                // Получаем bounding box для X,Y
                var bbox = element.GetBoundingBox();
                minX = Math.Min(minX, bbox.Left);
                minY = Math.Min(minY, bbox.Top);
                maxX = Math.Max(maxX, bbox.Right);
                maxY = Math.Max(maxY, bbox.Bottom);

                // Получаем Z-координаты в зависимости от типа элемента
                if (element is LineElement3D line3D)
                {
                    float lineMinZ = Math.Min(line3D.StartPoint3D.Z, line3D.EndPoint3D.Z);
                    float lineMaxZ = Math.Max(line3D.StartPoint3D.Z, line3D.EndPoint3D.Z);

                    minZ = Math.Min(minZ, lineMinZ);
                    maxZ = Math.Max(maxZ, lineMaxZ);
                }
                else if (element is Cube3D cube)
                {
                    // Для куба учитываем его размер по Z
                    float cubeMinZ = cube.Center.Z - cube.Size / 2;
                    float cubeMaxZ = cube.Center.Z + cube.Size / 2;

                    minZ = Math.Min(minZ, cubeMinZ);
                    maxZ = Math.Max(maxZ, cubeMaxZ);
                }
            }

            return new Point3D((minX + maxX) / 2, (minY + maxY) / 2, (minZ + maxZ) / 2);
        }

        public Point3D GetGroupCenter3D(string groupId)
        {
            return GetGroupCenter3D(GetGroupElements(groupId));
        }

        private void RotateLineAroundPoint(LineElement line, PointF center, float angle)
        {
            float angleRad = angle * (float)Math.PI / 180f;
            float cos = (float)Math.Cos(angleRad);
            float sin = (float)Math.Sin(angleRad);

            line.StartPoint = RotatePoint(line.StartPoint, center, cos, sin);
            line.EndPoint = RotatePoint(line.EndPoint, center, cos, sin);
        }

        private void ScaleLineAroundPoint(LineElement line, PointF center, float scaleFactor)
        {
            line.StartPoint = ScalePoint(line.StartPoint, center, scaleFactor);
            line.EndPoint = ScalePoint(line.EndPoint, center, scaleFactor);
        }

        private void MirrorLineAroundPoint(LineElement line, PointF center, bool horizontal)
        {
            if (horizontal)
            {
                line.StartPoint = new PointF(2 * center.X - line.StartPoint.X, line.StartPoint.Y);
                line.EndPoint = new PointF(2 * center.X - line.EndPoint.X, line.EndPoint.Y);
            }
            else
            {
                line.StartPoint = new PointF(line.StartPoint.X, 2 * center.Y - line.StartPoint.Y);
                line.EndPoint = new PointF(line.EndPoint.X, 2 * center.Y - line.EndPoint.Y);
            }
        }

        private PointF RotatePoint(PointF point, PointF center, float cos, float sin)
        {
            float translatedX = point.X - center.X;
            float translatedY = point.Y - center.Y;

            float rotatedX = translatedX * cos - translatedY * sin;
            float rotatedY = translatedX * sin + translatedY * cos;

            return new PointF(rotatedX + center.X, rotatedY + center.Y);
        }

        private PointF ScalePoint(PointF point, PointF center, float scaleFactor)
        {
            float dx = point.X - center.X;
            float dy = point.Y - center.Y;

            return new PointF(center.X + dx * scaleFactor, center.Y + dy * scaleFactor);
        }

        public void DrawGroupSelection(Graphics graphics, string groupId)
        {
            if (groups.ContainsKey(groupId))
            {
                var groupElements = groups[groupId];
                var groupBoundingBox = GetGroupBoundingBox(groupElements);

                // Рисуем bounding box вокруг всей группы
                using (Pen groupPen = new Pen(Color.Green, 2))
                {
                    groupPen.DashStyle = DashStyle.DashDot;
                    graphics.DrawRectangle(groupPen,
                        groupBoundingBox.X, groupBoundingBox.Y,
                        groupBoundingBox.Width, groupBoundingBox.Height);
                }

                // Рисуем маркеры для группы
                float handleSize = 8f;
                using (Brush groupHandleBrush = new SolidBrush(Color.Green))
                {
                    // Угловые маркеры
                    graphics.FillRectangle(groupHandleBrush,
                        groupBoundingBox.Left - handleSize / 2, groupBoundingBox.Top - handleSize / 2,
                        handleSize, handleSize);
                    graphics.FillRectangle(groupHandleBrush,
                        groupBoundingBox.Right - handleSize / 2, groupBoundingBox.Top - handleSize / 2,
                        handleSize, handleSize);
                    graphics.FillRectangle(groupHandleBrush,
                        groupBoundingBox.Left - handleSize / 2, groupBoundingBox.Bottom - handleSize / 2,
                        handleSize, handleSize);
                    graphics.FillRectangle(groupHandleBrush,
                        groupBoundingBox.Right - handleSize / 2, groupBoundingBox.Bottom - handleSize / 2,
                        handleSize, handleSize);
                }
            }
        }

        private RectangleF GetGroupBoundingBox(List<FigureElement> elements)
        {
            if (elements.Count == 0)
                return RectangleF.Empty;

            float minX = elements.Min(e => e.GetBoundingBox().Left);
            float minY = elements.Min(e => e.GetBoundingBox().Top);
            float maxX = elements.Max(e => e.GetBoundingBox().Right);
            float maxY = elements.Max(e => e.GetBoundingBox().Bottom);

            return new RectangleF(minX, minY, maxX - minX, maxY - minY);
        }

        public List<string> GetAllGroupIds()
        {
            return groups.Keys.ToList();
        }

        public bool IsElementInAnyGroup(FigureElement element)
        {
            return element.IsGrouped && !string.IsNullOrEmpty(element.GroupId);
        }
    }
}