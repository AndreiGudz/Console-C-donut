using System;
using System.Threading;

namespace donut.net
{
    class Program
    {
        // Добавлены новые параметры управления
        static double cameraDistance = 5.0;    // Дистанция камеры
        static double zoomSpeed = 0.1;         // Скорость приближения/отдаления
        const int consoleWidth = 80;
        const int consoleHeight = 22;

        static void Main(string[] args)
        {
            double rotationAngleX = 0, rotationAngleY = 0, rotationAngleZ = 0;
            var depthBuffer = new double[consoleWidth * consoleHeight];
            var pixelBuffer = new char[consoleWidth * consoleHeight];

            while (true)
            {
                FillArray(pixelBuffer, ' ');
                FillArray(depthBuffer, 0.0f);

                // Управление камерой (для примера - автоматическое движение)
                cameraDistance += zoomSpeed * 0.1;
                if (cameraDistance > 6.0 || cameraDistance < 4.0) 
                    zoomSpeed *= -1;

                for (double phi = 0; phi < 6.28; phi += 0.07)
                {
                    for (double theta = 0; theta < 6.28; theta += 0.02)
                    {
                        double sinTheta = Math.Sin(theta);
                        double cosTheta = Math.Cos(theta);
                        double sinPhi = Math.Sin(phi);
                        double cosPhi = Math.Cos(phi);

                        // 1. Базовые координаты тора
                        double xTorus = (2 + cosPhi) * cosTheta;
                        double yTorus = (2 + cosPhi) * sinTheta;
                        double zTorus = sinPhi;

                        // 2. Вращение вокруг всех трех осей
                        double sinX = Math.Sin(rotationAngleX);
                        double cosX = Math.Cos(rotationAngleX);
                        double sinY = Math.Sin(rotationAngleY);
                        double cosY = Math.Cos(rotationAngleY);
                        double sinZ = Math.Sin(rotationAngleZ);
                        double cosZ = Math.Cos(rotationAngleZ);

                        // 3. Комбинированное вращение (порядок: Z -> Y -> X)
                        // Поворот вокруг Z
                        double xz = xTorus * cosZ - yTorus * sinZ;
                        double yz = xTorus * sinZ + yTorus * cosZ;
                        double zz = zTorus;

                        // Поворот вокруг Y
                        double xy = xz * cosY - zz * sinY;
                        double yy = yz;
                        double zy = xz * sinY + zz * cosY;

                        // Поворот вокруг X
                        double xx = xy;
                        double yx = yy * cosX - zy * sinX;
                        double zx = yy * sinX + zy * cosX;

                        // 4. Перспективная проекция с переменной дистанцией
                        double depth = 1 / (zx + cameraDistance);

                        // 5. Экранные координаты
                        int screenX = (int)(40 + 30 * depth * xx);
                        int screenY = (int)(11 + 15 * depth * yx);

                        // 6. Освещение с учетом всех вращений
                        double normalX = cosTheta * cosPhi * cosY - sinPhi * sinY;
                        double normalY = sinTheta * cosPhi * cosX - sinPhi * sinX;
                        double normalZ = sinTheta * cosPhi * sinZ + sinPhi * cosZ;
                        int brightness = (int)(8 * (normalX + normalY + normalZ));

                        int bufferIndex = screenX + consoleWidth * screenY;
                        if (consoleHeight > screenY && screenY > 0 &&
                            consoleWidth > screenX && screenX > 0 &&
                            depth > depthBuffer[bufferIndex])
                        {
                            depthBuffer[bufferIndex] = depth;
                            pixelBuffer[bufferIndex] = ".,-~:;=!*#$@"[Math.Clamp(brightness, 0, 11)];
                        }
                    }
                }

                Console.SetCursorPosition(0, 0);
                InsertNewlines(pixelBuffer);
                Console.Write(pixelBuffer);

                // Обновление углов вращения (теперь и по Z)
                rotationAngleX += 0.04;
                rotationAngleY += 0.02;
                rotationAngleZ += 0.02;  // Новая ось вращения

                Thread.Sleep(20);
            }
        }

        static void FillArray<T>(T[] buffer, T value)
        {
            for (int i = 0; i < buffer.Length; i++)
                buffer[i] = value;
        }

        static void InsertNewlines(char[] buffer)
        {
            for (int i = consoleWidth; i < consoleWidth * consoleHeight; i += consoleWidth)
                buffer[i] = '\n';
        }
    }
}