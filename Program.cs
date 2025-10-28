using System;
using System.Reflection;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
class ReviseInfoAttribute : Attribute
{
    public string Name { get; }
    public string Description { get; }
    public ReviseInfoAttribute(string name, string description)
    {
        Name = name;
        Description = description;
    }

}


class AimConfig
{
    [ReviseInfo("A","blblblblblb")]
    public string HeroName { get; set; }="default";
    [ReviseInfo("B","blblblblblb")]
    public string HeroDesc { get; set; }="No description";
    [ReviseInfo("C","blblblblblb")]
    public double Hp { get; set; } = 1000;
    [ReviseInfo("D","blblblblblb")]
    public double Mp { get; set; } = 1000;
    [ReviseInfo("E","blblblblblb")]
    public bool Evolvable { get; set; } = false;
}

class ConfigManager<T> where T:new()
{
    private readonly string _filepath;
    private readonly object _lock=new object();
    public T Config { get; private set; }
    public ConfigManager(string filepath)
    {
        _filepath = filepath;
            Config = new T();
    }

    public void Load()
    {
        lock (_lock)
        {
            if (!File.Exists(_filepath))
            {
                Console.WriteLine("配置文件找不到，将使用默认值");
                return;
            }

            try
            {
                var lines = File.ReadAllLines(_filepath);
                var props = typeof(T).GetProperties(BindingFlags.Public|BindingFlags.Instance);
                foreach (var line in lines)
                {
                    string trimedLine = line.Trim();
                    if (trimedLine.StartsWith("#")||trimedLine.StartsWith("//")||string.IsNullOrWhiteSpace(trimedLine))
                        continue;
                    string[] parts = trimedLine.Split("=", 2);
                    if (parts.Length != 2)
                        continue;
                    string name = parts[0].Trim();
                    string value = parts[1].Trim();
                    foreach (var prop in props)
                    {
                        if (string.Equals(prop.Name, name))
                        {
                            try
                            {
                                var val=Convert.ChangeType(value, prop.PropertyType);
                                prop.SetValue(Config, val);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine($"{prop}属性赋值错误，错误原因为：{e}");
                                throw;
                            }
                        }
                    }
                }


            }
            catch (Exception e)
            {
                Console.WriteLine($"加载文件错误，错误原因为{e}");
                throw;
            }
            
        }
    }

    public void Save()
    {
        lock (_lock)
        {
            try
            {
                var props = typeof(T).GetProperties(BindingFlags.Public|BindingFlags.Instance);
                var lines = new List<string>{"#生成配置文件"};
                foreach (var prop in props)
                {
                    var attr = prop.GetCustomAttribute<ReviseInfoAttribute>();
                    if (attr != null)
                    {
                        lines.Add($"修改人:{prop.Name},批注:{attr.Description}");
                    }
                    var value = prop.GetValue(Config);
                    lines.Add($"{prop.Name}={value}");
                }
                File.WriteAllLines(_filepath, lines);
            }
            catch (Exception e)
            {
                Console.WriteLine($"写入配置文件失败，原因为{e}");
                throw;
            }
        }
    }
}

class Program
{
    static void Main(string[] args)
    {
        string configpath = "C:\\Users\\saguraaa\\Desktop\\视觉组软开\\项目管理器\\config.cfg";
        var manager=new ConfigManager<AimConfig>(configpath);
        var hero = manager.Config;
        hero.Hp = 100;
        hero.Mp = 100;
        manager.Save();
        Console.WriteLine("配置保存完毕");
        ConfigManager<AimConfig> manager2 = new(configpath);
        manager2.Load();
        Console.WriteLine($"Hp:{manager2.Config.Hp},Mp:{manager2.Config.Mp}");
    }
}