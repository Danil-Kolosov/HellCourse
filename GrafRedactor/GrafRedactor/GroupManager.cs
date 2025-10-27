using GrafRedactor;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Xml.Linq;

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
        public void MoveGroup(string groupId, PointF delta, float height, float weight)
        {
            if (groups.ContainsKey(groupId))
            {
                    // Сначала проверяем, можно ли переместить всю группу
                if (!CanMoveGroup(groupId, delta, height, weight))
                {
                    return;
                }

                // Если можно - перемещаем все элементы
                foreach (var element in groups[groupId])
                {
                    element.Move(delta, height, weight);
                }       
            }
        }
        public bool CanMoveGroup(string groupId, PointF delta, float height, float width)
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
                    testLine.Move(delta, height, width);

                    // Проверяем bounding box после перемещения
                    var testBbox = testLine.GetBoundingBox();

                    // Если хотя бы один элемент выходит за границы - перемещение невозможно
                    if (testBbox.Left < 0 || testBbox.Right > width ||
                        testBbox.Top < 0 || testBbox.Bottom > height)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public bool RotateGroup(string groupId, float angle, Rectangle drawingArea)
        {
            if (groups.ContainsKey(groupId))
            {
                var groupElements = groups[groupId];
                PointF center = GetGroupCenter(groupElements);
                if (CanPerformRotation(angle, center, drawingArea, groupId))
                {
                    foreach (var element in groupElements)
                    {
                        if (element is LineElement line)
                        {
                            RotateLineAroundPoint(line, center, angle);
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        private bool CanPerformRotation(float angle, PointF center, Rectangle drawingArea, string groupId)
        {
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

        private bool CanPerformScaling(float scaleX, float scaleY, Rectangle drawingArea, string groupId)
        {
            var groupElements = groups[groupId];
            foreach (var figure in groupElements)
            {
                if (figure is LineElement line)
                {
                    // Создаем копию линии для проверки
                    var testLine = new LineElement(line.StartPoint, line.EndPoint, line.Color, line.Thickness);

                    PointF center = new PointF(
                        (testLine.StartPoint.X + testLine.EndPoint.X) / 2,
                        (testLine.StartPoint.Y + testLine.EndPoint.Y) / 2
                    );

                    testLine.Scale(center, scaleX, scaleY);

                    if (testLine.EndPoint.X < 0 || testLine.EndPoint.X > drawingArea.Width
                        || testLine.EndPoint.Y < 0 || testLine.EndPoint.Y > drawingArea.Height
                        || testLine.StartPoint.X < 0 || testLine.StartPoint.X > drawingArea.Width
                        || testLine.StartPoint.Y < 0 || testLine.StartPoint.Y > drawingArea.Width)
                        return false;
                }
            }
            return true;
        }

        public bool ScaleGroupAverage(string groupId, float scaleFactor, Rectangle drawingArea)
        {
            if (groups.ContainsKey(groupId))
            {
                if(CanPerformScaling(scaleFactor, scaleFactor, drawingArea, groupId))
                {
                    var groupElements = groups[groupId];
                    PointF center = GetGroupCenter(groupElements);

                    foreach (var element in groupElements)
                    {
                        if (element is LineElement line)
                        {
                            ScaleLineAroundPoint(line, center, scaleFactor);
                        }
                    }
                    return true;
                }
            }
            return false;
        }


        public bool ScaleGroup(string groupId, float sx, float sy, Rectangle drawingArea)
        {
            if (groups.ContainsKey(groupId))
            {
                if(CanPerformScaling(sx, sy, drawingArea, groupId))
                {
                    var groupElements = groups[groupId];
                    PointF center = GetGroupCenter(groupElements);

                    foreach (var element in groupElements)
                    {
                        if (element is LineElement line)
                        {
                            line.Scale(center, sx, sy);
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
                PointF center = GetGroupCenter(groupElements);

                foreach (var element in groupElements)
                {
                    if (element is LineElement line)
                    {
                        MirrorLineAroundPoint(line, center, horizontal);
                    }
                }
            }
        }

        public bool MirrorGroup(string groupId, float A, float B, float C, Rectangle drawingArea) 
        {
            if (groups.ContainsKey(groupId))
            {
                if (CanPerformMirror(groupId, drawingArea, A, B, C))
                {
                    var groupElements = groups[groupId];

                    foreach (var element in groupElements)
                    {
                        if (element is LineElement line)
                        {
                            line.Mirror(A, B, C);
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        private bool CanPerformMirror(string groupId, Rectangle drawingArea, float A, float B, float C)
        {
            var groupElements = groups[groupId];
            foreach (var figure in groupElements)
            {
                if (figure is LineElement line)
                {
                    var testLine = new LineElement(line.StartPoint, line.EndPoint, line.Color, line.Thickness);
                    // Зеркалируем точки линии для проверки
                    testLine.Mirror(A, B, C);

                    // Проверяем, остаются ли точки в пределах области рисования
                    if (!drawingArea.Contains(Point.Round(testLine.StartPoint)) ||
                        !drawingArea.Contains(Point.Round(testLine.EndPoint)))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private PointF GetGroupCenter(List<FigureElement> elements)
        {
            if (elements.Count == 0)
                return PointF.Empty;

            float minX = elements.Min(e => e.GetBoundingBox().Left);
            float minY = elements.Min(e => e.GetBoundingBox().Top);
            float maxX = elements.Max(e => e.GetBoundingBox().Right);
            float maxY = elements.Max(e => e.GetBoundingBox().Bottom);

            return new PointF((minX + maxX) / 2, (minY + maxY) / 2);
        }

        public PointF GetGroupCenter(string groupId)
        {            
            return GetGroupCenter(GetGroupElements(groupId));
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