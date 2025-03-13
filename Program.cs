using System;
using System.Threading;

namespace donut.net
{
    class Program
    {

        // Размеры консоли
        const int consoleWidth = 80;
        const int consoleHeight = 22;

        static void Main(string[] args)
        {
            // Углы вращения вокруг осей X и Y
            double rotationAngleX = 0, rotationAngleY = 0;

            // Буферы для хранения глубины и пикселей
            var depthBuffer = new double[1760];  // Z-буфер (80x22)
            var pixelBuffer = new char[1760];    // Буфер вывода


            while (true)
            {
                // Очистка буферов
                FillArray(pixelBuffer, ' ');
                FillArray(depthBuffer, 0.0f);

                // Параметризация тора (θ - угол вдоль трубки, φ - угол вокруг центра)
                for (double phi = 0; phi < 6.28; phi += 0.07)  // φ ∈ [0, 2π)
                {
                    for (double theta = 0; theta < 6.28; theta += 0.02)  // θ ∈ [0, 2π)
                    {
                        // 1. Параметрические координаты тора
                        // ----------------------------------
                        // Тор задается формулами:
                        // x = (R + r*cosθ)cosφ
                        // y = (R + r*cosθ)sinφ
                        // z = r*sinθ
                        // Где R = 2 (большой радиус), r = 1 (радиус трубки)
                        double sinTheta = Math.Sin(theta);
                        double cosTheta = Math.Cos(theta);
                        double sinPhi = Math.Sin(phi);
                        double cosPhi = Math.Cos(phi);

                        // 2. Вращение вокруг оси X
                        // -------------------------
                        double sinRotationX = Math.Sin(rotationAngleX);
                        double cosRotationX = Math.Cos(rotationAngleX);

                        // 3. Вращение вокруг оси Y
                        // -------------------------
                        double sinRotationY = Math.Sin(rotationAngleY);
                        double cosRotationY = Math.Cos(rotationAngleY);

                        // 4. Вычисление 3D позиции с учетом вращения
                        // -------------------------------------------
                        // Большой радиус тора (2) + радиус трубки (cosPhi)
                        double torusRadius = cosPhi + 2;

                        // Перспективная проекция: 1/(z + 5) 
                        // (5 - смещение камеры вдоль Z)
                        double perspectiveFactor = 1 / (
                            sinTheta * torusRadius * sinRotationX +
                            sinPhi * cosRotationX +
                            5
                        );

                        // 5. Преобразование 3D -> 2D с вращением
                        // --------------------------------------
                        // Вращение вокруг Y:
                        double xRotated = cosTheta * torusRadius * cosRotationY -
                                       (sinTheta * torusRadius * cosRotationX -
                                        sinPhi * sinRotationX) * sinRotationY;

                        // Вращение вокруг X:
                        double yRotated = cosTheta * torusRadius * sinRotationY +
                                       (sinTheta * torusRadius * cosRotationX -
                                        sinPhi * sinRotationX) * cosRotationY;

                        // 6. Перевод в экранные координаты
                        // --------------------------------
                        // Центр экрана: (40, 12)
                        int screenX = (int)(40 + 30 * perspectiveFactor * xRotated);
                        int screenY = (int)(12 + 15 * perspectiveFactor * yRotated);

                        // 7. Расчет освещения
                        // -------------------
                        // Нормаль к поверхности и скалярное произведение с источником света
                        int brightness = (int)(8 * (
                            (sinPhi * sinRotationX -
                            sinTheta * cosPhi * cosRotationX) * cosRotationY -
                            sinTheta * cosPhi * sinRotationX -
                            sinPhi * cosRotationX -
                            cosTheta * cosPhi * sinRotationY
                        ));

                        // 8. Проверка границ и обновление буферов
                        // ----------------------------------------
                        int bufferIndex = screenX + consoleWidth * screenY;
                        if (consoleHeight > screenY && screenY > 0 &&
                            consoleWidth > screenX && screenX > 0 &&
                            perspectiveFactor > depthBuffer[bufferIndex])
                        {
                            depthBuffer[bufferIndex] = perspectiveFactor;
                            pixelBuffer[bufferIndex] = ".,-~:;=!*#$@"[brightness > 0 ? brightness : 0];
                        }
                    }
                }

                // Вывод кадра
                Console.SetCursorPosition(0, 0);
                InsertNewlines(pixelBuffer);
                Console.Write(pixelBuffer);

                // Обновление углов вращения
                rotationAngleX += 0.04;  // Скорость вращения вокруг X
                rotationAngleY += 0.02;  // Скорость вращения вокруг Y

                Thread.Sleep(20);  // 50 FPS
            }
        }

        /// <summary> Заполняет массив указанным значением </summary>
        static void FillArray<T>(T[] buffer, T value)
        {
            for (int i = 0; i < buffer.Length; i++)
                buffer[i] = value;
        }

        /// <summary> Вставляет переносы строк каждые 80 символов </summary>
        static void InsertNewlines(char[] buffer)
        {
            for (int i = consoleWidth; i < 1760; i += consoleWidth)
                buffer[i] = '\n';
        }
    }
}