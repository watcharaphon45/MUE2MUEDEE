-- ==============================================================================
-- MUE2MUEDEE: ชุดข้อมูลตัวอย่างรูปภาพสินค้า (Inventory Item Images Seed Script)
-- Database: PostgreSQL (Neon)
-- ==============================================================================

-- ตรวจสอบรายการสินค้า 3 ชิ้นล่าสุดในระบบก่อนรัน
-- SELECT id, sku, store_id, product_id FROM inventory_items WHERE is_deleted = false ORDER BY created_at DESC LIMIT 3;

DO $$
DECLARE
    v_item1 RECORD;
    v_item2 RECORD;
    v_item3 RECORD;
BEGIN
    -- 1. ดึงรายการสินค้า 3 ชิ้นแรกที่มีอยู่ในร้าน
    SELECT id, store_id INTO v_item1 
    FROM inventory_items 
    WHERE is_deleted = false 
    ORDER BY created_at DESC 
    OFFSET 0 LIMIT 1;

    SELECT id, store_id INTO v_item2 
    FROM inventory_items 
    WHERE is_deleted = false 
    ORDER BY created_at DESC 
    OFFSET 1 LIMIT 1;

    SELECT id, store_id INTO v_item3 
    FROM inventory_items 
    WHERE is_deleted = false 
    ORDER BY created_at DESC 
    OFFSET 2 LIMIT 1;

    -- ==============================================================================
    -- ชุดที่ 1: กล้อง Mirrorless (เช่น Sony A7 IV) — 5 มุมมองรอบตัวเครื่อง
    -- ==============================================================================
    IF v_item1.id IS NOT NULL THEN
        -- ลบรูปเดิมของชิ้นนี้ออกก่อนเพื่อป้องกันการซ้ำซ้อน
        DELETE FROM inventory_item_images WHERE inventory_item_id = v_item1.id;

        INSERT INTO inventory_item_images (id, store_id, inventory_item_id, url, sort_order, created_at, created_by)
        VALUES
        -- รูปที่ 1 (Sort Order 0 = รูปปกหน้าร้าน): ตัวเครื่องด้านหน้าเฉียง 45 องศา
        (gen_random_uuid(), v_item1.store_id, v_item1.id, 
         'https://images.unsplash.com/photo-1516035069371-29a1b244cc32?auto=format&fit=crop&w=1200&q=80', 
         0, NOW(), 'seed_script'),
        
        -- รูปที่ 2 (Sort Order 1): จอหลังและปุ่มควบคุม
        (gen_random_uuid(), v_item1.store_id, v_item1.id, 
         'https://images.unsplash.com/photo-1502982720700-bfff97f2ecac?auto=format&fit=crop&w=1200&q=80', 
         1, NOW(), 'seed_script'),

        -- รูปที่ 3 (Sort Order 2): ด้านบน แป้นไดอัลหมุน และชัตเตอร์
        (gen_random_uuid(), v_item1.store_id, v_item1.id, 
         'https://images.unsplash.com/photo-1512790182412-b19e6d62bc39?auto=format&fit=crop&w=1200&q=80', 
         2, NOW(), 'seed_script'),

        -- รูปที่ 4 (Sort Order 3): ส่องเซนเซอร์และเมาท์โลหะ (ไร้ฝ้าไร้รา)
        (gen_random_uuid(), v_item1.store_id, v_item1.id, 
         'https://images.unsplash.com/photo-1513694203232-719a280e022f?auto=format&fit=crop&w=1200&q=80', 
         3, NOW(), 'seed_script'),

        -- รูปที่ 5 (Sort Order 4): กล่อง อุปกรณ์ สายคล้อง และแบตเตอรี่แท้
        (gen_random_uuid(), v_item1.store_id, v_item1.id, 
         'https://images.unsplash.com/photo-1526170375885-4d8ecf77b99f?auto=format&fit=crop&w=1200&q=80', 
         4, NOW(), 'seed_script');

        RAISE NOTICE 'Inserted 5 images for Item 1 (ID: %)', v_item1.id;
    END IF;

    -- ==============================================================================
    -- ชุดที่ 2: กล้อง Classic Retro (เช่น Fujifilm X-T5 / X100V) — 4 มุมมอง
    -- ==============================================================================
    IF v_item2.id IS NOT NULL THEN
        DELETE FROM inventory_item_images WHERE inventory_item_id = v_item2.id;

        INSERT INTO inventory_item_images (id, store_id, inventory_item_id, url, sort_order, created_at, created_by)
        VALUES
        -- รูปที่ 1 (Sort Order 0 = รูปปกหน้าร้าน): ด้านหน้าสไตล์เรโทร
        (gen_random_uuid(), v_item2.store_id, v_item2.id, 
         'https://images.unsplash.com/photo-1508921912186-1d1a45ebb3c1?auto=format&fit=crop&w=1200&q=80', 
         0, NOW(), 'seed_script'),

        -- รูปที่ 2 (Sort Order 1): แป้นหมุน Shutter Speed & Exposure ด้านบน
        (gen_random_uuid(), v_item2.store_id, v_item2.id, 
         'https://images.unsplash.com/photo-1500648767791-00dcc994a43e?auto=format&fit=crop&w=1200&q=80', 
         1, NOW(), 'seed_script'),

        -- รูปที่ 3 (Sort Order 2): เลนส์ชิ้นหน้าและกริปจับยาง
        (gen_random_uuid(), v_item2.store_id, v_item2.id, 
         'https://images.unsplash.com/photo-1495745966610-2a67f2297e5e?auto=format&fit=crop&w=1200&q=80', 
         2, NOW(), 'seed_script'),

        -- รูปที่ 4 (Sort Order 3): หน้าจอพับและช่องมองภาพ Viewfinder
        (gen_random_uuid(), v_item2.store_id, v_item2.id, 
         'https://images.unsplash.com/photo-1588702547919-26089e690ecc?auto=format&fit=crop&w=1200&q=80', 
         3, NOW(), 'seed_script');

        RAISE NOTICE 'Inserted 4 images for Item 2 (ID: %)', v_item2.id;
    END IF;

    -- ==============================================================================
    -- ชุดที่ 3: เลนส์กล้องระดับโปร (เช่น Canon RF 50mm / Sony GM) — 4 มุมมอง
    -- ==============================================================================
    IF v_item3.id IS NOT NULL THEN
        DELETE FROM inventory_item_images WHERE inventory_item_id = v_item3.id;

        INSERT INTO inventory_item_images (id, store_id, inventory_item_id, url, sort_order, created_at, created_by)
        VALUES
        -- รูปที่ 1 (Sort Order 0 = รูปปกหน้าร้าน): ตัวกระบอกเลนส์ด้านข้าง
        (gen_random_uuid(), v_item3.store_id, v_item3.id, 
         'https://images.unsplash.com/photo-1617005082133-548c4dd27f35?auto=format&fit=crop&w=1200&q=80', 
         0, NOW(), 'seed_script'),

        -- รูปที่ 2 (Sort Order 1): ชิ้นแก้วหน้าเลนส์ ใส โค้ทติ้งสมบูรณ์
        (gen_random_uuid(), v_item3.store_id, v_item3.id, 
         'https://images.unsplash.com/photo-1510127034890-ba27508e9f1c?auto=format&fit=crop&w=1200&q=80', 
         1, NOW(), 'seed_script'),

        -- รูปที่ 3 (Sort Order 2): ท้ายเลนส์ เมาท์โลหะ ขั้วไฟฟ้ารับส่งข้อมูล
        (gen_random_uuid(), v_item3.store_id, v_item3.id, 
         'https://images.unsplash.com/photo-1507679799987-c73779587ccf?auto=format&fit=crop&w=1200&q=80', 
         2, NOW(), 'seed_script'),

        -- รูปที่ 4 (Sort Order 3): ฮูดแท้ ฝาปิดหน้า-หลัง และถุงผ้ากำมะหยี่
        (gen_random_uuid(), v_item3.store_id, v_item3.id, 
         'https://images.unsplash.com/photo-1520390138845-fd2d229dd553?auto=format&fit=crop&w=1200&q=80', 
         3, NOW(), 'seed_script');

        RAISE NOTICE 'Inserted 4 images for Item 3 (ID: %)', v_item3.id;
    END IF;
END $$;

-- ==============================================================================
-- คำสั่ง SELECT ตรวจสอบผลลัพธ์
-- ==============================================================================
SELECT 
    i.sku,
    p.model_name,
    img.sort_order,
    CASE WHEN img.sort_order = 0 THEN '⭐ รูปปก' ELSE 'รูปมุมเสริม' END AS role,
    img.url,
    img.created_at
FROM inventory_item_images img
JOIN inventory_items i ON img.inventory_item_id = i.id
JOIN products p ON i.product_id = p.id
ORDER BY i.sku, img.sort_order ASC;
