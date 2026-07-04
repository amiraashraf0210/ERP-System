using ERP.Core.Models;
using ERP.Data;
using Microsoft.EntityFrameworkCore;

namespace ERP.UI.Forms
{
    /// <summary>
    /// Seeds realistic demo data for a plastic bags and beads manufacturing/trading company.
    /// Safe to call on every startup — the guard at the top prevents duplicate inserts.
    /// </summary>
    public static class SeedDemoData
    {
        public static void Seed(AppDbContext db)
        {
            if (db.Goods.Any()) return;

            SeedLookups(db);
            SeedTraders(db);
            SeedImporters(db);
            SeedCustomers(db);
            SeedGoods(db);
            SeedBuyBills(db);
            SeedSellBills(db);
            SeedMovements(db);
            SeedFinance(db);
            SeedEmployees(db);
            SeedAccounts(db);
            SeedBanksAndLoans(db);
            SeedOrdersAndDelivery(db);
            SeedProduction(db);
        }

        // Called separately so production data can be added even if the main seed already ran
        public static void SeedProductionOnly(AppDbContext db)
        {
            if (db.RawMaterials.Any()) return;
            SeedProduction(db);
        }

        // Run missing parts of the seed if their tables are empty.
        // This is safe to call after a partial seed run — each sub-seeder
        // performs its own existence checks where appropriate.
        public static void SeedMissing(AppDbContext db)
        {
            if (!db.Set<BuyBill>().Any()) SeedBuyBills(db);
            if (!db.Set<SellBill>().Any()) SeedSellBills(db);
            if (!db.Set<Movement>().Any()) SeedMovements(db);
            if (!db.Set<BoxTransaction>().Any()) SeedFinance(db);
            if (!db.Set<Employee>().Any()) SeedEmployees(db);
            if (!db.Roots.Any()) SeedAccounts(db);
            if (!db.Set<Bank>().Any()) SeedBanksAndLoans(db);
            if (!db.Set<Order>().Any()) SeedOrdersAndDelivery(db);
            if (!db.RawMaterials.Any()) SeedProduction(db);
        }

        // Lookups: units, groups, colors, models, markets, cost types
        static void SeedLookups(AppDbContext db)
        {
            if (!db.Set<Unit>().Any())
                db.Set<Unit>().AddRange(
                    new Unit { Code = 1, UnitName = "قطعة" },
                    new Unit { Code = 2, UnitName = "كيلو" },
                    new Unit { Code = 3, UnitName = "جرام" },
                    new Unit { Code = 4, UnitName = "كرتون" },
                    new Unit { Code = 5, UnitName = "طن" },
                    new Unit { Code = 6, UnitName = "رول" },
                    new Unit { Code = 7, UnitName = "لتر" }
                );

            if (!db.Set<GoodGroup>().Any())
                db.Set<GoodGroup>().AddRange(
                    new GoodGroup { Code = 1, GroupName = "أكياس بلاستيك" },
                    new GoodGroup { Code = 2, GroupName = "خرز" },
                    new GoodGroup { Code = 3, GroupName = "خامات إنتاج" },
                    new GoodGroup { Code = 4, GroupName = "مواد تعبئة" }
                );

            if (!db.Set<CustomerGroup>().Any())
                db.Set<CustomerGroup>().AddRange(
                    new CustomerGroup { Code = 1, GroupName = "عملاء جملة" },
                    new CustomerGroup { Code = 2, GroupName = "عملاء تجزئة" },
                    new CustomerGroup { Code = 3, GroupName = "عملاء مصانع" }
                );

            if (!db.Set<ImporterGroup>().Any())
                db.Set<ImporterGroup>().AddRange(
                    new ImporterGroup { Code = 1, GroupName = "موردو بلاستيك وخامات" },
                    new ImporterGroup { Code = 2, GroupName = "موردو خرز وإكسسوار" }
                );

            if (!db.Set<GoodColor>().Any())
                db.Set<GoodColor>().AddRange(
                    new GoodColor { Code = 1, ColorName = "أبيض" },
                    new GoodColor { Code = 2, ColorName = "أسود" },
                    new GoodColor { Code = 3, ColorName = "أحمر" },
                    new GoodColor { Code = 4, ColorName = "أزرق" },
                    new GoodColor { Code = 5, ColorName = "أخضر" },
                    new GoodColor { Code = 6, ColorName = "أصفر" },
                    new GoodColor { Code = 7, ColorName = "شفاف" },
                    new GoodColor { Code = 8, ColorName = "متعدد" }
                );

            if (!db.Set<GoodModel>().Any())
                db.Set<GoodModel>().AddRange(
                    new GoodModel { Code = 1, ModelName = "صغير" },
                    new GoodModel { Code = 2, ModelName = "متوسط" },
                    new GoodModel { Code = 3, ModelName = "كبير" },
                    new GoodModel { Code = 4, ModelName = "جامبو" }
                );

            if (!db.Set<Market>().Any())
                db.Set<Market>().AddRange(
                    new Market { Code = 1, MarketName = "السوق المحلي" },
                    new Market { Code = 2, MarketName = "السوق الإقليمي" }
                );

            if (!db.Set<Cost>().Any())
                db.Set<Cost>().AddRange(
                    new Cost { Code = "1", CostName = "إيجار" },
                    new Cost { Code = "2", CostName = "رواتب وأجور" },
                    new Cost { Code = "3", CostName = "كهرباء وماء" },
                    new Cost { Code = "4", CostName = "صيانة آلات" },
                    new Cost { Code = "5", CostName = "نقل ومواصلات" },
                    new Cost { Code = "6", CostName = "مصاريف إدارية" }
                );

            db.SaveChanges();
        }

        // Traders (sales reps / mandoubs)
        static void SeedTraders(AppDbContext db)
        {
            if (db.Traders.Any()) return;
            db.Traders.AddRange(
                new Trader { Code = 1, Name = "أحمد سالم", Mobile = "0501234561", Tel = "0112345671", Address = "الرياض" },
                new Trader { Code = 2, Name = "محمد علي", Mobile = "0501234562", Tel = "0112345672", Address = "جدة" },
                new Trader { Code = 3, Name = "خالد يوسف", Mobile = "0501234563", Tel = "0112345673", Address = "الدمام" },
                new Trader { Code = 4, Name = "سارة العتيبي", Mobile = "0501234564", Tel = "0112345674", Address = "المدينة" }
            );
            db.SaveChanges();
        }

        // Importers (Suppliers)
        static void SeedImporters(AppDbContext db)
        {
            var g1 = db.Set<ImporterGroup>().First(g => g.Code == 1);
            var g2 = db.Set<ImporterGroup>().First(g => g.Code == 2);
            db.Set<Importer>().AddRange(
                new Importer { Code = 1, Name = "شركة البلاستيك الحديثة", Manager = "عمر الشمري", GroupId = g1.Id, Tel = "0112001001", Mobile = "0551001001" },
                new Importer { Code = 2, Name = "مصنع النور للبولي إيثيلين", Manager = "فهد النور", GroupId = g1.Id, Tel = "0112001002", Mobile = "0551001002" },
                new Importer { Code = 3, Name = "شركة الخرز العربية", Manager = "نواف القحطاني", GroupId = g2.Id, Tel = "0112001003", Mobile = "0551001003" },
                new Importer { Code = 4, Name = "مؤسسة الأمل للخامات", Manager = "سعود الزهراني", GroupId = g1.Id, Tel = "0112001004", Mobile = "0551001004" },
                new Importer { Code = 5, Name = "توريدات الخليج للحبيبات", Manager = "ناصر الدوسري", GroupId = g2.Id, Tel = "0112001005", Mobile = "0551001005" }
            );
            db.SaveChanges();
        }

        // Customers
        static void SeedCustomers(AppDbContext db)
        {
            var gJ = db.Set<CustomerGroup>().First(g => g.Code == 1);
            var gT = db.Set<CustomerGroup>().First(g => g.Code == 2);
            var gM = db.Set<CustomerGroup>().First(g => g.Code == 3);
            db.Set<Customer>().AddRange(
                new Customer { Code = 1, Name = "سوبرماركت الوفاء", Manager = "بدر العتيبي", GroupId = gT.Id, Tel = "0112002001", Mobile = "0552002001" },
                new Customer { Code = 2, Name = "شركة التوزيع الكبرى", Manager = "راشد المطيري", GroupId = gJ.Id, Tel = "0112002002", Mobile = "0552002002" },
                new Customer { Code = 3, Name = "مصنع المنتجات الغذائية", Manager = "سلطان الحارثي", GroupId = gM.Id, Tel = "0112002003", Mobile = "0552002003" },
                new Customer { Code = 4, Name = "محلات النعيم", Manager = "ظافر العنزي", GroupId = gT.Id, Tel = "0112002004", Mobile = "0552002004" },
                new Customer { Code = 5, Name = "شركة الأمل التجارية", Manager = "وليد الشهري", GroupId = gJ.Id, Tel = "0112002005", Mobile = "0552002005" },
                new Customer { Code = 6, Name = "مؤسسة الفارس", Manager = "فيصل الزيد", GroupId = gT.Id, Tel = "0112002006", Mobile = "0552002006" },
                new Customer { Code = 7, Name = "مصنع الحلويات الذهبية", Manager = "عبدالله القرني", GroupId = gM.Id, Tel = "0112002007", Mobile = "0552002007" },
                new Customer { Code = 8, Name = "سلسلة محلات البيان", Manager = "حمدان الغامدي", GroupId = gJ.Id, Tel = "0112002008", Mobile = "0552002008" },
                new Customer { Code = 9, Name = "شركة التغليف الحديثة", Manager = "يوسف العسيري", GroupId = gM.Id, Tel = "0112002009", Mobile = "0552002009" },
                new Customer { Code = 10, Name = "متاجر الريف", Manager = "مساعد السبيعي", GroupId = gT.Id, Tel = "0112002010", Mobile = "0552002010" },
                new Customer { Code = 11, Name = "مجمع الإنتاج الوطني", Manager = "تركي العقيل", GroupId = gM.Id, Tel = "0112002011", Mobile = "0552002011" },
                new Customer { Code = 12, Name = "شركة هلال للتعبئة", Manager = "مبارك الدخيل", GroupId = gJ.Id, Tel = "0112002012", Mobile = "0552002012" }
            );
            db.SaveChanges();
        }

        // Goods — plastic bags + beads + raw materials
        static void SeedGoods(AppDbContext db)
        {
            var store = db.Stores.First();
            var uPcs = db.Set<Unit>().First(u => u.Code == 1); // قطعة
            var uKg = db.Set<Unit>().First(u => u.Code == 2); // كيلو
            var uGram = db.Set<Unit>().First(u => u.Code == 3); // جرام
            var uBox = db.Set<Unit>().First(u => u.Code == 4); // كرتون
            var uTon = db.Set<Unit>().First(u => u.Code == 5); // طن
            var gBags = db.Set<GoodGroup>().First(g => g.Code == 1);
            var gBeads = db.Set<GoodGroup>().First(g => g.Code == 2);
            var gRaw = db.Set<GoodGroup>().First(g => g.Code == 3);
            var gPack = db.Set<GoodGroup>().First(g => g.Code == 4);
            var imp1 = db.Set<Importer>().First(i => i.Code == 1);
            var imp2 = db.Set<Importer>().First(i => i.Code == 2);
            var imp3 = db.Set<Importer>().First(i => i.Code == 3);
            var imp4 = db.Set<Importer>().First(i => i.Code == 4);
            var imp5 = db.Set<Importer>().First(i => i.Code == 5);
            var cWhite = db.Set<GoodColor>().First(c => c.Code == 1);
            var cBlack = db.Set<GoodColor>().First(c => c.Code == 2);
            var cRed = db.Set<GoodColor>().First(c => c.Code == 3);
            var cBlue = db.Set<GoodColor>().First(c => c.Code == 4);
            var cGreen = db.Set<GoodColor>().First(c => c.Code == 5);
            var cYellow = db.Set<GoodColor>().First(c => c.Code == 6);
            var cClear = db.Set<GoodColor>().First(c => c.Code == 7);
            var now = DateTime.Now;

            db.Set<Good>().AddRange(
                // ── Plastic Bags ──
                new Good { Code = "KB001", Name = "كيس بلاستيك شفاف صغير (25×35)", GroupId = gBags.Id, UnitId = uBox.Id, StoreId = store.Id, ImporterId = imp1.Id, BuyPrice = 18f, SellPrice = 35f, HalfPrice = 28f, CustPrice = 32f, MinStock = 500, MaxStock = 20000, DayOfRegister = now, ColorId = cClear.Id },
                new Good { Code = "KB002", Name = "كيس بلاستيك شفاف متوسط (35×50)", GroupId = gBags.Id, UnitId = uBox.Id, StoreId = store.Id, ImporterId = imp1.Id, BuyPrice = 28f, SellPrice = 55f, HalfPrice = 45f, CustPrice = 50f, MinStock = 400, MaxStock = 15000, DayOfRegister = now, ColorId = cClear.Id },
                new Good { Code = "KB003", Name = "كيس بلاستيك شفاف كبير (50×70)", GroupId = gBags.Id, UnitId = uBox.Id, StoreId = store.Id, ImporterId = imp1.Id, BuyPrice = 40f, SellPrice = 80f, HalfPrice = 65f, CustPrice = 75f, MinStock = 300, MaxStock = 10000, DayOfRegister = now, ColorId = cClear.Id },
                new Good { Code = "KB004", Name = "كيس بلاستيك أسود صغير", GroupId = gBags.Id, UnitId = uBox.Id, StoreId = store.Id, ImporterId = imp1.Id, BuyPrice = 20f, SellPrice = 40f, HalfPrice = 32f, CustPrice = 37f, MinStock = 500, MaxStock = 20000, DayOfRegister = now, ColorId = cBlack.Id },
                new Good { Code = "KB005", Name = "كيس بلاستيك أسود كبير (قمامة)", GroupId = gBags.Id, UnitId = uBox.Id, StoreId = store.Id, ImporterId = imp1.Id, BuyPrice = 35f, SellPrice = 68f, HalfPrice = 55f, CustPrice = 63f, MinStock = 400, MaxStock = 15000, DayOfRegister = now, ColorId = cBlack.Id },
                new Good { Code = "KB006", Name = "كيس بلاستيك أبيض ناعم", GroupId = gBags.Id, UnitId = uBox.Id, StoreId = store.Id, ImporterId = imp2.Id, BuyPrice = 22f, SellPrice = 45f, HalfPrice = 36f, CustPrice = 42f, MinStock = 500, MaxStock = 18000, DayOfRegister = now, ColorId = cWhite.Id },
                new Good { Code = "KB007", Name = "كيس مقبض بلاستيك أبيض", GroupId = gBags.Id, UnitId = uBox.Id, StoreId = store.Id, ImporterId = imp2.Id, BuyPrice = 45f, SellPrice = 90f, HalfPrice = 75f, CustPrice = 85f, MinStock = 300, MaxStock = 10000, DayOfRegister = now, ColorId = cWhite.Id },
                new Good { Code = "KB008", Name = "كيس مقبض ملون متوسط", GroupId = gBags.Id, UnitId = uBox.Id, StoreId = store.Id, ImporterId = imp2.Id, BuyPrice = 50f, SellPrice = 100f, HalfPrice = 82f, CustPrice = 92f, MinStock = 200, MaxStock = 8000, DayOfRegister = now, ColorId = cRed.Id },
                new Good { Code = "KB009", Name = "كيس مقبض ملون كبير", GroupId = gBags.Id, UnitId = uBox.Id, StoreId = store.Id, ImporterId = imp2.Id, BuyPrice = 65f, SellPrice = 130f, HalfPrice = 108f, CustPrice = 120f, MinStock = 200, MaxStock = 6000, DayOfRegister = now, ColorId = cBlue.Id },
                new Good { Code = "KB010", Name = "كيس زيبلوك شفاف صغير", GroupId = gBags.Id, UnitId = uBox.Id, StoreId = store.Id, ImporterId = imp2.Id, BuyPrice = 55f, SellPrice = 110f, HalfPrice = 90f, CustPrice = 100f, MinStock = 300, MaxStock = 10000, DayOfRegister = now, ColorId = cClear.Id },
                // ── Beads ──
                new Good { Code = "XR001", Name = "خرز زجاجي أبيض 6مم (100جم)", GroupId = gBeads.Id, UnitId = uGram.Id, StoreId = store.Id, ImporterId = imp3.Id, BuyPrice = 0.08f, SellPrice = 0.2f, HalfPrice = 0.16f, CustPrice = 0.18f, MinStock = 10000, MaxStock = 500000, DayOfRegister = now, ColorId = cWhite.Id },
                new Good { Code = "XR002", Name = "خرز زجاجي ملون 6مم (100جم)", GroupId = gBeads.Id, UnitId = uGram.Id, StoreId = store.Id, ImporterId = imp3.Id, BuyPrice = 0.1f, SellPrice = 0.25f, HalfPrice = 0.2f, CustPrice = 0.22f, MinStock = 10000, MaxStock = 500000, DayOfRegister = now, ColorId = cRed.Id },
                new Good { Code = "XR003", Name = "خرز بلاستيك أزرق 8مم", GroupId = gBeads.Id, UnitId = uGram.Id, StoreId = store.Id, ImporterId = imp3.Id, BuyPrice = 0.06f, SellPrice = 0.15f, HalfPrice = 0.12f, CustPrice = 0.14f, MinStock = 10000, MaxStock = 500000, DayOfRegister = now, ColorId = cBlue.Id },
                new Good { Code = "XR004", Name = "خرز أكريليك شفاف 10مم", GroupId = gBeads.Id, UnitId = uGram.Id, StoreId = store.Id, ImporterId = imp5.Id, BuyPrice = 0.12f, SellPrice = 0.3f, HalfPrice = 0.24f, CustPrice = 0.27f, MinStock = 5000, MaxStock = 200000, DayOfRegister = now, ColorId = cClear.Id },
                new Good { Code = "XR005", Name = "خرز ذهبي معدني 4مم", GroupId = gBeads.Id, UnitId = uGram.Id, StoreId = store.Id, ImporterId = imp5.Id, BuyPrice = 0.2f, SellPrice = 0.5f, HalfPrice = 0.4f, CustPrice = 0.45f, MinStock = 3000, MaxStock = 100000, DayOfRegister = now },
                new Good { Code = "XR006", Name = "خرز فضي معدني 4مم", GroupId = gBeads.Id, UnitId = uGram.Id, StoreId = store.Id, ImporterId = imp5.Id, BuyPrice = 0.18f, SellPrice = 0.45f, HalfPrice = 0.36f, CustPrice = 0.4f, MinStock = 3000, MaxStock = 100000, DayOfRegister = now },
                new Good { Code = "XR007", Name = "خرز لؤلؤي أبيض 6مم", GroupId = gBeads.Id, UnitId = uGram.Id, StoreId = store.Id, ImporterId = imp3.Id, BuyPrice = 0.15f, SellPrice = 0.38f, HalfPrice = 0.3f, CustPrice = 0.35f, MinStock = 5000, MaxStock = 200000, DayOfRegister = now, ColorId = cWhite.Id },
                new Good { Code = "XR008", Name = "خرز أخضر زمردي 8مم", GroupId = gBeads.Id, UnitId = uGram.Id, StoreId = store.Id, ImporterId = imp3.Id, BuyPrice = 0.14f, SellPrice = 0.35f, HalfPrice = 0.28f, CustPrice = 0.32f, MinStock = 5000, MaxStock = 150000, DayOfRegister = now, ColorId = cGreen.Id },
                // ── Raw Materials ──
                new Good { Code = "KH001", Name = "حبيبات HDPE بيضاء (كيلو)", GroupId = gRaw.Id, UnitId = uKg.Id, StoreId = store.Id, ImporterId = imp4.Id, BuyPrice = 8f, SellPrice = 15f, HalfPrice = 12f, CustPrice = 13f, MinStock = 1000, MaxStock = 50000, DayOfRegister = now, ColorId = cWhite.Id },
                new Good { Code = "KH002", Name = "حبيبات LDPE شفافة (كيلو)", GroupId = gRaw.Id, UnitId = uKg.Id, StoreId = store.Id, ImporterId = imp4.Id, BuyPrice = 9f, SellPrice = 17f, HalfPrice = 14f, CustPrice = 15f, MinStock = 1000, MaxStock = 50000, DayOfRegister = now, ColorId = cClear.Id },
                new Good { Code = "KH003", Name = "صبغة بلاستيك سوداء (كيلو)", GroupId = gRaw.Id, UnitId = uKg.Id, StoreId = store.Id, ImporterId = imp4.Id, BuyPrice = 25f, SellPrice = 50f, HalfPrice = 40f, CustPrice = 45f, MinStock = 200, MaxStock = 5000, DayOfRegister = now, ColorId = cBlack.Id },
                new Good { Code = "KH004", Name = "صبغة بلاستيك ملونة (كيلو)", GroupId = gRaw.Id, UnitId = uKg.Id, StoreId = store.Id, ImporterId = imp4.Id, BuyPrice = 30f, SellPrice = 60f, HalfPrice = 50f, CustPrice = 55f, MinStock = 100, MaxStock = 3000, DayOfRegister = now, ColorId = cRed.Id },
                // ── Packing ──
                new Good { Code = "TG001", Name = "كرتون تغليف كبير", GroupId = gPack.Id, UnitId = uPcs.Id, StoreId = store.Id, ImporterId = imp1.Id, BuyPrice = 3f, SellPrice = 7f, HalfPrice = 5.5f, CustPrice = 6.5f, MinStock = 500, MaxStock = 20000, DayOfRegister = now },
                new Good { Code = "TG002", Name = "شريط لاصق بلاستيك", GroupId = gPack.Id, UnitId = uPcs.Id, StoreId = store.Id, ImporterId = imp1.Id, BuyPrice = 2f, SellPrice = 5f, HalfPrice = 4f, CustPrice = 4.5f, MinStock = 1000, MaxStock = 30000, DayOfRegister = now, ColorId = cClear.Id }
            );
            db.SaveChanges();
        }

        // ── Lookup helpers ──
        static Importer Imp(AppDbContext db, int code) => db.Set<Importer>().First(i => i.Code == code);
        static Good Gd(AppDbContext db, string code) => db.Set<Good>().First(g => g.Code == code);
        static Customer Cu(AppDbContext db, int code) => db.Set<Customer>().First(c => c.Code == code);
        static Store St(AppDbContext db) => db.Stores.First();
        static User AdminUser(AppDbContext db) => db.Users.First();
        static Trader Tr(AppDbContext db, int code) => db.Traders.First(t => t.Code == code);

        // Returns the current open fiscal year id, creating one if needed
        static int GetOrCreateFiscalYear(AppDbContext db)
        {
            var fy = db.FiscalYears.FirstOrDefault(f => !f.IsClosed);
            if (fy != null) return fy.Id;

            // Create current year fiscal year if no open year exists
            var newFy = new FiscalYear
            {
                Year = DateTime.Today.Year,
                StartDate = new DateTime(DateTime.Today.Year, 1, 1),
                EndDate = new DateTime(DateTime.Today.Year, 12, 31),
                IsClosed = false
            };
            db.FiscalYears.Add(newFy);
            db.SaveChanges();
            return newFy.Id;
        }

        // Buy Bills — purchasing plastic granules, bags, beads from suppliers
        static void SeedBuyBills(AppDbContext db)
        {
            var store = St(db);
            int fyId = GetOrCreateFiscalYear(db);
            var bills = new List<(int code, int imp, DateTime date, (string g, int qty, float price)[] lines)>
            {
                (1,  1, DateTime.Now.AddMonths(-5), new[]{ ("KB001",200,18f),  ("KB002",150,28f), ("KB003",100,40f) }),
                (2,  2, DateTime.Now.AddMonths(-5), new[]{ ("KB006",200,22f),  ("KB007",150,45f), ("KB008",100,50f) }),
                (3,  3, DateTime.Now.AddMonths(-4), new[]{ ("XR001",50000,0.08f),("XR002",30000,0.1f),("XR003",40000,0.06f) }),
                (4,  4, DateTime.Now.AddMonths(-4), new[]{ ("KH001",500,8f),   ("KH002",500,9f),  ("KH003",100,25f) }),
                (5,  1, DateTime.Now.AddMonths(-3), new[]{ ("KB004",250,20f),  ("KB005",200,35f) }),
                (6,  2, DateTime.Now.AddMonths(-3), new[]{ ("KB009",120,65f),  ("KB010",180,55f) }),
                (7,  5, DateTime.Now.AddMonths(-2), new[]{ ("XR004",20000,0.12f),("XR005",10000,0.2f),("XR006",10000,0.18f) }),
                (8,  3, DateTime.Now.AddMonths(-2), new[]{ ("XR007",25000,0.15f),("XR008",20000,0.14f) }),
                (9,  4, DateTime.Now.AddMonths(-1), new[]{ ("KH004",80,30f),   ("TG001",300,3f),  ("TG002",500,2f) }),
                (10, 1, DateTime.Now.AddMonths(-1), new[]{ ("KB001",300,18f),  ("KB002",200,28f) }),
            };

            foreach (var (code, impCode, date, lines) in bills)
            {
                var imp = Imp(db, impCode);
                float total = lines.Sum(l => l.qty * l.price);
                var bill = new BuyBill { ImporterId = imp.Id, Code = code.ToString(), Date = date, Asked = total, Paid = total, FiscalYearId = fyId };
                foreach (var (gCode, qty, price) in lines)
                {
                    var g = Gd(db, gCode);
                    bill.Items.Add(new ImporterInstallment
                    { ImporterId = imp.Id, GoodId = g.Id, Quantity = qty, Price = price, Total = qty * price, Pay = qty * price, StoreId = store.Id, Date = date });
                }
                db.Set<BuyBill>().Add(bill);
            }
            db.SaveChanges();
        }

        // Sell Bills — selling bags and beads to customers
        static void SeedSellBills(AppDbContext db)
        {
            var store = St(db);
            var user = AdminUser(db);
            int fyId = GetOrCreateFiscalYear(db);
            var bills = new List<(int code, int cust, int seller, DateTime date, (string g, int qty, float price)[] lines)>
            {
                (1,  1, 1, DateTime.Now.AddMonths(-4), new[]{ ("KB001",50,35f),  ("KB002",30,55f),  ("KB004",40,40f)  }),
                (2,  2, 2, DateTime.Now.AddMonths(-4), new[]{ ("KB006",60,45f),  ("KB007",40,90f),  ("KB008",25,100f) }),
                (3,  3, 1, DateTime.Now.AddMonths(-4), new[]{ ("XR001",5000,0.2f),("XR002",3000,0.25f),("XR004",2000,0.3f) }),
                (4,  4, 3, DateTime.Now.AddMonths(-3), new[]{ ("KB003",80,80f),  ("KB005",60,68f)  }),
                (5,  5, 2, DateTime.Now.AddMonths(-3), new[]{ ("XR005",2000,0.5f),("XR006",2000,0.45f),("XR007",3000,0.38f) }),
                (6,  6, 4, DateTime.Now.AddMonths(-3), new[]{ ("KB009",50,130f), ("KB010",70,110f) }),
                (7,  7, 1, DateTime.Now.AddMonths(-2), new[]{ ("XR001",8000,0.2f),("XR003",6000,0.15f),("XR008",4000,0.35f) }),
                (8,  8, 2, DateTime.Now.AddMonths(-2), new[]{ ("KB001",100,35f), ("KB004",80,40f),  ("KB006",60,45f) }),
                (9,  9, 3, DateTime.Now.AddMonths(-2), new[]{ ("KH001",50,15f),  ("KH002",50,17f),  ("KH003",20,50f) }),
                (10,10, 4, DateTime.Now.AddMonths(-2), new[]{ ("TG001",100,7f),  ("TG002",200,5f)  }),
                (11, 1, 1, DateTime.Now.AddMonths(-1), new[]{ ("KB002",80,55f),  ("KB003",50,80f)  }),
                (12, 2, 2, DateTime.Now.AddMonths(-1), new[]{ ("XR004",3000,0.3f),("XR005",1500,0.5f) }),
                (13, 3, 1, DateTime.Now.AddMonths(-1), new[]{ ("KB007",60,90f),  ("KB008",40,100f), ("KB009",30,130f) }),
                (14,11, 3, DateTime.Now.AddDays(-20),  new[]{ ("KH004",15,60f),  ("XR001",10000,0.2f) }),
                (15,12, 4, DateTime.Now.AddDays(-15),  new[]{ ("KB001",120,35f), ("KB006",90,45f),  ("TG001",150,7f) }),
                (16, 4, 1, DateTime.Now.AddDays(-10),  new[]{ ("XR007",5000,0.38f),("XR008",4000,0.35f) }),
                (17, 5, 2, DateTime.Now.AddDays(-7),   new[]{ ("KB010",100,110f),("KB003",70,80f)  }),
                (18, 6, 4, DateTime.Now.AddDays(-5),   new[]{ ("XR002",8000,0.25f),("XR006",3000,0.45f) }),
                (19, 7, 3, DateTime.Now.AddDays(-3),   new[]{ ("KB004",90,40f),  ("KB005",70,68f)  }),
                (20, 8, 1, DateTime.Now,               new[]{ ("XR005",2500,0.5f),("XR003",5000,0.15f),("KB002",60,55f) }),
            };

            foreach (var (code, custCode, sellerCode, date, lines) in bills)
            {
                var cust = Cu(db, custCode);
                var seller = Tr(db, sellerCode);
                double total = lines.Sum(l => (double)l.qty * l.price);
                var bill = new SellBill
                {
                    CustomerId = cust.Id,
                    Code = code,
                    Date = date,
                    Time = date,
                    Asked = total,
                    Paid = total,
                    UserId = user.Id,
                    StoreNo = store.Id,
                    BillType = 1,
                    SellerId = seller.Id,
                    FiscalYearId = fyId
                };
                foreach (var (gCode, qty, price) in lines)
                {
                    var g = Gd(db, gCode);
                    bill.Items.Add(new CustomerInstallment
                    {
                        CustomerId = cust.Id,
                        GoodId = g.Id,
                        Quantity = qty,
                        Price = price,
                        Total = qty * price,
                        Pay = qty * price,
                        StoreId = store.Id,
                        Date = date,
                        BuyPrice = g.BuyPrice
                    });
                }
                db.Set<SellBill>().Add(bill);
            }
            db.SaveChanges();
        }

        // Stock Movements — opening balance + bill mirrors
        static void SeedMovements(AppDbContext db)
        {
            var store = St(db);
            var goods = db.Set<Good>().ToList();
            var rng = new Random(42);
            var list = new List<Movement>();
            int fyId = GetOrCreateFiscalYear(db);

            // Opening stock for every product
            foreach (var g in goods)
                list.Add(new Movement
                {
                    GoodId = g.Id,
                    Quantity = rng.Next(500, 5000),
                    Date = DateTime.Now.AddMonths(-6),
                    IsBill = false,
                    Out = false,
                    StoreNo = store.Id,
                    BillNo = "افتتاح",
                    Notes = "رصيد افتتاحي",
                    FiscalYearId = fyId
                });

            // Mirror sell-bill lines as stock-out movements
            foreach (var bill in db.Set<SellBill>().Include(b => b.Items).ToList())
                foreach (var item in bill.Items)
                    list.Add(new Movement
                    {
                        GoodId = item.GoodId,
                        Quantity = item.Quantity,
                        Date = bill.Date,
                        IsBill = true,
                        Out = true,
                        StoreNo = store.Id,
                        BillNo = bill.Code.ToString(),
                        CustomerId = bill.CustomerId,
                        SellPrice = item.Price,
                        BuyPrice = item.BuyPrice,
                        FiscalYearId = fyId
                    });

            // Mirror buy-bill lines as stock-in movements
            foreach (var bill in db.Set<BuyBill>().Include(b => b.Items).ToList())
                foreach (var item in bill.Items)
                    list.Add(new Movement
                    {
                        GoodId = item.GoodId,
                        Quantity = item.Quantity,
                        Date = bill.Date,
                        IsBill = true,
                        Out = false,
                        StoreNo = store.Id,
                        BillNo = bill.Code,
                        ImporterId = bill.ImporterId,
                        BuyPrice = item.Price,
                        FiscalYearId = fyId
                    });

            db.Set<Movement>().AddRange(list);
            db.SaveChanges();
        }

        // Finance — treasury inflows, expenses, incomes
        static void SeedFinance(AppDbContext db)
        {
            var costs = db.Set<Cost>().ToList();
            var cRent = costs.First(c => c.Code == "1");
            var cSal = costs.First(c => c.Code == "2");
            var cElec = costs.First(c => c.Code == "3");
            var cMnt = costs.First(c => c.Code == "4");
            var cTrn = costs.First(c => c.Code == "5");
            var cAdm = costs.First(c => c.Code == "6");
            int fyId = GetOrCreateFiscalYear(db);

            // Treasury inflow for every sell bill (full payment)
            int no = 1;
            var boxList = new List<BoxTransaction>();
            foreach (var b in db.Set<SellBill>().ToList())
                boxList.Add(new BoxTransaction
                {
                    Out = false,
                    Value = b.Paid,
                    Date = b.Date,
                    Time = b.Date,
                    No = no++,
                    BoxNo = 1,
                    SellBillId = b.Id,
                    FiscalYearId = fyId,
                    Notes = $"تحصيل فاتورة مبيعات رقم {b.Code}"
                });

            // Treasury outflow for every buy bill (full payment)
            foreach (var b in db.Set<BuyBill>().ToList())
                boxList.Add(new BoxTransaction
                {
                    Out = true,
                    Value = b.Paid,
                    Date = b.Date,
                    Time = b.Date,
                    No = no++,
                    BoxNo = 1,
                    BuyBillId = b.Id,
                    FiscalYearId = fyId,
                    Notes = $"دفعة فاتورة مشتريات رقم {b.Code}"
                });

            db.Set<BoxTransaction>().AddRange(boxList);

            // 6 months of recurring expenses
            var expenses = new List<Expense>();
            for (int m = 5; m >= 0; m--)
            {
                var d = DateTime.Now.AddMonths(-m);
                expenses.Add(new Expense { Value = 4500, Detail = "إيجار المصنع", Date = d, CostId = cRent.Id, MainActive = true, FiscalYearId = fyId });
                expenses.Add(new Expense { Value = 9000, Detail = "رواتب العمال", Date = d, CostId = cSal.Id, MainActive = true, FiscalYearId = fyId });
                expenses.Add(new Expense { Value = 1200, Detail = "فاتورة كهرباء وماء", Date = d, CostId = cElec.Id, MainActive = true, FiscalYearId = fyId });
                expenses.Add(new Expense { Value = 600, Detail = "صيانة آلة البلاستيك", Date = d, CostId = cMnt.Id, MainActive = true, FiscalYearId = fyId });
                expenses.Add(new Expense { Value = 800, Detail = "نقل ومواصلات", Date = d, CostId = cTrn.Id, MainActive = true, FiscalYearId = fyId });
                expenses.Add(new Expense { Value = 400, Detail = "مصاريف إدارية", Date = d, CostId = cAdm.Id, MainActive = true, FiscalYearId = fyId });
            }
            db.Set<Expense>().AddRange(expenses);

            // Some extra incomes
            db.Set<Income>().AddRange(
                new Income { Value = 3500, Detail = "مردود عروض موسمية", Date = DateTime.Now.AddMonths(-3), MainActive = true, FiscalYearId = fyId },
                new Income { Value = 2000, Detail = "إيراد إيجار مستودع", Date = DateTime.Now.AddMonths(-2), MainActive = true, FiscalYearId = fyId },
                new Income { Value = 5000, Detail = "مبيعات خردة بلاستيك", Date = DateTime.Now.AddMonths(-1), MainActive = true, FiscalYearId = fyId }
            );
            db.SaveChanges();
        }

        // Employees
        static void SeedEmployees(AppDbContext db)
        {
            if (db.Set<Employee>().Any()) return;
            db.Set<Employee>().AddRange(
                new Employee { Code = 1, Name = "علي حسن", Job = "مدير المصنع", Mobile = "0511100001", ByMonth = true, Salary = "7500" },
                new Employee { Code = 2, Name = "ريم العمري", Job = "محاسبة", Mobile = "0511100002", ByMonth = true, Salary = "5500" },
                new Employee { Code = 3, Name = "أحمد سالم", Job = "مندوب مبيعات", Mobile = "0511100003", ByMonth = true, Salary = "4500" },
                new Employee { Code = 4, Name = "محمد علي", Job = "مندوب مبيعات", Mobile = "0511100004", ByMonth = true, Salary = "4500" },
                new Employee { Code = 5, Name = "خالد يوسف", Job = "مشرف إنتاج", Mobile = "0511100005", ByMonth = true, Salary = "5000" },
                new Employee { Code = 6, Name = "سارة العتيبي", Job = "مسؤول مشتريات", Mobile = "0511100006", ByMonth = true, Salary = "4800" },
                new Employee { Code = 7, Name = "فهد الدوسري", Job = "عامل إنتاج", Mobile = "0511100007", ByDay = true, Salary = "220" },
                new Employee { Code = 8, Name = "يوسف الزهراني", Job = "عامل إنتاج", Mobile = "0511100008", ByDay = true, Salary = "220" },
                new Employee { Code = 9, Name = "نورة الشهري", Job = "خدمة عملاء", Mobile = "0511100009", ByMonth = true, Salary = "3800" }
            );
            db.SaveChanges();
        }

        // Chart of Accounts — seed roots + sub-accounts
        static void SeedAccounts(AppDbContext db)
        {
            if (db.Roots.Any()) return;

            db.Roots.AddRange(
                new Root { Code = "1", Name = "الأصول", AccountType = true },
                new Root { Code = "2", Name = "الالتزامات", AccountType = false },
                new Root { Code = "3", Name = "حقوق الملكية", AccountType = true },
                new Root { Code = "4", Name = "الإيرادات", AccountType = true },
                new Root { Code = "5", Name = "المصروفات", AccountType = false }
            );
            db.SaveChanges();

            db.RootAccounts.AddRange(
                // Assets
                new RootAccount { Code = "1-1", Name = "الخزينة", TypeAccount = "1" },
                new RootAccount { Code = "1-2", Name = "البنك الأهلي", TypeAccount = "1" },
                new RootAccount { Code = "1-3", Name = "المخزون — أكياس بلاستيك", TypeAccount = "1" },
                new RootAccount { Code = "1-4", Name = "المخزون — خرز", TypeAccount = "1" },
                new RootAccount { Code = "1-5", Name = "المخزون — خامات", TypeAccount = "1" },
                new RootAccount { Code = "1-6", Name = "ذمم مدينة (عملاء)", TypeAccount = "1" },
                new RootAccount { Code = "1-7", Name = "آلات ومعدات", TypeAccount = "1" },
                // Liabilities
                new RootAccount { Code = "2-1", Name = "ذمم دائنة (موردون)", TypeAccount = "2" },
                new RootAccount { Code = "2-2", Name = "قروض بنكية", TypeAccount = "2" },
                new RootAccount { Code = "2-3", Name = "مستحقات رواتب", TypeAccount = "2" },
                // Equity
                new RootAccount { Code = "3-1", Name = "رأس المال", TypeAccount = "3" },
                new RootAccount { Code = "3-2", Name = "الأرباح المحتجزة", TypeAccount = "3" },
                // Revenue
                new RootAccount { Code = "4-1", Name = "إيرادات مبيعات أكياس", TypeAccount = "4" },
                new RootAccount { Code = "4-2", Name = "إيرادات مبيعات خرز", TypeAccount = "4" },
                new RootAccount { Code = "4-3", Name = "إيرادات أخرى", TypeAccount = "4" },
                // Expenses
                new RootAccount { Code = "5-1", Name = "تكلفة البضاعة المباعة", TypeAccount = "5" },
                new RootAccount { Code = "5-2", Name = "مصاريف رواتب", TypeAccount = "5" },
                new RootAccount { Code = "5-3", Name = "مصاريف إيجار", TypeAccount = "5" },
                new RootAccount { Code = "5-4", Name = "مصاريف كهرباء وماء", TypeAccount = "5" },
                new RootAccount { Code = "5-5", Name = "مصاريف صيانة", TypeAccount = "5" },
                new RootAccount { Code = "5-6", Name = "مصاريف نقل", TypeAccount = "5" }
            );
            db.SaveChanges();
        }

        // Banks & Loans — sample bank accounts and loan schedules
        static void SeedBanksAndLoans(AppDbContext db)
        {
            if (db.Set<Bank>().Any()) return;

            db.Set<Bank>().AddRange(
                new Bank { Code = "B001", BankName = "البنك الأهلي", AccountNo = "1001234567", Notes = "حساب رئيسي" },
                new Bank { Code = "B002", BankName = "بنك الرياض", AccountNo = "2007654321", Notes = "حساب فرعي" },
                new Bank { Code = "B003", BankName = "البنك السعودي للاستثمار", AccountNo = "3009988776", Notes = "حساب التصدير" }
            );
            db.SaveChanges();

            var bank1 = db.Set<Bank>().First(b => b.Code == "B001");
            var bank2 = db.Set<Bank>().First(b => b.Code == "B002");

            db.Set<BankLoan>().AddRange(
                new BankLoan { BankId = bank1.Id, LoanCode = "LN001", LoanDate = DateTime.Now.AddMonths(-6), Amount = 150000, InterestRate = 4.5, LoanType = "Loan", Status = "Active", Notes = "قرض تشغيل" },
                new BankLoan { BankId = bank2.Id, LoanCode = "LN002", LoanDate = DateTime.Now.AddMonths(-2), Amount = 80000, InterestRate = 3.2, LoanType = "Deposit", Status = "Active", Notes = "وديعة مؤقتة" },
                new BankLoan { BankId = bank1.Id, LoanCode = "LN003", LoanDate = DateTime.Now.AddMonths(-4), Amount = 50000, InterestRate = 2.8, LoanType = "Loan", Status = "Settled", Notes = "قرض سابق" }
            );
            db.SaveChanges();

            var loan1 = db.Set<BankLoan>().First(l => l.LoanCode == "LN001");
            db.Set<LoanPayment>().AddRange(
                new LoanPayment { LoanId = loan1.Id, PayDate = DateTime.Now.AddMonths(-5), Amount = 30000, Notes = "دفعة أولى" },
                new LoanPayment { LoanId = loan1.Id, PayDate = DateTime.Now.AddMonths(-2), Amount = 20000, Notes = "دفعة ثانية" }
            );
            db.SaveChanges();
        }

        // Orders & Delivery — sample customer orders and deliveries
        static void SeedOrdersAndDelivery(AppDbContext db)
        {
            if (db.Set<Order>().Any()) return;

            var order1 = new Order
            {
                Code = 1001,
                Book = true,
                Delivery = true,
                CashName = "سوبرماركت الوفاء",
                DateStart = DateTime.Now.AddDays(-25),
                DateEnd = DateTime.Now.AddDays(-20),
                CustName = "سوبرماركت الوفاء",
                CustTel = "0112002001",
                CustAddress = "الرياض",
                Detail = "طلب أولي لعدد من الأكياس",
                Total = 4200,
                Pay = 2500,
                Paid = false,
                NotPaid = true,
                Discount = 50,
                ResetNo = "R1"
            };
            order1.Details.Add(new OrderDetail { ItemCode = "KB001", Items = "كيس شفاف صغير", Quantity = 100, Price = 35, Total = 3500, Date = order1.DateStart });
            order1.Details.Add(new OrderDetail { ItemCode = "KB002", Items = "كيس شفاف متوسط", Quantity = 20, Price = 55, Total = 1100, Date = order1.DateStart });

            var order2 = new Order
            {
                Code = 1002,
                Book = false,
                Delivery = false,
                CashName = "شركة التوزيع الكبرى",
                DateStart = DateTime.Now.AddDays(-8),
                DateEnd = DateTime.Now.AddDays(-5),
                CustName = "شركة التوزيع الكبرى",
                CustTel = "0112002002",
                CustAddress = "جدة",
                Detail = "طلب فورى",
                Total = 7800,
                Pay = 7800,
                Paid = true,
                NotPaid = false,
                Discount = 0,
                ResetNo = "R2"
            };
            order2.Details.Add(new OrderDetail { ItemCode = "XR001", Items = "خرز أبيض", Quantity = 5000, Price = 0.2f, Total = 1000, Date = order2.DateStart });
            order2.Details.Add(new OrderDetail { ItemCode = "KB006", Items = "كيس أبيض ناعم", Quantity = 200, Price = 45, Total = 9000, Date = order2.DateStart });

            db.Set<Order>().AddRange(order1, order2);
            db.SaveChanges();

            db.Set<DeliveryBill>().AddRange(
                new DeliveryBill { CustomerId = db.Customers.First(c => c.Code == 1).Id, Code = 1001, Date = DateTime.Now.AddDays(-18), Time = DateTime.Now.AddDays(-18), Asked = 3500, Paid = 2500, Notes = "تسليم جزئي", StoreNo = db.Stores.First().Id },
                new DeliveryBill { CustomerId = db.Customers.First(c => c.Code == 2).Id, Code = 1002, Date = DateTime.Now.AddDays(-4), Time = DateTime.Now.AddDays(-4), Asked = 9000, Paid = 9000, Notes = "تسليم كامل", StoreNo = db.Stores.First().Id }
            );
            db.SaveChanges();

            var delivery1 = db.Set<DeliveryBill>().First(d => d.Code == 1001);
            var delivery2 = db.Set<DeliveryBill>().First(d => d.Code == 1002);
            db.Set<DeliveryMovement>().AddRange(
                new DeliveryMovement { GoodId = db.Goods.First(g => g.Code == "KB001").Id, Quantity = 50, Date = delivery1.Date, Notes = "تسليم أول", StoreNo = db.Stores.First().Id, SellPrice = 35, BuyPrice = 18, IsBill = true, Out = true, BillNo = delivery1.Code.ToString(), CustomerId = delivery1.CustomerId },
                new DeliveryMovement { GoodId = db.Goods.First(g => g.Code == "KB006").Id, Quantity = 200, Date = delivery2.Date, Notes = "تسليم ثاني", StoreNo = db.Stores.First().Id, SellPrice = 45, BuyPrice = 22, IsBill = true, Out = true, BillNo = delivery2.Code.ToString(), CustomerId = delivery2.CustomerId }
            );
            db.SaveChanges();
        }

        // Production — raw materials, recipes, orders, waste records
        static void SeedProduction(AppDbContext db)
        {
            if (db.RawMaterials.Any()) return;

            var store = db.Stores.First();
            var uKg = db.Set<Unit>().First(u => u.Code == 2);
            var uGram = db.Set<Unit>().First(u => u.Code == 3);
            var user = db.Users.First();

            // ── Raw Materials ──
            db.RawMaterials.AddRange(
                new RawMaterial { Code = "RM001", Name = "حبيبات HDPE بيضاء", UnitId = uKg.Id, StoreId = store.Id, BuyPrice = 8, CurrentStock = 2000, MinStock = 500, LastPurchase = DateTime.Now.AddMonths(-1) },
                new RawMaterial { Code = "RM002", Name = "حبيبات LDPE شفافة", UnitId = uKg.Id, StoreId = store.Id, BuyPrice = 9, CurrentStock = 1800, MinStock = 500, LastPurchase = DateTime.Now.AddMonths(-1) },
                new RawMaterial { Code = "RM003", Name = "صبغة سوداء", UnitId = uKg.Id, StoreId = store.Id, BuyPrice = 25, CurrentStock = 150, MinStock = 50, LastPurchase = DateTime.Now.AddMonths(-2) },
                new RawMaterial { Code = "RM004", Name = "صبغة ملونة متعددة", UnitId = uKg.Id, StoreId = store.Id, BuyPrice = 30, CurrentStock = 120, MinStock = 40, LastPurchase = DateTime.Now.AddMonths(-2) },
                new RawMaterial { Code = "RM005", Name = "خرز زجاجي خام", UnitId = uGram.Id, StoreId = store.Id, BuyPrice = 0.07, CurrentStock = 50000, MinStock = 10000, LastPurchase = DateTime.Now.AddMonths(-1) },
                new RawMaterial { Code = "RM006", Name = "سلك ربط خرز", UnitId = uKg.Id, StoreId = store.Id, BuyPrice = 15, CurrentStock = 80, MinStock = 20, LastPurchase = DateTime.Now.AddMonths(-3) }
            );
            db.SaveChanges();

            var rm1 = db.RawMaterials.First(r => r.Code == "RM001");
            var rm2 = db.RawMaterials.First(r => r.Code == "RM002");
            var rm3 = db.RawMaterials.First(r => r.Code == "RM003");
            var rm4 = db.RawMaterials.First(r => r.Code == "RM004");
            var rm5 = db.RawMaterials.First(r => r.Code == "RM005");
            var rm6 = db.RawMaterials.First(r => r.Code == "RM006");

            // ── Products for recipes ──
            var bagSmall = db.Goods.First(g => g.Code == "KB001");
            var bagMed = db.Goods.First(g => g.Code == "KB002");
            var bagBlack = db.Goods.First(g => g.Code == "KB004");
            var beads = db.Goods.First(g => g.Code == "XR001");

            var rec1 = new ProductionRecipe
            {
                Code = "PR001",
                Name = "وصفة كيس شفاف صغير",
                GoodId = bagSmall.Id,
                OutputQuantity = 1000,
                IsActive = true,
                CreatedDate = DateTime.Now.AddMonths(-6)
            };
            rec1.RecipeItems.Add(new RecipeItem { RawMaterialId = rm1.Id, Quantity = 12 });
            rec1.RecipeItems.Add(new RecipeItem { RawMaterialId = rm2.Id, Quantity = 8 });

            var rec2 = new ProductionRecipe
            {
                Code = "PR002",
                Name = "وصفة كيس شفاف متوسط",
                GoodId = bagMed.Id,
                OutputQuantity = 800,
                IsActive = true,
                CreatedDate = DateTime.Now.AddMonths(-6)
            };
            rec2.RecipeItems.Add(new RecipeItem { RawMaterialId = rm1.Id, Quantity = 18 });
            rec2.RecipeItems.Add(new RecipeItem { RawMaterialId = rm2.Id, Quantity = 12 });

            var rec3 = new ProductionRecipe
            {
                Code = "PR003",
                Name = "وصفة كيس أسود",
                GoodId = bagBlack.Id,
                OutputQuantity = 1000,
                IsActive = true,
                CreatedDate = DateTime.Now.AddMonths(-5)
            };
            rec3.RecipeItems.Add(new RecipeItem { RawMaterialId = rm1.Id, Quantity = 15 });
            rec3.RecipeItems.Add(new RecipeItem { RawMaterialId = rm3.Id, Quantity = 1.5 });

            var rec4 = new ProductionRecipe
            {
                Code = "PR004",
                Name = "وصفة خرز زجاجي أبيض",
                GoodId = beads.Id,
                OutputQuantity = 5000,
                IsActive = true,
                CreatedDate = DateTime.Now.AddMonths(-4)
            };
            rec4.RecipeItems.Add(new RecipeItem { RawMaterialId = rm5.Id, Quantity = 5200 });
            rec4.RecipeItems.Add(new RecipeItem { RawMaterialId = rm6.Id, Quantity = 0.5 });

            db.ProductionRecipes.AddRange(rec1, rec2, rec3, rec4);
            db.SaveChanges();

            var cust1 = db.Customers.FirstOrDefault();
            var orders = new List<ProductionOrder>
            {
                new ProductionOrder { Code="PO001", ModelName="موديل A", OrderDate=DateTime.Now.AddMonths(-4), StartDate=DateTime.Now.AddMonths(-4).AddDays(1), DeliveryDate=DateTime.Now.AddMonths(-4).AddDays(3), RequestedQty=5000, ProducedWeight=4850, WasteWeight=150, Status="Delivered", CreatedByUserId=user.Id, CustomerId=cust1?.Id },
                new ProductionOrder { Code="PO002", ModelName="موديل B", OrderDate=DateTime.Now.AddMonths(-3), StartDate=DateTime.Now.AddMonths(-3).AddDays(1), DeliveryDate=DateTime.Now.AddMonths(-3).AddDays(4), RequestedQty=4000, ProducedWeight=3920, WasteWeight=80, Status="Delivered", CreatedByUserId=user.Id, CustomerId=cust1?.Id },
                new ProductionOrder { Code="PO003", ModelName="موديل A", OrderDate=DateTime.Now.AddMonths(-2), StartDate=DateTime.Now.AddMonths(-2).AddDays(1), DeliveryDate=DateTime.Now.AddMonths(-2).AddDays(3), RequestedQty=6000, ProducedWeight=5800, WasteWeight=200, Status="Done", CreatedByUserId=user.Id },
                new ProductionOrder { Code="PO004", ModelName="موديل C", OrderDate=DateTime.Now.AddMonths(-1), StartDate=DateTime.Now.AddMonths(-1).AddDays(1), DeliveryDate=DateTime.Now.AddMonths(-1).AddDays(2), RequestedQty=10000, ProducedWeight=9750, WasteWeight=250, Status="Done", CreatedByUserId=user.Id },
                new ProductionOrder { Code="PO005", ModelName="موديل B", OrderDate=DateTime.Now.AddDays(-10), StartDate=DateTime.Now.AddDays(-9), DeliveryDate=DateTime.Now.AddDays(5), RequestedQty=8000, ProducedWeight=0, WasteWeight=0, Status="InProgress", CreatedByUserId=user.Id },
                new ProductionOrder { Code="PO006", ModelName="موديل A", OrderDate=DateTime.Now.AddDays(-3), DeliveryDate=DateTime.Now.AddDays(10), RequestedQty=5000, ProducedWeight=0, WasteWeight=0, Status="InProgress", CreatedByUserId=user.Id },
            };
            db.ProductionOrders.AddRange(orders);
            db.SaveChanges();

            foreach (var ord in orders)
            {
                db.ProductionMaterials.AddRange(
                    new ProductionMaterial { ProductionOrderId = ord.Id, RawMaterialId = rm1.Id },
                    new ProductionMaterial { ProductionOrderId = ord.Id, RawMaterialId = rm2.Id },
                    new ProductionMaterial { ProductionOrderId = ord.Id, RawMaterialId = rm5.Id }
                );
            }
            db.SaveChanges();

            // ── Raw Material Movements ──
            var rmMoves = new List<RawMaterialMovement>();
            rmMoves.Add(new RawMaterialMovement { RawMaterialId = rm1.Id, Quantity = 3000, Date = DateTime.Now.AddMonths(-5), IsPurchase = true, Out = false, Notes = "شراء دفعة أولى", UnitPrice = 8 });
            rmMoves.Add(new RawMaterialMovement { RawMaterialId = rm2.Id, Quantity = 2500, Date = DateTime.Now.AddMonths(-5), IsPurchase = true, Out = false, Notes = "شراء دفعة أولى", UnitPrice = 9 });
            rmMoves.Add(new RawMaterialMovement { RawMaterialId = rm3.Id, Quantity = 300, Date = DateTime.Now.AddMonths(-4), IsPurchase = true, Out = false, Notes = "شراء صبغة سوداء", UnitPrice = 25 });
            rmMoves.Add(new RawMaterialMovement { RawMaterialId = rm5.Id, Quantity = 80000, Date = DateTime.Now.AddMonths(-3), IsPurchase = true, Out = false, Notes = "شراء خرز خام", UnitPrice = 0.07 });
            rmMoves.Add(new RawMaterialMovement { RawMaterialId = rm1.Id, Quantity = 1500, Date = DateTime.Now.AddMonths(-3), IsProduction = true, Out = true, Notes = "استهلاك PO001+PO002", UnitPrice = 8 });
            rmMoves.Add(new RawMaterialMovement { RawMaterialId = rm2.Id, Quantity = 1200, Date = DateTime.Now.AddMonths(-3), IsProduction = true, Out = true, Notes = "استهلاك PO001+PO002", UnitPrice = 9 });
            rmMoves.Add(new RawMaterialMovement { RawMaterialId = rm1.Id, Quantity = 2000, Date = DateTime.Now.AddMonths(-1), IsPurchase = true, Out = false, Notes = "شراء دفعة ثانية", UnitPrice = 8.5 });
            db.RawMaterialMovements.AddRange(rmMoves);
            db.SaveChanges();
        }

        static void SeedBankLoans(AppDbContext db)
        {
            if (!db.Banks.Any()) return;

            var banks = db.Banks.ToList();
            if (banks.Count == 0) return;

            var bank1 = banks[0];
            var bank2 = banks.Count > 1 ? banks[1] : banks[0];

            var loan1 = new BankLoan
            {
                BankId = bank1.Id,
                LoanCode = "LN2025001",
                LoanType = "Loan",
                Amount = 50000,
                InterestRate = 5,
                LoanDate = DateTime.Now.AddMonths(-8),
                Status = "Active",
                Notes = "قرض تمويل مخزون أكياس"
            };
            loan1.Payments.Add(new LoanPayment { Amount = 10000, PayDate = DateTime.Now.AddMonths(-6), Notes = "دفعة أولى" });
            loan1.Payments.Add(new LoanPayment { Amount = 10000, PayDate = DateTime.Now.AddMonths(-3), Notes = "دفعة ثانية" });

            var loan2 = new BankLoan
            {
                BankId = bank2.Id,
                LoanCode = "LN2026001",
                LoanType = "Loan",
                Amount = 30000,
                InterestRate = 4.5,
                LoanDate = DateTime.Now.AddMonths(-3),
                Status = "Active",
                Notes = "قرض تطوير خط الإنتاج"
            };
            loan2.Payments.Add(new LoanPayment { Amount = 5000, PayDate = DateTime.Now.AddMonths(-1), Notes = "دفعة أولى" });

            var deposit1 = new BankLoan
            {
                BankId = bank1.Id,
                LoanCode = "DEP2026001",
                LoanType = "Deposit",
                Amount = 20000,
                InterestRate = 2,
                LoanDate = DateTime.Now.AddMonths(-2),
                Status = "Active",
                Notes = "إيداع احتياطي"
            };

            var oldLoan = new BankLoan
            {
                BankId = bank2.Id,
                LoanCode = "LN2024001",
                LoanType = "Loan",
                Amount = 15000,
                InterestRate = 6,
                LoanDate = DateTime.Now.AddMonths(-18),
                Status = "Settled",
                Notes = "قرض قديم مسدد بالكامل"
            };
            oldLoan.Payments.Add(new LoanPayment { Amount = 15000, PayDate = DateTime.Now.AddMonths(-6), Notes = "سداد كامل" });

            db.BankLoans.AddRange(loan1, loan2, deposit1, oldLoan);
            db.SaveChanges();

            // Add FiscalYears seed too
            if (!db.FiscalYears.Any())
            {
                db.FiscalYears.AddRange(
                    new FiscalYear { Year = 2025, StartDate = new DateTime(2025, 1, 1), EndDate = new DateTime(2025, 12, 31), IsClosed = true, Notes = "السنة المالية 2025 - مغلقة" },
                    new FiscalYear { Year = 2026, StartDate = new DateTime(2026, 1, 1), EndDate = new DateTime(2026, 12, 31), IsClosed = false, Notes = "السنة المالية الحالية" }
                );
                db.SaveChanges();
            }
        }
    }
}
