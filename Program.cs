// See https://aka.ms/new-console-template for more information

using InventorySyetem1.Models;
using InventorySyetem1.Repositories;
using InventorySyetem1.Services;
using InventorySyetem1.Utils;
using ZstdSharp.Unsafe;

//Server: mysql所在伺服器位置（localhost or ip xxx.xxx.xxx.xxx）
//Port: mysql端口（預設3306）
//Database: inventory_db(CREATE DATABASE inventory_db;)
//uid: mysql使用者名稱
//pwd: mysql使用者密碼
const string MYSQL_CONNETION_STRING = "Server=localhost;Port=3306;Database=inventory_db;uid=root;pwd=password;";
string connectionString = "";
string configFile = "appsettings.ini";

if (File.Exists(configFile))
{
    Console.WriteLine($"Reading {configFile} file");
    try
    {
        Dictionary<string, Dictionary<string, string>> config = ReadFile(configFile);
        
        if (config.ContainsKey("Database"))
        {
            var dbConfig = config["Database"];
            connectionString = $"Server={dbConfig["Server"]};Port={dbConfig["Port"]};Database={dbConfig["Database"]};uid={dbConfig["uid"]};pwd={dbConfig["pwd"]};";
            Console.WriteLine($"讀取資料庫連接字串成功! ");
        }
    }
    catch (Exception e)
    {
        Console.WriteLine($"錯誤: 讀取配置檔案失敗: {e}");
        // throw;
        connectionString = MYSQL_CONNETION_STRING; 
    }
}
else
{
    Console.WriteLine($"錯誤: 配置檔案 {configFile} 不存在");
    connectionString = MYSQL_CONNETION_STRING; 
}

IProductRepository productRepository = new MySqlProductRepository(connectionString);
InventoryServices inventoryServices = new InventoryServices(productRepository);

INotifier emailNotifier = new EmailNotifier();
NotificationService emailService = new NotificationService(emailNotifier);

INotifier smsNotifier = new SmsNotifier();
NotificationService smsService = new NotificationService(smsNotifier);

RunMenu();

void RunMenu()
{
    while (true)
    {
        DisplayMenu();
        string input = Console.ReadLine();
        switch (input) 
        {
            case "1": GetAllProducts();
                break;
            case "2": SearchProductById();
                break;
            case "3": AddProduct();
                break;
            case "4": UpdateProduct();
                break;
            case "5": SearchProduct();
                break;
            case "6": SearchLowStock();
                break;
            case "0": 
                Console.WriteLine("Goodbye !");
                return;
        }
    }
}

void DisplayMenu()
{
    Console.WriteLine("Welcome to the inventory system!");
    Console.WriteLine("What would you like to do?");
    Console.WriteLine("1. 查看所有產品");
    Console.WriteLine("2. 查詢產品ID");
    Console.WriteLine("3. 新增產品");
    Console.WriteLine("4. 更新產品");
    Console.WriteLine("5. 查詢產品");
    Console.WriteLine("6. 查詢庫存偏低");
    Console.WriteLine("0. 離開");
}

void GetAllProducts()
{
    Console.WriteLine("\n--- 所有產品列表 ---");
    var products = inventoryServices.GetAllProducts();
    Console.WriteLine("-----------------------------------------------");
    Console.WriteLine("ID | Name | Price | Quantity | Status");
    Console.WriteLine("-----------------------------------------------");
    foreach (var product in products)
    {
        Console.WriteLine(product);
    }
    Console.WriteLine("-----------------------------------------------");
    emailService.NotifyUser(recipient: "kevin", message: "已顯示所有產品!");
}

void SearchProductById()
{
    Console.WriteLine("輸入欲查詢的產品編號");
    int intput = ReadIntLine(defaultValue: 1);
    var product = inventoryServices.GetProductById(intput);
    if (product != null)
    {
        Console.WriteLine("-----------------------------------------------");
        Console.WriteLine("ID | Name | Price | Quantity | Status");
        Console.WriteLine("-----------------------------------------------");
        Console.WriteLine(product);
        Console.WriteLine("-----------------------------------------------");
    }
}

void SearchProduct()
{
    Console.WriteLine("查詢產品名稱關鍵字:");
    string input = Console.ReadLine();
    List<Product> products = inventoryServices.SearchProducts(input);
    if (products.Any())
    {
        Console.WriteLine($"-------------- 查詢條件為: ({input}) ------------");
        Console.WriteLine("ID | Name | Price | Quantity | Status");
        Console.WriteLine("-----------------------------------------------");
        foreach (var product in products)
        {
            Console.WriteLine(product);
        }
        Console.WriteLine("-----------------------------------------------");
    }
}

void SearchLowStock()
{
    Console.WriteLine("查詢低庫存產品:");
    List<Product> products = inventoryServices.SearchLowStock();
    if (products.Any())
    {
        Console.WriteLine($"-------------- 查詢條件為: (低庫存) ------------");
        Console.WriteLine("ID | Name | Price | Quantity | Status");
        Console.WriteLine("-----------------------------------------------");
        foreach (var product in products)
        {
            Console.WriteLine(product);
        }
        Console.WriteLine("-----------------------------------------------");
    }
}

void AddProduct()
{
    Console.WriteLine("輸入產品名稱: ");
    string name = Console.ReadLine();
    Console.WriteLine("輸入產品價格: ");
    decimal price = ReadDecimalLine();
    Console.WriteLine("輸入產品數量: ");
    int quantity = ReadIntLine();
    inventoryServices.AddProduct(name, price, quantity);
    smsService.NotifyUser(recipient: "kevin", message: "已新增產品");
}

void UpdateProduct()
{
    Console.WriteLine("請輸入要更新的產品id:");
    int id = ReadIntLine(defaultValue: 1);
    // 找到對應產品
    var product = inventoryServices.GetProductById(id);
    if (product == null)
    {
        Console.WriteLine("查無此產品。");
        return;
    }
        // 輸入新名稱
        Console.WriteLine("請輸入要更新的產品名稱:");
        string name = Console.ReadLine();
        // 輸入新價格
        Console.WriteLine("請輸入要更新的產品價格:");
        decimal price = ReadDecimalLine();
        // 輸入新數量
        Console.WriteLine("請輸入要更新的產品數量:");
        int quantity = ReadIntLine();
    // service.UpdateProduct
    inventoryServices.UpdateProduct(product, name, price, quantity);
}

int ReadInt(string input)
{
    try
    {
        return Convert.ToInt32(input);
    }
    catch
    {
        Console.WriteLine("請輸入有效數字");
        return 0;
    }
}

int ReadIntLine(int defaultValue = 0)
{
    while (true)
    {
        string intput = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(intput) && defaultValue != 0)
        {
            return defaultValue;
        }
        // string parsing to int
        if (int.TryParse(intput, out int value))
        {
            return value;
        }
        else
        {
            Console.WriteLine("請輸入有效數字");
        }
    }
}

decimal ReadDecimalLine(decimal defaultValue = 0.0m)
{
    while (true)
    {
        string intput = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(intput) && defaultValue != 0.0m)
        {
            return defaultValue;
        }
        // string parsing to int
        if (decimal.TryParse(intput, out decimal value))
        {
            return value;
        }
        else
        {
            Console.WriteLine("請輸入有效數字");
        }
    }
}

Dictionary<string, Dictionary<string, string>> ReadFile(string s)
{
    var config = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
    string currentSection = "";

    foreach (string line in File.ReadLines(s))
    {
        string trimmedLine = line.Trim();
        if (trimmedLine.StartsWith("#") || string.IsNullOrWhiteSpace(trimmedLine))
        {
            continue; // 跳過註釋和空行
        }

        if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
        {
            currentSection = trimmedLine.Substring(1, trimmedLine.Length - 2);
            config[currentSection] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
        else if (currentSection != "" && trimmedLine.Contains("="))
        {
            int equalsIndex = trimmedLine.IndexOf('=');
            string key = trimmedLine.Substring(0, equalsIndex).Trim();
            string value = trimmedLine.Substring(equalsIndex + 1).Trim();
            config[currentSection][key] = value;
        }
    }
    return config;
}