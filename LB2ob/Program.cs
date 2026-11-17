using System;
using System.Collections.Generic;
using System.IO;

Console.OutputEncoding = System.Text.Encoding.UTF8;

while (true)
{
    Console.Write("Ім'я файлу з матрицею (exit - вихід): ");
    string? input = Console.ReadLine();
    if (input == null || input.Trim().ToLower() == "exit" ) break;

    string? path = FindFile(input.Trim().Trim('"'));
    if (path == null) Console.WriteLine("Помилка: файл не знайдено, спробуйте ще раз.");
    else Solve(path);
    Console.WriteLine();
}

static void Solve(string path)
{
    const double INF = double.PositiveInfinity;

    string[] lines;
    try { lines = File.ReadAllLines(path); }
    catch (Exception e)
    {
        Console.WriteLine("Помилка читання файлу: " + e.Message);
        return;
    }

    var rows = new List<string[]>();
    foreach (string line in lines)
        if (line.Trim() != "") 
            rows.Add(line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries));

    int n = rows.Count;
    if (n == 0)
    {
        Console.WriteLine("Помилка: файл порожній.");
        return;
    }

    double[,] d = new double[n, n];
    for (int i = 0; i < n; i++)
    {
        if (rows[i].Length != n)
        {
            Console.WriteLine($"Помилка: матриця не квадратна: рядків {n}, а в рядку {i + 1} елементів {rows[i].Length}.");
            return;
        }
        for (int j = 0; j < n; j++)
        {
            string t = rows[i][j];
            if (t == "∞" || t.ToLower() == "inf") d[i, j] = INF;
            else if (int.TryParse(t, out int x)) d[i, j] = x;
            else
            {
                Console.WriteLine($"Помилка: елемент [{i + 1}][{j + 1}] = '{t}' не є цілим числом або inf.");
                return;
            }
        }
        if (d[i, i] != 0)
        {
            Console.WriteLine($"Помилка: діагональний елемент [{i + 1}][{i + 1}] має бути 0.");
            return;
        }
    }

    Console.Write("Режим (1 - усі пари вершин, 2 - одна пара): ");
    string mode = (Console.ReadLine() ?? "").Trim();
    if (mode != "1" && mode != "2")
    {
        Console.WriteLine("Помилка: режим має бути 1 або 2.");
        return;
    }

    int start = 0, end = 0;
    if (mode == "2")
    {
        Console.Write($"Початкова вершина (1..{n}): ");
        bool ok1 = int.TryParse(Console.ReadLine(), out start);
        Console.Write($"Кінцева вершина (1..{n}): ");
        bool ok2 = int.TryParse(Console.ReadLine(), out end);
        if (!ok1 || !ok2 || start < 1 || start > n || end < 1 || end > n)
        {
            Console.WriteLine($"Помилка: номер вершини має бути цілим числом від 1 до {n}.");
            return;
        }
        start--;
        end--;
    }

    int[,] next = new int[n, n];
    for (int i = 0; i < n; i++)
        for (int j = 0; j < n; j++)
            next[i, j] = (i != j && d[i, j] != INF) ? j : -1;

    for (int k = 0; k < n; k++)
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                if (d[i, k] + d[k, j] < d[i, j])
                {
                    d[i, j] = d[i, k] + d[k, j];
                    next[i, j] = next[i, k];
                }

    for (int i = 0; i < n; i++)
        if (d[i, i] < 0)
        {
            Console.WriteLine("У графі є цикл від'ємної ваги. Найкоротші шляхи не існують.");
            return;
        }

    if (mode == "1")
    {
        string[,] dist = new string[n, n];
        string[,] paths = new string[n, n];
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
            {
                dist[i, j] = d[i, j] == INF ? "∞" : d[i, j].ToString();
                paths[i, j] = i == j ? (i + 1).ToString() : (next[i, j] == -1 ? "-" : (next[i, j] + 1).ToString());
            }
        PrintTable("Матриця мінімальних відстаней:", dist);
        PrintTable("Матриця найкоротших шляхів:", paths);
    }
    else if (d[start, end] == INF)
        Console.WriteLine($"Шляху з вершини {start + 1} у вершину {end + 1} не існує.");
    else
    {
        Console.WriteLine($"Мінімальна відстань {start + 1} -> {end + 1}: {d[start, end]}");
        Console.WriteLine($"Найкоротший шлях: {(start == end ? start + 1 : next[start, end] + 1)}");
    }
}


static string? FindFile(string name)
{
    foreach (string variant in new[] { name, name + ".txt", "tests/" + name, "tests/" + name + ".txt" })
    {
        if (File.Exists(variant)) return variant;
        string? dir = AppContext.BaseDirectory;
        while (dir != null && !File.Exists(Path.Combine(dir, variant)))
            dir = Path.GetDirectoryName(dir);
        if (dir != null) return Path.Combine(dir, variant);
    }
    return null;
}

static void PrintTable(string title, string[,] a)
{
    int n = a.GetLength(0);
    int w = n.ToString().Length;
    foreach (string s in a) w = Math.Max(w, s.Length);
    w += 2;
    Console.WriteLine(title);
    Console.Write("    |");
    for (int j = 1; j <= n; j++) Console.Write(j.ToString().PadLeft(w));
    Console.WriteLine();
    Console.WriteLine("----+" + new string('-', n * w));
    for (int i = 0; i < n; i++)
    {
        Console.Write((i + 1).ToString().PadLeft(3) + " |");
        for (int j = 0; j < n; j++) Console.Write(a[i, j].PadLeft(w));
        Console.WriteLine();
    }
    Console.WriteLine();
}
