using ImGuiNET;
using System.Numerics;
using System.Text.Json;
using Color = Raylib_cs.Color;
internal class Program
{
    [STAThread()]
    private static void Main(string[] args)
    {
        new Simulation().Start();
    }
}
class Simulation
{
    public void Start()
    {
        rl.InitWindow(1024, 1024, "Primordial Kinetic Life");
        rl.SetTargetFPS(60);
        rlImGui.Setup(true, true);
        Init();
        while (!rl.WindowShouldClose())
        {
            rl.BeginDrawing();
            rl.ClearBackground(Color.BLACK);
            rlImGui.Begin();
            Update();
            GUI();
            rlImGui.End();
            rl.EndDrawing();
        }
        rlImGui.Shutdown();
        rl.CloseWindow();
    }
    List<Atom> Atoms = new();
    Dictionary<string, List<Atom>> T2A = new();
    public Simulation()
    {

    }
    public void Init()
    {
        T2A.Clear();
        Atoms.Clear();
        T2A.Add("R", Create(Counts[0], Color.RED));
        T2A.Add("G", Create(Counts[1], Color.GREEN));
        T2A.Add("B", Create(Counts[2], Color.BLUE));
        T2A.Add("W", Create(Counts[3], Color.WHITE));
    }
    float[] FA = new float[16];
    int[] Counts = new int[4] { 100, 100, 100, 100 };
    float LocalFactor = 100;
    float deltaTime = 1;
    class Save
    {
        public float[] FA { get; set; }
        public int[] Counts { get; set; }
        public float LocalFactor { get; set; }
    }
    public void GUI()
    {
        ImGui.Begin($"Controller");
        ImGui.Text($"FPS: {rl.GetFPS()}");
        ImGui.DragFloat($"LocalFactor", ref LocalFactor);
        ImGui.DragFloat($"Delta Time", ref deltaTime,0.01f);
        if (ImGui.Button("Reset"))
            Init();
        int idx = 0;
        for (int i = 0; i < T2A.Keys.Count; i++)
        {
            ImGui.InputInt($"{T2A.Keys.ElementAt(i)} Count", ref Counts[i], 100, 500);
            for (int j = 0; j < T2A.Keys.Count; j++, idx++)
            {
                var label = $"{T2A.Keys.ElementAt(i)}x{T2A.Keys.ElementAt(j)}";
                ImGui.DragFloat(label, ref FA[idx], 0.01f, -3, 3);
            }
            ImGui.Separator();
        }
        if (ImGui.Button("Save"))
        {
            Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "Saves"));
            var SFD = new SaveFileDialog()
            {
                InitialDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Saves"),
                FileName = "Save1"
            };

            if (SFD.ShowDialog() == DialogResult.OK)
            {
                var save = new Save()
                {
                    FA = FA,
                    Counts = Counts,
                    LocalFactor = LocalFactor,
                };
                File.WriteAllText(SFD.FileName,JsonSerializer.Serialize(save));
            }

        }
        ImGui.SameLine();
        if (ImGui.Button("Load"))
        {
            Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "Saves"));
            var SFD = new OpenFileDialog()
            {
                InitialDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Saves")
            };
            if (SFD.ShowDialog() == DialogResult.OK)
            {
                var save = JsonSerializer.Deserialize<Save>(File.ReadAllText(SFD.FileName));
                if (save != null)
                {
                    FA = save.FA;
                    Counts = save.Counts;
                    LocalFactor = save.LocalFactor;
                    Init();
                }
            }
        }
    }
    public void Update()
    {
        int idx = 0;
        for (int i = 0; i < T2A.Keys.Count; i++)
        {
            for (int j = 0; j < T2A.Keys.Count; j++, idx++)
            {
                var A = T2A.Keys.ElementAt(i);
                var B = T2A.Keys.ElementAt(j);
                Rule(A, B, FA[idx]);
            }
        }
        foreach (var atom in Atoms)
        {
            rl.DrawCircleV(atom.P, 4, atom.C);
        }
    }
    List<Atom> Create(int count, Color c)
    {
        var cluster = new List<Atom>();
        for (int i = 0; i < count; i++)
        {
            var atom = new Atom(new Vector2(Random.Shared.Next(rl.GetScreenWidth()), Random.Shared.Next(rl.GetScreenHeight())), c);
            cluster.Add(atom);
            Atoms.Add(atom);
        }
        return cluster;
    }
    void Rule(string L1i, string L2i, float g)
    {
        var L1 = T2A[L1i];
        var L2 = T2A[L2i];
        Parallel.ForEach(L1, (a) =>
        {
            Vector2 df = Vector2.Zero;
            foreach (var b in L2)
            {
                Vector2 dir = a.P - b.P;
                float d = dir.Length();
                if (d > 0 && d < LocalFactor)
                {
                    var F = g * 1 / d;
                    df += dir * F * deltaTime;
                }
            }
            a.V = df * 0.5f;
            a.P += a.V;
            if (a.P.X < 0 || a.P.X > rl.GetScreenWidth())
            {
                a.V *= -Vector2.UnitX;
                a.P = new Vector2(a.P.X < 0 ? 0 : a.P.X > rl.GetScreenWidth() ? rl.GetScreenWidth() : a.P.X, a.P.Y);
            }
            if (a.P.Y <= 0 || a.P.Y >= rl.GetScreenHeight())
            {
                a.V *= -Vector2.UnitY;
                a.P = new Vector2(a.P.X, a.P.Y < 0 ? 0 : a.P.Y > rl.GetScreenHeight() ? rl.GetScreenHeight() : a.P.Y);
            }
        });
    }
}
class Atom
{
    public Color C { get; set; }
    public Vector2 P { get; set; }
    public Vector2 V { get; set; } = Vector2.Zero;
    public Atom(Vector2 p, Color c)
    {
        P = p;
        C = c;
    }
}